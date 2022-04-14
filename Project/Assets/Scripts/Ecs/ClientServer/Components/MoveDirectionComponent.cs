using System;
using UnityEngine;

namespace Game.Ecs.ClientServer.Components
{
    [Serializable]
    public struct MoveDirectionComponent
    {
        public Vector3 value;
    }
    
    [Serializable]
    public struct LookDirectionComponent
    {
        public Vector3 value;
    }
}