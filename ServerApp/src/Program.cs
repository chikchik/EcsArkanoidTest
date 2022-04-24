using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Fabros.Ecs;
using Fabros.EcsModules.Tick.Components;
using Fabros.EcsModules.Tick.Other;
using Fabros.P2P;
using Game.ClientServer;
using Game.Ecs.ClientServer.Components;
using Game.Fabros.Net.ClientServer;
using Game.Fabros.Net.ClientServer.Protocol;
using Leopotam.EcsLite;
using Newtonsoft.Json;

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

        //private List<byte[]> receivedMessages = new List<byte[]>();

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
                    ProcessMessage(msgBytes);
                    //receivedMessages.Add(msgBytes);
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


                var pool = SystemsAndComponents.CreateComponentsPool();

    
                //на сервере может быть tickrate отличный от клиента, например 20 вместо 60
                //это надо учесть при симуляции
                var config = new TickrateConfigComponent { clientTickrate = 30, serverSyncStep = 1, serverTickrate = 30 };

                world = WorldUtils.CreateWorld("main", pool);

                inputWorld = new EcsWorld("input");

                LeoContexts.InitNewWorld(world, config);

                systems = new EcsSystems(world);
                systems.AddWorld(inputWorld, "input");
                SystemsAndComponents.AddSystems(pool, systems, false);

                leo = new LeoContexts(Config.TMP_HASHES_PATH, pool, 
                    new SyncLog(Config.SYNC_LOG_PATH), InputService.ApplyInput);
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
                    //var copy = receivedMessages.ToArray();
                    //receivedMessages.Clear();

                    //for (int i = 0; i < copy.Length; ++i)
                    //    ProcessMessage(copy[i]);

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

            var packet = P2P.ParseResponse<Packet>(msgBytes);

            if (packet.hasHello)
            {
                var client = new Client(packet.playerID);

                Console.WriteLine($"got hello from client {packet.playerID}");
                

                SendAsync(new Packet { hello = new Hello(), hasHello = true }, client.Address);

                var emptyWorld = WorldUtils.CreateWorld("empty", leo.Pool);
                SendInitialWorld(emptyWorld, client);

                clients.Add(client);
                BaseServices.JoinPlayer(inputWorld, packet.playerID);                
            }

            if (packet.input != null && packet.input.time.Value != 0)
            {
                GotInput(packet);
            }
        }

        private void GotInput(Packet packet)
        {
            Client client = clients.FirstOrDefault(client => client.ID == packet.playerID);
            if (client == null)
            {
                if (!missingClients.Contains(packet.playerID)) 
                {
                    missingClients.Add(packet.playerID);
                    Console.WriteLine($"not found player {packet.playerID}");
                }
                return;
            }
            var currentTick = leo.GetCurrentTick(world);// + world.GetUnique<TickDelta>().Value;

            if (!packet.isPing)
            {
                Console.WriteLine($"got input from {client.ID}, {packet.input.time} at {currentTick}");
                leo.SyncLog.WriteLine($"got input from {client.ID}, {packet.input.time} at {currentTick}");
            }


            var step = world.GetUnique<TickDeltaComponent>().Value;            
            //на сколько тиков мы опередили сервер или отстали
            var delay = packet.input.time - currentTick;

            //если ввод от клиента не успел прийти вовремя, то выполним его уже в текущем тике
            if (delay < 0)
            {
                Console.WriteLine($"delay {delay}");
                leo.SyncLog.WriteLine($"input from {client.ID}, {packet.input.time} at {currentTick} too late {delay}");

                packet.input.time = currentTick;
            }

            var sentWorldTick = leo.GetCurrentTick(sentWorld) - step;

            if (delay == 0 && sentWorldTick == packet.input.time) 
            {
                Console.WriteLine($"state already sent");
                leo.SyncLog.WriteLine($"state already sent, {packet.input.time} - {leo.GetCurrentTick(sentWorld)}");
                //мир уже отправлен без инпута, значит мы опоздали и применим на след тик
                packet.input.time = currentTick + world.GetUnique<TickDeltaComponent>().Value;
                delay = -1;
            }

            leo.Inputs.Add(packet.input);
                
            world.GetUniqueRef<PendingInputComponent>().data = leo.Inputs.ToArray();
            //var inputs = component.data.ToList();
            //component.data = inputs.ToArray();

            if (packet.isPing)
                client.Delay = delay;
            else
                client.Delay = -999;
            //Debug.Log($"rtt player={client.playerID} {client.ping}");
        }

        private void Tick()
        {
            var time = leo.GetCurrentTick(world);
            leo.FilterInputs(time);            

            ref var component = ref world.GetUniqueRef<PendingInputComponent>();
         
            //обновляем мир 1 раз
            leo.Tick(systems, inputWorld, world, component.data, Config.SyncDataLogging);
        
            time = leo.GetCurrentTick(world);

            var cfg = leo.GetConfig(world);

            if (leo.GetCurrentTick(world).Value % cfg.serverSyncStep == 0)
            {
                //если у сервера высокий tickrate, например 60
                //то отправлять миру 60 раз в секунду - накладно
                //можно делать это реже, например 20 раз в секунду если serverSyncStep==3
                SendWorldToClients();
                //удаляем ввод игрока который устарел
                var abc = component.data.Where(input => input.time >= time);
                component.data = abc.ToArray();
            }
        }

        void SendWorldToClients()
        {
            var dif = WorldUtils.BuildDiff(leo.Pool, sentWorld, world);
            leo.SyncLog.WriteLine($"send world {leo.GetPrevTick(world)}->{leo.GetCurrentTick(world)} to clients\n");

            //clients list could be modified
            Array.ForEach(clients.ToArray(), client =>
            {
                var packet = new Packet
                {
                    hasWorldUpdate = true,
                    WorldUpdate = new WorldUpdateProto
                    {
                        dif = dif,
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
            var dif = WorldUtils.BuildDiff(leo.Pool, prevWorld, sentWorld);

            var packet = new Packet
            {
                hasWelcomeFromServer = true,
                WorldUpdate = new WorldUpdateProto
                {
                    dif = dif,
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