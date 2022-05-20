using Fabros.EcsModules.Base.Components;
using Game.Ecs.ClientServer.Components;
using Leopotam.EcsLite;
using UnityEngine;

namespace Game.Ecs.ClientServer.Systems
{
    public class MoveToTargetPositionSystem : IEcsRunSystem
    {
        public void Run(EcsSystems systems)
        {
            var world = systems.GetWorld();
            var filter = world
                .Filter<TargetPositionComponent>()
                .Inc<PositionComponent>()
                .End();
            
            var poolTargetPosition = world.GetPool<TargetPositionComponent>();
            var poolPosition = world.GetPool<PositionComponent>();
            var poolMoveDirection = world.GetPool<MoveDirectionComponent>();
            var poolLookDirection = world.GetPool<LookDirectionComponent>();
            var poolMoving = world.GetPool<MovingComponent>();

            foreach (var entity in filter)
            {
                var targetPositionComponent = poolTargetPosition.Get(entity);
                var positionComponent = poolPosition.Get(entity);

                var direction = targetPositionComponent.Value - positionComponent.value;
                var distance = direction.magnitude;
                var moveDirection = new Vector3
                {
                    x = direction.normalized.x,
                    y = 0,
                    z = direction.normalized.z
                };

                if (poolLookDirection.Has(entity))
                    poolLookDirection.GetRef(entity).value = moveDirection;

                if (distance < 0.1f)
                {
                    poolTargetPosition.Del(entity);
                    poolMoving.Del(entity);
                    poolMoveDirection.Del(entity);
                }
                else
                {
                    poolMoveDirection.Replace(entity).value = moveDirection;
                    poolMoving.Replace(entity, new MovingComponent());
                }
            }
        }
    }
}