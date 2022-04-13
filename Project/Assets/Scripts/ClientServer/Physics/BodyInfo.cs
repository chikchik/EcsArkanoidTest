using UnityEngine;

namespace Game.ClientServer.Physics
{
    public struct BodyInfo
    {
        public Vector2 position;
        public Vector2 linearVelocity;
        public float angularVelocity;
        public float angle;
    }
}