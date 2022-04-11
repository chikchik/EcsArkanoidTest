using System;
using Fabros.Ecs.Utils;
using Fabros.EcsModules.Base.Components;
using Game.Ecs.ClientServer.Components;
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
            entity.EntityAddComponent<PositionComponent>(world).value = new Vector3(-1.5f, 0, 9f);

            ref var radiusComponent = ref entity.EntityAddComponent<RadiusComponent>(world);
            radiusComponent.radius = 0.4f;

            ref var healthComponent = ref entity.EntityAddComponent<HealthComponent>(world);
            healthComponent.maxHealth = 100;
            healthComponent.health = 50;

            ref var speedComponent = ref entity.EntityAddComponent<SpeedComponent>(world);
            speedComponent.speed = 2f;

            Console.WriteLine($"GenerateUnitEntity {entity}");

            return entity;
        }

        public static void ResetUnitEntity(EcsWorld world, int unitEntity)
        {
            ref var healthComponent = ref unitEntity.EntityGetRefComponent<HealthComponent>(world);
            healthComponent.maxHealth = 100;
            healthComponent.health = 50;
        }
    }
}