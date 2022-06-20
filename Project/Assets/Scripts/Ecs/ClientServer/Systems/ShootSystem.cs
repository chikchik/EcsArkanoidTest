﻿using Fabros.Ecs.ClientServer.Components;
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

                if (shootingComponent.TotalTime < tm)
                {
                    entity.EntityDel<ShootingComponent>(world);
                    entity.EntityDel<CantMoveComponent>(world);
                }
            }
        }
    }
}