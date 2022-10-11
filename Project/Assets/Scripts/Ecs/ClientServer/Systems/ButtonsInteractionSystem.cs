using System;
using System.Collections.Generic;
using Game.ClientServer;
using Game.Ecs.ClientServer.Components;

using XFlow.Ecs.ClientServer.Components;
using XFlow.EcsLite;
using XFlow.Modules.Box2D.ClientServer.Components;
using XFlow.Modules.Grid.Other;
using XFlow.Modules.Tick.Other;
using XFlow.Utils;

namespace Game.Ecs.ClientServer.Systems
{
    public class ButtonsInteractionSystem : IEcsRunSystem
    {
        private List<int> _entities = new List<int>(32);
        
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
            var poolStateChanged = world.GetPool<ButtonStateChangedComponent>();
            var poolBody = world.GetPool<Box2DBodyComponent>();
            var poolBullet = world.GetPool<BulletComponent>();
            
            var deltaTime = world.GetDeltaSeconds();


            foreach (var buttonEntity in filter)
            {
                var pos = buttonEntity.EntityGet<PositionComponent>(world).Value;

                world.GetNearestEntities(buttonEntity, pos, 0.5f, ref _entities, entity =>
                    !poolBullet.Has(entity) &&(poolUnit.Has(entity) || poolBody.Has(entity)));
                
                var pressed = _entities.Count > 0;
                poolButton.ReplaceIfChanged(buttonEntity, new ButtonComponent{isActivated = pressed});


                if (pressed)
                {
                    if (poolPressed.Has(buttonEntity))
                        poolStateChanged.Del(buttonEntity);
                    else
                        poolStateChanged.GetOrCreateRef(buttonEntity);
                    
                    poolPressed.GetOrCreateRef(buttonEntity);
                }
                else
                {
                    if (!poolPressed.Has(buttonEntity))
                        poolStateChanged.Del(buttonEntity);
                    else
                        poolStateChanged.GetOrCreateRef(buttonEntity);
                    
                    poolPressed.Del(buttonEntity);
                }
                    
                var progressComponent = poolProgress.Get(buttonEntity);
                
                var direction = pressed ? 1f : -1f;
                var progress = progressComponent.progress + deltaTime * direction * 4;
                
                poolProgress.ReplaceIfChanged(buttonEntity, new ProgressComponent{progress = Math.Clamp(progress, 0, 1)});
            }
        }
    }
}