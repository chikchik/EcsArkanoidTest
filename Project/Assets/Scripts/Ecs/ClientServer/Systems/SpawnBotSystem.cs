using Fabros.Ecs.Utils;
using Game.ClientServer;
using Game.Ecs.ClientServer.Components;
using Game.Fabros.EcsModules.Fire.ClientServer.Components;
using Leopotam.EcsLite;

namespace Game.Ecs.ClientServer.Systems
{
    public class SpawnBotSystem : IEcsInitSystem
    {
        public void Init(EcsSystems systems)
        {
            var world = systems.GetWorld();
            var botEntity = UnitService.CreateUnitEntity(world);
            botEntity.EntityAddComponent<AIPlayerComponent>(world);

            botEntity.EntityAdd<FireComponent>(world) = new FireComponent
            {
                size = 1,
                endTime = 5,
                destroyEntity = false
            };
        }
    }
}