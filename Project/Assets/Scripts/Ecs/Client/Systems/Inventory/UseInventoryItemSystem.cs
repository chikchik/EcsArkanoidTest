using Fabros.Ecs.Utils;
using Game.Ecs.ClientServer.Components.Events;
using Game.Ecs.ClientServer.Components.Inventory;
using Leopotam.EcsLite;
using UnityEngine.UI;

namespace Game.Ecs.Client.Systems.Inventory
{
    public class UseInventoryItemSystem : IEcsInitSystem, IEcsDestroySystem
    {
        private Button useItemButton;
        private EcsWorld world;

        public void Destroy(EcsSystems systems)
        {
            useItemButton.onClick.RemoveListener(OnUseButtonClick);
        }

        public void Init(EcsSystems systems)
        {
            /*
            world = systems.GetWorld();
            useItemButton = world.GetUnique<LeoSharedComponent>().ui.UseButton;
            useItemButton.onClick.AddListener(OnUseButtonClick);
            */
        }

        private void OnUseButtonClick()
        {
            var filter = world
                .Filter<InventoryItemComponent>()
                .End();

            foreach (var entity in filter)
            {
                var inventoryItemRemovedEventEntity = world.NewEntity();
                ref var inventoryItemRemovedEventComponent =
                    ref inventoryItemRemovedEventEntity.EntityAddComponent<InventoryItemRemovedEventComponent>(world);
                inventoryItemRemovedEventComponent.inventoryItemId = entity;

                break;
            }
        }
    }
}