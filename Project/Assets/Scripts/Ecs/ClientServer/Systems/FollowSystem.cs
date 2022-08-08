using Game.Ecs.ClientServer.Components;
using XFlow.Ecs.ClientServer.Components;
using XFlow.EcsLite;

namespace Game.Ecs.ClientServer.Systems
{
    public class FollowSystem : IEcsInitSystem, IEcsRunSystem
    {
        private EcsWorld world;
        
        public void Init(EcsSystems systems)
        {
            world = systems.GetWorld();
        }

        public void Run(EcsSystems systems)
        {
            var filterFollow = world.Filter<FollowComponent>().End();

            var poolFollow = world.GetPool<FollowComponent>();
            var poolPosition = world.GetPool<PositionComponent>();

            foreach (var entity in filterFollow)
            {
                var follow = poolFollow.Get(entity);
                if (!follow.Entity.Unpack(world, out var entityToFollow))
                {
                    continue;
                }

                if (!poolPosition.Has(entityToFollow))
                {
                    continue;
                }

                poolPosition.GetOrCreateRef(entity).value = poolPosition.Get(entityToFollow).value;
            }
        }
    }
}