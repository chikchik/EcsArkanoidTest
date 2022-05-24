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

    
    [EmptyComponent]
    [Serializable]
    public struct ButtonSpawnComponent
    {
    }
}