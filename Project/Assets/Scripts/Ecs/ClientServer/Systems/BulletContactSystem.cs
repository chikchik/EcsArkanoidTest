using Fabros.EcsModules.Box2D.ClientServer.Components.Other;
using Game.Ecs.ClientServer.Components;
using Flow.EcsLite;

namespace Game.Ecs.ClientServer.Systems
{
    public class BulletContactSystem : IEcsRunSystem, IEcsInitSystem
    {
        private EcsWorld _world;
        public void Init(EcsSystems systems)
        {
            _world = systems.GetWorld();
        }

        public void Run(EcsSystems systems)
        {
            var filter = _world.Filter<Box2DBeginContactComponent>().End();
            var poolContacts = _world.GetPool<Box2DBeginContactComponent>();
            var poolBulletHits = _world.GetPool<BulletHitComponent>();
            var poolDestructibleHealth = _world.GetPool<DestructibleHealthComponent>();
            var poolBullet = _world.GetPool<BulletComponent>();
            foreach (var entity in filter)
            {
                var contact = poolContacts.Get(entity);

                if (!contact.Data.EntityA.Unpack(_world, out var entityA))
                    continue;

                if (!contact.Data.EntityB.Unpack(_world, out var entityB))
                    continue;

                if (poolBullet.Has(entityA))
                {
                    if (poolDestructibleHealth.Has(entityB))
                    {
                        CreateHitEntity(poolBullet.Get(entityA), _world.PackEntity(entityB), poolBulletHits);
                    }
                }

                if (poolBullet.Has(entityB))
                {
                    if (poolDestructibleHealth.Has(entityA))
                    {
                        CreateHitEntity(poolBullet.Get(entityB), _world.PackEntity(entityA), poolBulletHits);
                    }
                }
            }
        }

        private void CreateHitEntity(
            BulletComponent bullet,
            EcsPackedEntity entity,
            EcsPool<BulletHitComponent> poolBulletHits)

        {
            var hitEntity = _world.NewEntity();
            ref var bulletHit = ref poolBulletHits.Add(hitEntity);
            bulletHit.Bullet = bullet;
            bulletHit.EntityHit = entity;
        }
    }
}