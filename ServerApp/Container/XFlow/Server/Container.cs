using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Fabros.EcsModules.Mech.ClientServer;
using Game.ClientServer;
using Game.ClientServer.Services;
using Gaming.ContainerManager.ImageContracts.V1;
using Gaming.ContainerManager.ImageContracts.V1.Channels;
using Gaming.ContainerManager.Models.V1;
using XFlow.Ecs.ClientServer;
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
using XFlow.Utils;
using Zenject;
using Debug = UnityEngine.Debug;

namespace XFlow.Server
{
    public class Container : IContainer
    {
        private readonly ContainerStartingContext _context;

        private IReliableChannel _reliableChannel;
        private IAsyncDisposable _reliableChannelSubs;

        private IUnreliableChannel _unreliableChannel;
        private IAsyncDisposable _unreliableChannelSubs;

        private ILogger _logger => _context.Host.LoggerFactory.System;
        
        private readonly object _locker = new object();

        private SyncDebugService _syncDebug;

        private EcsWorld _mainWorld;
        private EcsWorld _deadWorld;
        private EcsWorld _inputWorld;
        private EcsWorld _eventWorld;

        private EcsSystems _systems;

        private List<string> _missingClients = new List<string>();
        private ApplyInputWorldService _inputService = new ApplyInputWorldService();
        private EntityDestroyedListener _destroyedListener = new EntityDestroyedListener();
        private CopyToDeadWorldListener _copyToDeadWorldListener;

        private HGlobalWriter _writer = new HGlobalWriter();
        private IEcsSystemsFactory _systemsFactory;

        private bool _worldInitialized;

        private ComponentsCollection _components;

        private TickrateConfigComponent _config = new TickrateConfigComponent { Tickrate = 30, ServerSyncStep = 1 };

        private EcsFilter _clientsFilter;
        private EcsPool<ClientComponent> _poolClients;

        private bool _isRun;
        private CancellationTokenSource _token;  
        
        private DateTime _nextTickAt = DateTime.UtcNow;

        public Container(ContainerStartingContext context)
        {
            try
            {
                _context = context;
                
                Debug.SetLogDelegate(log => { _logger.Log(LogLevel.Information, log); });

                Box2DServices.CheckNative();

                _components = new ComponentsCollection();
                ComponentsCollectionUtils.AddComponentsFromAssembly(_components,
                    System.Reflection.Assembly.GetExecutingAssembly());

                _syncDebug = new SyncDebugService(Config.TMP_HASHES_PATH);
                WorldLoggerExt.logger = _syncDebug.CreateLogger();
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error, e);
                throw;
            }
        }

