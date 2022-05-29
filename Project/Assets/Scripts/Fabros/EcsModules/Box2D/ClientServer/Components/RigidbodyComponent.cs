using Game.ClientServer.Box2D;
using UnityEngine;

namespace Game.Ecs.ClientServer.Components.Physics
{
    [System.Serializable]
    public struct RigidbodyComponent
    {
        public Vector2 LinearVelocity;
        public float AngularVelocity;
        public BodyType BodyType;
    }
}
