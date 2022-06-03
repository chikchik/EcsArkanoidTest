using Fabros.Ecs.ClientServer.Components;
using Fabros.EcsModules.Box2D.ClientServer.Api;
using Fabros.EcsModules.Box2D.ClientServer.Components;
using Leopotam.EcsLite;

namespace Fabros.EcsModules.Box2D.ClientServer.Systems
{
    public class Box2DWriteStateToComponentsSystem :  IEcsRunSystem
    {
        public void Run(EcsSystems systems)
        {
            var world = systems.GetWorld();

            var filter = world
                .Filter<Box2DBodyComponent>()
                .Inc<Box2DRigidbodyComponent>()
                .Inc<PositionComponent>()
                .Inc<Rotation2DComponent>()
                .End();
            
            var bodyReferenceComponent = world.GetPool<Box2DBodyComponent>();
            var poolRigidBodyComponent = world.GetPool<Box2DRigidbodyComponent>();
            var poolPosition = world.GetPool<PositionComponent>();
            var poolRotation = world.GetPool<Rotation2DComponent>(); 
            
            foreach (var entity in filter)
            {
                var bodyReference = bodyReferenceComponent.Get(entity).BodyReference;
                var bodyInfo = Box2DApi.GetBodyInfo(bodyReference);
                
                ref var rigidBodyComponent = ref poolRigidBodyComponent.GetRef(entity);
                if (rigidBodyComponent.BodyType == BodyType.Dynamic)
                {
                    ref var positionComponent = ref poolPosition.GetRef(entity);
                    positionComponent.value.x = bodyInfo.Position.x;
                    positionComponent.value.z = bodyInfo.Position.y;
                    
                    rigidBodyComponent.LinearVelocity = bodyInfo.LinearVelocity;
                    rigidBodyComponent.AngularVelocity = bodyInfo.AngularVelocity;
                    
                    poolRotation.GetRef(entity).Angle = bodyInfo.Angle;
                }
            }
        }
    }
}