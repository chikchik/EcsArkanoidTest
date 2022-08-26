using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Contracts.XFlow.Container;
using Contracts.XFlow.Container.Host;
using Fabros.EcsModules.Mech.ClientServer;
using Game.ClientServer;
using Game.ClientServer.Services;
using UnityEngine;
using XFlow.Ecs.ClientServer;
using XFlow.Ecs.ClientServer.Utils;
using XFlow.Ecs.ClientServer.WorldDiff;
using XFlow.EcsLite;
using XFlow.Modules.Box2D.ClientServer.Systems;
using XFlow.Modules.Tick.ClientServer.Components;
using XFlow.Modules.Tick.ClientServer.Systems;
using XFlow.Modules.Tick.Other;
using XFlow.Net.ClientServer;
using XFlow.Net.ClientServer.Ecs.Components;
using XFlow.Net.ClientServer.Protocol;
using XFlow.P2P;
using XFlow.Utils;
using Zenject;
using Random = UnityEngine.Random;

namespace XFlow.Server
{
    class Client
    {
        public int ID;
        public int Delay;
        public int LastClientTick;
        public int LastServerTick;
        public ClientAddr Address;
        public EndPoint EndPoint;

        public EcsWorld SentWorld;
        public EcsWorld SentWorldRelaible;
        
        public Client(int id)
        {
            this.ID = id;
            Address = new ClientAddr(id.ToString());
        }
    }

    public class Container: IContainer
    {
        private SyncDebugService syncDebug;
        private ClientWebSocket socket;

        private EcsWorld world;

        private EcsWorld sentWorld_;
        private EcsWorld inputWorld;
        private EcsWorld eventWorld;


        private EcsSystems systems;
        private List<Client> clients = new List<Client>();
        private List<int> missingClients = new List<int>();
        private ApplyInputWorldService inputService = new ApplyInputWorldService();
        private EntityDestroyedListener destroyedListener = new EntityDestroyedListener();

        private List<byte[]> receivedMessages = new List<byte[]>();
        private HGlobalWriter writer = new HGlobalWriter();
        private IEcsSystemsFactory systemsFactory;

        private bool worldInitialized;

        private ComponentsCollection components;

        private TickrateConfigComponent config = new TickrateConfigComponent { Tickrate = 30, ServerSyncStep = 1 };

        private Task runTask;
        private CancellationTokenSource runCancellationTokenSource;
        
        private IContainerConfig containerConfig;
        private string url;

        private UDPServer udpServer = new UDPServer();

        public Container(IContainerConfig containerConfig, ILogger logger)
        {
            Debug.SetLogDelegate(logger.Log);
            this.containerConfig = containerConfig;
            
            url =  $"{P2P.P2P.DEV_SERVER_WS}/{containerConfig.GetValue(ContainerConfigParams.ROOM)}/{P2P.P2P.ADDR_SERVER.AddressString}";
            
            components = new ComponentsCollection();
            ComponentsCollectionUtils.AddComponentsFromAssembly(components, System.Reflection.Assembly.GetExecutingAssembly());
            
            var dir = Directory.GetCurrentDirectory();
            Debug.Log($"working dir: {dir}");
            socket = new ClientWebSocket();
            
            runCancellationTokenSource = new CancellationTokenSource();
        }

        public void Init()
        {
        }

        public void Start()
        {
            runTask = Task.Run(AsyncRun, runCancellationTokenSource.Token);
            udpServer.Run(runCancellationTokenSource.Token);
        }

        public async  void Stop()
        {
            Debug.Log("Container.Stop");
            try
            {
                udpServer.Dispose();
            }
            catch (Exception e)
            {
                
            }
            
            Debug.Log("Container.Stop1");
            runCancellationTokenSource.Cancel();
            
            try
            {
                await runTask;
            }
            catch (OperationCanceledException e)
            {
                Console.WriteLine($"{nameof(OperationCanceledException)} thrown with message: {e.Message}");
            }
            finally
            {
                Debug.Log("Container.Stop2");
                runCancellationTokenSource.Dispose();
            }
        }

