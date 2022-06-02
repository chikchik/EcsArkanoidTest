using System;
using UnityEngine;

namespace Game.Ecs.ClientServer.Components
{
    [Serializable]
    public struct ApplyForceComponent
    {
        public float Time;
        public Vector3 Direction;
    }
}