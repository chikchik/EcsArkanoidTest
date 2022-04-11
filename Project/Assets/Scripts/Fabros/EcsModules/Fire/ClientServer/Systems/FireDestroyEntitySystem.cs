using Game.Ecs.ClientServer.Components;
using Game.Ecs.ClientServer.Components.Objective;
using Game.Fabros.EcsModules.Fire.ClientServer.Components;
using Leopotam.EcsLite;

namespace Game.Fabros.EcsModules.Fire.ClientServer.Systems
{
    public class FireDestroyEntitySystem : IEcsRunSystem
    {
        public void Run(EcsSystems systems)
        {
            var world = systems.GetWorld();
            var filter = world
                .Filter<FireComponent>().Inc<BurnedOutComponent>()
                .End();
            foreach (var entity in filter) world.DelEntity(entity);
        }
    }
}