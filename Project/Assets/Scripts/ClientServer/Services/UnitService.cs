using System;
using Fabros.Ecs.ClientServer.Components;
using Fabros.Ecs.Utils;
using Fabros.EcsModules.Box2D.ClientServer.Api;
using Fabros.EcsModules.Box2D.ClientServer.Components;
using Fabros.EcsModules.Box2D.ClientServer.Components.Colliders;
using Game.Ecs.ClientServer.Components;
using Flow.EcsLite;
using UnityEngine;

namespace Game.ClientServer
{
    public static class UnitService
    {
        public static int CreateUnitEntity(EcsWorld world)
        {
            var entity = world.NewEntity();
            
            entity.EntityAddComponent<UnitComponent>(world);
            entity.EntityAddComponent<LookDirectionComponent>(world).value = new Vector3(0,0,1);
            entity.EntityAddComponent<PositionComponent>(world).value = new Vector3(0, 0, 0);
            entity.EntityAddComponent<FoodCollectedComponent>(world).Value = 0;
            entity.EntityAddComponent<Rotation2DComponent>(world);


            ref var radiusComponent = ref entity.EntityAddComponent<RadiusComponent>(world);
            radiusComponent.radius = 0.4f;

            ref var healthComponent = ref entity.EntityAddComponent<HealthComponent>(world);
            healthComponent.maxHealth = 100;
            healthComponent.health = 50;
            
            ref var rigidBodyDefinitionComponent = ref entity.EntityAddComponent<Box2DRigidbodyDefinitionComponent>(world);
            rigidBodyDefinitionComponent.BodyType = BodyType.Kinematic;
            rigidBodyDefinitionComponent.Density = 985f;
            rigidBodyDefinitionComponent.Friction = 0.3f;
            rigidBodyDefinitionComponent.Restitution = 0;
            rigidBodyDefinitionComponent.RestitutionThreshold = 0.5f;   

            ref var collider = ref entity.EntityAddComponent<Box2DCircleColliderComponent>(world);
            collider.Radius = 0.4f;

            entity.EntityAdd<AverageSpeedComponent>(world) = world.GetUnique<AverageSpeedComponent>();

            Debug.Log($"GenerateUnitEntity {entity}");

            return entity;
        }
    }
}