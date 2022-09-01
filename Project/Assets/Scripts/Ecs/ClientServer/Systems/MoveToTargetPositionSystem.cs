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
        private EcsPool<Box2DRigidbodyComponent> poolRigidBody;
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
            poolRigidBody = world.GetPool<Box2DRigidbodyComponent>();
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
                {
                    poolRigidBody.GetRef(entity).LinearVelocity = Vector2.zero;
                    continue;
                }

                var targetPositionComponent = poolTargetPosition.Get(entity);
                var positionComponent = poolPosition.Get(entity);

                var directionToTarget = (targetPositionComponent.Value - positionComponent.value);
                var direction2D = new Vector2(directionToTarget.x, directionToTarget.z);

                //too close to target
                if (direction2D.magnitude < _distanceTolerance)
                {
                    poolRigidBody.GetRef(entity).LinearVelocity = Vector2.zero;
                    poolTargetPosition.Del(entity);
                    poolMoving.Del(entity);
                    continue;
                }

                var speed = poolAverageSpeed.GetNullable(entity)?.Value ?? 1.0f;
                poolRigidBody.GetRef(entity).LinearVelocity = direction2D.normalized * speed;
                poolLookDirection.GetOrCreateRef(entity).value = directionToTarget.normalized;
                poolMoving.Replace(entity, new MovingComponent());
            }
        }

    }
}