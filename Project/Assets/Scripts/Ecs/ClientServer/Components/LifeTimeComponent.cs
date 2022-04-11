using System;

namespace Game.Ecs.ClientServer.Components
{
    [Serializable]
    public struct LifeTimeComponent
    {
        public int destroyTick;
    }
}