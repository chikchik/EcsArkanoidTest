using System.Linq;
using Gaming.ContainerManager.ImageContracts.V1;
using Gaming.ContainerManager.ImageContracts.V1.Channels;
using XFlow.Ecs.ClientServer;
using XFlow.Ecs.ClientServer.Utils;
using XFlow.Ecs.ClientServer.WorldDiff;
using XFlow.EcsLite;
using XFlow.Modules.Tick.ClientServer.Components;
using XFlow.Modules.Tick.Other;
using XFlow.Net.ClientServer;
using XFlow.Net.ClientServer.Protocol;
using XFlow.Server.Components;
using XFlow.Utils;

namespace XFlow.Server.Systems
{
    public class SendDiffToClientsSystem:IEcsRunSystem, IEcsInitSystem
    {
        private EcsWorld _mainWorld;
        private EcsFilter _clientsFilter;
        private EcsPool<ClientComponent> _poolClients;

        private readonly ComponentsCollection _components;
        private readonly IUnreliableChannel _unreliableChannel;
        private readonly IReliableChannel _reliableChannel;
        
        private HGlobalWriter _writer = new HGlobalWriter();

        public SendDiffToClientsSystem(ComponentsCollection componentsCollection, 
            IUnreliableChannel unreliableChannel, IReliableChannel reliableChannel)
        {
            _components = componentsCollection;
            _unreliableChannel = unreliableChannel;
            _reliableChannel = reliableChannel;
        }
        
        public void Init(EcsSystems systems)
        {
            _mainWorld = systems.GetWorld();
            _clientsFilter = _mainWorld.Filter<ClientComponent>().End();
            _poolClients = _mainWorld.GetPool<ClientComponent>();
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
            BinaryProtocol.Write(_writer, data);

            var compressed = P2P.P2P.Compress(_writer.ToByteArray());
            return compressed;
        }
        
        public void Run(EcsSystems systems)
        {
            //UDP send to clients
            foreach (var clientEntity in _clientsFilter)
            {
                ref var client = ref _poolClients.GetRef(clientEntity);
                
                if (client.SentWorld == null)
                {
                    //если клиент еще совсем ничего не получал
                    
                    
                    client.SentWorld = new EcsWorld("sent");
                    client.SentWorldReliable = new EcsWorld("rela");

                    var dif = WorldDiff.BuildDiff(_components, client.SentWorldReliable, _mainWorld);
                    client.SentWorldReliable.CopyFrom(_mainWorld, _components.ContainsCollection);
                    client.SentWorld.CopyFrom(_mainWorld, _components.ContainsCollection);

                    
                    var Components = _components.Components.Select(component => component.GetComponentType().FullName)
                        .ToArray();
                    
                    var data = new BinaryProtocol.DataHelloResponse{
                        WorldState = dif.ToByteArray(true),
                        Components = Components
                    };

                    var writer = new HGlobalWriter();
                    BinaryProtocol.Write(writer, data);

                    _reliableChannel.SendAsync(client.ReliableAddress, writer.ToByteArray());
                }
                
                if (client.UnreliableAddress != null)
                {
                    var compressed = BuildDiffBytes(client, client.SentWorld);

                    //if (Random.Range(0, 1) > 0.8f)//sim lost
                    //if (false)
                    {
                        //SimRandomSend(compressed, client);

                        //_logger.Log(LogLevel.Debug,$"Send udp diff l={compressed.Length}, h={P2P.P2P.GetMessageHash(compressed)}");
                        _unreliableChannel.SendAsync(client.UnreliableAddress, compressed);
                    }
                    
                    //он может пропасть из пула если SendAsync по цепочке ошибок привел к удалению из пула внутри
                    if (_poolClients.Has(clientEntity))
                    {
                        client.SentWorld.CopyFrom(_mainWorld, _components.ContainsCollection);
                        client.Delay = -999;
                    }
                }
            }

            //TCP send to clients
            if ((_mainWorld.GetTick() % 15) != 0)
                return;
            
            foreach (var clientEntity in _clientsFilter)
            {
                ref var client = ref _poolClients.GetRef(clientEntity);

                var compressed = BuildDiffBytes(client, client.SentWorldReliable);
                var bytes = P2P.P2P.BuildRequest(compressed);
                //_logger.Log(LogLevel.Debug,$"Send tcp diff l={bytes.Length}, h={P2P.P2P.GetMessageHash(bytes)}");
                _reliableChannel.SendAsync(client.ReliableAddress, bytes);
                //он может пропасть из пула если SendAsync по цепочке ошибок привел к удалению из пула внутри
                if (_poolClients.Has(clientEntity))
                {
                    client.SentWorldReliable.CopyFrom(_mainWorld, _components.ContainsCollection);
                }
            }

            //теперь можно удалять Deleted сущности по настоящему, все клиенты ее получили гарантированно
            var filter = _mainWorld.Filter<DeletedEntityComponent>(false).End();
            foreach (var entity in filter)
            {
                _mainWorld.DelEntity(entity);
            }
        }
    }
}