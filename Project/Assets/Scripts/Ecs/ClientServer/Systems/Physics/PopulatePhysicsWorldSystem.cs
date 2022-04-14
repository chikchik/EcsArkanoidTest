using Fabros.Ecs.Utils;
using Fabros.EcsModules.Base.Components;
using Game.ClientServer.Physics;
using Game.Ecs.ClientServer.Components.Physics;
using Leopotam.EcsLite;
using UnityEngine;

namespace Game.Ecs.ClientServer.Systems.Physics
{
    public class PopulatePhysicsWorldSystem : IEcsRunSystem
    {
        public void Run(EcsSystems systems)
        {
            var world = systems.GetWorld();
            if(!world.HasUnique<PhysicsWorldComponent>()) return;
            
            var physicsWorld = world.GetUnique<PhysicsWorldComponent>();
            var filter = world
                .Filter<RigidbodyDefinitionComponent>()
                .Inc<BoxColliderComponent>()
                .Exc<BodyReferenceComponent>()
                .Exc<RigidbodyComponent>()
                .End();
            var poolRigidbodyDefinition = world.GetPool<RigidbodyDefinitionComponent>();
            var poolBoxCollider = world.GetPool<BoxColliderComponent>();
            var poolPositionComponent = world.GetPool<PositionComponent>();
            
            foreach (var entity in filter)
            {
                var rigidbodyDefinitionComponent = poolRigidbodyDefinition.Get(entity);
                var boxColliderComponent = poolBoxCollider.Get(entity);
                var positionComponent = poolPositionComponent.Get(entity);
                var bodyReference = Box2DPhysics.CreateBody(
                    physicsWorld.worldReference, 
                    (int)rigidbodyDefinitionComponent.bodyType,
                    (Vector2)positionComponent.value,
                    boxColliderComponent.size / 2f,
                    0f,
                    rigidbodyDefinitionComponent.density,
                    rigidbodyDefinitionComponent.friction,
                    rigidbodyDefinitionComponent.restitution,
                    rigidbodyDefinitionComponent.restitutionThreshold);
                
                ref var bodyReferenceComponent = ref entity.EntityAddComponent<BodyReferenceComponent>(world);
                bodyReferenceComponent.bodyReference = bodyReference;

                ref var rigidbodyComponent = ref entity.EntityAddComponent<RigidbodyComponent>(world);
                rigidbodyComponent.position = new Vector2(positionComponent.value.x, positionComponent.value.z);
            }
        }
    }
}
