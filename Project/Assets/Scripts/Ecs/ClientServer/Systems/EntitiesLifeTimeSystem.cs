using Fabros.EcsModules.Tick.ClientServer.Components;
using Game.Ecs.ClientServer.Components;
using Flow.EcsLite;
using XFlow.EcsLite;
using XFlow.Utils;

namespace Game.Ecs.ClientServer.Systems
{
    public class EntitiesLifeTimeSystem : IEcsRunSystem
    {
        public void Run(EcsSystems systems)
        {
            var world = systems.GetWorld();
            var filter = world
                .Filter<LifeTimeComponent>()
                .End();
            var poolLifeTime = world.GetPool<LifeTimeComponent>();
            var tick = world.GetUnique<TickComponent>().Value.Value;

            foreach (var entity in filter)
            {
                var lifeTimeComponent = poolLifeTime.Get(entity);
                if (tick < lifeTimeComponent.destroyTick)
                    continue;

                entity.EntityAddComponent<DestroyComponent>(world);
            }
        }
    }
}