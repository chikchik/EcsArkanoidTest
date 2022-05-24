using Fabros.Ecs.Utils;
using Game.ClientServer;
using Game.Ecs.ClientServer.Components;
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
            }
        }
    }
}