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
        
        
        EcsPool<TransformComponent> _poolTransform;
        EcsPool<AverageSpeedComponent> _poolAverage;
        EcsPool<LerpComponent> _poolLerp;
        EcsPool<PositionComponent> _poolPosition;
        
        public void Init(EcsSystems systems)
        {
            
            _world = systems.GetWorld();
            
            _poolTransform = _world.GetPool<TransformComponent>();
            _poolPosition = _world.GetPool<PositionComponent>();
            _poolLerp = _world.GetPool<LerpComponent>();
            _poolAverage = _world.GetPool<AverageSpeedComponent>();
            
            _filter = _world
                .Filter<TransformComponent>()
                .Inc<PositionComponent>()
                .End();
            
            foreach (var entity in _filter)
            {
                var transform = _poolTransform.Get(entity).Transform;
                var targetPosition = _poolPosition.Get(entity).value;

                transform.position = targetPosition;
            }
        }
        
        public void Run(EcsSystems systems)
        {
            var poolMainPlayer = _world.GetPool<IsMainPlayerComponent>();
            
            var dt = Time.deltaTime;
            var lerpScale = 12;

            foreach (var entity in _filter)
            {
                var transform = _poolTransform.Get(entity).Transform;
                var targetPosition = _poolPosition.Get(entity).value;
                
                var lerp = _poolLerp.GetNullable(entity)?.value??1f;
                
                //transform.position = Vector3.Lerp(transform.position, targetPosition, lerp);
                //transform.position = targetPosition;

                if (_poolAverage.Has(entity))
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