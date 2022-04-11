using System;
using UnityEngine;

namespace Game.Ecs.ClientServer.Components
{
    [Serializable]
    public struct TargetPositionComponent
    {
        public Vector3 Value;
        //public bool isReached;
    }
}