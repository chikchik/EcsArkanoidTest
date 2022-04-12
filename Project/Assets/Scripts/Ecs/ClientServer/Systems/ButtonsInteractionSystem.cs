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
    public class ButtonsInteractionSystem : IEcsRunSystem
    {
        private List<int> entities = new List<int>();
        
        public void Run(EcsSystems systems)
        {
            var world = systems.GetWorld();
            
            var filter = world
                .Filter<ButtonComponent>()
                .End();
            
            var poolProgress = world.GetPool<ProgressComponent>();
            var poolUnit = world.GetPool<UnitComponent>();
            var poolButton = world.GetPool<ButtonComponent>();
            var poolSpeed = world.GetPool<SpeedComponent>();
            var deltaTime = world.GetDeltaSeconds();


            foreach (var buttonEntity in filter)
            {
                var pos = buttonEntity.EntityGet<PositionComponent>(world).value;
                world.GetNearestEntities(pos, 0.5f, ref entities, entity => poolUnit.Has(entity));

                var buttonComponent = poolButton.Get(buttonEntity);
                
                var pressed = entities.Count > 0;

                if (buttonComponent.isActivated != pressed)
                {
                    //используем GetRef только если действительно что-то изменилось у кнопки
                    poolButton.GetRef(buttonEntity).isActivated = pressed;
                }

                var speedComponent = poolSpeed.Get(buttonEntity);
                ref var progressComponent = ref poolProgress.GetRef(buttonEntity);
                
                var direction = pressed ? 1f : -1f;
                var progress = progressComponent.progress + speedComponent.speed * deltaTime * direction;
                progressComponent.progress = Math.Clamp(progress, 0, 1);
            }
        }
    }
}