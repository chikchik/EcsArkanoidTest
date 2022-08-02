using System;
using XFlow.Ecs.ClientServer.WorldDiff.Attributes;

namespace Game.Ecs.ClientServer.Components
{
    [ForceJsonSerialize]
    [Serializable]
    public struct ButtonLinkComponent
    {
        public int[] Entities;
    }
}