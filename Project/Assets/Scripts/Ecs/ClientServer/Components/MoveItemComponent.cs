using System;
using XFlow.EcsLite;
using XFlow.Net.ClientServer.Ecs.Components;

namespace Game.Ecs.ClientServer.Components
{
    [Serializable]
    public struct MoveItemComponent : IInputComponent
    {
        public EcsPackedEntity Inventory;
        public EcsPackedEntity Item;
        public int Amount;
    }
}