using UnityEngine;

namespace Game.View
{
    public class Box2dPhysicsExtend : MonoBehaviour
    {
        public float RestitutionThreshold = 0.5f;
        public float Density = 10f;
        
        public float LinearDamping = 0;
        public float AngularDamping = 0;
    }
}