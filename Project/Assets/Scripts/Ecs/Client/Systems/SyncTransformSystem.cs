using Fabros.EcsModules.Base.Components;
using Game.Ecs.Client.Components;
using Game.Ecs.ClientServer.Components;
using Leopotam.EcsLite;
using UnityEngine;

namespace Game.Ecs.Client.Systems
{
    public class SyncTransformSystem : IEcsRunSystem
    {
        public void Run(EcsSystems systems)
        {
            var world = systems.GetWorld();
            var filter = world
                .Filter<TransformComponent>()
                .Inc<PositionComponent>()
                .Exc<StaticPositionComponent>()
                .End();

            var poolTransform = world.GetPool<TransformComponent>();
            var poolPosition = world.GetPool<PositionComponent>();
            var poolLerp = world.GetPool<LeoLerpComponent>();

            foreach (var entity in filter)
            {
                var transform = poolTransform.GetRef(entity).transform;
                var targetPosition = poolPosition.Get(entity).value;
                var lerp = poolLerp.Has(entity) ? poolLerp.Get(entity).value : 1f;

                transform.position = Vector3.Lerp(transform.position, targetPosition, lerp);
            }
        }
    }
}