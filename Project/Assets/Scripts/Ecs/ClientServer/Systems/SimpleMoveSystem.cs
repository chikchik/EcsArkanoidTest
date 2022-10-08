using Game.Ecs.ClientServer.Components;

using XFlow.Ecs.ClientServer.Components;
using XFlow.EcsLite;
using XFlow.Modules.Box2D.ClientServer.Api;
using XFlow.Modules.Box2D.ClientServer.Components;
using XFlow.Modules.Tick.Other;
using XFlow.Utils;

namespace Game.Ecs.ClientServer.Systems
{
    public class SimpleMoveSystem : IEcsRunSystem, IEcsInitSystem
    {
        EcsFilter filter;
        EcsWorld world;
        

        public void Init(EcsSystems systems)
        {
            world = systems.GetWorld();
            filter = world
                .Filter<MoveSimpleDirectionComponent>()
                .Inc<PositionComponent>()
                .Inc<TimeComponent>()
                .End();
        }
        
        public void Run(EcsSystems systems)
        {
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
                var pos = poolPosition.Get(entity).Value;

                var ret = new Box2DApiInternal.RaycastOutputReturnType();
                Box2DApiInternal.RayCast(b2world, pos, direction, ref ret, 10);
                //if (ret == )
                    
                var time = poolTime.Get(entity).Value;
                if (time > world.GetTime())
                {
                    
                    poolPosition.GetRef(entity).Value += direction * deltaTime;
                }
                else
                {
                    if (entity.EntityHas<DestroyWhenTimeIsOutComponent>(world))
                    {
                        world.MarkEntityAsDeleted(entity);
                    }
                }
            }
        }
    }
}