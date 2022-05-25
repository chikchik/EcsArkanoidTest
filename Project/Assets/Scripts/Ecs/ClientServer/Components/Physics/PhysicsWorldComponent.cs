using System;

namespace Game.Ecs.ClientServer.Components.Physics
{
    [System.Serializable]
    public struct PhysicsWorldComponent
    {
        public System.IntPtr WorldReference;
    }
    
    [Serializable]
    public struct PhysicsConfigComponent
    {
        public int PositionIterations;
        public int VelocityIterations;
    }
}
