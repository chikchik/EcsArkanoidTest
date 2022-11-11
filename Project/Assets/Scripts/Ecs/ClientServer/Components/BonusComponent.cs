using System;

namespace Game.Ecs.ClientServer.Components
{
    public enum BonusType
    {
        MultiBalls
    }

    [Serializable]
    public struct BonusComponent
    {
        public BonusType BonusType;
    }
}