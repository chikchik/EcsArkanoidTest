using System;

namespace Game.Ecs.ClientServer.Components
{
    [Serializable]
    public struct HpComponent
    {
        public float Value;
    }
    
    [Serializable]
    public struct MaxHpComponent
    {
        public float Value;
    }
}