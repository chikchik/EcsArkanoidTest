using System;
using Fabros.EcsModules.Tick.Other;
using Game.Ecs.ClientServer.Components;
using Flow.EcsLite;

namespace Game.Ecs.ClientServer.Systems
{
    public class GateSystem : IEcsRunSystem
    {
        public void Run(EcsSystems systems)
        {
            var world = systems.GetWorld();
            var filter = world
                .Filter<GateComponent>()
                .Inc<ButtonLinkComponent>()
                .Inc<ProgressComponent>()
                .End();
            var poolButtonLink = world.GetPool<ButtonLinkComponent>();
            var poolButtonPressed = world.GetPool<ButtonPressedComponent>();
            var poolProgress = world.GetPool<ProgressComponent>();
            var poolOpened = world.GetPool<GateOpenedComponent>();

            var deltaTime = world.GetDeltaSeconds();

            foreach (var gateEntity in filter)
            {
                if (poolOpened.Has(gateEntity))
                    continue;

                var buttons = poolButtonLink.Get(gateEntity).Entities;
                
                if (!AreAllButtonsPressed(buttons, poolButtonPressed))
                    continue;

                var progressComponent = poolProgress.Get(gateEntity);
               // var speedComponent = poolSpeed.Get(gateEntity);

                var progress = progressComponent.progress + deltaTime;

                if (progress >= 1f)
                    poolOpened.Add(gateEntity);

                poolProgress.ReplaceIfChanged(gateEntity, new ProgressComponent{progress = Math.Clamp(progress, 0, 1)});
            }
        }

        private bool AreAllButtonsPressed(int[] entities, EcsPool<ButtonPressedComponent> poolButton)
        {
            for (var i = 0; i < entities.Length; i++)
            {
                if (!poolButton.Has(entities[i]))
                    return false;
            }

            return true;
        }
    }
}