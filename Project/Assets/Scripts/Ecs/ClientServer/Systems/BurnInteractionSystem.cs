using Fabros.Ecs.Utils;
using Fabros.EcsModules.Base.Components;
using Fabros.EcsModules.Tick.Other;
using Game.Ecs.ClientServer.Components.Events;
using Game.Fabros.EcsModules.Fire.ClientServer.Components;
using Leopotam.EcsLite;

namespace Game.Ecs.ClientServer.Systems
{
    public class BurnInteractionSystem : IEcsRunSystem
    {
        public void Run(EcsSystems systems)
        {
            /*
            var world = systems.GetWorld();
            var filter = world
                .Filter<InteractionEventComponent>()
                .End();

            foreach (var entity in filter)
            {
                var fire = world.NewEntity();
                fire.EntityAddComponent<FireComponent>(world) = new FireComponent
                {
                    size = 1.5f,
                    startTime = world.GetTime(),
                    endTime = world.GetTime() + 5,
                    destroyEntity = true
                };
                fire.EntityAddComponent<PositionComponent>(world) = entity.EntityGetComponent<PositionComponent>(world);
            }*/
        }
    }
}