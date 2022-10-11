using Game.Ecs.ClientServer.Components;
using XFlow.Ecs.ClientServer.Utils;
using XFlow.EcsLite;
using XFlow.Modules.Tick.Other;
using XFlow.Utils;
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
                world.Log($"entity time is out {entity.e2name(world)}");                             
                world.MarkEntityAsDeleted(entity);
            }
        }
    }
}