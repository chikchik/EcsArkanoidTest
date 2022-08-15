using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

namespace XFlow.Server
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

    public class Container: IContainer
    {
        private SyncDebugService syncDebug;
        private ClientWebSocket socket;

        private EcsWorld world;

        private EcsWorld sentWorld;
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
        private CancellationToken runCancellationToken;
        private CancellationTokenSource runCancellationTokenSource;
        
        private IContainerConfig containerConfig;
        private string url;

        public Container(IContainerConfig containerConfig, ILogger logger)
        {
            Debug.SetLogDelegate(logger.Log);
            this.containerConfig = containerConfig;
            
            url =  $"{P2P.P2P.DEV_SERVER_WS}/{containerConfig.GetValue(ContainerConfigParams.ROOM)}/{P2P.P2P.ADDR_SERVER.AddressString}";
            
            components = new ComponentsCollection();
            ComponentsCollectionUtils.AddComponents(components);
            
            var dir = Directory.GetCurrentDirectory();
            Debug.Log($"working dir: {dir}");
            socket = new ClientWebSocket();
            
            runCancellationTokenSource = new CancellationTokenSource();
            runCancellationToken = runCancellationTokenSource.Token;
        }

        public void Init()
        {
        }

        public void Start()
        {
            
            runTask = Task.Run(AsyncRun, runCancellationToken);
            //runTask = AsyncRun();
        }

        public async  void Stop()
        {
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

            sentWorld = WorldUtils.CopyWorld(components, world);

            worldInitialized = true;
        }

        protected virtual void CreateSystemsFactory()
        {
            var container = new DiContainer();
            container.Bind<Box2DUpdateSystem.Options>().FromInstance(new Box2DUpdateSystem.Options());
            container.Bind<MechService>().AsSingle();
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

                SendWorldToClients();


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
                GotInput(msgBytes);
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
                        Debug.Log($"not found player {playerId}");
                    }
                    return;
                }


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


                var sentWorldTick = sentWorld.GetTick() - step.Value;

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
                    var cname = component.GetComponentType().Name;
                    cname = cname.Replace("Component", "C.");
                    var end = inputTime < currentTick ? "!!!" : "";
                    //log($"got input {cname}:{inputTime} at {currentTick.Value} {end}");
                    
                    var componentData = component.ReadSingleComponent(reader) as IInputComponent;

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
            var time = world.GetTick();

            SyncServices.FilterInputs(inputWorld, time);

            //ref var component = ref world.GetUniqueRef<PendingInputComponent>();

            //Console.WriteLine($"tick {time} at {TimeUtils.GetUnixTimeMS()}");
            //обновляем мир 1 раз
            SyncServices.Tick(systems, inputWorld, world);
        
            //time = world.GetTick();

            //var cfg = leo.GetConfig(world);

            //if (world.GetTick() % cfg.ServerSyncStep == 0)
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

            var dif = WorldDiff.BuildDiff(components, sentWorld, world);
            dif.WriteBinary(false, writer);

            var difBinary = Convert.ToBase64String(P2P.P2P.Compress(writer.CopyToByteArray()));

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
            sentWorld = WorldUtils.CopyWorld(components, world);
        }

        void SendInitialWorld(EcsWorld prevWorld, Client client)
        {
            var dif = WorldDiff.BuildDiff(components, prevWorld, sentWorld);

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
            
            Debug.Log($"initial world send to client {client.ID}");
        }

        private void SendAsync(Packet packet, ClientAddr addr)
        {
            var bytes = P2P.P2P.BuildRequest(addr, packet);            
            socket.SendAsync(bytes, WebSocketMessageType.Text, true, new CancellationToken());            
        }
    }
}