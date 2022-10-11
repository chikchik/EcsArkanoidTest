using System;
using UnityEngine;

namespace Game.Ecs.ClientServer.Components
{
    [Serializable]
    public struct FootprintComponent
    {
        public Vector3 Direction;
        public bool Left;
    }
}