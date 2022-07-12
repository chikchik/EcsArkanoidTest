using Fabros.Ecs.ClientServer.Components;
using Fabros.Ecs.Utils;
using Fabros.EcsModules.Box2D.ClientServer.Api;
using Fabros.EcsModules.Box2D.ClientServer.Components;
using Fabros.EcsModules.Tick.Other;
using Game.Ecs.ClientServer.Components;
using Flow.EcsLite;

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

            var b2world = world.GetUnique<Box2DWorldComponent>().WorldReference;
            
            //Box2DApi.app
            
            
            
            var poolPosition = world.GetPool<PositionComponent>();
            var poolMoveDirection = world.GetPool<MoveSimpleDirectionComponent>();
            var poolTime = world.GetPool<TimeComponent>();
            var poolStartTime = world.GetPool<StartSimpleMoveAtComponent>();
            
            foreach (var entity in filter)
            {
                var startTime = poolStartTime.Get(entity).Time;
                if (startTime > world.GetTime())
                    continue;
                
                var direction = poolMoveDirection.Get(entity).value;
                var pos = poolPosition.Get(entity).value;

                Box2DApi.RaycastOutputReturnType ret = new Box2DApi.RaycastOutputReturnType();
                Box2DApi.RayCast(b2world, pos, direction, ref ret, 10);
                //if (ret == )
                    
                var time = poolTime.Get(entity).time;
                if (time > world.GetTime())
                {
                    
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