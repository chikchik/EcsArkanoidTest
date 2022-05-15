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
                .End();

            var poolTransform = world.GetPool<TransformComponent>();
            var poolPosition = world.GetPool<PositionComponent>();
            var poolLerp = world.GetPool<LerpComponent>();
            var poolAverage = world.GetPool<AverageSpeedComponent>();
            var poolMainPlayer = world.GetPool<IsMainPlayerComponent>();

            foreach (var entity in filter)
            {
                var transform = poolTransform.Get(entity).transform;
                var targetPosition = poolPosition.Get(entity).value;
                var lerp = poolLerp.GetNullable(entity)?.value ?? 1f;
                
                //transform.position = Vector3.Lerp(transform.position, targetPosition, lerp);
                //transform.position = targetPosition;

                if (poolAverage.Has(entity))
                {
                    //poolPosition.GetRef(entity).value = transform.position;

                    var position = transform.position;

                    if (poolMainPlayer.Has(entity))
                    {
                        world.ReplaceUnique(new RootMotionComponent {Position = transform.position});
                    }
                    else
                    {
                        var err = position - targetPosition;
                        if (err.magnitude > 0.1f)
                        {
                            transform.position = Vector3.Lerp(transform.position, targetPosition, lerp / 5);
                        }
                    }
                }
                else
                {
                    transform.position = Vector3.Lerp(transform.position, targetPosition, lerp);
                    //transform.position = targetPosition;
                }
            }
        }
    }
}