using Game.Ecs.Client.Components;
using Game.Ecs.ClientServer.Components;
using Leopotam.EcsLite;

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
                ref var collectableComponent = ref poolCollectable.GetRef(entity);
                ref var collectableTargetComponent = ref poolCollectableTarget.GetRef(entity);

                collectableTargetComponent.targetObject.SetActive(!collectableComponent.isCollected);
            }
        }
    }
}