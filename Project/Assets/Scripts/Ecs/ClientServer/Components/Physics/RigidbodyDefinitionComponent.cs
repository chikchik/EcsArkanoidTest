using System;
using Game.ClientServer.Physics;
using Leopotam.EcsLite;

namespace Game.Ecs.ClientServer.Components.Physics
{
    [System.Serializable]
    public struct RigidbodyDefinitionComponent : IEcsAutoReset<RigidbodyDefinitionComponent>
    {
        public BodyType bodyType;
        public float density;
        public float friction;
        public float restitution;
        public float restitutionThreshold;
        public float linearDamping;
        public float angularDamping;
        public UInt16 CategoryBits;
        public UInt16 MaskBits;
        public Int16 GroupIndex;
        public bool IsTrigger;
        
        public void AutoReset(ref RigidbodyDefinitionComponent c)
        {
            c.bodyType = BodyType.Static;
            c.density = 1f;
            c.restitutionThreshold = 0.5f;
            c.linearDamping = 2.0f;
            c.angularDamping = 5.0f;
            c.friction = 0f;
            c.restitution = 0f;
            c.CategoryBits = 0x0001;
            c.MaskBits = 0x0001;
            c.GroupIndex = 0;
            c.IsTrigger = false;
        }
    }
}
