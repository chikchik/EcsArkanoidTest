using Fabros.Ecs.ClientServer.Components;
using Fabros.Ecs.Utils;
using Fabros.EcsModules.Box2D.ClientServer;
using Fabros.EcsModules.Box2D.ClientServer.Api;
using Fabros.EcsModules.Box2D.ClientServer.Components;
using Fabros.EcsModules.Box2D.ClientServer.Components.Colliders;
using Fabros.EcsModules.Box2D.ClientServer.Systems;
using Fabros.EcsModules.Tick.Other;
using Game.ClientServer;
using Game.Ecs.ClientServer.Components;
using Leopotam.EcsLite;
using UnityEngine;

namespace Game.Ecs.ClientServer.Systems
{
    public class ShotSystem : IEcsRunSystem
    {
        public void Run(EcsSystems systems)
        {
            var world = systems.GetWorld();
            var filter = world.Filter<MakeShotComponent>().End();
            foreach (var entity in filter)
            {
                if (entity.EntityGet<MakeShotComponent>(world).Time < world.GetTime())
                {
                    entity.EntityDel<MakeShotComponent>(world);

                    var dir = entity.EntityGet<LookDirectionComponent>(world);
                    var shotEntity = world.NewEntity();

                    var pos = entity.EntityGet<PositionComponent>(world).value;
                    shotEntity.EntityAdd<PositionComponent>(world).value = pos;
                    shotEntity.EntityAdd<Rotation2DComponent>(world);

                    var def = shotEntity.EntityAdd<Box2DRigidbodyDefinitionComponent>(world);
                    def.BodyType = BodyType.Dynamic;

                    ref var collider = ref shotEntity.EntityAdd<Box2DCircleColliderComponent>(world);
                    collider.Radius = 0.1f;
                    
                    var body = Box2DServices.CreateBodyNow(world, shotEntity);
                    Box2DApi.ApplyForce(body, dir.value.ToVector2XZ() * 100, pos.ToVector2XZ());
                    //Box2DApi.SetBullet(body, true);
                }
            }
        }
    }
}