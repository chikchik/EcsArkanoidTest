using Fabros.EcsModules.Tick.Other;
using Game.Ecs.ClientServer.Components.Input;
using Leopotam.EcsLite;

namespace Game.Fabros.Net.ClientServer.Ecs.Systems
{
    /**
     * система удаляет гарантированно устаревший input, в которых указан input 
     */
    public class DeleteOutdatedInputEntitiesSystem : IEcsRunSystem
    {
        public void Run(EcsSystems systems)
        {
            
            var inputWorld = systems.GetWorld("input");

            var filter = inputWorld.Filter<InputTickComponent>().End();
            var pool = inputWorld.GetPool<InputTickComponent>();
            var tick = systems.GetWorld().GetTick();
            
            foreach (var entity in filter)
            {
                if (pool.Get(entity).Tick < tick)
                {
                    inputWorld.DelEntity(entity);
                }
            }
        }
    }
}