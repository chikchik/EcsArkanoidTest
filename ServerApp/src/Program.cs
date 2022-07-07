using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Fabros.Ecs.ClientServer.Utils;
using Fabros.Ecs.ClientServer.WorldDiff;
using Fabros.Ecs.Utils;
using Fabros.EcsModules.Tick.ClientServer.Components;
using Fabros.EcsModules.Tick.Other;
using Fabros.P2P;
using Game.ClientServer;
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
        public int LastClientTick;
        public int LastServerTick;
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
        private ApplyInputWorldService inputService = new ApplyInputWorldService();
        private EntityDestroyedListener destroyedListener = new EntityDestroyedListener();

        private List<byte[]> receivedMessages = new List<byte[]>();
        private HGlobalWriter writer = new HGlobalWriter();

        readonly ComponentsCollection pool = SharedComponents.CreateComponentsPool();

        private TickrateConfigComponent config = new TickrateConfigComponent { Tickrate = 30, ServerSyncStep = 1 };

        async Task Run()
        {
            socket = new ClientWebSocket();

            AsyncRun();
            while (true)
            {
                Thread.Sleep(1000);
            }
        }


        private static void log(string str)
        {
            var tm = TimeUtils.GetUnixTimeMS();
            Console.WriteLine($"{tm} {str}");
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
        
        private void StartSystems(byte[] initialWorld)
        {
            Console.WriteLine("StartSystems");
            WorldDiff dif = null;
            if (initialWorld?.Length > 0)
            {
                Console.WriteLine($"FromByteArray {initialWorld.Length}");
                dif = WorldDiff.FromByteArray(pool, initialWorld);
            }
            else
            {
                dif = WorldDiff.FromJsonString(pool, File.ReadAllText("world.ecs.json"));
            }


            world = new EcsWorld("serv");
            world.EntityDestroyedListeners.Add(destroyedListener);
             
            inputWorld = new EcsWorld("input");

            world.AddUnique(config);
            world.AddUnique<TickComponent>().Value = new Tick(0);
            world.AddUnique(new TickDeltaComponent { Value = new TickDelta(config.Tickrate) });


            systems = new EcsSystems(world);
            systems.AddWorld(inputWorld, "input");

            var factory = new EcsSystemsFactory(pool);
            factory.AddNewSystems(systems, new IEcsSystemsFactory.Settings { client = false, server = true });


            leo = new LeoContexts(Config.TMP_HASHES_PATH, pool, inputWorld);
            /*
            leo.WriteToConsole = (string str) =>
            {
                Console.WriteLine(str);
            };*/

            
            dif.ApplyChanges(world);

            systems.Init();

            sentWorld = WorldUtils.CopyWorld(pool, world);
        }

        async Task AsyncRun()
        {
            try
            {
                //StartSystems(null);
                WorldLoggerExt.logger = new SyncWorldLogger();

                var url = $"{Config.url}/{P2P.ADDR_SERVER.AddressString}";
                await socket.ConnectAsync(new Uri(url, UriKind.Absolute), new CancellationToken());

                log($"connected to host\n{url}");


                _ = Task.Factory.StartNew(ReceiveData);
                
                SendWorldToClients();


                log("loop");
                var next = DateTime.UtcNow;
                var step = 1.0 / config.Tickrate;
                while (true)
                {
                    byte[][] receivedCopy = null;
                    lock (receivedMessages) {
                        receivedCopy = receivedMessages.ToArray();
                        receivedMessages.Clear();
                    }


                    for (int i = 0; i < receivedCopy.Length; ++i)
                        ProcessMessage(receivedCopy[i]);

                    if (next <= DateTime.UtcNow && systems != null)
                    {
                        //Console.WriteLine($"tick {leo.GetCurrentTick(world)}");
                        next = next.AddSeconds(step);
                        if (next <= DateTime.UtcNow)
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
                    log($"removed client {client.ID}");
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

                log($"got hello from client {packet.playerID}");

                var components = pool.Components.Select(component => component.GetComponentType().FullName).ToArray();
                var hello = new Hello {Components = components};
                                
                if (clients.Count == 0 &&  systems == null) {
                    //первый игрок присылает игровой стейт на сервер и сервер стартует с ним
                    StartSystems(Convert.FromBase64String(packet.hello.InitialWorld));
                }

                SendAsync(new Packet { hello = hello, hasHello = true }, client.Address);

                var emptyWorld = new EcsWorld("empty");
                SendInitialWorld(emptyWorld, client);

                clients.Add(client);
                BaseServices.JoinPlayer(inputWorld, packet.playerID);                
            }
        }

        private void GotInput(byte[] data)
        {
            var reader = new HGlobalReader(data);

            try            
            {
                reader.ReadInt32();//0xff

                var playerId = reader.ReadInt32();

                var inputTime = reader.ReadInt32();
                var time = inputTime;

                var type = reader.ReadInt32();


                Client client = clients.FirstOrDefault(client => client.ID == playerId);
                if (client == null)
                {
                    if (!missingClients.Contains(playerId))
                    {
                        missingClients.Add(playerId);
                        log($"not found player {playerId}");
                    }
                    return;
                }


                var currentTick = leo.GetCurrentTick(world);
                var step = world.GetUnique<TickDeltaComponent>().Value;
                //на сколько тиков мы опередили сервер или отстали
                var delay = time - currentTick.Value;
                /**
                 * delay > 0 - клиент опережает сервер
                 * delay == 0 - клиент идет оптимально с сервером
                 * delay < 0 клиент опоздал и тик на сервере уже прошел
                 */

                //если ввод от клиента не успел прийти вовремя, то выполним его уже в текущем тике
                if (delay < 0)
                    time = currentTick.Value;


                var sentWorldTick = leo.GetCurrentTick(sentWorld) - step;

                if (delay == 0 && sentWorldTick == time)
                    time = currentTick.Value + step.Value;

                client.LastClientTick = inputTime;
                client.LastServerTick = currentTick.Value;

                var component = leo.Pool.GetComponent(type);                

                if (component.GetComponentType() == typeof(PingComponent))//ping
                {                    
                    client.Delay = delay;
                }
                else
                {
                    var cname = component.GetComponentType().Name;
                    cname = cname.Replace("Component", "C.");
                    var end = inputTime < currentTick.Value ? "!!!" : "";
                    log($"got input {cname}:{inputTime} at {currentTick.Value} {end}");
                    
                    var componentData = component.ReadSingleComponent(reader) as IInputComponent;

                    var input = new UserInput
                    {
                        PlayerID = playerId,
                        Component = componentData,
                        Tick = new Tick(time)
                    };

                    inputService.Input(inputWorld, playerId, time, componentData);
                    //leo.Inputs.Add(input);
                    //world.GetUniqueRef<PendingInputComponent>().data = leo.Inputs.ToArray();
                }
            } finally
            {
                reader.Dispose();
            }
        }

        private void Tick()
        {
            //var time0 = leo.GetCurrentTick(world);

            //Console.WriteLine($"{Thread.CurrentThread.ManagedThreadId} Tick Begin {time0.Value}");
            var time = leo.GetCurrentTick(world);
            leo.FilterInputs(time);

            //ref var component = ref world.GetUniqueRef<PendingInputComponent>();

            //Console.WriteLine($"tick {time} at {TimeUtils.GetUnixTimeMS()}");
            //обновляем мир 1 раз
            leo.Tick(systems, inputWorld, world, Config.SyncDataLogging);
        
            time = leo.GetCurrentTick(world);

            var cfg = leo.GetConfig(world);

            if (leo.GetCurrentTick(world).Value % cfg.ServerSyncStep == 0)
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
            if (clients.Count == 0)
                return;

            var dif = WorldDiff.BuildDiff(leo.Pool, sentWorld, world);
            dif.WriteBinary(false, writer);

            var difBinary = Convert.ToBase64String(P2P.Compress(writer.CopyToByteArray()));

            //leo.SyncLog.WriteLine($"send world {leo.GetPrevTick(world)}->{leo.GetCurrentTick(world)} to clients\n");

            //clients list could be modified
            Array.ForEach(clients.ToArray(), client =>
            {
                var packet = new Packet
                {
                    hasWorldUpdate = true,
                    WorldUpdate = new WorldUpdateProto
                    {
                        difBinary = difBinary,
                        delay = client.Delay,
                        LastClientTick = client.LastClientTick,
                        LastServerTick = client.LastServerTick

                    }
                };
                
                SendAsync(packet, client.Address);
                client.Delay = -999;
            });
                                        
            //сохраняем отправленный мир чтоб с ним потом считать diff
            sentWorld = WorldUtils.CopyWorld(leo.Pool, world);
        }

        void SendInitialWorld(EcsWorld prevWorld, Client client)
        {
            var dif = WorldDiff.BuildDiff(leo.Pool, prevWorld, sentWorld);

            var packet = new Packet
            {
                hasWelcomeFromServer = true,
                WorldUpdate = new WorldUpdateProto
                {
                    difStr = dif.ToBase64String(),
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
        }
    }
}