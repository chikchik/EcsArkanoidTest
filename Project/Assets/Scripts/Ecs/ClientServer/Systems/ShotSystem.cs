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

                    var dir = entity.EntityGet<LookDirectionComponent>(world).value;
                    var shotEntity = world.NewEntity();
                    shotEntity.EntityAdd<BulletComponent>(world);

                    var pos = entity.EntityGet<PositionComponent>(world).value;
                    shotEntity.EntityAdd<PositionComponent>(world).value = (pos + dir/2).WithY(1.35f);
                    shotEntity.EntityAdd<Rotation2DComponent>(world);

                    ref var def = ref shotEntity.EntityAdd<Box2DRigidbodyDefinitionComponent>(world);
                    def.BodyType = BodyType.Dynamic;
                    def.Bullet = true;
                    def.Density = 5;

                    ref var collider = ref shotEntity.EntityAdd<Box2DCircleColliderComponent>(world);
                    collider.Radius = 0.02f;
                    
                    var body = Box2DServices.CreateBodyNow(world, shotEntity);
                    Box2DApi.ApplyForce(body, dir.ToVector2XZ() * 10, pos.ToVector2XZ());
                }
            }
        }
    }
}