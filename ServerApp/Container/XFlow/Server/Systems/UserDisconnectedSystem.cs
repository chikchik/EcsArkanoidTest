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
        public void Init(EcsSystems systems)
        {
            _mainWorld = systems.GetWorld();
            _inputWorld = systems.GetWorld(EcsWorlds.Input);
            _filter = _inputWorld.Filter<UserDisconnectedInputComponent>().End();
        }
        
        public void Run(EcsSystems systems)
        {
            foreach (var entity in _filter)
            {
                var address = entity.EntityGet<UserAddressComponent>(_inputWorld).Address;
                
                if (!PlayerService.TryGetPlayerEntityByPlayerId(_mainWorld, address.UserId, out int playerEntity))
                    continue;
                
                _mainWorld.Log($"leave player {playerEntity}, id={address}");
                PlayerService.InputLeavePlayer(_inputWorld, address.UserId, true);
            }
        }
    }
}