using UnityEngine;

namespace Game.ClientServer.Physics
{
    public struct BodyInfo
    {
        public Vector2 Position;
        public Vector2 LinearVelocity;
        public float AngularVelocity;
        public float Angle;
    }
}