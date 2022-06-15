using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Fabros.Ecs.ClientServer.Serializer;
using Fabros.EcsModules.Tick.Components;
using Fabros.EcsModules.Tick.Other;
using Fabros.P2P;
using Game.ClientServer;
using Game.Ecs.ClientServer.Components.Input.Proto;
using Game.Fabros.Net.ClientServer;
using Game.Fabros.Net.ClientServer.Ecs.Components;
using Game.Fabros.Net.ClientServer.Protocol;
using Leopotam.EcsLite;

namespace ConsoleApp
{
    class Client
    {
        public int ID;
        public int Delay;
        public ClientAddr Address;
        public Client(int id)
        {
            this.ID = id;
            Address = new ClientAddr(id.ToString());
        }
    }

    class Program
    {
        private LeoContexts leo;
        private ClientWebSocket socket;

        private EcsWorld world;
        private EcsWorld sentWorld;
        private EcsWorld inputWorld;

        private EcsSystems systems;
        private List<Client> clients = new List<Client>();
        private List<int> missingClients = new List<int>();
        private ApplyWorldChangesInputService inputService = new ApplyWorldChangesInputService();

        private List<byte[]> receivedMessages = new List<byte[]>();
        private HGlobalWriter writer = new HGlobalWriter();

        async Task Run()
        {
            socket = new ClientWebSocket();

            AsyncRun();
            while (true)
            {
                Thread.Sleep(1000);
            }
        }

        async Task ReceiveData()
        {

            var ct = new CancellationTokenSource();

            var rcvBytes = new byte[64000];
            var rcvBuffer = new ArraySegment<byte>(rcvBytes);

            try
            {
                while (true)
                {
                    WebSocketReceiveResult rcvResult = await socket.ReceiveAsync(rcvBuffer, ct.Token);

                    byte[] msgBytes = rcvBuffer.Skip(rcvBuffer.Offset).Take(rcvResult.Count).ToArray();
                    //ProcessMessage(msgBytes);
                    lock (receivedMessages)
                    {
                        receivedMessages.Add(msgBytes);
                    }                    
                }
            }
            catch (Exception e)
            {
                Console.Write(e);
            }
        }
        
        async Task AsyncRun()
        {
            try
            {
                var url = $"{Config.url}/{P2P.ADDR_SERVER.AddressString}";
                await socket.ConnectAsync(new Uri(url, UriKind.Absolute), new CancellationToken());

                Console.WriteLine($"connected to host\n{url}");


                var pool = SharedComponents.CreateComponentsPool();

    
                //на сервере может быть tickrate отличный от клиента, например 20 вместо 60
                //это надо учесть при симуляции
                var config = new TickrateConfigComponent { clientTickrate = 30, serverSyncStep = 1, serverTickrate = 30 };

                world = WorldUtils.CreateWorld("serv", pool);

                inputWorld = new EcsWorld("input");

                LeoContexts.InitNewWorld(world, config);

                systems = new EcsSystems(world);
                systems.AddWorld(inputWorld, "input");

                var factory = new EcsSystemsFactory(pool);
                factory.AddNewSystems(systems, new IEcsSystemsFactory.Settings { client = false, server = true });


                leo = new LeoContexts(Config.TMP_HASHES_PATH, pool, 
                    new SyncLog(Config.SYNC_LOG_PATH), inputWorld);
                /*
                leo.WriteToConsole = (string str) =>
                {
                    Console.WriteLine(str);
                };*/


                systems.Init();


                sentWorld = WorldUtils.CopyWorld(pool, world);

                //leo.Init(world, config);


                world.AddUnique(new TickDeltaComponent { Value = new TickDelta (config.clientTickrate / config.serverTickrate, config.serverTickrate) });


                _ = Task.Factory.StartNew(ReceiveData);
                
                SendWorldToClients();


                Console.WriteLine("loop");
                var next = DateTime.UtcNow;
                var step = 1.0 / config.serverTickrate;
                while (true)
                {
                    byte[][] receivedCopy = null;
                    lock (receivedMessages) {
                        receivedCopy = receivedMessages.ToArray();
                        receivedMessages.Clear();
                    }


                    for (int i = 0; i < receivedCopy.Length; ++i)
                        ProcessMessage(receivedCopy[i]);

                    if (next <= DateTime.UtcNow)
                    {
                        //Console.WriteLine($"tick {leo.GetCurrentTick(world)}");
                        next = DateTime.UtcNow.AddSeconds(step);
                        Tick();                        
                    }

                    Thread.Sleep(0);
                }
            }
            catch(Exception e)
            {
                Console.Write(e);
            }
        }


