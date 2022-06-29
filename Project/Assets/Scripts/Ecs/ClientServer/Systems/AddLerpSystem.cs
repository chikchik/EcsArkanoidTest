using Fabros.Ecs.ClientServer.Components;
using Fabros.Ecs.Utils;
using Fabros.EcsModules.Box2D.ClientServer.Components;
using Game.Ecs.ClientServer.Components;
using Leopotam.EcsLite;

namespace Game.Ecs.ClientServer.Systems
{
    public class AddLerpSystem : IEcsInitSystem, IEcsRunSystem
    {
        private EcsWorld world;
        private EcsFilter filter;
        private EcsPool<LerpComponent> poolLerp;
        
        public void Run(EcsSystems systems)
        {
            foreach (var entity in filter)
            {
                if (entity.EntityHas<LerpComponent>(world))
                    continue;
                
                poolLerp.Add(entity).value = 0.5f;
            }
        }

        public void Init(EcsSystems systems)
        {
            world = systems.GetWorld();
            filter = world.Filter<PositionComponent>().IncAdded<Box2DBodyComponent>().End();
            poolLerp = world.GetPool<LerpComponent>();
        }
    }
}