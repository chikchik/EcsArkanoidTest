using System;
using Fabros.Ecs.ClientServer.WorldDiff;

namespace Game.Ecs.ClientServer.Components
{
    [DontSerialize]
    public struct LerpComponent
    {
        public float value;
    }
}