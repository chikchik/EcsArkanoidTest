using Fabros.Ecs.Utils;
using Fabros.EcsModules.Box2D.ClientServer.Api;
using Fabros.EcsModules.Box2D.ClientServer.Components;
using Fabros.EcsModules.Tick.Other;
using Game.ClientServer;
using Game.Ecs.ClientServer.Components;
using Game.Fabros.EcsModules.Fire.ClientServer.Components;
using Leopotam.EcsLite;
using UnityEngine;
using Random = System.Random;

namespace Game.Ecs.ClientServer.Systems
{
    public class ButtonCustomSystem : IEcsRunSystem
    {
        
        private Random rnd = new Random(1);


        private float NextFloat(float left, float right)
        {
            return (float)rnd.NextDouble() * (right - left) + left;
        }

        
        public void Run(EcsSystems systems)
        {
            var world = systems.GetWorld();
            
            var filter = world
                .Filter<ButtonComponent>()
                .Inc<ButtonCustomComponent>()
                .IncAdded<ButtonPressedComponent>()
                .End();

            var poolCustom = world.GetPool<ButtonCustomComponent>();
            
            foreach (var buttonEntity in filter)
            {
                var custom = poolCustom.Get(buttonEntity);
                if (custom.Spawn)
                {
                    var botEntity = UnitService.CreateUnitEntity(world);
                    botEntity.EntityAddComponent<AIPlayerComponent>(world);

                    botEntity.EntityAdd<FireComponent>(world) = new FireComponent
                    {
                        size = 1,
                        endTime = world.GetTime() + 10,
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
                        Box2DApi.ApplyForce(b2d, dir * NextFloat(5,20), new Vector2(0.4f,0));
                            
                        var dir2 = new Vector2(NextFloat(-1,1), NextFloat(-1,1));
                        dir2.Normalize();
                            
                        Box2DApi.ApplyForceToCenter(b2d, dir2 * NextFloat(50,100));
                    }
                }
            }
        }
    }
}