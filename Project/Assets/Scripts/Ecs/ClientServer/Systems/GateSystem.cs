using System;
using Fabros.EcsModules.Tick.Other;
using Game.Ecs.ClientServer.Components;
using Leopotam.EcsLite;

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
            var poolGate = world.GetPool<GateComponent>();
            var poolButtonLink = world.GetPool<ButtonLinkComponent>();
            var poolButton = world.GetPool<ButtonComponent>();
            var poolProgress = world.GetPool<ProgressComponent>();
            var poolSpeed = world.GetPool<SpeedComponent>();
            var poolOpened = world.GetPool<GateOpenedComponent>();

            var deltaTime = world.GetDeltaSeconds();

            foreach (var entity in filter)
            {
                //var gateComponent = poolGate.Get(entity);
                if (poolOpened.Has(entity))
                    continue;

                if (!CanOpenGate(entity, poolButtonLink, poolButton))
                    continue;

                var progressComponent = poolProgress.Get(entity);
                var speedComponent = poolSpeed.Get(entity);

                var progress = progressComponent.progress + speedComponent.speed * deltaTime;

                if (progress >= 1f)
                    poolOpened.Add(entity);

                poolProgress.ReplaceIfChanged(entity, new ProgressComponent{progress = Math.Clamp(progress, 0, 1)});
            }
        }

        private bool CanOpenGate(int gateEntity,
            EcsPool<ButtonLinkComponent> poolButtonLick,
            EcsPool<ButtonComponent> poolButton)
        {
            var buttonIds = poolButtonLick.Get(gateEntity).buttonIds;

            for (var i = 0; i < buttonIds.Length; i++)
            {
                var buttonComponent = poolButton.Get(buttonIds[i]);

                if (!buttonComponent.isActivated) return false;
            }

            return true;
        }
    }
}