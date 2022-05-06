using UnityEngine;

namespace Game.Ecs.ClientServer.Components.Physics
{
    [System.Serializable]
    public struct RigidbodyComponent
    {
        public Vector2 linearVelocity;
        public float angularVelocity;
    }
}