        private void ProcessMessage(byte[] msgBytes)
        {
            string errAddr = "";
            if (P2P.CheckError(msgBytes, out errAddr))
            {
                var client = clients.FirstOrDefault(client => client.ID.ToString() == errAddr);
                if (client != null)
                {
                    clients.Remove(client);
                    Console.WriteLine($"removed client {client.ID}");
                    BaseServices.LeavePlayer(inputWorld, client.ID);
                }
                return;
            }

            if (msgBytes[0] == 0xff && msgBytes[1] == 0 && msgBytes[2] == 0 && msgBytes[3] == 0)
            {
                GotInput(msgBytes);
                return;
            }

            var packet = P2P.ParseResponse<Packet>(msgBytes);

            if (packet.hasHello)
            {
                var client = new Client(packet.playerID);

                Console.WriteLine($"got hello from client {packet.playerID}");

                var components = leo.Pool.Components.Select(component => component.GetFullName()).ToArray();
                var hello = new Hello {Components = components};

                SendAsync(new Packet { hello = hello, hasHello = true }, client.Address);

                var emptyWorld = WorldUtils.CreateWorld("empty", leo.Pool);
                SendInitialWorld(emptyWorld, client);

                clients.Add(client);
                BaseServices.JoinPlayer(inputWorld, packet.playerID);                
            }
        }

        private void GotInput(byte[] data)
        {
            //var buffer = Marshal.AllocHGlobal(data.Length);
            var allocatedBuffer = Marshal.AllocHGlobal(data.Length);
            var buffer = allocatedBuffer;

            Marshal.Copy(data, 0, buffer, data.Length);

            try            
            {
                buffer += 4;//0xff

                var playerId = Marshal.ReadInt32(buffer);
                buffer += 4;

                var inputTime = Marshal.ReadInt32(buffer);
                var time = inputTime;
                buffer += 4;

                var type = Marshal.ReadInt32(buffer);
                buffer += 4;


                Client client = clients.FirstOrDefault(client => client.ID == playerId);
                if (client == null)
                {
                    if (!missingClients.Contains(playerId))
                    {
                        missingClients.Add(playerId);
                        Console.WriteLine($"not found player {playerId}");
                    }
                    return;
                }


                var currentTick = leo.GetCurrentTick(world);
                var step = world.GetUnique<TickDeltaComponent>().Value;
                //на сколько тиков мы опередили сервер или отстали
                var delay = time - currentTick.Value;

                //если ввод от клиента не успел прийти вовремя, то выполним его уже в текущем тике
                if (delay < 0)
                    time = currentTick.Value;


                var sentWorldTick = leo.GetCurrentTick(sentWorld) - step;

                if (delay == 0 && sentWorldTick == time)
                    time = currentTick.Value + step.Value;


                client.Delay = delay;

                if (type == 0)//ping
                {
                    
                }
                else
                {

                    Console.WriteLine($"{Thread.CurrentThread.ManagedThreadId} got input {inputTime} at {currentTick.Value} will be executed at {time}");
                    IInputComponent component = null;
                    if (type == 1)
                    {
                        component = Marshal.PtrToStructure<InputActionComponent>(buffer);
                    }
                    if (type == 2)
                    {
                        component = Marshal.PtrToStructure<InputMoveDirectionComponent>(buffer);
                    }
                    if (type == 3)
                    {
                        component = Marshal.PtrToStructure<InputMoveToPointComponent>(buffer);
                    }
                    if (type == 4)
                    {
                        component = Marshal.PtrToStructure<InputShotComponent>(buffer);
                    }

                    var input = new UserInput
                    {
                        PlayerID = playerId,
                        Component = component,
                        Tick = new Tick(time)
                    };

                    inputService.Input(inputWorld, playerId, time, component);
                    //leo.Inputs.Add(input);
                    //world.GetUniqueRef<PendingInputComponent>().data = leo.Inputs.ToArray();
                }
            } finally
            {
                Marshal.FreeHGlobal(allocatedBuffer);
            }
        }

