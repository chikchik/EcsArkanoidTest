using Fabros.EcsModules.Box2D.ClientServer.Components.Other;
using Game.Ecs.ClientServer.Components;
using Flow.EcsLite;

namespace Game.Ecs.ClientServer.Systems
{
    public class BulletContactSystem : IEcsRunSystem, IEcsInitSystem
    {
        EcsWorld world;
        public void Init(EcsSystems systems)
        {
            world = systems.GetWorld();
        }

        public void Run(EcsSystems systems)
        {
            var filter = world.Filter<Box2DBeginContactComponent>().End();
            var poolContacts = world.GetPool<Box2DBeginContactComponent>();
            var poolBulletHits = world.GetPool<BulletHit>();
            var poolBullet = world.GetPool<BulletComponent>();
            foreach (var entity in filter)
            {
                var contact = poolContacts.Get(entity);
                int entityA, entityB;

                if (!contact.Data.EntityA.Unpack(world, out entityA))
                    continue;

                if (!contact.Data.EntityB.Unpack(world, out entityB))
                    continue;

                if (poolBullet.Has(entityA))
                {
                    if (poolBulletHits.Has(entityB))
                    {
                        poolBulletHits.Get(entityB).BulletHits.Enqueue(poolBullet.Get(entityA));
                    }
                }

                if (poolBullet.Has(entityB))
                {
                    if (poolBulletHits.Has(entityA))
                    {
                        poolBulletHits.Get(entityA).BulletHits.Enqueue(poolBullet.Get(entityB));
                    }
                }
            }
        }
    }
}