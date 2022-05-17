using System;
using UnityEngine;

namespace Game.Ecs.ClientServer.Components
{
    [Serializable]
    public struct ButtonComponent
    {
        public bool isActivated;
    }

    [Serializable]
    public struct ButtonPushCompleted
    {
    }
    
    [Serializable]
    public struct RootMotionComponent
    {
        public Vector3 Position;
    }
}