using Game.ClientServer;
using Game.Ecs.ClientServer.Components;

using Game.ClientServer.Services;
using UnityEngine;
using XFlow.EcsLite;
using XFlow.Modules.Box2D.ClientServer.Api;
using XFlow.Modules.Box2D.ClientServer.Components;
using XFlow.Modules.Fire.ClientServer.Components;
using XFlow.Modules.Tick.Other;
using XFlow.Utils;
using Random = System.Random;

namespace Game.Ecs.ClientServer.Systems
{
    public class ButtonCustomSystem : IEcsRunSystem
    {
        private Random _rnd = new Random(1);


        private float NextFloat(float left, float right)
        {
            return (float)_rnd.NextDouble() * (right - left) + left;
        }

        
        public void Run(EcsSystems systems)
        {
            var world = systems.GetWorld();
            
            var filter = world
                .Filter<ButtonStateComponent>()
                .Inc<ButtonCustomComponent>()
                .Inc<ButtonStateChangedComponent>()
                .End();

            var poolCustom = world.GetPool<ButtonCustomComponent>();
            var poolPressed = world.GetPool<ButtonPressedComponent>();
            
            foreach (var buttonEntity in filter)
            {
                var custom = poolCustom.Get(buttonEntity);
                if (!poolPressed.Has(buttonEntity))
                    continue;
                
                if (custom.Spawn)
                {
                    var botEntity = UnitService.CreateUnitEntity(world);
                    botEntity.EntityAdd<AIPlayerComponent>(world);

                    botEntity.EntityAdd<FireComponent>(world) = new FireComponent
                    {
                        size = 1,
                        endTime = world.GetTime() + 4,
                        destroyEntity = false
                    };
                }

                if (custom.Shake)
                {
                    var bodies = world.Filter<Box2DBodyComponent>().End();
                    foreach (var bodyEntity in bodies)
                    {
                        var b2d = bodyEntity.EntityGet<Box2DBodyComponent>(world).BodyReference;
                        var dir = new Vector2(NextFloat(-1,1), NextFloat(-1,1));
                        dir.Normalize();
                        Box2DApiSafe.ApplyForce(b2d, dir * NextFloat(5,20), new Vector2(0.4f,0));
                            
                        var dir2 = new Vector2(NextFloat(-1,1), NextFloat(-1,1));
                        dir2.Normalize();
                            
                        Box2DApiSafe.ApplyForceToCenter(b2d, dir2 * NextFloat(50,100));
                    }
                }
            }
        }
    }
}