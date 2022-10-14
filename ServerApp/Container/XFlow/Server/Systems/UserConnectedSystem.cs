using XFlow.Ecs.ClientServer;
using XFlow.Ecs.ClientServer.Utils;
using XFlow.EcsLite;
using XFlow.Net.ClientServer;
using XFlow.Server.Components;
using XFlow.Utils;

namespace XFlow.Server.Systems
{
    public class UserConnectedSystem:IEcsRunSystem, IEcsInitSystem
    {
        private EcsWorld _mainWorld;
        private EcsWorld _inputWorld;
        private EcsFilter _filter;
        public void Init(EcsSystems systems)
        {
            _mainWorld = systems.GetWorld();
            _inputWorld = systems.GetWorld(EcsWorlds.Input);
            _filter = _inputWorld.Filter<UserConnectedInputComponent>().Inc<ClientComponent>().End();
        }
        
        public void Run(EcsSystems systems)
        {
            foreach (var entity in _filter)
            {
                var client = entity.EntityGet<ClientComponent>(_inputWorld);
                var playerEntity = PlayerService.CreatePlayerEntity(_mainWorld, client.UserId);
                playerEntity.EntityAdd<ClientComponent>(_mainWorld) = client;
                PlayerService.InputJoinPlayer(_mainWorld, _inputWorld, client.UserId, playerEntity);
                _mainWorld.Log($"created player entity {playerEntity}, id={client.UserId}");
            }
        }
    }
}