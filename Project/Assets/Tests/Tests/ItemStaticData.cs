using System.Collections.Generic;

public static class ItemStaticData
{
    public class ItemData
    {
        public string itemId;
        public bool isStackable;
    }

    private static readonly List<ItemData> items = new()
    {
        new() {itemId = ItemConfig.StackableItem1, isStackable = true},
        new() {itemId = ItemConfig.StackableItem2, isStackable = true},
        new() {itemId = ItemConfig.UniqueItem1, isStackable = false},
        new() {itemId = ItemConfig.UniqueItem2, isStackable = false},
    };

    public static ItemData GetItemData(string itemId)
    {
        foreach (var itemData in items)
        {
            if (itemData.itemId == itemId)
                return itemData;
        }

        throw new KeyNotFoundException($"Not found item id [{itemId}] in data");
    }
}