using Game.Ecs.ClientServer.Components;

using XFlow.EcsLite;
using XFlow.Modules.Box2D.ClientServer.Components.Other;
using XFlow.Utils;

namespace Game.Ecs.ClientServer.Systems
{
    public class BulletContactSystem : IEcsRunSystem, IEcsInitSystem
    {
        private EcsWorld world;
        public void Init(EcsSystems systems)
        {
            world = systems.GetWorld();
        }

        public void Run(EcsSystems systems)
        {
            var filter = world.Filter<Box2DBeginContactComponent>().End();
            var poolContacts = world.GetPool<Box2DBeginContactComponent>();
            var poolBulletHits = world.GetPool<BulletHitComponent>();
            var poolDestructibleHealth = world.GetPool<DestructibleHealthComponent>();
            var poolBullet = world.GetPool<BulletComponent>();
            foreach (var entity in filter)
            {
                var contact = poolContacts.Get(entity);

                if (!contact.Data.EntityA.Unpack(world, out var entityA))
                    continue;

                if (!contact.Data.EntityB.Unpack(world, out var entityB))
                    continue;

                if (poolBullet.Has(entityA))
                {
                    if (poolDestructibleHealth.Has(entityB))
                    {
                        CreateHitEntity(poolBullet.Get(entityA), world.PackEntity(entityB), poolBulletHits);
                    }
                    world.DelEntity(entityA);
                }

                if (poolBullet.Has(entityB))
                {
                    if (poolDestructibleHealth.Has(entityA))
                    {
                        CreateHitEntity(poolBullet.Get(entityB), world.PackEntity(entityA), poolBulletHits);
                    }
                    world.DelEntity(entityB);
                }
            }
        }

        private void CreateHitEntity(
            BulletComponent bullet,
            EcsPackedEntity entity,
            EcsPool<BulletHitComponent> poolBulletHits)

        {
            var hitEntity = world.NewEntity();
            ref var bulletHit = ref poolBulletHits.Add(hitEntity);
            bulletHit.Bullet = bullet;
            bulletHit.EntityHit = entity;
        }
    }
}