using System;
using Leopotam.EcsLite;

namespace Game.Ecs.ClientServer.Components
{
    [Serializable]
    public struct ButtonComponent
    {
        public bool isActivated;
    }

    [Serializable]
    public struct ButtonPressedComponent
    {
    }

    [Serializable]
    public struct ButtonCustomComponent
    {
        public bool Spawn;
        public bool Shake;
    }
    
}