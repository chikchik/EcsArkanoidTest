using System;
using UnityEngine;

namespace Game.Ecs.ClientServer.Components
{
    [Serializable]
    public struct MoveSimpleDirectionComponent
    {
        public Vector3 Value;
    }
}