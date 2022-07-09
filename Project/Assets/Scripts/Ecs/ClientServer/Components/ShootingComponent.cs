using System;
using UnityEngine;

namespace Game.Ecs.ClientServer.Components
{
    [Serializable]
    public struct ShootingComponent
    {
        public Vector3 Direction;
        public Vector3 Position;
        public float ShootAtTime;
        public float TotalTime;
        public bool ShootMade;
    }
}