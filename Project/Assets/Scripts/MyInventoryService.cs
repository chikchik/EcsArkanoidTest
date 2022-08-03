using XFlow.EcsLite;
using XFlow.Modules.Inventory.ClientServer;

namespace Game
{
    public class MyInventoryService : InventoryService
    {
        public override bool IsItemStackable(EcsWorld world, int itemEntity)
        {
            return false;
        }

        protected override int CreateStackableSlotEntity(EcsWorld world, int fromItemEntity, int storageEntity)
        {
            throw new System.NotImplementedException();
        }

        protected override int CreateUniqueSlotEntity(EcsWorld world, int fromItemEntity, int storageEntity)
        {
            throw new System.NotImplementedException();
        }
    }
}