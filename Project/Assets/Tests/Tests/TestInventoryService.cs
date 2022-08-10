using System.Collections.Generic;
using UnityEngine;
using XFlow.EcsLite;
using XFlow.Modules.Inventory.ClientServer;
using XFlow.Modules.Inventory.ClientServer.Components;
using XFlow.Utils;

public class TestInventoryService : InventoryService
{
    private int inventoryCapacity = 100;
    
    public TestInventoryService() { }
    
    public TestInventoryService(int inventoryCapacity)
    {
        this.inventoryCapacity = inventoryCapacity;
    }
    
    protected override int CreateStackableSlotEntity(EcsWorld world, int fromItemEntity, int storageEntity)
    {
        return world.NewEntity();
    }

    protected override int CreateUniqueSlotEntity(EcsWorld world, int fromItemEntity, int storageEntity)
    {
        return world.NewEntity();
    }

    public override bool IsEnoughSpaceForItems(EcsWorld world, int inventoryEntity, string itemId, int amount, bool isStackable)
    {
        var allItems = GetAllItems(world, inventoryEntity);
        HasItems(world, inventoryEntity, itemId, out List<int> slotsWithItem);
        
        int slotCapacity = isStackable 
            ? inventoryEntity.EntityGet<InventoryComponent>(world).SlotCapacity
            : 1;
        
        int filledSlotsCount = allItems.Count;
        int filledSlotsWithItemCount = slotsWithItem.Count;
        int filledSlotsByOtherItemsCount = filledSlotsCount - filledSlotsWithItemCount;
        
        int amountItems = GetAmount(world, inventoryEntity, itemId);
        int possibleAmount = amountItems + amount;
        int possibleSlots = Mathf.CeilToInt((float)possibleAmount / (float)slotCapacity);
        
        return possibleSlots + filledSlotsByOtherItemsCount <= inventoryCapacity;
    }

    public override bool IsItemStackable(EcsWorld world, int itemEntity)
    {
        if (!itemEntity.EntityHas<ItemIdComponent>(world))
            return false;

        var itemData = ItemStaticData.GetItemData(itemEntity.EntityGet<ItemIdComponent>(world).ItemId);

        return itemData.isStackable;
    }
}