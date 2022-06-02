using Fabros.Ecs.ClientServer.Components;
using Fabros.Ecs.Utils;
using Fabros.EcsModules.Box2D.ClientServer.Components;
using Game.Ecs.ClientServer.Components;
using Leopotam.EcsLite;

namespace Game.Ecs.ClientServer.Systems
{
    public class AddLerpSystem : IEcsRunSystem
    {
        public void Run(EcsSystems systems)
        {
            var world = systems.GetWorld();
            var filter = world.Filter<PositionComponent>().IncAdded<Box2DBodyComponent>().End();
            var poolLerp = world.GetPool<LerpComponent>();
            
            foreach (var entity in filter)
            {
                if (entity.EntityHas<LerpComponent>(world))
                    continue;
                
                poolLerp.Add(entity).value = 0.5f;
            }
        }
    }
}