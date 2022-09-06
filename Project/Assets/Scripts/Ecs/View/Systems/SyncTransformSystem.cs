using Game.Ecs.ClientServer.Components;

using UnityEngine;
using XFlow.Ecs.Client.Components;
using XFlow.Ecs.ClientServer.Components;
using XFlow.EcsLite;
using XFlow.Net.ClientServer.Ecs.Components;

namespace Game.Ecs.View.Systems
{
    public class SyncTransformSystem : IEcsRunSystem, IEcsInitSystem
    {
        EcsFilter _filter;
        EcsWorld _world;
        
        public SyncTransformSystem()
        {
        }
        
        
        public void Init(EcsSystems systems)
        {
            _world = systems.GetWorld();
            _filter = _world
                .Filter<TransformComponent>()
                .Inc<PositionComponent>()
                .End();
        }
        
        public void Run(EcsSystems systems)
        {
            var poolTransform = _world.GetPool<TransformComponent>();
            var poolPosition = _world.GetPool<PositionComponent>();
            var poolLerp = _world.GetPool<LerpComponent>();
            var poolAverage = _world.GetPool<AverageSpeedComponent>();
            var poolMainPlayer = _world.GetPool<IsMainPlayerComponent>();
            
            var dt = Time.deltaTime;
            var lerpScale = 12;

            foreach (var entity in _filter)
            {
                var transform = poolTransform.Get(entity).Transform;
                var targetPosition = poolPosition.Get(entity).value;
                
                var lerp = poolLerp.GetNullable(entity)?.value??1f;
                
                //transform.position = Vector3.Lerp(transform.position, targetPosition, lerp);
                //transform.position = targetPosition;

                if (poolAverage.Has(entity))
                {
                    //poolPosition.GetRef(entity).value = transform.position;

                    var position = transform.position;

                    /*
                    if (poolMainPlayer.Has(entity))
                    {
                        if (singlePlayer)
                            poolPosition.GetRef(entity).value = position;
                    }
                    else
                    {*/
                        var err = position - targetPosition;
                        if (err.magnitude > 0.1f)
                        {
                            //Debug.Log($"qq {err.magnitude}");
                            transform.position = Vector3.Lerp(transform.position, targetPosition, 0.25f);
                        }
                    //}
                }
                else
                {
                    transform.position = Vector3.Lerp(transform.position, targetPosition, lerp);                    
                }
            }
        }

    }
}