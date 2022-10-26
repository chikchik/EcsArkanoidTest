using XFlow.Ecs.ClientServer;
using XFlow.Ecs.ClientServer.Utils;
using XFlow.EcsLite;
using XFlow.Net.ClientServer;
using XFlow.Net.ClientServer.Services;
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
            _filter = _inputWorld.Filter<UserConnectedInputComponent>().End();
        }
        
        public void Run(EcsSystems systems)
        {
            foreach (var entity in _filter)
            {
                var userAddress = entity.EntityGet<UserAddressComponent>(_inputWorld).Address;

                int playerEntity = -1;
                if (!PlayerService.TryGetPlayerEntityByPlayerId(_mainWorld, userAddress.UserId , out playerEntity))
                    playerEntity = PlayerService.CreatePlayerEntity(_mainWorld, userAddress.UserId);
                
                
                ref var client = ref playerEntity.EntityAdd<ClientComponent>(_mainWorld);
                client.UserId = userAddress.UserId;
                client.ReliableAddress = userAddress;
                //нужно ли обнулять udp ?
                //client.UnreliableAddress = null;
            }
        }
    }
}