using Game.ClientServer.Physics;
using Game.Ecs.ClientServer.Components.Physics;
using Leopotam.EcsLite;

namespace Game.Ecs.ClientServer.Systems.Physics
{
    public class SyncPhysicsWorldSystem : IEcsRunSystem
    {
        public void Run(EcsSystems systems)
        {
            var world = systems.GetWorld();
            var filter = world
                .Filter<BodyReferenceComponent>()
                .Inc<RigidbodyComponent>()
                .End();
            var bodyReferenceComponent = world.GetPool<BodyReferenceComponent>();
            var poolRigidBodyComponent = world.GetPool<RigidbodyComponent>();
            
            foreach (var entity in filter)
            {
                var bodyReference = bodyReferenceComponent.Get(entity).bodyReference;
                var rigidBodyComponent = poolRigidBodyComponent.Get(entity);
                var bodyInfo = new BodyInfo
                {
                    position = rigidBodyComponent.position,
                    linearVelocity = rigidBodyComponent.linearVelocity,
                    angularVelocity = rigidBodyComponent.angularVelocity,
                    angle = rigidBodyComponent.angle
                };
                
                Box2DPhysics.SetBodyInfo(bodyReference, bodyInfo);
            }
        }
    }
}
