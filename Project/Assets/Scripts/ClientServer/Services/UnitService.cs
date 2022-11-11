using Game.Ecs.ClientServer.Components;
using UnityEngine;
using XFlow.Ecs.ClientServer.Components;
using XFlow.Ecs.ClientServer.Utils;
using XFlow.EcsLite;
using XFlow.Utils;

namespace Game.ClientServer.Services
{
    public static class UnitService
    {
        public static int CreateUnitEntity(EcsWorld world)
        {
            var entity = world.NewEntity();

            entity.EntityAdd<UnitComponent>(world);
            entity.EntityAdd<PositionComponent>(world).Value = new Vector3(0, 0.5f, -8);
            entity.EntityAdd<Rotation2DComponent>(world);

            world.Log($"GenerateUnitEntity {entity}");

            return entity;
        }
    }
}