using Game.ClientServer;
using Game.Ecs.ClientServer.Components;

using UnityEngine;
using XFlow.Ecs.ClientServer.Components;
using XFlow.EcsLite;
using XFlow.Modules.Box2D.ClientServer.Components;
using XFlow.Modules.Tick.Other;
using XFlow.Utils;

namespace Game.Ecs.ClientServer.Systems
{
    public class MoveToTargetPositionSystem : IEcsRunSystem, IEcsInitSystem
    {
        private EcsPool<TargetPositionComponent> poolTargetPosition;
        private EcsPool<PositionComponent> poolPosition;
        private EcsPool<MovingComponent> poolMoving;
        private EcsPool<CantMoveComponent> poolCantMove;
        private EcsPool<LookDirectionComponent> poolLookDirection;
        private EcsPool<AverageSpeedComponent> poolAverageSpeed;
        private EcsPool<Box2DLinearVelocityComponent> poolLinearVelocity;
        private EcsWorld world;

        private const float _distanceTolerance = 0.1f;

        public void Init(EcsSystems systems)
        {
            world = systems.GetWorld();
            poolTargetPosition = world.GetPool<TargetPositionComponent>();
            poolPosition = world.GetPool<PositionComponent>();
            poolLookDirection = world.GetPool<LookDirectionComponent>();
            poolMoving = world.GetPool<MovingComponent>();
            poolCantMove = world.GetPool<CantMoveComponent>();
            poolAverageSpeed = world.GetPool<AverageSpeedComponent>();
            poolLinearVelocity = world.GetPool<Box2DLinearVelocityComponent>();
        }

        public void Run(EcsSystems systems)
        {
            var filter = world
                .Filter<TargetPositionComponent>()
                .Inc<PositionComponent>().Inc<Box2DBodyComponent>()
                .End();

            var deltaTime = world.GetDeltaSeconds();

            foreach (var entity in filter)
            {
                if (poolCantMove.Has(entity))
                {
                    poolLinearVelocity.GetRef(entity).Value = Vector2.zero;
                    continue;
                }

                var targetPositionComponent = poolTargetPosition.Get(entity);
                var positionComponent = poolPosition.Get(entity);

                var directionToTarget = (targetPositionComponent.Value - positionComponent.Value);
                var direction2D = new Vector2(directionToTarget.x, directionToTarget.z);

                //too close to target
                if (direction2D.magnitude < _distanceTolerance)
                {
                    poolLinearVelocity.GetRef(entity).Value = Vector2.zero;
                    poolTargetPosition.Del(entity);
                    poolMoving.Del(entity);
                    continue;
                }

                var speed = poolAverageSpeed.GetNullable(entity)?.Value ?? 1.0f;
                poolLinearVelocity.GetRef(entity).Value = direction2D.normalized * speed;
                poolLookDirection.GetOrCreateRef(entity).value = directionToTarget.normalized;
                poolMoving.Replace(entity, new MovingComponent());
            }
        }

    }
}