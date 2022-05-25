using Fabros.EcsModules.Base.Components;
using Fabros.EcsModules.Tick.Components;
using Fabros.EcsModules.Tick.Other;
using Game.ClientServer;
using Game.ClientServer.Physics;
using Game.ClientServer.Physics.Components;
using Game.Ecs.ClientServer.Components.Physics;
using Leopotam.EcsLite;
using UnityEngine;

namespace Game.Ecs.ClientServer.Systems.Physics
{
    public class UpdatePhysicsSystem : IEcsRunSystem
    {
        public void Run(EcsSystems systems)
        {
            var world = systems.GetWorld();
            if(!world.HasUnique<PhysicsWorldComponent>()) return;
            
            var physicsWorld = world.GetUnique<PhysicsWorldComponent>();
            var deltaTime = world.GetDeltaSeconds();

            Box2DPhysics.UpdateWorld(
                physicsWorld.WorldReference, 
                deltaTime, 
                Config.POSITION_ITERATIONS, 
                Config.VELOCITY_ITERATIONS);

            var filter = world
                .Filter<BodyReferenceComponent>()
                .Inc<RigidbodyComponent>()
                .Inc<PositionComponent>()
                .Inc<RotationComponent>()
                .End();
            
            var bodyReferenceComponent = world.GetPool<BodyReferenceComponent>();
            var poolRigidBodyComponent = world.GetPool<RigidbodyComponent>();
            var poolPosition = world.GetPool<PositionComponent>();
            var poolRotation = world.GetPool<RotationComponent>(); 
            
            foreach (var entity in filter)
            {
                var bodyReference = bodyReferenceComponent.Get(entity).BodyReference;
                var bodyInfo = Box2DPhysics.GetBodyInfo(bodyReference);
                
                ref var rigidBodyComponent = ref poolRigidBodyComponent.GetRef(entity);
                if (rigidBodyComponent.BodyType == BodyType.Dynamic)
                {
                    ref var positionComponent = ref poolPosition.GetRef(entity);
                    ref var rotationComponent = ref poolRotation.GetRef(entity);

                    positionComponent.value.x = bodyInfo.Position.x;
                    positionComponent.value.z = bodyInfo.Position.y;
                    rigidBodyComponent.LinearVelocity = bodyInfo.LinearVelocity;
                    rigidBodyComponent.AngularVelocity = bodyInfo.AngularVelocity;
                    rotationComponent.value = bodyInfo.Angle;
                }
            }
        }
    }
}
