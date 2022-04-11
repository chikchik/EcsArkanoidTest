using System;
using Fabros.EcsModules.Tick.Other;
using Game.Ecs.ClientServer.Components;
using Leopotam.EcsLite;

namespace Game.Ecs.ClientServer.Systems
{
    public class ButtonActivationSystem : IEcsRunSystem
    {
        public void Run(EcsSystems systems)
        {
            var world = systems.GetWorld();
            var filter = world
                .Filter<ButtonComponent>()
                .Inc<InteractableComponent>()
                .Inc<ProgressComponent>()
                .End();
            var poolInteractable = world.GetPool<InteractableComponent>();
            var poolButton = world.GetPool<ButtonComponent>();
            var poolProgress = world.GetPool<ProgressComponent>();
            var poolSpeed = world.GetPool<SpeedComponent>();
            var poolButtonPushCompleted = world.GetPool<ButtonPushCompleted>();
            var deltaTime = world.GetDeltaSeconds();

            foreach (var entity in filter)
            {
                ref var buttonComponent = ref poolButton.GetRef(entity);
                ref var progressComponent = ref poolProgress.GetRef(entity);
                var interactableComponent = poolInteractable.Get(entity);
                var speedComponent = poolSpeed.Get(entity);

                buttonComponent.isActivated = interactableComponent.canInteract;

                var direction = buttonComponent.isActivated ? 1f : -1f;
                var progress = progressComponent.progress + speedComponent.speed * deltaTime * direction;
                progressComponent.progress = Math.Clamp(progress, 0, 1);
            }
        }
    }
}