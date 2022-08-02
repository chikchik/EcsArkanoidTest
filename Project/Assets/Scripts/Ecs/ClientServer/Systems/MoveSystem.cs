using Game.Ecs.ClientServer.Components;

using XFlow.Ecs.ClientServer.Components;
using XFlow.EcsLite;
using XFlow.Modules.Tick.Other;
using XFlow.Utils;

namespace Game.Ecs.ClientServer.Systems
{
    public class MoveSystem : IEcsRunSystem
    {
        public void Run(EcsSystems systems)
        {
            var world = systems.GetWorld();
            var filter = world
                .Filter<MoveDirectionComponent>()
                .Inc<PositionComponent>()
                .End();
            
            var deltaTime = world.GetDeltaSeconds();
            var poolPosition = world.GetPool<PositionComponent>();
            var poolMoveDirection = world.GetPool<MoveDirectionComponent>();
            var poolLookDirection = world.GetPool<LookDirectionComponent>();
            //var poolSpeed = world.GetPool<SpeedComponent>();
            var poolMoving = world.GetPool<MovingComponent>();
            var poolCantMove = world.GetPool<CantMoveComponent>();

            foreach (var entity in filter)
            {
                if (poolCantMove.Has(entity))
                    continue;
                
                var moveDirectionComponent = poolMoveDirection.Get(entity);

                var speed = entity.EntityGetNullable<AverageSpeedComponent>(world)?.Value??1f;
                var dir = moveDirectionComponent.value * deltaTime * speed; //speedComponent.speed;
                poolPosition.GetRef(entity).value += dir;
                poolMoving.Replace(entity, new MovingComponent());
                poolLookDirection.GetOrCreateRef(entity).value = dir.normalized;
            }
        }
    }
}