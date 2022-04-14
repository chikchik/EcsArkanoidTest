using Fabros.EcsModules.Base.Components;
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
                .Inc<PositionComponent>()
                .End();
            var poolBodyReference = world.GetPool<BodyReferenceComponent>();
            var poolRigidbody = world.GetPool<RigidbodyComponent>();
            var poolPosition = world.GetPool<PositionComponent>();
            
            foreach (var entity in filter)
            {
                var bodyReference = poolBodyReference.Get(entity).bodyReference;
                var positionComponent = poolPosition.Get(entity);
                ref var rigidBodyComponent = ref poolRigidbody.GetRef(entity);
                rigidBodyComponent.position = positionComponent.value;
                
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
