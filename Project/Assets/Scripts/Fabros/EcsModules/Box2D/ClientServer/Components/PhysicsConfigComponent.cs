using System;

namespace Game.Ecs.ClientServer.Components.Physics
{
    [Serializable]
    public struct PhysicsConfigComponent
    {
        public int PositionIterations;
        public int VelocityIterations;
    }
}