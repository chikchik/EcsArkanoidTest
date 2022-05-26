using Game.ClientServer.Box2D;
using UnityEngine;

namespace Game.Physics
{
    public class Box2dRigidbody : MonoBehaviour
    {
        public BodyType bodyType;
        public float density;
        public float friction;
        public float restitution;
        public float restitutionThreshold;
    }
}