        public async Task Start()
        {
            try
            {
                _reliableChannel = await _context.Host.ChannelProvider.GetReliableChannelAsync();
                _reliableChannelSubs = await _reliableChannel.SubscribeAsync(OnReliableMessageReceived);

                _unreliableChannel = await _context.Host.ChannelProvider.GetUnreliableChannelAsync();
                _unreliableChannelSubs = await _unreliableChannel.SubscribeAsync(OnUnreliableMessageReceived);

                InitWorld();

                _logger.Log(LogLevel.Information, "Start done");

                _isRun = true;

                _token = new CancellationTokenSource();
                Task.Run(Loop, _token.Token);
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error, e);
                throw;
            }
        }

        public async ValueTask StopAsync()
        {
            _logger.Log(LogLevel.Debug, "Container.Stop");

            _token.Cancel();

            _isRun = false;

            await _reliableChannelSubs.DisposeAsync();
            await _reliableChannel.DisposeAsync();
            await _unreliableChannelSubs.DisposeAsync();
            await _unreliableChannel.DisposeAsync();
        }

        public async ValueTask<string> GetInfoAsync()
        {
            lock (_locker)
            {
                if (!_worldInitialized)
                    return $"World not initialized";
                
                var sb = new StringBuilder(512);
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
        }

        public async ValueTask<ContainerState> GetStateAsync()
        {
            return ContainerState.Empty;
        }

        private async ValueTask OnUnreliableMessageReceived(UnreliableChannelMessage message)
        {
            try
            {
                switch (message.Type)
                {
                    case UnreliableChannelMessageType.MessageReceived:
                        var messageArgs = message.GetMessageReceivedArguments().Value;
                        lock (_locker)
                        {
                            GotInput1(new HGlobalReader(messageArgs.Message.ToArray()), messageArgs.UserAddress);
                        }

                        break;

                    case UnreliableChannelMessageType.ChannelClosed:
                        var closedArgs = message.GetChannelClosedArguments().Value;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            catch (Exception e)
            {            
                _logger.Log(LogLevel.Error, e);
                throw;
            }
        }

        private async ValueTask OnReliableMessageReceived(ReliableChannelMessage message)
        {
            _logger.Log(LogLevel.Trace, $"OnReliableMessageReceived.{message.Type}");
            try
            {
                switch (message.Type)
                {
                    case ReliableChannelMessageType.UserConnected:
                        var connectedArgs = message.GetUserConnectedArguments().Value;
                        _logger.Log(LogLevel.Debug, $"Connected {connectedArgs.UserAddress.UserId}");
                        break;

                    case ReliableChannelMessageType.UserDisconnected:
                        var disconnectedArgs = message.GetUserDisconnectedArguments().Value;
                        _logger.Log(LogLevel.Debug, $"Disconnected {disconnectedArgs.UserAddress.UserId}");
                        lock (_locker)
                        {
                            var user = GetClientEntity(disconnectedArgs.UserAddress.UserId);
                            _mainWorld.MarkEntityAsDeleted(user);
                            PlayerService.InputLeavePlayer(_inputWorld, user);
                        }

                        break;

                    case ReliableChannelMessageType.MessageReceived:
                        var messageArgs = message.GetMessageReceivedArguments().Value;
                        lock (_locker)
                        {
                            ProcessMessage(messageArgs.Message.ToArray(), messageArgs.UserAddress);
                        }

                        break;

                    case ReliableChannelMessageType.ChannelClosed:
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error, e);
                throw;
            }
        }

        private void InitWorld()
        {
            CreateSystemsFactory();

            _deadWorld = new EcsWorld(EcsWorlds.Dead);
            _copyToDeadWorldListener = new CopyToDeadWorldListener(_deadWorld);

            _mainWorld = new EcsWorld("serv");
            _mainWorld.Flags |= EcsWorldFlags.PrimaryMainWorld;
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
            
            _logger.Log(LogLevel.Information, "Init world done");
        }

        private void StartSystems(byte[] initialWorld)
        {
            if (_worldInitialized)
                return;

            _logger.Log(LogLevel.Information, "StartSystems");
            WorldDiff dif = null;
            if (initialWorld?.Length > 0)
            {
                _logger.Log(LogLevel.Debug, $"FromByteArray {initialWorld.Length}");
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
            _mainWorld.AddUnique(new TickDeltaComponent { Value = new TickDelta(_config.Tickrate) });

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

        private void Loop()
        {
            try
            {
                _logger.Log(LogLevel.Debug, "loop");
                var next = DateTime.UtcNow;

                var step = 1.0 / _config.Tickrate;
                while (_isRun)
                {
                    if (next > DateTime.UtcNow || !_worldInitialized)
                        continue;
                    
                    if (_nextTickAt <= DateTime.UtcNow && _worldInitialized)
                    {
                        //Console.WriteLine($"tick {leo.GetCurrentTick(world)}");
                        _nextTickAt = _nextTickAt.AddSeconds(step);
                        if (_nextTickAt <= DateTime.UtcNow)
                            _nextTickAt = DateTime.UtcNow.AddSeconds(step);
                        lock (_locker)
                        {
                            Tick();
                        }
                    }

                    Thread.Sleep(1);
                }

                _logger.Log(LogLevel.Debug, "Ended0");
            }
            catch (OperationCanceledException e)
            {
                // not error
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error, e);
            }
            finally
            {
                _logger.Log(LogLevel.Debug, "Ended1");
            }
        }

        private void ProcessMessage(byte[] msgBytes, IUserAddress userAddress)
        {
            _logger.Log(LogLevel.Debug,
                $"ProcessMessage id={userAddress.UserId} hash={P2P.P2P.GetMessageHash(msgBytes)} size={msgBytes.Length}");

            if (P2P.P2P.CheckError(msgBytes))
            {
                var clientEntity = GetClientEntity(userAddress.UserId);
                if (clientEntity != -1)
                {
                    var client = _poolClients.Get(clientEntity);
                    _mainWorld.MarkEntityAsDeleted(clientEntity);
                    _logger.Log(LogLevel.Warning, $"removed client {userAddress}");
                    PlayerService.InputLeavePlayer(_inputWorld, client.ID);
                }

                return;
            }

            if (msgBytes[0] == 0xff && msgBytes[1] == 0 && msgBytes[2] == 0 && msgBytes[3] == 0)
            {
                _logger.Log(LogLevel.Debug, $"receive input");
                GotInput1(new HGlobalReader(msgBytes), userAddress);
                return;
            }

            var packet = P2P.P2P.ParseResponse<Packet>(msgBytes);

            if (packet.hasHello)
            {
                var client = new ClientComponent();
                client.ID = packet.playerID;
                client.UserAddressId = userAddress.UserId;
                client.ReliableAddress = userAddress;

                _logger.Log(LogLevel.Information, $"got hello from client {packet.playerID}");

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


                client.SentWorld = new EcsWorld("sent");
                // client.SentWorld.CopyFrom(world);

                client.SentWorldRelaible = new EcsWorld("rela");

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


                var data = P2P.P2P.BuildRequest(packet);
                _logger.Log(LogLevel.Debug, $"Send hello, {data.Length}");
                _reliableChannel.SendAsync(userAddress, data);

                _logger.Log(LogLevel.Information, $"send initial world at tick {_mainWorld.GetTick()}");

                var entity = _mainWorld.NewEntity();
                _poolClients.Add(entity) = client;

                var inputEntity = PlayerService.InputJoinPlayer(_inputWorld, client.ID);
                inputEntity.EntityAdd<ClientComponent>(_inputWorld) = client;
            }
        }

        private int GetClientEntity(string userAddressId)
        {
            foreach (var entity in _clientsFilter)
            {
                var component = _poolClients.Get(entity);
                if (component.UserAddressId == userAddressId)
                    return entity;
            }

            return -1;
        }

        private void GotInput1(HGlobalReader reader, IUserAddress address)
        {
            try
            {
                var clientEntity = GetClientEntity(address.UserId);
                if (clientEntity == -1)
                {
                    if (!_missingClients.Contains(address.UserId))
                    {
                        _missingClients.Add(address.UserId);
                        _logger.Log(LogLevel.Information, $"not found player {address}");
                    }

                    return;
                }
                
                ref var client = ref _poolClients.GetRef(clientEntity);

                client.UnreliableAddress ??= address;
                
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
                    var ms = _nextTickAt - DateTime.UtcNow;
                    client.DelayMs = ms.Milliseconds;
                    Debug.Log(client.DelayMs);
                }
                else
                {
                    //var cname = component.GetComponentType().Name;
                    //cname = cname.Replace("Component", "C.");
                    //var end = inputTime < currentTick ? "!!!" : "";
                    //log($"got input {cname}:{inputTime} at {currentTick.Value} {end}");

                    var componentData = component.ReadSingleComponent(reader) as IInputComponent;

                    _inputService.Input(_inputWorld, client.ID, time, componentData);
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
            if (_mainWorld.GetTick() % 50 == 0)
            {
                _logger.Log(LogLevel.Information, $"tick {_mainWorld.GetTick()}");
            }
            
            var time = _mainWorld.GetTick();
            SyncServices.FilterInputs(_inputWorld, time);
            //обновляем мир 1 раз
            SyncServices.Tick(_systems, _inputWorld, _mainWorld);

            //UDP send to clients
            foreach (var clientEntity in _clientsFilter)
            {
                ref var client = ref _poolClients.GetRef(clientEntity);
                if (!string.IsNullOrEmpty(client.UserAddressId) && client.UnreliableAddress != null)
                {
                    var compressed = BuildDiffBytes(client, client.SentWorld);

                    //if (Random.Range(0, 1) > 0.8f)//sim lost
                    //if (false)
                    {
                        //SimRandomSend(compressed, client);

                        _logger.Log(LogLevel.Debug,$"Send udp diff l={compressed.Length}, h={P2P.P2P.GetMessageHash(compressed)}");
                        _unreliableChannel.SendAsync(client.UnreliableAddress, compressed);
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
                    var bytes = P2P.P2P.BuildRequest(compressed);
                    _logger.Log(LogLevel.Debug,$"Send tcp diff l={bytes.Length}, h={P2P.P2P.GetMessageHash(bytes)}");
                    _reliableChannel.SendAsync(client.ReliableAddress, bytes);
                    client.SentWorldRelaible.CopyFrom(_mainWorld, _components.ContainsCollection);
                }

                var filter = _mainWorld.Filter<DeletedEntityComponent>(false).End();
                foreach (var entity in filter)
                {
                    _mainWorld.DelEntity(entity);
                }
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
            data.TickDelayMs = client.DelayMs;
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
    }
}