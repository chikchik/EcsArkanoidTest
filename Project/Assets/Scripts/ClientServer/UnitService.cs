using System;
using Fabros.Ecs.Utils;
using Fabros.EcsModules.Base.Components;
using Game.ClientServer.Box2D;
using Game.ClientServer.Box2D.Components;
using Game.Ecs.Client.Systems;
using Game.Ecs.ClientServer.Components;
using Game.Ecs.ClientServer.Components.Physics;
using Leopotam.EcsLite;
using UnityEngine;

namespace Game.ClientServer
{
    public static class UnitService
    {
        public static int CreateUnitEntity(EcsWorld world)
        {
            var entity = world.NewEntity();

            //Console.WriteLine($"Generate LeoPlayerEntity: {entity} with PlayerID: {playerId}");
            entity.EntityAddComponent<UnitComponent>(world);
            entity.EntityAddComponent<MoveDirectionComponent>(world);
            entity.EntityAddComponent<LookDirectionComponent>(world).value = new Vector3(1,0,0);

            //entity.EntityAddComponent<PositionComponent>(world).value = new Vector3(-1.5f, 0, 9f);
            entity.EntityAddComponent<PositionComponent>(world).value = new Vector3(2, 0, 8.5f);

            entity.EntityAddComponent<FoodCollectedComponent>(world).Value = 0;
            entity.EntityAddComponent<AnimationStateComponent>(world).id = "angry";
            
            entity.EntityAddComponent<RotationComponent>(world);


            ref var radiusComponent = ref entity.EntityAddComponent<RadiusComponent>(world);
            radiusComponent.radius = 0.4f;

            ref var healthComponent = ref entity.EntityAddComponent<HealthComponent>(world);
            healthComponent.maxHealth = 100;
            healthComponent.health = 50;

            ref var speedComponent = ref entity.EntityAddComponent<SpeedComponent>(world);
            speedComponent.speed = 2f;
            
            ref var rigidBodyDefinitionComponent = ref entity.EntityAddComponent<RigidbodyDefinitionComponent>(world);
            rigidBodyDefinitionComponent.BodyType = BodyType.Kinematic;
            rigidBodyDefinitionComponent.Density = 985f;
            rigidBodyDefinitionComponent.Friction = 0.3f;
            rigidBodyDefinitionComponent.Restitution = 0;
            rigidBodyDefinitionComponent.RestitutionThreshold = 0.5f;   

            ref var collider = ref entity.EntityAddComponent<CircleColliderComponent>(world);
            collider.Radius = 0.3f;

            entity.EntityAdd<AverageSpeedComponent>(world) = world.GetUnique<AverageSpeedComponent>();
            
            Console.WriteLine($"GenerateUnitEntity {entity}");

            return entity;
        }

        public static void ResetUnitEntity(EcsWorld world, int unitEntity)
        {
            ref var healthComponent = ref unitEntity.EntityGetRefComponent<HealthComponent>(world);
            healthComponent.maxHealth = 100;
            healthComponent.health = 50;
            
            unitEntity.EntityReplace<PositionComponent>(world).value = new Vector3(-1.5f, 0, 9f);
        }

        public static void MoveUnit(EcsWorld world, int entity)
        {
            
        }
    }
}