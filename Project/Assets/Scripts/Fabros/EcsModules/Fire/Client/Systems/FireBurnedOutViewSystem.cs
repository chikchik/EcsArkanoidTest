using Game.Fabros.EcsModules.Fire.ClientServer.Components;
using Flow.EcsLite;

namespace Game.Fabros.EcsModules.Fire.Client.Systems
{
    public class FireBurnedOutViewSystem : IEcsRunSystem
    {
        public void Run(EcsSystems systems)
        {
            var world = systems.GetWorld();
            var filter = world.Filter<BurnedOutComponent>();
        }
    }
}