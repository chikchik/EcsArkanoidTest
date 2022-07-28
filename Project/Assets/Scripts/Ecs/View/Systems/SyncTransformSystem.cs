using Game.Ecs.ClientServer.Components;
using Flow.EcsLite;
using UnityEngine;
using XFlow.Ecs.Client.Components;
using XFlow.Ecs.ClientServer.Components;
using XFlow.EcsLite;
using XFlow.Net.ClientServer.Ecs.Components;

namespace Game.Ecs.View.Systems
{
    public class SyncTransformSystem : IEcsRunSystem
    {
        private bool singlePlayer;
        public SyncTransformSystem(bool singlePlayer)
        {
            this.singlePlayer = singlePlayer;
        }
        
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
            
            var dt = Time.deltaTime;
            var lerpScale = 12;

            foreach (var entity in filter)
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