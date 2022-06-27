using System;
using Fabros.Ecs.ClientServer.WorldDiff;

namespace Game.Ecs.ClientServer.Components
{
    [ForceJsonSerialize]
    [Serializable]
    public struct ButtonLinkComponent
    {
        public int[] Entities;
    }
}