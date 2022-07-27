using Fabros.Ecs.ClientServer.Components;
using Fabros.Ecs.Utils;
using Fabros.EcsModules.Tick.Other;
using Game.ClientServer;
using Game.Ecs.ClientServer.Components;
using Flow.EcsLite;
using UnityEngine;
using XFlow.EcsLite;

namespace Game.Ecs.ClientServer.Systems
{
    public class MoveToTargetPositionSystem : IEcsRunSystem, IEcsInitSystem
    {
        EcsPool<TargetPositionComponent> poolTargetPosition;
        EcsPool<PositionComponent> poolPosition;
        EcsPool<MovingComponent> poolMoving;
        EcsPool<CantMoveComponent> poolCantMove;
        EcsPool<LookDirectionComponent> poolLookDirection;
        EcsPool<AverageSpeedComponent> poolAverageSpeed;
        EcsWorld world;
        
        public void Init(EcsSystems systems)
        {
            world = systems.GetWorld();
            poolTargetPosition = world.GetPool<TargetPositionComponent>();
            poolPosition = world.GetPool<PositionComponent>();
            poolLookDirection = world.GetPool<LookDirectionComponent>();
            poolMoving = world.GetPool<MovingComponent>();
            poolCantMove = world.GetPool<CantMoveComponent>();
            poolAverageSpeed = world.GetPool<AverageSpeedComponent>();
        }

        private void ReachedTarget(int entity, Vector3 targetPosition)
        {
            poolPosition.GetRef(entity).value = targetPosition;
            poolTargetPosition.Del(entity);
            poolMoving.Del(entity);
        }
        
        public void Run(EcsSystems systems)
        {
            var filter = world
                .Filter<TargetPositionComponent>()
                .Inc<PositionComponent>()
                .End();

            var deltaTime = world.GetDeltaSeconds();

            foreach (var entity in filter)
            {
                if (poolCantMove.Has(entity))
                    continue;
                
                var targetPositionComponent = poolTargetPosition.Get(entity);
                var positionComponent = poolPosition.Get(entity);

                var directionToTarget = (targetPositionComponent.Value - positionComponent.value).WithY(0);

                //too close to target
                if (directionToTarget.magnitude < 0.00001f)
                {
                    ReachedTarget(entity, targetPositionComponent.Value);
                    continue;
                }
                
                poolMoving.Replace(entity, new MovingComponent());
                
                var speed = poolAverageSpeed.GetNullable(entity)?.Value ?? 1.0f;
                var step = directionToTarget.normalized * deltaTime * speed;
                
                if (step.magnitude < directionToTarget.magnitude)
                {
                    poolPosition.GetRef(entity).value += step;
                    poolLookDirection.GetOrCreateRef(entity).value = step.normalized;
                    continue;
                }
                
                ReachedTarget(entity, targetPositionComponent.Value);
            }
        }

    }
}