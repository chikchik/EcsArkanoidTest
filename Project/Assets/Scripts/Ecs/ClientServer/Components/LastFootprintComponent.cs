using System;
using UnityEngine;

namespace Game.Ecs.ClientServer.Components
{
    [Serializable]
    public struct LastFootprintComponent
    {
        public Vector3 Position;
        public Vector3 Direction;
        public bool Left;
    }
}