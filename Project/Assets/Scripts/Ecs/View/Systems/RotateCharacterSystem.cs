using Game.Ecs.Client.Components;
using Game.Ecs.ClientServer.Components;
using Leopotam.EcsLite;
using UnityEngine;

namespace Game.Ecs.Client.Systems
{
    public class RotateCharacterSystem : IEcsRunSystem
    {
        public void Run(EcsSystems systems)
        {
            var world = systems.GetWorld();
            var filter = world
                .Filter<UnitComponent>()
                .Inc<MoveDirectionComponent>()
                .Inc<TransformComponent>()
                .End();

            var poolTransform = world.GetPool<TransformComponent>();
            var poolLookDirection = world.GetPool<LookDirectionComponent>();
            var poolLerp = world.GetPool<LerpComponent>();
            
            foreach (var entity in filter)
            {
                var transform = poolTransform.Get(entity).transform;
                var direction = poolLookDirection.Get(entity).value;

                if (Mathf.Approximately(direction.magnitude, 0))
                    continue;
                //if (direction.magnitude < 0.99)
                  //  continue;

                var lerp = poolLerp.GetNullable(entity)?.value??1f;
                
                var quat = Quaternion.LookRotation(direction);
                transform.localRotation = Quaternion.Lerp(transform.localRotation, quat, lerp);
                
                //transform.forward = direction.normalized;
            }
        }
    }
}