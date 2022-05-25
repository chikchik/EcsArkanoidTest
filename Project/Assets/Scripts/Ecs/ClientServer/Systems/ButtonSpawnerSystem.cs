using Fabros.Ecs.Utils;
using Fabros.EcsModules.Tick.Other;
using Game.ClientServer;
using Game.Ecs.ClientServer.Components;
using Game.Fabros.EcsModules.Fire.ClientServer.Components;
using Leopotam.EcsLite;

namespace Game.Ecs.ClientServer.Systems
{
    public class ButtonSpawnerSystem : IEcsRunSystem
    {
        public void Run(EcsSystems systems)
        {
            var world = systems.GetWorld();
            
            var filter = world
                .Filter<ButtonComponent>()
                .Inc<ButtonSpawnComponent>()
                .IncAdded<ButtonPressedComponent>()
                .End();

            var poolButtonSpawn = world.GetPool<ButtonSpawnComponent>();
            
            foreach (var buttonEntity in filter)
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
        }
    }
}