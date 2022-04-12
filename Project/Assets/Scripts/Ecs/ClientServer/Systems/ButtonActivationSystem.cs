using System;
using System.Collections.Generic;
using Fabros.Ecs.Utils;
using Fabros.EcsModules.Base.Components;
using Fabros.EcsModules.Grid.Other;
using Fabros.EcsModules.Tick.Other;
using Game.Ecs.ClientServer.Components;
using Leopotam.EcsLite;

namespace Game.Ecs.ClientServer.Systems
{
    public class ButtonActivationSystem : IEcsRunSystem
    {
        private List<int> entities = new List<int>();
        
        public void Run(EcsSystems systems)
        {
            var world = systems.GetWorld();
            /*
            var filter = world
                .Filter<UnitComponent>()
                .Inc<PositionComponent>()
                .End();
                */
            
            var filter = world
                .Filter<UnitComponent>()
                .Inc<PositionComponent>()
                .End();
            
            var poolProgress = world.GetPool<ProgressComponent>();
            var poolButton = world.GetPool<ButtonComponent>();
            var poolSpeed = world.GetPool<SpeedComponent>();
            var deltaTime = world.GetDeltaSeconds();
            
            foreach (var unitEntity in filter)
            {
                var pos = unitEntity.EntityGet<PositionComponent>(world).value;
                world.GetNearestEntities(pos, 1, ref entities, entity =>
                {
                    return poolButton.Has(entity);
                });

                if (entities.Count > 0)
                {
                    var buttonEntity = entities[0];
                    ref var buttonComponent = ref poolButton.GetRef(buttonEntity);
                    ref var progressComponent = ref poolProgress.GetRef(buttonEntity);
                    var speedComponent = poolSpeed.Get(buttonEntity);
                    
                    
                    buttonComponent.isActivated = true;

                    
                    var direction = buttonComponent.isActivated ? 1f : -1f;
                    var progress = progressComponent.progress + speedComponent.speed * deltaTime * direction;
                    progressComponent.progress = Math.Clamp(progress, 0, 1);
                }
            }
            
            /*
            var world = systems.GetWorld();
            var filter = world
                .Filter<ButtonComponent>()
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

                buttonComponent.isActivated = true;

                var direction = buttonComponent.isActivated ? 1f : -1f;
                var progress = progressComponent.progress + speedComponent.speed * deltaTime * direction;
                progressComponent.progress = Math.Clamp(progress, 0, 1);
            }*/
        }
    }
}