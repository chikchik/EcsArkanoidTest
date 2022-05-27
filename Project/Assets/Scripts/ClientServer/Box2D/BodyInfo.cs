using UnityEngine;

namespace Game.ClientServer.Box2D
{
    public struct BodyInfo
    {
        public Vector2 Position;
        public Vector2 LinearVelocity;
        public float AngularVelocity;
        public float Angle;
        public bool Awake;
        
        public override string ToString() =>
            $"Position: {Position}; LinearVelocity: {LinearVelocity};" +
            $" AngularVelocity: {AngularVelocity}; Angle: {Angle}" +
            $"awake {Awake}";
    }
}