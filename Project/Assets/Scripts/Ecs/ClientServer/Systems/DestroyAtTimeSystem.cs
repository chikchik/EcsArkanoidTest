using Game.Ecs.ClientServer.Components;
using XFlow.Ecs.ClientServer.Utils;
using XFlow.EcsLite;
using XFlow.Modules.Tick.Other;
#if CLIENT
#endif

namespace Game.Ecs.ClientServer.Systems
{
    public class DestroyAtTimeSystem : IEcsRunSystem
    {
        public void Run(EcsSystems systems)
        {
            var world = systems.GetWorld();
            var tm = world.GetTime();
            
            var filter = world.Filter<DestroyAtTimeComponent>().End();

            var poolDestroyAt = world.GetPool<DestroyAtTimeComponent>();
            foreach (var entity in filter)
            {
                if (poolDestroyAt.Get(entity).Time > tm)
                    continue;
                world.Log($"time is out bullet {entity}");                             
                world.MarkEntityAsDeleted(entity);
            }
        }
    }
}