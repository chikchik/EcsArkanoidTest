using Fabros.Ecs.ClientServer.Components;
using Flow.EcsLite;
using Game.Ecs.ClientServer.Components;

namespace Game.Ecs.Client.Systems
{
    public class FollowSystem : IEcsInitSystem, IEcsRunSystem
    {
        private EcsWorld _world;
        
        public void Init(EcsSystems systems)
        {
            _world = systems.GetWorld();
        }

        public void Run(EcsSystems systems)
        {
            var filterFollow = _world.Filter<FollowComponent>().End();

            var poolFollow = _world.GetPool<FollowComponent>();
            var poolTransform = _world.GetPool<PositionComponent>();

            foreach (var entity in filterFollow)
            {
                var follow = poolFollow.Get(entity);
                if (!follow.Entity.Unpack(_world, out var entityToFollow))
                {
                    continue;
                }

                if (!poolTransform.Has(entityToFollow))
                {
                    continue;
                }

                poolTransform.GetOrCreateRef(entity).value = poolTransform.Get(entityToFollow).value;
            }
        }
    }
}