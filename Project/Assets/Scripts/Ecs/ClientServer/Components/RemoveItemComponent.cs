using XFlow.EcsLite;
using XFlow.Net.ClientServer.Ecs.Components;

namespace Game.Ecs.ClientServer.Components
{
    public struct ClearInventoryComponent : IInputComponent
    {
        public EcsPackedEntity Inventory;
    }
}