using System;
using System.Collections.Generic;
using Game.Ecs.ClientServer.Components;
using UnityEngine;
using XFlow.Ecs.ClientServer.Components;
using XFlow.Ecs.ClientServer.Utils;
using XFlow.EcsLite;
using XFlow.Modules.Box2D.ClientServer;
using XFlow.Modules.Box2D.ClientServer.Api;
using XFlow.Modules.Box2D.ClientServer.Components;
using XFlow.Modules.Tick.Other;
using XFlow.Net.ClientServer.Ecs.Components;
using XFlow.Net.ClientServer.Utils;
using XFlow.Utils;
using Random = UnityEngine.Random;
using Utils = XFlow.Net.ClientServer.Utils.Utils;

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
                ref var ammo = ref entity.EntityGetRef<AmmoCollectedComponent>(world);
                if (shootingComponent.ShootAtTime < tm && !shootingComponent.ShootMade && ammo.Value > 0)
                {
                    shootingComponent.ShootMade = true;
                    ammo.Value--;

                    var shoot = entity.EntityGet<ShootingComponent>(world);

                    int rd = Random.Range(1, 5);
                    var lst = new List<int>();
                    for (int i = 0; i < rd; ++i)
                    {
                        lst.Add(world.NewEntity());
                    }

                    var bulletEntity = world.NewEntity();
                    bulletEntity.EntityAdd<BulletComponent>(world).Damage = 1f;
                    
                    foreach (var en in lst)
                    {
                        //world.DelEntity(en);
                    }

                    //var unitPos = entity.EntityGet<PositionComponent>(world).value;
                    var pos = shoot.Position;
                    var dir = entity.EntityGet<ShootingComponent>(world).Direction;
                    bulletEntity.EntityAdd<PositionComponent>(world).value = pos;
                    bulletEntity.EntityAdd<Rotation2DComponent>(world);

                    bulletEntity.EntityAdd<DebugNameComponent>(world).Name = "Bullet";

                    var customHash = (uint)Random.Range(1, 1111); // HashUtils.CustomHash(entity, world.GetTick());
                    bulletEntity.EntityAdd<UniqueIdComponent>(world).Value = customHash;
                    
                    Box2DServices.AddRigidbodyDefinition(world, bulletEntity).SetBullet(true).SetDensity(20).SetLinearDamping(0);
                    Box2DServices.AddCircleColliderToDefinition(world, bulletEntity, 0.02f, Vector2.zero);
                    
                    bulletEntity.EntityAdd<DebugMeComponent>(world);


                    var body = Box2DServices.CreateBodyNow(world, bulletEntity);

                    var force = dir.ToVector2XZ() * 20;
                    Box2DApiSafe.ApplyForce(body, force, pos.ToVector2XZ());
                    world.LogVerbose($"ApplyForce {pos.ToVector2XZ()} {force}");
                    //var bodyInfo = Box2DApi.GetBodyInfo(body);

                    //защита от бесконечного полета в пустоте
                    bulletEntity.EntityAdd<DestroyAtTimeComponent>(world).Time = tm + 1.5f;
                    
                    
                    world.Log($"created bullet {bulletEntity.e2name(world)} at tick {world.GetTick()} , hash: {customHash}");
                }

                if (shootingComponent.TotalTime < tm)
                {
                    entity.EntityDel<ShootingComponent>(world);
                    //entity.EntityDel<CantMoveComponent>(world);
                }
            }
        }
    }
}