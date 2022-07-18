using Fabros.Ecs.ClientServer.Components;
using Fabros.Ecs.Utils;
using Fabros.EcsModules.Mech.ClientServer;
using Game.ClientServer;
using Game.Ecs.ClientServer.Components;
using Game.Fabros.EcsModules.Fire.ClientServer.Components;
using Flow.EcsLite;
using UnityEngine;

namespace Game.Ecs.ClientServer.Systems
{
    public class CustomInitSystem : IEcsInitSystem
    {
        public void Init(EcsSystems systems)
        {
            var service = new MechService();
            var world = systems.GetWorld();
            var mechEntity = service.CreateMechEntity(world);
            mechEntity.EntityGetRef<PositionComponent>(world).value = new Vector3(-5, 0, -5f);

            /*
            var world = systems.GetWorld();
            var botEntity = UnitService.CreateUnitEntity(world);
            botEntity.EntityAddComponent<AIPlayerComponent>(world);

            botEntity.EntityAdd<FireComponent>(world) = new FireComponent
            {
                size = 1,
                endTime = 5,
                destroyEntity = false
            };*/
        }
    }
}