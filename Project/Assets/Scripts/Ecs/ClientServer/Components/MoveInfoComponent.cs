using System;
using UnityEngine;

namespace Game.Ecs.ClientServer.Components
{
    [Serializable]
    public struct MoveInfoComponent
    {
        public Vector3 StartPosition;
        public Vector3 EndPosition;
    }
}