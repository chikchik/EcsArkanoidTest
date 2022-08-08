using System.Collections.Generic;
using UnityEngine;
using XFlow.EcsLite;
using XFlow.Modules.Inventory.Client.Interfaces;
using XFlow.Modules.Inventory.ClientServer;
using XFlow.Modules.Inventory.ClientServer.Components;
using XFlow.Utils;

namespace Game.ClientServer.Services
{
    public class MyInventoryService : InventoryService
    {
        private readonly IInventoryStaticData staticData;

        public MyInventoryService(IInventoryStaticData staticData)
        {
            this.staticData = staticData;
            staticData.Init();
        }
        
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
                itemEntity.EntityAdd<EntityRemoveEventComponent>(world);
            }
        }

        public override bool IsEnoughSpaceForItems(EcsWorld world, int inventoryEntity, string itemId, int amount,
            bool isStackable)
        {
            int slotCapacity = isStackable 
                ? inventoryEntity.EntityGet<InventoryComponent>(world).SlotCapacity
                : 1;

            string categoryId = staticData.GetInventoryItemInfo(itemId).CategoryId;

            HasItems(world, inventoryEntity, out List<int> filledSlots, Predicate);
            HasItems(world, inventoryEntity, itemId, out List<int> slotsWithItem, Predicate);

            int filledSlotsCount = filledSlots.Count;
            int filledSlotsWithItemCount = slotsWithItem.Count;
            int filledSlotsByOtherItemsCount = filledSlotsCount - filledSlotsWithItemCount;
            
            int amountItems = GetAmount(world, inventoryEntity, itemId);
            int possibleAmount = amountItems + amount;
            int possibleSlots = Mathf.CeilToInt((float)possibleAmount / (float)slotCapacity);

            return possibleSlots + filledSlotsByOtherItemsCount <= staticData.MaxSlotsInCategory;

            bool Predicate(int slotEntity)
            {
                string slotItemId = slotEntity.EntityGet<ItemIdComponent>(world).ItemId;
                string slotCategoryId = staticData.GetInventoryItemInfo(slotItemId).CategoryId;

                return slotCategoryId == categoryId;
            }
        }
    }
}