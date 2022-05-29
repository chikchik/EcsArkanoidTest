using Game.Fabros.EcsModules.Box2D.ClientServer.Api;
using UnityEngine;

namespace Game.Fabros.EcsModules.Box2D.ClientServer.Components
{
    [System.Serializable]
    public struct RigidbodyComponent
    {
        public Vector2 LinearVelocity;
        public float AngularVelocity;
        public BodyType BodyType;
    }
}
