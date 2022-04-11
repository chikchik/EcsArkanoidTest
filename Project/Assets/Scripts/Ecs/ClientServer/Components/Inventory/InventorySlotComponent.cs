using System;

namespace Game.Ecs.ClientServer.Components.Inventory
{
    [Serializable]
    public struct InventorySlotComponent
    {
        public int slotId;
        public bool hasItem;
    }
}