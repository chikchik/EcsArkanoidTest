using Game.ClientServer.Physics;

namespace Game.Ecs.ClientServer.Components.Physics
{
    [System.Serializable]
    public struct RigidbodyDefinitionComponent
    {
        public BodyType bodyType;
        public float density;
        public float friction;
        public float restitution;
        public float restitutionThreshold;
    }
}
