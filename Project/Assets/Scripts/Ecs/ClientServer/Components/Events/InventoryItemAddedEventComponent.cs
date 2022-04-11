using System;

namespace Game.Ecs.ClientServer.Components.Events
{
    [Serializable]
    public struct InventoryItemAddedEventComponent
    {
        public string itemName;
        public int count;
    }
}