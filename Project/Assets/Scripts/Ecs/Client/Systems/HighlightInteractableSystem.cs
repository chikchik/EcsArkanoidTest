using System;
using Game.Ecs.Client.Components;
using Game.Ecs.ClientServer.Components;
using Game.Ecs.ClientServer.Systems;
using Game.View;

using UnityEngine;
using XFlow.EcsLite;

namespace Game.Ecs.Client.Systems
{
    public class HighlightInteractableSystem : IEcsRunSystem
    {
        /*
        private const int HIGHLIGHT_RADIUS = 3;

        public void Run(EcsSystems systems)
        {
            var world = systems.GetWorld();

            if (!world.HasUnique<ClientPlayerComponent>()) return;

            var global = world.GetUniqueRef<ClientViewComponent>().Global;
            var poolInteractable = world.GetPool<InteractableComponent>();
            var poolPosition = world.GetPool<PositionComponent>();
            var poolTransform = world.GetPool<TransformComponent>();
            var clientPlayerEntity = world.GetUnique<ClientPlayerComponent>().entity;
            var playerPosition = poolPosition.Get(clientPlayerEntity).value;
            var gridPosition = new Vector2Int
            {
                x = Convert.ToInt32(playerPosition.x),
                y = Convert.ToInt32(playerPosition.z)
            };

            HandleHighlightInteractable(world, gridPosition, global.highlightView, poolInteractable, poolTransform);
        }


        private void HandleHighlightInteractable(
            EcsWorld world,
            Vector2Int playerGridPosition,
            HighlightView highlightView,
            EcsPool<InteractableComponent> poolInteractable,
            EcsPool<TransformComponent> poolTransform)
        {
            var interactableEntities =
                InteractionSystem.GetNearestEntitiesWithComponent<InteractableComponent>(world, playerGridPosition,
                    HIGHLIGHT_RADIUS);

            highlightView.highLightObject.SetActive(false);

            foreach (var entity in interactableEntities)
            {
                var interactableComponent = poolInteractable.Get(entity);
                var interactablePosition = poolTransform.Get(entity).transform.position;
                interactablePosition.y = 0;

                if (interactableComponent.canInteract)
                {
                    highlightView.transform.position = interactablePosition;
                    highlightView.highLightObject.SetActive(true);
                    break;
                }
            }
        }*/
        public void Run(EcsSystems systems)
        {
            
        }
    }
}