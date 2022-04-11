using System;

namespace Game.Fabros.EcsModules.Fire.ClientServer.Components
{
    [Serializable]
    public struct FireComponent
    {
        public float size;
        public float startTime;
        public float endTime;
        public bool  destroyEntity;
    }
}