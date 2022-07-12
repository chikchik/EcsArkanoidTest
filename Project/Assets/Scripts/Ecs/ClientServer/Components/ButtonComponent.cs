using System;
using Flow.EcsLite;

namespace Game.Ecs.ClientServer.Components
{
    [Serializable]
    public struct ButtonComponent
    {
        public bool isActivated;
    }
}