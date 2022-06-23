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
using Game.Fabros.Net.ClientServer;
using Game.Fabros.Net.ClientServer.Ecs.Components;
using Leopotam.EcsLite;
using UnityEngine;

namespace Game.Ecs.ClientServer.Systems
{
    public class ShootSystem : IEcsRunSystem
    {
        public void Run(EcsSystems systems)
        {
            var world = systems.GetWorld();
            var filter = world.Filter<ShootingComponent>().End();
            var poolShooting = world.GetPool<ShootingComponent>();

            var tm = world.GetTime();
            foreach (var entity in filter)
            {
                ref var shootingComponent = ref poolShooting.GetRef(entity);
                if (shootingComponent.ShootAtTime < tm && !shootingComponent.ShootMade)
                {
                    shootingComponent.ShootMade = true;
                    
                    var dir = entity.EntityGet<ShootingComponent>(world).Direction;
                    
                    var bulletEntity = world.NewEntity();
                    bulletEntity.EntityAdd<BulletComponent>(world);

                    var pos = entity.EntityGet<PositionComponent>(world).value;
                    bulletEntity.EntityAdd<PositionComponent>(world).value = (pos + dir/2).WithY(1.35f);
                    bulletEntity.EntityAdd<Rotation2DComponent>(world);

                    ref var def = ref bulletEntity.EntityAdd<Box2DRigidbodyDefinitionComponent>(world);
                    def.BodyType = BodyType.Dynamic;
                    def.Bullet = true;
                    def.Density = 5;

                    bulletEntity.EntityAdd<DebugMeComponent>(world);
                    
                    
                    world.Log($"create bullet {dir}");

                    ref var collider = ref bulletEntity.EntityAdd<Box2DCircleColliderComponent>(world);
                    collider.Radius = 0.02f;
                    
                    var body = Box2DServices.CreateBodyNow(world, bulletEntity);
                    Box2DApi.ApplyForce(body, dir.ToVector2XZ() * 3, pos.ToVector2XZ());
                    var bodyInfo = Box2DApi.GetBodyInfo(body);
                }

                if (shootingComponent.TotalTime < tm)
                {
                    entity.EntityDel<ShootingComponent>(world);
                    entity.EntityDel<CantMoveComponent>(world);
                }
            }
        }
    }
}