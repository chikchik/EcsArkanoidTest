using Fabros.Ecs.Client.Components;
using Fabros.EcsModules.Tick.Other;
using Flow.EcsLite;
using Game.Ecs.Client.Components;
using Game.Ecs.ClientServer.Components;
using UnityEngine;

namespace Game.Ecs.Client.Systems
{
    public class VFXCreationSystem : IEcsInitSystem, IEcsRunSystem
    {

        private readonly string _fireVFX = "FireRed";

        private EcsWorld _world;

        public void Init(EcsSystems systems)
        {
            _world = systems.GetWorld();
        }

        public void Run(EcsSystems systems)
        {
            var tm = _world.GetTime();

            var filterDestructibleDamaged = _world.Filter<DestructibleDamagedComponent>().End();

            var poolDestructibleDamaged = _world.GetPool<DestructibleDamagedComponent>();
            var poolActiveVFX = _world.GetPool<ActiveVFX>();
            var poolTransform = _world.GetPool<TransformComponent>();

            foreach (var request in filterDestructibleDamaged)
            {
                if (!poolDestructibleDamaged.Get(request).vfxEntity.Unpack(_world, out var vfxEntity))
                {
                    poolDestructibleDamaged.Del(request);
                    continue;
                }

                if (poolActiveVFX.Has(vfxEntity)) // This can happen if the world was resimulated between frames
                {
                    continue;
                }

                var vfxGameObject = Object.Instantiate(Resources.Load<GameObject>(_fireVFX));

                ref var activeVfx = ref poolActiveVFX.Add(vfxEntity);
                activeVfx.VFX = vfxGameObject;

                poolTransform.Add(vfxEntity).Transform = vfxGameObject.transform;
                poolDestructibleDamaged.Del(request);
            }
        }
    }
}