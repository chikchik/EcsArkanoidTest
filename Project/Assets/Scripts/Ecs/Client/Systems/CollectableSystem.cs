using Game.Ecs.Client.Components;
using Game.Ecs.ClientServer.Components;

using XFlow.EcsLite;

namespace Game.Ecs.Client.Systems
{
    public class CollectableSystem : IEcsRunSystem
    {
        public void Run(EcsSystems systems)
        {
            var world = systems.GetWorld();
            var filter = world
                .Filter<CollectableComponent>()
                .Inc<CollectableTargetComponent>()
                .End();
            var poolCollectable = world.GetPool<CollectableComponent>();
            var poolCollectableTarget = world.GetPool<CollectableTargetComponent>();

            foreach (var entity in filter)
            {
                var collectableComponent = poolCollectable.Get(entity);
                var collectableTargetComponent = poolCollectableTarget.Get(entity);

                collectableTargetComponent.GameObject.SetActive(!collectableComponent.IsCollected);
            }
        }
    }
}