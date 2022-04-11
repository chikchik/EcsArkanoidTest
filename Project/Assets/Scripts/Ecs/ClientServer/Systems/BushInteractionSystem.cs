using Fabros.Ecs.Utils;
using Game.Ecs.ClientServer.Components;
using Game.Ecs.ClientServer.Components.Events;
using Leopotam.EcsLite;

namespace Game.Ecs.ClientServer.Systems
{
    public class BushInteractionSystem : IEcsRunSystem
    {
        public void Run(EcsSystems systems)
        {
            var world = systems.GetWorld();
            var filter = world
                .Filter<InteractionEventComponent>()
                .End();

            foreach (var entity in filter) HandleBushInteraction(world, entity);
        }

        private void HandleBushInteraction(EcsWorld world, int actionEntity)
        {
            var filter = world
                .Filter<BushComponent>()
                .Inc<InteractableComponent>()
                .Inc<CollectableComponent>()
                .End();

            var poolBush = world.GetPool<BushComponent>();
            var poolInteractable = world.GetPool<InteractableComponent>();
            var poolCollectable = world.GetPool<CollectableComponent>();

            foreach (var entity in filter)
            {
                ref var interactableComponent = ref poolInteractable.GetRef(entity);
                ref var bushComponent = ref poolBush.GetRef(entity);
                ref var collectableComponent = ref poolCollectable.GetRef(entity);

                if (!interactableComponent.canInteract || collectableComponent.isCollected) continue;

                collectableComponent.isCollected = true;
                interactableComponent.isInteractable = false;

                // InventoryItemAddedEvent(world, bushComponent.name);
            }
        }

        private void InventoryItemAddedEvent(EcsWorld world, string itemName)
        {
            var inventoryItemAddedEventEntity = world.NewEntity();
            ref var inventoryItemAddedEventComponent =
                ref inventoryItemAddedEventEntity.EntityAddComponent<InventoryItemAddedEventComponent>(world);

            inventoryItemAddedEventComponent.itemName = itemName;
            inventoryItemAddedEventComponent.count = 1;
        }
    }
}