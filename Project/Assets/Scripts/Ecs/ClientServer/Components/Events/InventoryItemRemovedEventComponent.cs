using System;

namespace Game.Ecs.ClientServer.Components.Events
{
    [Serializable]
    public struct InventoryItemRemovedEventComponent
    {
        public int inventoryItemId;
    }
}