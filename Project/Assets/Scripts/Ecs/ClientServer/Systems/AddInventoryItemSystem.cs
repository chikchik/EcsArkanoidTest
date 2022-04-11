using Fabros.Ecs.Utils;
using Game.Ecs.ClientServer.Components.Events;
using Game.Ecs.ClientServer.Components.Inventory;
using Leopotam.EcsLite;

namespace Game.Ecs.ClientServer.Systems
{
    public class AddInventoryItemSystem : IEcsRunSystem
    {
        public void Run(EcsSystems systems)
        {
            var world = systems.GetWorld();
            var filter = world
                .Filter<InventoryItemAddedEventComponent>()
                .End();
            var inventoryItemAddedEventComponents = world.GetPool<InventoryItemAddedEventComponent>();

            foreach (var entity in filter)
            {
                ref var inventoryItemAddedEventComponent = ref inventoryItemAddedEventComponents.GetRef(entity);

                if (TryAddToSlot(world, ref inventoryItemAddedEventComponent)) world.DelEntity(entity);
            }
        }

        private bool TryAddToSlot(EcsWorld world, ref InventoryItemAddedEventComponent inventoryItemAddedEventComponent)
        {
            var filter = world
                .Filter<InventorySlotComponent>()
                .End();
            var poolInventorySlot = world.GetPool<InventorySlotComponent>();

            foreach (var entity in filter)
            {
                ref var inventorySlotComponent = ref poolInventorySlot.GetRef(entity);
                if (inventorySlotComponent.hasItem) continue;

                inventorySlotComponent.hasItem = true;

                var inventoryItemEntity = world.NewEntity();
                ref var inventoryItemComponent =
                    ref inventoryItemEntity.EntityAddComponent<InventoryItemComponent>(world);
                inventoryItemComponent.itemName = inventoryItemAddedEventComponent.itemName;
                inventoryItemComponent.count = inventoryItemAddedEventComponent.count;
                inventoryItemComponent.slotId = inventorySlotComponent.slotId;

                return true;
            }

            return false;
        }
    }
}