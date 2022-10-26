using XFlow.Ecs.ClientServer;
using XFlow.Ecs.ClientServer.Utils;
using XFlow.EcsLite;
using XFlow.Net.ClientServer;
using XFlow.Net.ClientServer.Services;
using XFlow.Server.Components;
using XFlow.Utils;

namespace XFlow.Server.Systems
{
    public class UserDisconnectedSystem:IEcsRunSystem, IEcsInitSystem
    {
        private EcsWorld _mainWorld;
        private EcsWorld _inputWorld;
        private EcsFilter _filter;
        private EcsPool<ClientComponent> _poolClient;
        private bool _deleteDisconnected;

        public UserDisconnectedSystem(bool deleteDisconnected)
        {
            _deleteDisconnected = deleteDisconnected;
        }
        
        public void Init(EcsSystems systems)
        {
            _mainWorld = systems.GetWorld();
            _inputWorld = systems.GetWorld(EcsWorlds.Input);
            _filter = _inputWorld.Filter<UserDisconnectedInputComponent>().End();
            _poolClient = _mainWorld.GetPool<ClientComponent>();
        }
        
        public void Run(EcsSystems systems)
        {
            foreach (var inputEntity in _filter)
            {
                var userId = inputEntity.EntityGet<UserAddressComponent>(_inputWorld).Address.UserId;
                
                if (!PlayerService.TryGetPlayerEntityByPlayerId(_mainWorld, userId, out int playerEntity))
                    continue;
                
                //_mainWorld.Log($"leave player {playerEntity}, id={userId}");
                //PlayerService.InputLeavePlayer(_inputWorld, userId, true);

                _poolClient.Del(playerEntity);
                
                if (_deleteDisconnected)
                    PlayerService.DeletePlayerEntity(_mainWorld, playerEntity);
                /*
                ref var client = ref _poolClient.GetRef(playerEntity);
                client.ReliableAddress = null;
                client.UnreliableAddress = null;
                client.SentWorld = null;
                client.SentWorldReliable = null;*/
            }
        }
    }
}