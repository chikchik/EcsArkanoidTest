using Game.Ecs.ClientServer.Components.Inventory;

using UnityEngine;
using XFlow.Ecs.Client.Components;
using XFlow.EcsLite;

namespace Game.Ecs.Client.Systems.Inventory
{
    public class InventoryImageSystem : IEcsRunSystem
    {
        public void Run(EcsSystems systems)
        {
            var world = systems.GetWorld();
            var filter = world
                .Filter<InventorySlotComponent>()
                .End();
            var poolInventoryItem = world.GetPool<InventoryItemComponent>();
            var poolInventorySlots = world.GetPool<InventorySlotComponent>();
            var poolImage = world.GetPool<ImageComponent>();

            foreach (var entity in filter)
            {
                ref var inventorySlotComponent = ref poolInventorySlots.GetRef(entity);
                ref var imageComponent = ref poolImage.GetRef(entity);
                var slotImage = imageComponent.Image;

                if (inventorySlotComponent.hasItem && TryGetItemBySlotId(
                        inventorySlotComponent.slotId,
                        world,
                        poolInventoryItem,
                        out var inventoryItemComponent))
                {
                    slotImage.enabled = true;
                    slotImage.sprite = LoadSprite(inventoryItemComponent.itemName);
                    continue;
                }

                slotImage.enabled = false;
            }
        }

        private bool TryGetItemBySlotId(
            int slotId,
            EcsWorld world,
            EcsPool<InventoryItemComponent> poolInventoryItem,
            out InventoryItemComponent inventoryItemComponent)
        {
            var filter = world
                .Filter<InventoryItemComponent>()
                .End();

            foreach (var entity in filter)
            {
                ref var component = ref poolInventoryItem.GetRef(entity);
                if (component.slotId == slotId)
                {
                    inventoryItemComponent = component;
                    return true;
                }
            }

            inventoryItemComponent = default;
            return false;
        }

        private Sprite LoadSprite(string name)
        {
            var sprite = Resources.Load<Sprite>(name);

            return sprite;
        }
    }
}