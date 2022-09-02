using System;
using UnityEngine;

namespace Game.Ecs.ClientServer.Components
{
    [Serializable]
    public struct MoveInfoComponent
    {
        public Vector3 startPoint;
        public Vector3 endPoint;
    }
}