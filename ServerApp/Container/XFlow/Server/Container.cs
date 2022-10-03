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
using Gaming.ContainerManager.ImageContracts.V1;
using Gaming.ContainerManager.Models.V1;
using UnityEngine;
using XFlow.Ecs.ClientServer;
using XFlow.Ecs.ClientServer.Components;
using XFlow.Ecs.ClientServer.Utils;
using XFlow.Ecs.ClientServer.WorldDiff;
using XFlow.EcsLite;
using XFlow.Modules.Box2D.ClientServer;
using XFlow.Modules.Box2D.ClientServer.Systems;
using XFlow.Modules.Tick.ClientServer.Components;
using XFlow.Modules.Tick.ClientServer.Systems;
using XFlow.Modules.Tick.Other;
using XFlow.Net.ClientServer;
using XFlow.Net.ClientServer.Ecs.Components;
using XFlow.Net.ClientServer.Ecs.Systems;
using XFlow.Net.ClientServer.Protocol;
using XFlow.P2P;
using XFlow.Utils;
using Zenject;
using Random = UnityEngine.Random;

namespace XFlow.Server
{
    public class Container : IContainer
    {
        private SyncDebugService _syncDebug;
        private ClientWebSocket _socket;

        private EcsWorld _mainWorld;
        private EcsWorld _deadWorld;
        private EcsWorld _inputWorld;
        private EcsWorld _eventWorld;


        private EcsSystems _systems;

        private List<int> _missingClients = new List<int>();
        private ApplyInputWorldService _inputService = new ApplyInputWorldService();
        private EntityDestroyedListener _destroyedListener = new EntityDestroyedListener();
        private CopyToDeadWorldListener _copyToDeadWorldListener;

        private List<byte[]> _receivedMessages = new List<byte[]>();
        private HGlobalWriter _writer = new HGlobalWriter();
        private IEcsSystemsFactory _systemsFactory;

        private bool _worldInitialized;

        private ComponentsCollection _components;

        private TickrateConfigComponent _config = new TickrateConfigComponent {Tickrate = 30, ServerSyncStep = 1};

        private Task _runTask;
        private CancellationTokenSource _runCancellationTokenSource;

        private IContainerConfig _containerConfig;
        private string _url;

        private UDPServer _udpServer = new UDPServer();

        private EcsFilter _clientsFilter;
        private EcsPool<ClientComponent> _poolClients;

        public Container(IContainerConfig containerConfig, ILogger logger)
        {
            Debug.SetLogDelegate(log => logger.Log(LogLevel.Trace, log));
            Debug.Log("created Container");

            Box2DServices.CheckNative();

            this._containerConfig = containerConfig;

            _url =
                $"{P2P.P2P.DEV_SERVER_WS}/{containerConfig.GetValue(ContainerConfigParams.ROOM)}/{P2P.P2P.ADDR_SERVER.AddressString}";

            _components = new ComponentsCollection();
            ComponentsCollectionUtils.AddComponentsFromAssembly(_components,
                System.Reflection.Assembly.GetExecutingAssembly());

            var dir = Directory.GetCurrentDirectory();
            Debug.Log($"working dir: {dir}");
            _socket = new ClientWebSocket();

            _runCancellationTokenSource = new CancellationTokenSource();

            _syncDebug = new SyncDebugService(Config.TMP_HASHES_PATH);
            WorldLoggerExt.logger = _syncDebug.CreateLogger();
        }

        public void Init()
        {
        }

        public void Start()
        {
            _runTask = Task.Run(AsyncRun, _runCancellationTokenSource.Token);
            _udpServer.Run(_runCancellationTokenSource.Token);
        }

        public async ValueTask StopAsync()
        {
            Debug.Log("Container.Stop");
            try
            {
                _udpServer.Dispose();
            }
            catch (Exception e)
            {
            }

            Debug.Log("Container.Stop1");
            _runCancellationTokenSource.Cancel();

            try
            {
                await _runTask;
            }
            catch (OperationCanceledException e)
            {
                Console.WriteLine($"{nameof(OperationCanceledException)} thrown with message: {e.Message}");
            }
            finally
            {
                Debug.Log("Container.Stop2");
                _runCancellationTokenSource.Dispose();
            }
        }


