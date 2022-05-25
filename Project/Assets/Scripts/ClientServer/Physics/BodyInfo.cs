using UnityEngine;

namespace Game.ClientServer.Physics
{
    public struct BodyInfo
    {
        public Vector2 Position;
        public Vector2 LinearVelocity;
        public float AngularVelocity;
        public float Angle;
        
        public override string ToString() =>
            $"Position: {Position}; LinearVelocity: {LinearVelocity};" +
            $" AngularVelocity: {AngularVelocity}; Angle: {Angle}";
    }
}