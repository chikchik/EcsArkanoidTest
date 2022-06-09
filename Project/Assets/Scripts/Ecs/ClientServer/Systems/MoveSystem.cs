using Fabros.Ecs.ClientServer.Components;
using Fabros.Ecs.Utils;
using Fabros.EcsModules.Tick.Other;
using Game.Ecs.Client.Systems;
using Game.Ecs.ClientServer.Components;
using Leopotam.EcsLite;

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
                .Inc<SpeedComponent>()
                .Inc<AverageSpeedComponent>()
                .End();
            
            var deltaTime = world.GetDeltaSeconds();
            var poolPosition = world.GetPool<PositionComponent>();
            var poolMoveDirection = world.GetPool<MoveDirectionComponent>();
            var poolLookDirection = world.GetPool<LookDirectionComponent>();
            //var poolSpeed = world.GetPool<SpeedComponent>();
            var poolMoving = world.GetPool<MovingComponent>();

            foreach (var entity in filter)
            {
                var moveDirectionComponent = poolMoveDirection.Get(entity);
                /*
                if (moveDirectionComponent.value.sqrMagnitude < 0.0001f)
                {
                    poolMoving.Del(entity);
                    poolMoveDirection.Del(entity);
                    continue;
                }*/

                var speed = entity.EntityGetComponent<AverageSpeedComponent>(world).Value;
                var dir = moveDirectionComponent.value * deltaTime * speed; //speedComponent.speed;
                poolPosition.GetRef(entity).value += dir;
                poolMoving.Replace(entity, new MovingComponent());
                poolLookDirection.Replace(entity).value = dir.normalized;
            }
        }
    }
}