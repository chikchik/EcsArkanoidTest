
using Fabros.Ecs.ClientServer.Components;
using Fabros.Ecs.Utils;
using Fabros.EcsModules.Mech.ClientServer.Components;
using Flow.EcsLite;
using UnityEngine;

namespace Fabros.EcsModules.Mech.ClientServer
{
    public class MechService
    {
        public int CreateMechEntity(EcsWorld world)
        {
            var entity = world.NewEntity();
            entity.EntityAdd<MechComponent>(world);
            entity.EntityAdd<PositionComponent>(world);
            entity.EntityAdd<Rotation2DComponent>(world).Angle = 1;
            //entity.EntityAdd<MechMovingComponent>(world);

            return entity;
        }
    }
}