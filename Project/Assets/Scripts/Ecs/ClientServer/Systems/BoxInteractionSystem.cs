using Fabros.Ecs.Utils;
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

            foreach (var entity in filter) HandleBoxInteraction(world, entity);
        }

        private void HandleBoxInteraction(EcsWorld world, int actionEntity)
        {
            var filter = world
                .Filter<BoxComponent>()
                .Inc<InteractableComponent>()
                .End();

            var poolBox = world.GetPool<BoxComponent>();
            var poolInteractable = world.GetPool<InteractableComponent>();

            foreach (var entity in filter)
            {
                ref var interactableComponent = ref poolInteractable.GetRef(entity);
                ref var boxComponent = ref poolBox.GetRef(entity);

                if (!interactableComponent.canInteract || boxComponent.isOpened) continue;

                boxComponent.isOpened = true;
                interactableComponent.isInteractable = false;

                world.DelEntity(actionEntity);

                CreateObjectiveEvent(world, entity);
            }
        }

        private void CreateObjectiveEvent(EcsWorld world, int entity)
        {
            var createObjectiveEventEntity = world.NewEntity();
            ref var createObjectiveEventComponent =
                ref createObjectiveEventEntity.EntityAddComponent<CreateObjectiveEventComponent>(world);

            createObjectiveEventComponent.text = $"loot box {entity}";
        }
    }
}