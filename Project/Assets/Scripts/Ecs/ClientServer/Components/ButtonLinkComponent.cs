using System;

namespace Game.Ecs.ClientServer.Components
{
    [Serializable]
    public struct ButtonLinkComponent
    {
        public int[] buttonIds;
    }
}