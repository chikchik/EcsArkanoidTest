using Fabros.EcsModules.Box2D.ClientServer.Api;
using UnityEngine;

namespace Fabros.EcsModules.Box2D.ClientServer.Components
{
    [System.Serializable]
    public struct RigidbodyComponent
    {
        public Vector2 LinearVelocity;
        public float AngularVelocity;
        public BodyType BodyType;
    }
}
