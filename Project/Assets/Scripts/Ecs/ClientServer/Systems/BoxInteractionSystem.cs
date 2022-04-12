using Fabros.Ecs.Utils;
using Game.ClientServer;
using Game.Ecs.ClientServer.Components;
using Game.Ecs.ClientServer.Components.Events;
using Leopotam.EcsLite;

namespace Game.Ecs.ClientServer.Systems
{
    public class BoxInteractionSystem : IEcsRunSystem
    {
        public void Run(EcsSystems systems)
        {
            var world = systems.GetWorld();
            var filter = world
                .Filter<InteractionEventComponent>()
                .End();

            foreach (var entity in filter) 
                HandleBoxInteraction(world, entity);
        }

        private void HandleBoxInteraction(EcsWorld world, int actionEntity)
        {
            var filter = world
                .Filter<BoxComponent>()
                .Inc<InteractableComponent>()
                .Exc<OpenedBoxComponent>()
                .End();

            var poolBox = world.GetPool<BoxComponent>();
            var poolBoxOpened = world.GetPool<OpenedBoxComponent>();
            var poolInteractable = world.GetPool<InteractableComponent>();

            foreach (var entity in filter)
            {
                ref var interactableComponent = ref poolInteractable.GetRef(entity);
                ref var boxComponent = ref poolBox.GetRef(entity);

                poolBoxOpened.Add(entity);

                world.DelEntity(actionEntity);
                poolInteractable.Del(entity);

                ObjectiveService.Triggered(world, entity);
            }
        }
    }
}