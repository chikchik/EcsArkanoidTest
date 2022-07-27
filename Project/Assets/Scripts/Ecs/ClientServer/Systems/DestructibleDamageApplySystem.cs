using Fabros.EcsModules.Tick.Other;
using Flow.EcsLite;
using Game.Ecs.ClientServer.Components;

namespace Game.Ecs.ClientServer.Systems
{
    public class DestructibleDamageApplySystem : IEcsRunSystem, IEcsInitSystem
    {
        private EcsWorld _world;

        public void Init(EcsSystems systems)
        {
            _world = systems.GetWorld();
        }

        public void Run(EcsSystems systems)
        {
            var filterHits = _world.Filter<BulletHitComponent>().End();
            var tm = _world.GetTime();

            var poolBulletHit = _world.GetPool<BulletHitComponent>();
            var poolDestructibleHealth = _world.GetPool<DestructibleHealthComponent>();
            var poolVFXRequest = _world.GetPool<VFXRequestComponent>();
            var poolDestroyAtTime = _world.GetPool<DestroyAtTimeComponent>();

            foreach (var hit in filterHits)
            {
                var bulletHit = poolBulletHit.Get(hit);
                if (!bulletHit.EntityHit.Unpack(_world, out var entityHit))
                {
                    continue;
                }

                if (!poolDestructibleHealth.Has(entityHit))
                {
                    continue;
                }

                ref var destructibleHealth = ref poolDestructibleHealth.GetRef(entityHit);

                if (destructibleHealth.Health > 0)
                {
                    destructibleHealth.Health -= bulletHit.Bullet.Damage;
                    if (destructibleHealth.Health <= 0)
                    {
                        poolVFXRequest.Add(entityHit).VFXName = "FireRed";
                        poolDestructibleHealth.Del(entityHit);
                        poolDestroyAtTime.Add(entityHit).Time = tm + 3f;
                    }
                }
            }
        }
    }
}