using System;
using System.Collections.Generic;
using Fabros.Ecs.ClientServer.Components;
using Fabros.Ecs.Utils;
using Fabros.EcsModules.Box2D.ClientServer.Components;
using Fabros.EcsModules.Grid.Other;
using Fabros.EcsModules.Tick.Other;
using Game.ClientServer;
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
            var poolPressed = world.GetPool<ButtonPressedComponent>();
            var poolBody = world.GetPool<Box2DRigidbodyComponent>();
            var poolBullet = world.GetPool<BulletComponent>();
            
            var deltaTime = world.GetDeltaSeconds();


            foreach (var buttonEntity in filter)
            {
                var pos = buttonEntity.EntityGet<PositionComponent>(world).value;

                world.GetNearestEntities(buttonEntity, pos, 0.5f, ref entities, entity =>
                    !poolBullet.Has(entity) &&(poolUnit.Has(entity) || poolBody.Has(entity)));
                
                var pressed = entities.Count > 0;
                poolButton.ReplaceIfChanged(buttonEntity, new ButtonComponent{isActivated = pressed});

                if (pressed)
                    poolPressed.GetOrCreateRef(buttonEntity);
                else
                    poolPressed.Del(buttonEntity);
                    
                var progressComponent = poolProgress.Get(buttonEntity);
                
                var direction = pressed ? 1f : -1f;
                var progress = progressComponent.progress + deltaTime * direction * 4;
                
                poolProgress.ReplaceIfChanged(buttonEntity, new ProgressComponent{progress = Math.Clamp(progress, 0, 1)});
            }
        }
    }
}