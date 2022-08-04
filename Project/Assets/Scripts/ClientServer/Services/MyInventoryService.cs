using XFlow.EcsLite;
using XFlow.Modules.Inventory.ClientServer;
using XFlow.Modules.Inventory.ClientServer.Components;
using XFlow.Modules.Inventory.Demo.Components;
using XFlow.Utils;

namespace Game.ClientServer.Services
{
    public class MyInventoryService : InventoryService
    {
        public override bool IsItemStackable(EcsWorld world, int itemEntity)
        {
            var poolStackable = world.GetPool<StackableComponent>();
            return poolStackable.Has(itemEntity);
        }

        protected override int CreateStackableSlotEntity(EcsWorld world, int fromItemEntity, int storageEntity)
        {
            var itemEntity = world.NewEntity();
            itemEntity.EntityAddComponent<StackableComponent>(world);
            return itemEntity;
        }

        protected override int CreateUniqueSlotEntity(EcsWorld world, int fromItemEntity, int storageEntity)
        {
            var itemEntity = world.NewEntity();
            return itemEntity;
        }

        protected override void OnAddCompleted(EcsWorld world, int inventoryEntity, int itemEntity, int amount)
        {
            base.OnAddCompleted(world, inventoryEntity, itemEntity, amount);
            if (GetEntityAmount(world, itemEntity) == 0)
            {
                itemEntity.EntityAdd<DeathEventComponent>(world);
            }
        }
    }
}