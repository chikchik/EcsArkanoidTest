using System;
using Fabros.Ecs.ClientServer.WorldDiff;
using UnityEngine;

namespace Game.Ecs.ClientServer.Components
{
    [DontSerialize]
    public struct LookDirectionComponent
    {
        public Vector3 value;
    }
}