        public string GetInfo()
        {
            var sb = new StringBuilder(512);
            sb.AppendLine($"url: {url}");
            sb.AppendLine($"tick: {world.GetTick()}");
            sb.AppendLine($"tickrate: {config.Tickrate}");
            sb.AppendLine($"world entities: {world.GetAliveEntitiesCount()}");
            sb.AppendLine($"world size: {world.GetAllocMemorySizeInBytes()/1024} kb");
            
            sb.AppendLine($"clients: {clients.Count}");
            for (int i = 0; i < clients.Count; ++i)
            {
                var client = clients[i];
                sb.AppendLine($"  id: {client.ID}, lastTick: {client.LastClientTick}");
            }
            return sb.ToString();
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
                Debug.LogError(e);
            }
        }
        
        private void StartSystems(byte[] initialWorld)
        {
            Debug.Log("StartSystems");
            WorldDiff dif = null;
            if (initialWorld?.Length > 0)
            {
                Debug.Log($"FromByteArray {initialWorld.Length}");
                dif = WorldDiff.FromByteArray(components, initialWorld);
            }
            else
            {
                dif = WorldDiff.FromJsonString(components, File.ReadAllText("world.ecs.json"));
            }

            
            world.EntityDestroyedListeners.Add(destroyedListener);

 
            inputWorld = new EcsWorld(EcsWorlds.Input);
            systems.AddWorld(inputWorld, EcsWorlds.Input);

            world.AddUnique(config);
            world.AddUnique<TickComponent>().Value = new Tick(0);
            world.AddUnique(new TickDeltaComponent { Value = new TickDelta(config.Tickrate) });
            

            eventWorld = new EcsWorld(EcsWorlds.Event);
            systems.AddWorld(eventWorld, EcsWorlds.Event);


            dif.ApplyChanges(world);

            systems.Init();

            //sentWorld = WorldUtils.CopyWorld(components, world);

            worldInitialized = true;
        }

        protected virtual void CreateSystemsFactory()
        {
            var container = new DiContainer();
            container.Bind<Box2DUpdateSystem.Options>().FromInstance(new Box2DUpdateSystem.Options());
            container.Bind<MechService>().AsSingle();
            container.Bind<MyInventoryService>().AsSingle();
            container.Bind<ComponentsCollection>().FromInstance(components).AsSingle();
            systemsFactory = new EcsSystemsFactory(container);
        }

