using Flow.EcsLite;
using XFlow.Ecs.Client.Components;
using XFlow.EcsLite;
using XFlow.Modules.Box2D.ClientServer.Components;

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
                poolLerp.Add(entity).value = 0.5f;
            }
        }

        public void Init(EcsSystems systems)
        {
            world = systems.GetWorld();
            filter = world.Filter<Box2DBodyComponent>().Exc<LerpComponent>().End();
            poolLerp = world.GetPool<LerpComponent>();
        }
    }
}