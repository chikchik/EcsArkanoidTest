using Game.Ecs.ClientServer.Components.Events;
using Game.Ecs.ClientServer.Components.Inventory;
using Leopotam.EcsLite;

namespace Game.Ecs.ClientServer.Systems
{
    public class RemoveInventoryItemSystem : IEcsRunSystem
    {
        public void Run(EcsSystems systems)
        {
            var world = systems.GetWorld();
            var filter = world
                .Filter<InventoryItemRemovedEventComponent>()
                .End();
            var poolInventoryItemRemovedEvent = world.GetPool<InventoryItemRemovedEventComponent>();
            var poolInventorySlot = world.GetPool<InventorySlotComponent>();
            var poolInventoryItem = world.GetPool<InventoryItemComponent>();

            foreach (var entity in filter)
            {
                ref var inventoryItemRemovedEventComponent = ref poolInventoryItemRemovedEvent.GetRef(entity);
                ref var inventoryItemComponent =
                    ref poolInventoryItem.GetRef(inventoryItemRemovedEventComponent.inventoryItemId);
                ref var inventorySlotComponent = ref poolInventorySlot.GetRef(inventoryItemComponent.slotId);

                inventorySlotComponent.hasItem = false;
                world.DelEntity(inventoryItemRemovedEventComponent.inventoryItemId);
                world.DelEntity(entity);
            }
        }
    }
}