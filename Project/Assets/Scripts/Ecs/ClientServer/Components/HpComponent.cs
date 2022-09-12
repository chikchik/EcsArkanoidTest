using System;

namespace Game.Ecs.ClientServer.Components
{
    [Serializable]
    public struct HpComponent
    {
        public float Value;
        public float MaxValue;
    }
}