using Fabros.Ecs.Client.Components;
using Fabros.EcsModules.Mech.ClientServer.Components;
using Flow.EcsLite;

namespace Fabros.EcsModules.Mech.Client.Systems
{
    public class CreateMechViewSystem: IEcsRunSystem, IEcsInitSystem
    {
        private EcsWorld world;
        private EcsFilter filter;
        public void Init(EcsSystems systems)
        {
            world = systems.GetWorld();
            filter = world.Filter<MechComponent>()
                .Exc<TransformComponent>().End();
        }
        
        public void Run(EcsSystems systems)
        {
            foreach (var entity in filter)
            {
                
            }
        }
    }
}