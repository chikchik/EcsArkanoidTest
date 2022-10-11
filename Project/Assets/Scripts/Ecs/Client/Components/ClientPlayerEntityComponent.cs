using System;
using XFlow.EcsLite;

namespace Game.Ecs.Client.Components
{
    [Serializable]
    public struct ClientPlayerEntityComponent
    {
        public EcsPackedEntity Value;
    }
}