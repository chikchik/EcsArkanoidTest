using Game.Ecs.ClientServer.Components;
using XFlow.Ecs.ClientServer;
using XFlow.Ecs.ClientServer.Components;
using XFlow.EcsLite;
using XFlow.Modules.Tick.Other;
using XFlow.Net.ClientServer;

namespace Game.Ecs.ClientServer.Systems
{
    public class DestructibleDamageApplySystem : IEcsRunSystem, IEcsInitSystem
    {
        private readonly float _boxDestructionTime = 3f;
        private readonly float _fireDuration = 5f;

        private EcsWorld _world;
        private EcsWorld _eventWorld;

        public void Init(EcsSystems systems)
        {
            _world = systems.GetWorld();
            _eventWorld = systems.GetWorld(EcsWorlds.Event);
        }

        public void Run(EcsSystems systems)
        {
            var filterHits = _eventWorld.Filter<BulletHitComponent>().End();
            var tm = _world.GetTime();

            var poolBulletHit = _eventWorld.GetPool<BulletHitComponent>();
            var poolFollow = _world.GetPool<FollowComponent>();
            var poolDestructibleHealth = _world.GetPool<HpComponent>();
            var poolDestructibleDamaged = _world.GetPool<DestructibleDamagedComponent>();
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

                if (destructibleHealth.Value > 0)
                {
                    destructibleHealth.Value -= bulletHit.Bullet.Damage;
                    if (destructibleHealth.Value <= 0)
                    {
                        var vfxEntity = _world.NewEntity();

                        poolFollow.Add(vfxEntity).Entity = _world.PackEntity(entityHit);
                       //poolDestructibleDamaged.Add(entityHit).vfxEntity = _world.PackEntity(vfxEntity);
                        //poolDestructibleHealth.Del(entityHit);
                        //poolDestroyAtTime.Add(entityHit).Time = tm + _boxDestructionTime;
                        _world.MarkEntityAsDeleted(entityHit);
                        
                        poolDestroyAtTime.Add(vfxEntity).Time = tm + _fireDuration;
                    }
                }
            }
        }
    }
}