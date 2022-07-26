using Flow.EcsLite;
using Game.Ecs.Client.Components;
using Game.Ecs.ClientServer.Components;
using UnityEngine;

namespace Game.Ecs.Client.Systems
{
    public class VFXCreationSystem : IEcsInitSystem, IEcsRunSystem
    {
        private EcsWorld _world;

        public void Init(EcsSystems systems)
        {
            _world = systems.GetWorld();
        }

        public void Run(EcsSystems systems)
        {
            var filterVFXRequest = _world.Filter<VFXRequestComponent>().End();
            var poolVFXRequest = _world.GetPool<VFXRequestComponent>();
            var poolActiveVFX = _world.GetPool<ActiveVFX>();
            var poolFollow = _world.GetPool<FollowComponent>();

            foreach (var request in filterVFXRequest)
            {
                var requestData = poolVFXRequest.Get(request);

                var vfxEntity = _world.NewEntity();
                ref var activeVfx = ref poolActiveVFX.Add(vfxEntity);

                activeVfx.DeathTime = 3;
                activeVfx.VFX = Object.Instantiate(Resources.Load<GameObject>(requestData.VFXName));

                poolVFXRequest.Del(request);
            }
        }
    }
}