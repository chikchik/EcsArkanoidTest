using Game.Ecs.ClientServer.Components;
using XFlow.EcsLite;

namespace Game.Ecs.ClientServer.Systems
{
    public class HitEntityDestructionSystem : IEcsRunSystem, IEcsInitSystem
    {
        private EcsWorld _world;
        public void Init(EcsSystems systems)
        {
            _world = systems.GetWorld();
        }

        public void Run(EcsSystems systems)
        {
            var filterHits = _world.Filter<BulletHitComponent>().End();
            foreach (var hit in filterHits)
            {
                _world.DelEntity(hit);
            }
        }
    }
}