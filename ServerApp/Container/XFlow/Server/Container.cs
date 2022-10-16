using System;
using System.Collections.Generic;
using System.IO;
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
using XFlow.Net.ClientServer.Internal;
using XFlow.Net.ClientServer.Protocol;
using XFlow.Net.ClientServer.Services;
using XFlow.Server.Services;
using XFlow.Server.Systems;
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
        private readonly EcsWorld _inputWorld;
        private EcsWorld _eventWorld;

        private EcsSystems _systems;

        private UserService _userService;
        private EntityDestroyedListener _destroyedListener = new EntityDestroyedListener();
        private CopyToDeadWorldListener _copyToDeadWorldListener;
        

        private IEcsSystemsFactory _systemsFactory;

        private bool _worldInitialized;

        private ComponentsCollection _components;

        private TickrateConfigComponent _config = new TickrateConfigComponent { Tickrate = 30, ServerSyncStep = 1 };

        private EcsFilter _clientsFilter;

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
                
                
                _inputWorld = new EcsWorld(EcsWorlds.Input);
                _userService = new UserService(_inputWorld, _components);
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
                
                CreateSystems();
                
                _logger.Log(LogLevel.Information, "Start done");

                _isRun = true;

                _token = new CancellationTokenSource();
                _ = Task.Run(Loop, _token.Token);
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
                sb.AppendLine($"tick rate: {_config.Tickrate}");
                sb.AppendLine($"world entities: {_mainWorld.GetAliveEntitiesCount()}");
                sb.AppendLine($"world size: {_mainWorld.GetAllocMemorySizeInBytes() / 1024} kb");

                sb.AppendLine($"clients: {_clientsFilter.GetEntitiesCount()}");
                foreach (var entity in _clientsFilter)
                {
                    var client = entity.EntityGet<ClientComponent>(_mainWorld);
                    sb.AppendLine($"  id: {client.UserId}, lastTick: {client.LastClientTick}");
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
            switch (message.Type)
            {
                case UnreliableChannelMessageType.MessageReceived:
                    var messageArgs = message.GetMessageReceivedArguments()!.Value;
                    lock (_locker)
                    {
                        _userService.InputUserMessage(messageArgs.UserAddress, messageArgs.Message);
                    }
                    break;

                case UnreliableChannelMessageType.ChannelClosed:
                    var closedArgs = message.GetChannelClosedArguments()!.Value;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        private async ValueTask OnReliableMessageReceived(ReliableChannelMessage message)
        {
            _logger.Log(LogLevel.Trace, $"OnReliableMessageReceived.{message.Type}");
            
            switch (message.Type)
            {
                case ReliableChannelMessageType.UserConnected:
                    var connectedArgs = message.GetUserConnectedArguments()!.Value;
                    _logger.Log(LogLevel.Debug, $"Connected {connectedArgs.UserAddress.UserId}");
                    break;

                case ReliableChannelMessageType.UserDisconnected:
                    var disconnectedArgs = message.GetUserDisconnectedArguments()!.Value;
                    var userId = disconnectedArgs.UserAddress.UserId;
                    _logger.Log(LogLevel.Debug, $"Disconnected {userId}");
                    lock (_locker)
                    {
                        _userService.InputUserDisconnected(disconnectedArgs.UserAddress);
                    }
                    break;

                case ReliableChannelMessageType.MessageReceived:
                    var messageArgs = message.GetMessageReceivedArguments()!.Value;
                    lock (_locker)
                    {
                        ProcessMessage(messageArgs.Message, messageArgs.UserAddress);
                    }

                    break;

                case ReliableChannelMessageType.ChannelClosed:
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void CreateSystems()
        {
            CreateSystemsFactory();

            _deadWorld = new EcsWorld(EcsWorlds.Dead);
            _copyToDeadWorldListener = new CopyToDeadWorldListener(_deadWorld);

            _mainWorld = new EcsWorld("serv");
            _mainWorld.Flags |= EcsWorldFlags.PrimaryMainWorld;
            _mainWorld.SetDefaultGen(InternalConfig.ServerWorldGenMin, InternalConfig.ServerWorldGenMax);
            
            //todo, надо избавиться от этого вызова
            _userService.SetMainWorld(_mainWorld);
            
            _systems = new EcsSystems(_mainWorld);
            _systems.Add(_systemsFactory.CreateSyncDebugSystem(true));
            _systems.Add(new UserConnectedSystem());
            _systems.Add(new UserDisconnectedSystem());
            
            _systemsFactory.AddNewSystems(_systems,
                new IEcsSystemsFactory.Settings { AddServerSystems = true });
            _systems.Add(new TickSystem());
            _systems.Add(_systemsFactory.CreateSyncDebugSystem(false));
            _systems.Add(new DeleteDeadWorldEntitiesSystem());

            _systems.Add(new SendDiffToClientsSystem(_components, _unreliableChannel, _reliableChannel));

            _clientsFilter = _mainWorld.Filter<ClientComponent>().End();
            
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

        private async void Loop()
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

                    await Task.Delay(5);
                    //await Task.Yield();
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

        private void ProcessMessage(ReadOnlyMemory<byte> data, IUserAddress userAddress)
        {
            _logger.Log(LogLevel.Debug, $"ProcessMessage id={userAddress.UserId}");

            var msgBytes = data.ToArray();
            if (msgBytes[0] == 0xff && msgBytes[1] == 0 && msgBytes[2] == 0 && msgBytes[3] == 0)
            {
                _logger.Log(LogLevel.Debug, $"receive input");
                _userService.InputUserMessage(userAddress, msgBytes);
                return;
            }

            var hello = new BinaryProtocol.DataClientHello();
            var reader = HGlobalReader.Create(data);
            BinaryProtocol.Read(reader, ref hello);
            
            _logger.Log(LogLevel.Information, $"got hello from client {userAddress.UserId}");
            
            if (!_worldInitialized)
            {
                //первый игрок присылает игровой стейт на сервер и сервер стартует с ним
                if (hello.WorldState != null)
                    StartSystems(hello.WorldState);
            }
            
            _userService.InputUserConnected(userAddress);
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
        }
    }
}