using Game.ClientServer.Services;
using Game.Ecs.ClientServer.Components;
using Game.Ecs.ClientServer.Components.Inventory;
using XFlow.EcsLite;
using XFlow.Modules.Inventory.ClientServer.Components;
using XFlow.Net.ClientServer.Ecs.Components;
using XFlow.Utils;

namespace Game.Ecs.ClientServer.Systems
{
    public class ManagePlayerSystem : IEcsInitSystem, IEcsRunSystem
    {
        private readonly GameEntityFactory _gameEntityFactory;

        private EcsWorld _mainWorld;
        private EcsFilter _filterNewPlayers;
        private EcsFilter _filterDeletedPlayers;

        public ManagePlayerSystem(GameEntityFactory gameEntityFactory)
        {
            _gameEntityFactory = gameEntityFactory;
        }
        
        public void Init(EcsSystems systems)
        {
            _mainWorld = systems.GetWorld();
            _filterDeletedPlayers = _mainWorld.FilterMarkedDeleted().Inc<PlayerComponent>().End();
        }
        
        public void Run(EcsSystems systems)
        {
            foreach (var playerEntity in _filterDeletedPlayers)
            {
                var packedEntity = playerEntity.EntityGet<PrimaryUnitEntityComponent>(_mainWorld).Value;
                if (packedEntity.Unpack(_mainWorld, out int unitEntity))
                {
                    _mainWorld.MarkEntityAsDeleted(unitEntity);

                    if (unitEntity.EntityTryGet<InventoryLinkComponent>(_mainWorld, out var link))
                    {
                        if (link.Inventory.Unpack(_mainWorld, out int entity))
                        {
                            _mainWorld.MarkEntityAsDeleted(entity);
                        }
                    }
                    
                    if (unitEntity.EntityTryGet<TrashLinkComponent>(_mainWorld, out var trashLink))
                    {
                        if (trashLink.Trash.Unpack(_mainWorld, out int entity))
                        {
                            _mainWorld.MarkEntityAsDeleted(entity);
                        }
                    }
                }
            }
        }

    }
}