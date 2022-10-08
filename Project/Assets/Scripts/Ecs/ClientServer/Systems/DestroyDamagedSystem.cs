using XFlow.Ecs.ClientServer.Components;
using XFlow.EcsLite;

namespace Game.Ecs.ClientServer.Systems
{
    public class DestroyDamagedSystem : IEcsRunSystem, IEcsInitSystem
    {
        private EcsWorld _world;
        private EcsFilter _filter;
        private EcsPool<HpComponent> _poolHealth;
        
        public void Init(EcsSystems systems)
        {
            _world = systems.GetWorld();
            _filter = _world.Filter<HpComponent>().End();
            
            _poolHealth = _world.GetPool<HpComponent>();
        }

        public void Run(EcsSystems systems)
        {
            foreach (var entity in _filter)
            {
                if (_poolHealth.Get(entity).Value > 0)
                    continue;
                _world.MarkEntityAsDeleted(entity);
            }
        }
    }
}