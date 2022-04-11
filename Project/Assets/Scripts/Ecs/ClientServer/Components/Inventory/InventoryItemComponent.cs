using System;

namespace Game.Ecs.ClientServer.Components.Inventory
{
    [Serializable]
    public struct InventoryItemComponent
    {
        public string itemName;
        public int count;
        public int slotId;
    }
}