        private void Tick()
        {
            //var time0 = leo.GetCurrentTick(world);

            //Console.WriteLine($"{Thread.CurrentThread.ManagedThreadId} Tick Begin {time0.Value}");
            var time = leo.GetCurrentTick(world);
            leo.FilterInputs(time);            

            //ref var component = ref world.GetUniqueRef<PendingInputComponent>();
         
            //обновляем мир 1 раз
            leo.Tick(systems, inputWorld, world, Config.SyncDataLogging);
        
            time = leo.GetCurrentTick(world);

            var cfg = leo.GetConfig(world);

            if (leo.GetCurrentTick(world).Value % cfg.serverSyncStep == 0)
            {
                //если у сервера высокий tickrate, например 60
                //то отправлять миру 60 раз в секунду - накладно
                //можно делать это реже, например 20 раз в секунду если serverSyncStep==3
                SendWorldToClients();
                //удаляем ввод игрока который устарел
                //var abc = component.data.Where(input => input.Tick >= time);
                //component.data = abc.ToArray();
                //leo.FilterInputs(time);
            }

           // Console.WriteLine($"{Thread.CurrentThread.ManagedThreadId}  Tick End {time0.Value}");
        }

        void SendWorldToClients()
        {
            var dif = WorldUtils.BuildDiff(leo.Pool, sentWorld, world, false, true);
            WorldUtils.BinarySerialize(leo.Pool, dif, writer);

            var difBinary = Convert.ToBase64String(P2P.Compress(writer.CopyToByteArray()));

            leo.SyncLog.WriteLine($"send world {leo.GetPrevTick(world)}->{leo.GetCurrentTick(world)} to clients\n");

            //clients list could be modified
            Array.ForEach(clients.ToArray(), client =>
            {
                var packet = new Packet
                {
                    hasWorldUpdate = true,
                    WorldUpdate = new WorldUpdateProto
                    {
                        difBinary = difBinary,
                        delay = client.Delay
                    }
                };
                
                SendAsync(packet, client.Address);
                client.Delay = -999;
            });
            
            
            /*
            
            var packet = new Packet
            {
                hasWorldUpdate = true,
                WorldUpdate = new WorldUpdateProto
                {
                    dif = dif,
                    delay = 1
                }
            };
            sendAsync(packet, P2P.ADDR_BROADCAST);
            */

                            
            //сохраняем отправленный мир чтоб с ним потом считать diff
            sentWorld = WorldUtils.CopyWorld(leo.Pool, world);
        }

        void SendInitialWorld(EcsWorld prevWorld, Client client)
        {
            var dif = WorldUtils.BuildDiff(leo.Pool, prevWorld, sentWorld, false, false);

            var packet = new Packet
            {
                hasWelcomeFromServer = true,
                WorldUpdate = new WorldUpdateProto
                {
                    difStr = dif,
                    delay = 1
                }
            };
            
            SendAsync(packet, client.Address);
            
            Console.WriteLine($"initial world send to client {client.ID}");
        }

        private void SendAsync(Packet packet, ClientAddr addr)
        {
            var bytes = P2P.BuildRequest(addr, packet);
            socket.SendAsync(bytes, WebSocketMessageType.Text, true, new CancellationToken());            
        }

        static void Main(string[] args)
        {
            var app = new Program();
            app.Run();
            
            /*
            var worldJson = File.ReadAllText("world.ecs");
            //var world = new EcsWorld();
            var res = JsonConvert.DeserializeObject<WorldDiff>(worldJson);
            */
        }
    }
}