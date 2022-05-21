using System;

namespace Game.Ecs.ClientServer.Components
{
    [Serializable]
    public struct DestroyWhenTimeIsOutComponent
    {
    }
    
    [Serializable]
    public struct StartSimpleMoveAtComponent
    {
        public float Time;
    }
}