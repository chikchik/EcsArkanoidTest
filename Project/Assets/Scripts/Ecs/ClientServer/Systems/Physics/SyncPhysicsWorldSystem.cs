using System;
using Fabros.EcsModules.Base.Components;
using Game.ClientServer.Physics;
using Game.ClientServer.Physics.Components;
using Game.Ecs.ClientServer.Components.Physics;
using Leopotam.EcsLite;
using UnityEngine;

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
            var poolRotation = world.GetPool<RotationComponent>(); 
            
            foreach (var entity in filter)
            {
                var bodyReference = poolBodyReference.Get(entity).bodyReference;
                var positionComponent = poolPosition.Get(entity);
                var rotationComponent = poolRotation.Get(entity);
                ref var rigidBodyComponent = ref poolRigidbody.GetRef(entity);
                // rigidBodyComponent.position.x = positionComponent.value.x;
                // rigidBodyComponent.position.y = positionComponent.value.z;

                var bodyInfo = new BodyInfo
                {
                    linearVelocity = rigidBodyComponent.linearVelocity,
                    angularVelocity = rigidBodyComponent.angularVelocity,
                    angle = rotationComponent.value
                };
                bodyInfo.position.x = positionComponent.value.x;
                bodyInfo.position.y = positionComponent.value.z;
                
                Box2DPhysics.SetBodyInfo(bodyReference, bodyInfo);
            }
        }
    }
}
