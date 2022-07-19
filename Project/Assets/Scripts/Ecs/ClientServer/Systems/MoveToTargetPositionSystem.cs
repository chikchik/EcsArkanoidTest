using Fabros.Ecs.ClientServer.Components;
using Fabros.Ecs.Utils;
using Fabros.EcsModules.Tick.Other;
using Game.ClientServer;
using Game.Ecs.ClientServer.Components;
using Flow.EcsLite;
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
            //var poolMoveDirection = world.GetPool<MoveDirectionComponent>();
            var poolLookDirection = world.GetPool<LookDirectionComponent>();
            var poolMoving = world.GetPool<MovingComponent>();
            var poolCantMove = world.GetPool<CantMoveComponent>();

            var deltaTime = world.GetDeltaSeconds();

            foreach (var entity in filter)
            {
                if (poolCantMove.Has(entity))
                    continue;
                
                var targetPositionComponent = poolTargetPosition.Get(entity);
                var positionComponent = poolPosition.Get(entity);

                var direction = (targetPositionComponent.Value - positionComponent.value).WithY(0);
                var distance = direction.magnitude;

                if (distance < 0.1f)
                {
                    poolTargetPosition.Del(entity);
                    poolMoving.Del(entity);
                    //poolMoveDirection.Del(entity);
                }
                else
                {
                    poolMoving.Replace(entity, new MovingComponent());
                    direction.Normalize();
                    
                    var speed = entity.EntityGetNullable<AverageSpeedComponent>(world)?.Value ?? 1.0f;

                    //poolMoveDirection.Replace(entity).value = direction;

                    var dir = direction * deltaTime * speed; //speedComponent.speed;
                    poolPosition.GetRef(entity).value += dir;
                    
                    if (dir.magnitude > 0.001f)
                    {
                        //if delta and speed not 0
                        poolLookDirection.GetOrCreateRef(entity).value = dir.normalized;
                    }
                }
            }
        }
    }
}