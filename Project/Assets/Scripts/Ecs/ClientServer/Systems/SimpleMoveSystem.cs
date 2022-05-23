using Fabros.Ecs.Utils;
using Fabros.EcsModules.Base.Components;
using Fabros.EcsModules.Tick.Other;
using Game.Ecs.ClientServer.Components;
using Leopotam.EcsLite;

namespace Game.Ecs.ClientServer.Systems
{
    public class SimpleMoveSystem : IEcsRunSystem
    {
        public void Run(EcsSystems systems)
        {
            var world = systems.GetWorld();
            var filter = world
                .Filter<MoveSimpleDirectionComponent>()
                .Inc<PositionComponent>()
                .Inc<TimeComponent>()
                .End();
            
            var deltaTime = world.GetDeltaSeconds();
            
            var poolPosition = world.GetPool<PositionComponent>();
            var poolMoveDirection = world.GetPool<MoveSimpleDirectionComponent>();
            var poolTime = world.GetPool<TimeComponent>();
            
            foreach (var entity in filter)
            {
                var time = poolTime.Get(entity).time;
                if (time > world.GetTime())
                {
                    var direction = poolMoveDirection.Get(entity).value;
                    poolPosition.GetRef(entity).value += direction * deltaTime;
                }
                else
                {
                    if (entity.EntityHas<DestroyWhenTimeIsOutComponent>(world))
                    {
                        world.DelEntity(entity);
                    }
                }
            }
        }
    }
}