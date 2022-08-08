using Game.Ecs.ClientServer.Components;
using UnityEngine;
using XFlow.Ecs.ClientServer.Components;
using XFlow.EcsLite;
using XFlow.Modules.Box2D.ClientServer;
using XFlow.Modules.Box2D.ClientServer.Api;
using XFlow.Modules.Box2D.ClientServer.Components;
using XFlow.Modules.Box2D.ClientServer.Components.Colliders;
using XFlow.Utils;

namespace Game.ClientServer.Services
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

            Box2DServices.AddRigidbodyDefinition(entity, world, BodyType.Kinematic).SetDensity(985f).SetFriction(0.3f)
                .SetRestitution(0).SetRestitutionThreshold(0.5f);
            Box2DServices.AddCircleCollider(entity, world, 0.4f);

            entity.EntityAdd<AverageSpeedComponent>(world) = world.GetUnique<AverageSpeedComponent>();

            UnityEngine.Debug.Log($"GenerateUnitEntity {entity}");

            return entity;
        }
    }
}