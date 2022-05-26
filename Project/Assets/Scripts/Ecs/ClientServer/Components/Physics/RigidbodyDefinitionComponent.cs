using System;
using Game.ClientServer.Box2D;
using Leopotam.EcsLite;

namespace Game.Ecs.ClientServer.Components.Physics
{
    [System.Serializable]
    public struct RigidbodyDefinitionComponent : IEcsAutoReset<RigidbodyDefinitionComponent>
    {
        public BodyType BodyType;
        public float Density;
        public float Friction;
        public float Restitution;
        public float RestitutionThreshold;
        public float LinearDamping;
        public float AngularDamping;
        public UInt16 CategoryBits;
        public UInt16 MaskBits;
        public Int16 GroupIndex;
        public bool IsTrigger;
        
        public void AutoReset(ref RigidbodyDefinitionComponent c)
        {
            c.BodyType = BodyType.Dynamic;
            c.Density = 1f;
            c.RestitutionThreshold = 0.5f;
            c.LinearDamping = 2.0f;
            c.AngularDamping = 5.0f;
            c.Friction = 0f;
            c.Restitution = 0f;
            c.CategoryBits = 0x0001;
            c.MaskBits = 0x0001;
            c.GroupIndex = 0;
            c.IsTrigger = false;
        }
    }
}