        async Task AsyncRun()
        {
            try
            {
                CreateSystemsFactory();

                world = new EcsWorld("serv");
                systems = new EcsSystems(world);
                systemsFactory.AddNewSystems(systems,
                    new IEcsSystemsFactory.Settings { AddClientSystems = false, AddServerSystems = true });
                systems.Add(new TickSystem());


                syncDebug = new SyncDebugService(Config.TMP_HASHES_PATH);
                WorldLoggerExt.logger = syncDebug.CreateLogger();

                //var url = $"{Config.URL}/{P2P.ADDR_SERVER.AddressString}";
                
                await socket.ConnectAsync(new Uri(url, UriKind.Absolute), new CancellationToken());

                Debug.Log($"connected to host\n{url}");


                _ = Task.Factory.StartNew(ReceiveData);

                //SendWorldToClients();


                Debug.Log("loop");
                var next = DateTime.UtcNow;
                var step = 1.0 / config.Tickrate;
                while (!runCancellationTokenSource.IsCancellationRequested)
                {
                    byte[][] receivedCopy = null;
                    lock (receivedMessages)
                    {
                        receivedCopy = receivedMessages.ToArray();
                        receivedMessages.Clear();
                    }


                    for (int i = 0; i < receivedCopy.Length; ++i)
                        ProcessMessage(receivedCopy[i]);

                    while (true)
                    {
                        var udpPacketOpt = udpServer.GetPacket();
                        if (udpPacketOpt == null)
                            break;
                        var udpPacket = udpPacketOpt.Value;
                        var reader = new HGlobalReader(udpPacket.Data);
                        var playerId = reader.ReadInt32();
                        var client = GotInput1(reader, playerId);
                        client.EndPoint = udpPacket.EndPoint;
                    }
                    

                    if (next <= DateTime.UtcNow && worldInitialized)
                    {
                        //Console.WriteLine($"tick {leo.GetCurrentTick(world)}");
                        next = next.AddSeconds(step);
                        if (next <= DateTime.UtcNow)
                            next = DateTime.UtcNow.AddSeconds(step);
                        Tick();
                    }

                    Thread.Sleep(10);
                }

                Debug.Log("Ended0");
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
            finally
            {
                Debug.Log("Ended1");   
            }
        }


        private void ProcessMessage(byte[] msgBytes)
        {
            string errAddr = "";
            if (P2P.P2P.CheckError(msgBytes, out errAddr))
            {
                var client = clients.FirstOrDefault(client => client.ID.ToString() == errAddr);
                if (client != null)
                {
                    clients.Remove(client);
                    Debug.Log($"removed client {client.ID}");
                    BaseServices.LeavePlayer(inputWorld, client.ID);
                }
                return;
            }

            if (msgBytes[0] == 0xff && msgBytes[1] == 0 && msgBytes[2] == 0 && msgBytes[3] == 0)
            {
                var reader = new HGlobalReader(msgBytes);
                reader.ReadInt32();//0xff
                var playerId = reader.ReadInt32();
                GotInput1(reader, playerId);
                return;
            }

            var packet = P2P.P2P.ParseResponse<Packet>(msgBytes);

            if (packet.hasHello)
            {
                var client = new Client(packet.playerID);

                Debug.Log($"got hello from client {packet.playerID}");

                var hello = new Hello();
                hello.Components = components.Components.Select(component => component.GetComponentType().FullName).ToArray();

                if (!worldInitialized) {
                    //первый игрок присылает игровой стейт на сервер и сервер стартует с ним
                    StartSystems(Convert.FromBase64String(packet.hello.InitialWorld));
                }

                //SendAsyncDeprecated(new Packet { hello = hello, hasHello = true }, client);

                
                client.SentWorld = new EcsWorld("sent");
               // client.SentWorld.CopyFrom(world);
                
                client.SentWorldRelaible = new EcsWorld("rela");
                
                
                
                /*
                var compressed = BuildDiffBytes(client, client.SentWorldRelaible);
                var bytes = P2P.P2P.BuildRequest(client.Address, compressed);
                socket.SendAsync(bytes, WebSocketMessageType.Binary, true, new CancellationToken());
                client.SentWorldRelaible = WorldUtils.CopyWorld(components, world);
                */
                
                var dif = WorldDiff.BuildDiff(components, client.SentWorldRelaible, world);
                client.SentWorldRelaible.CopyFrom(world, components.ContainsCollection);
                client.SentWorld.CopyFrom(world, components.ContainsCollection);
                
                packet = new Packet
                {
                    hasWelcomeFromServer = true,
                    hello =  hello,
                    hasHello = true,
                    WorldUpdate = new WorldUpdateProto
                    {
                        difStr = dif.ToBase64String(),
                        delay = 1
                    }
                };


                var data = P2P.P2P.BuildRequest(client.Address, packet);
                socket.SendAsync(data, WebSocketMessageType.Binary, true, new CancellationToken());
                
                //SendAsyncDeprecated(packet, client);
                Debug.Log($"send initial world at tick {world.GetTick()}");

                

                clients.Add(client);
                BaseServices.JoinPlayer(inputWorld, client.ID);                
            }
        }
        
        private Client GotInput1(HGlobalReader reader, int playerId)
        {
            try            
            {
                Client client = clients.FirstOrDefault(client => client.ID == playerId);
                if (client == null)
                {
                    if (!missingClients.Contains(playerId))
                    {
                        missingClients.Add(playerId);
                        Debug.Log($"not found player {playerId}");
                    }
                    return null;
                }
                
                var inputTime = reader.ReadInt32();
                var time = inputTime;

                var type = reader.ReadInt32();


                var currentTick = world.GetTick();
                var step = world.GetUnique<TickDeltaComponent>().Value;
                //на сколько тиков мы опередили сервер или отстали
                var delay = time - currentTick;
                /**
                 * delay > 0 - клиент опережает сервер
                 * delay == 0 - клиент идет оптимально с сервером
                 * delay < 0 клиент опоздал и тик на сервере уже прошел
                 */

                //если ввод от клиента не успел прийти вовремя, то выполним его уже в текущем тике
                if (delay < 0)
                    time = currentTick;


                var sentWorldTick = client.SentWorld.GetTick() - step.Value;

                if (delay == 0 && sentWorldTick == time)
                    time = currentTick + step.Value;

                client.LastClientTick = inputTime;
                client.LastServerTick = currentTick;

                var component = components.GetComponent(type);                

                if (component.GetComponentType() == typeof(PingComponent))//ping
                {                    
                    client.Delay = delay;
                }
                else
                {
                    //var cname = component.GetComponentType().Name;
                    //cname = cname.Replace("Component", "C.");
                    //var end = inputTime < currentTick ? "!!!" : "";
                    //log($"got input {cname}:{inputTime} at {currentTick.Value} {end}");
                    
                    var componentData = component.ReadSingleComponent(reader) as IInputComponent;

                    inputService.Input(inputWorld, playerId, time, componentData);
                    //leo.Inputs.Add(input);
                    //world.GetUniqueRef<PendingInputComponent>().data = leo.Inputs.ToArray();
                }

                return client;
            } finally
            {
                reader.Dispose();
            }
        }

        private void Tick()
        {
            var time = world.GetTick();
            SyncServices.FilterInputs(inputWorld, time);
            //обновляем мир 1 раз
            SyncServices.Tick(systems, inputWorld, world);
            
            foreach (var client in clients)
            {
                SendWorldToClient(client);
            }
        }

        void BuildDiff(Client client, EcsWorld srcWorld, ref BinaryProtocol.DataWorldDiff data)
        {
            var dif = WorldDiff.BuildDiff(components, srcWorld, world);
            data.DestTick = world.GetTick();
            if (srcWorld.HasUnique<TickComponent>())
                data.SrcTick = srcWorld.GetTick();
            data.Delay = client.Delay;
            data.Diff = dif;
        }

        byte[] BuildDiffBytes(Client client, EcsWorld srcWorld)
        {
            var data = new BinaryProtocol.DataWorldDiff();
            BuildDiff(client, srcWorld, ref data);
            
            writer.Reset();
            BinaryProtocol.WriteWorldDiff(writer, data);
            
            var compressed = P2P.P2P.Compress(writer.CopyToByteArray());
            return compressed;
        }

        async void SimRandomSend(byte[] compressed, Client client)
        {
            await Task.Delay(Random.Range(0, 1000));
            int r = udpServer.Socket.SendTo(compressed, client.EndPoint);
            if (r <= 0)
            {
                Debug.LogError($"udpServer.Socket.SendTo {client.EndPoint} failed");
            }
        } 
        
        void SendWorldToClient(Client client)
        {
            if (client.EndPoint != null)
            {
                var compressed = BuildDiffBytes(client, client.SentWorld);

                //if (Random.Range(0, 1) > 0.8f)//sim lost
                //if (false)
                {
                    //SimRandomSend(compressed, client);
                    
                    int r = udpServer.Socket.SendTo(compressed, client.EndPoint);
                    if (r <= 0)
                    {
                        Debug.LogError($"udpServer.Socket.SendTo {client.EndPoint} failed");
                    }
                }

                client.SentWorld.CopyFrom(world, components.ContainsCollection);
                client.Delay = -999;
            }

            //send to websocket server
            //if (false)
            if ((world.GetTick() % 15) == 0)
            {
                var compressed = BuildDiffBytes(client, client.SentWorldRelaible);
                var bytes = P2P.P2P.BuildRequest(client.Address, compressed);
                socket.SendAsync(bytes, WebSocketMessageType.Binary, true, new CancellationToken());
                client.SentWorldRelaible.CopyFrom(world, components.ContainsCollection);
            }
        }
    }
}