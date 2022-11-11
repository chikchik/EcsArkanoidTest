using System;
using XFlow.EcsLite;

namespace Game.Ecs.ClientServer.Components
{
    [Serializable]
    public struct PickupedComponent
    {
        public EcsPackedEntity PickuperEntity;
    }
}