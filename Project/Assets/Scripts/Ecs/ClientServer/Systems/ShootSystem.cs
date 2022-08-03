using Game.Ecs.ClientServer.Components;
using XFlow.Ecs.ClientServer.Components;
using XFlow.Ecs.ClientServer.Utils;
using XFlow.EcsLite;
using XFlow.Modules.Box2D.ClientServer;
using XFlow.Modules.Box2D.ClientServer.Api;
using XFlow.Modules.Box2D.ClientServer.Components;
using XFlow.Modules.Box2D.ClientServer.Components.Colliders;
using XFlow.Modules.Tick.Other;
using XFlow.Utils;

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

                    var bulletEntity = world.NewEntity();
                    bulletEntity.EntityAdd<BulletComponent>(world);

                    //var unitPos = entity.EntityGet<PositionComponent>(world).value;
                    var pos = shoot.Position;
                    var dir = entity.EntityGet<ShootingComponent>(world).Direction;
                    bulletEntity.EntityAdd<PositionComponent>(world).value = pos;
                    bulletEntity.EntityAdd<Rotation2DComponent>(world);

                    ref var def = ref bulletEntity.EntityAdd<Box2DRigidbodyDefinitionComponent>(world);
                    def.BodyType = BodyType.Dynamic;
                    def.Bullet = true;
                    def.Density = 20;
                    def.LinearDamping = 0;

                    bulletEntity.EntityAdd<DebugMeComponent>(world);


                    world.Log($"create bullet {bulletEntity} {dir} {pos}");

                    ref var collider = ref bulletEntity.EntityAdd<Box2DCircleColliderComponent>(world);
                    collider.Radius = 0.02f;

                    var body = Box2DServices.CreateBodyNow(world, bulletEntity);

                    var force = dir.ToVector2XZ() * 20;
                    Box2DApi.ApplyForce(body, force, pos.ToVector2XZ());
                    world.Log($"ApplyForce {pos.ToVector2XZ()} {force}");
                    //var bodyInfo = Box2DApi.GetBodyInfo(body);

                    //защита от бесконечного полета в пустоте
                    bulletEntity.EntityAdd<DestroyAtTimeComponent>(world).Time = tm + 5f;
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