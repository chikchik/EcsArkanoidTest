using System;
using XFlow.EcsLite;

namespace Game.Ecs.ClientServer.Components
{
    [Serializable]
    public struct UnitComponent
    {
        public EcsPackedEntity PlayerEntity;
    }
}