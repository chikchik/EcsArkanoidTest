using System;
using System.Collections.Generic;
using Fabros.Ecs.Utils;
using Fabros.EcsModules.Base.Components;
using Fabros.EcsModules.Grid.Other;
using Fabros.EcsModules.Tick.Other;
using Game.ClientServer;
using Game.Ecs.ClientServer.Components;
using Game.Ecs.ClientServer.Components.Physics;
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
            var poolButtonSpawn = world.GetPool<ButtonSpawnComponent>();
            var poolPressed = world.GetPool<ButtonPressedComponent>();
            var poolSpeed = world.GetPool<SpeedComponent>();
            var poolBody = world.GetPool<RigidbodyComponent>();
            
            var deltaTime = world.GetDeltaSeconds();


            foreach (var buttonEntity in filter)
            {
                var pos = buttonEntity.EntityGet<PositionComponent>(world).value;

                world.GetNearestEntities(buttonEntity, pos, 0.5f, ref entities, entity => poolUnit.Has(entity) || poolBody.Has(entity));
                
                var pressed = entities.Count > 0;
                poolButton.ReplaceIfChanged(buttonEntity, new ButtonComponent{isActivated = pressed});

                if (pressed)
                    poolPressed.Replace(buttonEntity);
                else
                    poolPressed.Del(buttonEntity);
                    
                var speedComponent = poolSpeed.Get(buttonEntity);
                var progressComponent = poolProgress.Get(buttonEntity);
                
                var direction = pressed ? 1f : -1f;
                var progress = progressComponent.progress + speedComponent.speed * deltaTime * direction;
                
                poolProgress.ReplaceIfChanged(buttonEntity, new ProgressComponent{progress = Math.Clamp(progress, 0, 1)});
            }
        }
    }
}