        public async ValueTask<string> GetInfoAsync()
        {
            var sb = new StringBuilder(512);
            sb.AppendLine($"url: {_url}");
            sb.AppendLine($"tick: {_mainWorld.GetTick()}");
            sb.AppendLine($"tickrate: {_config.Tickrate}");
            sb.AppendLine($"world entities: {_mainWorld.GetAliveEntitiesCount()}");
            sb.AppendLine($"world size: {_mainWorld.GetAllocMemorySizeInBytes() / 1024} kb");

            sb.AppendLine($"clients: {_clientsFilter.GetEntitiesCount()}");
            foreach (var entity in _clientsFilter)
            {
                var client = entity.EntityGet<ClientComponent>(_mainWorld);
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
                    WebSocketReceiveResult rcvResult = await _socket.ReceiveAsync(rcvBuffer, ct.Token);

                    byte[] msgBytes = rcvBuffer.Skip(rcvBuffer.Offset).Take(rcvResult.Count).ToArray();
                    //ProcessMessage(msgBytes);
                    lock (_receivedMessages)
                    {
                        _receivedMessages.Add(msgBytes);
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
            if (_worldInitialized)
                return;

            Debug.Log("StartSystems");
            WorldDiff dif = null;
            if (initialWorld?.Length > 0)
            {
                Debug.Log($"FromByteArray {initialWorld.Length}");
                dif = WorldDiff.FromByteArray(_components, initialWorld);
            }
            else
            {
                dif = WorldDiff.FromJsonString(_components, File.ReadAllText("world.ecs.json"));
            }


            _mainWorld.EntityDestroyedListeners.Add(_destroyedListener);
            _mainWorld.EntityDestroyedListeners.Add(_copyToDeadWorldListener);


            _inputWorld = new EcsWorld(EcsWorlds.Input);
            _systems.AddWorld(_inputWorld, EcsWorlds.Input);

            _mainWorld.AddUnique(_config);
            _mainWorld.AddUnique<TickComponent>().Value = new Tick(0);
            _mainWorld.AddUnique(new TickDeltaComponent {Value = new TickDelta(_config.Tickrate)});
            _mainWorld.AddUnique<PrimaryWorldComponent>();

            _systems.AddWorld(_deadWorld, EcsWorlds.Dead);

            _eventWorld = new EcsWorld(EcsWorlds.Event);
            _systems.AddWorld(_eventWorld, EcsWorlds.Event);

            _systems.PreInit();

            dif.ApplyChanges(_mainWorld);

            _systems.Init();

            //sentWorld = WorldUtils.CopyWorld(components, world);

            _worldInitialized = true;
        }

        protected virtual void CreateSystemsFactory()
        {
            var container = new DiContainer();
            container.Bind<Box2DUpdateSystem.Options>().FromInstance(new Box2DUpdateSystem.Options());
            container.Bind<MechService>().AsSingle();
            container.Bind<MyInventoryService>().AsSingle();
            container.Bind<ComponentsCollection>().FromInstance(_components).AsSingle();
            _systemsFactory = new EcsSystemsFactory(container);
        }

        async Task AsyncRun()
        {
            try
            {
                CreateSystemsFactory();

                _deadWorld = new EcsWorld(EcsWorlds.Dead);
                _copyToDeadWorldListener = new CopyToDeadWorldListener(_deadWorld);
                
                
                _mainWorld = new EcsWorld("serv");
                _mainWorld.SetDefaultGen(InternalConfig.ServerWorldGenMin, InternalConfig.ServerWorldGenMax);
                _systems = new EcsSystems(_mainWorld);
                _systems.Add(_systemsFactory.CreateSyncDebugSystem(true));
                _systemsFactory.AddNewSystems(_systems,
                    new IEcsSystemsFactory.Settings { AddServerSystems = true });
                _systems.Add(new TickSystem());
                _systems.Add(_systemsFactory.CreateSyncDebugSystem(false));
                _systems.Add(new DeleteDeadWorldEntitiesSystem());

                _clientsFilter = _mainWorld.Filter<ClientComponent>().End();
                _poolClients = _mainWorld.GetPool<ClientComponent>();

                //var url = $"{Config.URL}/{P2P.ADDR_SERVER.AddressString}";

                await _socket.ConnectAsync(new Uri(_url, UriKind.Absolute), new CancellationToken());

                Debug.Log($"connected to host\n{_url}");


                _ = Task.Factory.StartNew(ReceiveData);

                //SendWorldToClients();


                /*
                 * по умолчанию сервер получает начальный мир от первого подключенного игрока
                 * если включи StartSystems тут, то сцена будет грузиться из файла 
                 */
                //StartSystems(null);


                Debug.Log("loop");
                var next = DateTime.UtcNow;
                var step = 1.0 / _config.Tickrate;
                while (!_runCancellationTokenSource.IsCancellationRequested)
                {
                    byte[][] receivedCopy = null;
                    lock (_receivedMessages)
                    {
                        receivedCopy = _receivedMessages.ToArray();
                        _receivedMessages.Clear();
                    }


                    for (int i = 0; i < receivedCopy.Length; ++i)
                        ProcessMessage(receivedCopy[i]);

                    while (true)
                    {
                        var udpPacketOpt = _udpServer.GetPacket();
                        if (udpPacketOpt == null)
                            break;
                        var udpPacket = udpPacketOpt.Value;
                        var reader = new HGlobalReader(udpPacket.Data);
                        GotInput1(reader, udpPacket.EndPoint);
                    }


                    if (next <= DateTime.UtcNow && _worldInitialized)
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
                var clientEntity =
                    GetClientEntity(
                        Int32.Parse(errAddr)); //  clients.FirstOrDefault(client => client.ID.ToString() == errAddr);
                if (clientEntity != -1)
                {
                    _mainWorld.DelEntity(clientEntity);
                    Debug.Log($"removed client {errAddr}");
                    BaseServices.InputLeavePlayer(_inputWorld, Int32.Parse(errAddr));
                }

                return;
            }

            if (msgBytes[0] == 0xff && msgBytes[1] == 0 && msgBytes[2] == 0 && msgBytes[3] == 0)
            {
                var reader = new HGlobalReader(msgBytes);
                reader.ReadInt32(); //0xff
                GotInput1(reader, null);
                return;
            }

            var packet = P2P.P2P.ParseResponse<Packet>(msgBytes);

            if (packet.hasHello)
            {
                var client = new ClientComponent();
                client.ID = packet.playerID;
                client.Address = new ClientAddr(client.ID.ToString());

                Debug.Log($"got hello from client {packet.playerID}");

                var hello = new Hello();
                hello.Components = _components.Components.Select(component => component.GetComponentType().FullName)
                    .ToArray();

                if (!_worldInitialized)
                {
                    //первый игрок присылает игровой стейт на сервер и сервер стартует с ним
                    var state = packet.hello.InitialWorld;
                    if (!String.IsNullOrEmpty(state))
                        StartSystems(Convert.FromBase64String(state));
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

                var dif = WorldDiff.BuildDiff(_components, client.SentWorldRelaible, _mainWorld);
                client.SentWorldRelaible.CopyFrom(_mainWorld, _components.ContainsCollection);
                client.SentWorld.CopyFrom(_mainWorld, _components.ContainsCollection);

                packet = new Packet
                {
                    hasWelcomeFromServer = true,
                    hello = hello,
                    hasHello = true,
                    WorldUpdate = new WorldUpdateProto
                    {
                        difStr = dif.ToBase64String(),
                        delay = 1
                    }
                };


                var data = P2P.P2P.BuildRequest(client.Address, packet);
                _socket.SendAsync(data, WebSocketMessageType.Binary, true, new CancellationToken());

                //SendAsyncDeprecated(packet, client);
                Debug.Log($"send initial world at tick {_mainWorld.GetTick()}");


                var entity = _mainWorld.NewEntity();
                _poolClients.Add(entity) = client;

                var inputEntity = BaseServices.InputJoinPlayer(_inputWorld, client.ID);
                inputEntity.EntityAdd<ClientComponent>(_inputWorld) = client;
            }
        }

        private int GetClientEntity(int playerID)
        {
            foreach (var entity in _clientsFilter)
            {
                var component = _poolClients.Get(entity);
                if (component.ID == playerID)
                    return entity;
            }

            return -1;
        }

        private void GotInput1(HGlobalReader reader, EndPoint endPoint)
        {
            try
            {
                var playerId = reader.ReadInt32();

                var clientEntity = GetClientEntity(playerId);
                if (clientEntity == -1)
                {
                    if (!_missingClients.Contains(playerId))
                    {
                        _missingClients.Add(playerId);
                        Debug.Log($"not found player {playerId}");
                    }

                    return;
                }

                ref var client = ref _poolClients.GetRef(clientEntity);

                if (endPoint != null)
                    client.EndPoint = endPoint;

                var inputTime = reader.ReadInt32();
                var time = inputTime;

                var type = reader.ReadInt32();


                var currentTick = _mainWorld.GetTick();
                var step = _mainWorld.GetUnique<TickDeltaComponent>().Value;
                //на сколько тиков мы опередили сервер или отстали
                var delay = time - currentTick;

                /*
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

                var component = _components.GetComponent(type);

                if (component.GetComponentType() == typeof(PingComponent)) //ping
                {
                    client.LastPingTick = inputTime;
                    client.Delay = delay;
                }
                else
                {
                    //var cname = component.GetComponentType().Name;
                    //cname = cname.Replace("Component", "C.");
                    //var end = inputTime < currentTick ? "!!!" : "";
                    //log($"got input {cname}:{inputTime} at {currentTick.Value} {end}");

                    var componentData = component.ReadSingleComponent(reader) as IInputComponent;

                    _inputService.Input(_inputWorld, playerId, time, componentData);
                    //leo.Inputs.Add(input);
                    //world.GetUniqueRef<PendingInputComponent>().data = leo.Inputs.ToArray();
                }
            }
            finally
            {
                reader.Dispose();
            }
        }

        private void Tick()
        {
            var time = _mainWorld.GetTick();
            SyncServices.FilterInputs(_inputWorld, time);
            //обновляем мир 1 раз
            SyncServices.Tick(_systems, _inputWorld, _mainWorld);

            //UDP send to clients
            foreach (var clientEntity in _clientsFilter)
            {
                ref var client = ref _poolClients.GetRef(clientEntity);
                if (client.EndPoint != null)
                {
                    var compressed = BuildDiffBytes(client, client.SentWorld);

                    //if (Random.Range(0, 1) > 0.8f)//sim lost
                    //if (false)
                    {
                        //SimRandomSend(compressed, client);
                    
                        int r = _udpServer.Socket.SendTo(compressed, client.EndPoint);
                        if (r <= 0)
                        {
                            Debug.LogError($"udpServer.Socket.SendTo {client.EndPoint} failed");
                        }
                    }

                    client.SentWorld.CopyFrom(_mainWorld, _components.ContainsCollection);
                    client.Delay = -999;
                }
            }
            
            
            //TCP send to clients
            if ((_mainWorld.GetTick() % 15) == 0)
            {
                foreach (var clientEntity in _clientsFilter)
                {
                    ref var client = ref _poolClients.GetRef(clientEntity);
                    
                    var compressed = BuildDiffBytes(client, client.SentWorldRelaible);
                    var bytes = P2P.P2P.BuildRequest(client.Address, compressed);
                    _socket.SendAsync(bytes, WebSocketMessageType.Binary, true, new CancellationToken());
                    client.SentWorldRelaible.CopyFrom(_mainWorld, _components.ContainsCollection);
                }

                var filter = _mainWorld.Filter<DeletedEntityComponent>(false).End();
                foreach (var entity in filter)
                {
                    _mainWorld.DelEntity(entity);
                }
            }

            if (_mainWorld.GetTick() % 50 == 0)
            {
                Debug.Log($"tick {_mainWorld.GetTick()}");
            }
        }

        void BuildDiff(ClientComponent client, EcsWorld srcWorld, ref BinaryProtocol.DataWorldDiff data)
        {
            var dif = WorldDiff.BuildDiff(_components, srcWorld, _mainWorld);
            data.DestTick = _mainWorld.GetTick();
            if (srcWorld.HasUnique<TickComponent>())
                data.SrcTick = srcWorld.GetTick();
            data.Delay = client.Delay;
            data.Diff = dif;
        }

        byte[] BuildDiffBytes(ClientComponent client, EcsWorld srcWorld)
        {
            var data = new BinaryProtocol.DataWorldDiff();
            BuildDiff(client, srcWorld, ref data);

            _writer.Reset();
            BinaryProtocol.WriteWorldDiff(_writer, data);

            var compressed = P2P.P2P.Compress(_writer.ToByteArray());
            return compressed;
        }

        async void SimRandomSend(byte[] compressed, ClientComponent client, int delayMin, int delayMax)
        {
            await Task.Delay(Random.Range(delayMin, delayMax));
            int r = _udpServer.Socket.SendTo(compressed, client.EndPoint);
            if (r <= 0)
            {
                Debug.LogError($"udpServer.Socket.SendTo {client.EndPoint} failed");
            }
        }

        public async ValueTask<ContainerState> GetStateAsync()
        {
            return ContainerState.Empty;
        }
    }
}