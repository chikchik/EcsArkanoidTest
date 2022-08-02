using Game.Ecs.Client.Components;
using Game.Ecs.ClientServer.Components;
using UnityEngine;
using XFlow.Ecs.Client.Components;
using XFlow.EcsLite;
using XFlow.Modules.Tick.Other;

namespace Game.Ecs.Client.Systems
{
    public class VFXCreationSystem : IEcsInitSystem, IEcsRunSystem
    {

        private readonly string fireVFX = "FireRed";

        private EcsWorld world;

        public void Init(EcsSystems systems)
        {
            world = systems.GetWorld();
        }

        public void Run(EcsSystems systems)
        {
            var tm = world.GetTime();

            var filterDestructibleDamaged = world.Filter<DestructibleDamagedComponent>().End();

            var poolDestructibleDamaged = world.GetPool<DestructibleDamagedComponent>();
            var poolActiveVFX = world.GetPool<ActiveVFX>();
            var poolTransform = world.GetPool<TransformComponent>();

            foreach (var request in filterDestructibleDamaged)
            {
                if (!poolDestructibleDamaged.Get(request).vfxEntity.Unpack(world, out var vfxEntity))
                {
                    poolDestructibleDamaged.Del(request);
                    continue;
                }

                if (poolActiveVFX.Has(vfxEntity)) // This can happen if the world was resimulated between frames
                {
                    continue;
                }

                var vfxGameObject = Object.Instantiate(Resources.Load<GameObject>(fireVFX));

                ref var activeVfx = ref poolActiveVFX.Add(vfxEntity);
                activeVfx.VFX = vfxGameObject;

                poolTransform.Add(vfxEntity).Transform = vfxGameObject.transform;
                poolDestructibleDamaged.Del(request);
            }
        }
    }
}