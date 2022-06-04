using System;

namespace Game.Ecs.ClientServer.Systems
{
    [Serializable]
    public struct PushingComponent
    {
        public float EndTime;
    }
    
    [Serializable]
    public struct ShootingComponent
    {
        //public float EndTime;
    }
}