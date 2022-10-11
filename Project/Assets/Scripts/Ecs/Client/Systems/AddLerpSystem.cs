using XFlow.Ecs.Client.Components;
using XFlow.EcsLite;
using XFlow.Modules.Box2D.ClientServer.Components;

namespace Game.Ecs.Client.Systems
{
    public class AddLerpSystem : IEcsInitSystem, IEcsRunSystem
    {
        private EcsWorld _world;
        private EcsFilter _filter;
        private EcsPool<LerpComponent> _poolLerp;
        
        public void Run(EcsSystems systems)
        {
            foreach (var entity in _filter)
            {
                _poolLerp.Add(entity).Value = 0.5f;
            }
        }

        public void Init(EcsSystems systems)
        {
            _world = systems.GetWorld();
            _filter = _world.Filter<Box2DBodyComponent>().Exc<LerpComponent>().End();
            _poolLerp = _world.GetPool<LerpComponent>();
        }
    }
}