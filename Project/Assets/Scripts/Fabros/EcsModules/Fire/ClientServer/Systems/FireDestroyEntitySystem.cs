using Game.Fabros.EcsModules.Fire.ClientServer.Components;
using Flow.EcsLite;

namespace Game.Fabros.EcsModules.Fire.ClientServer.Systems
{
    public class FireDestroyEntitySystem : IEcsInitSystem, IEcsRunSystem
    {
        EcsWorld world;
        EcsFilter filter;
        
        public void Init(EcsSystems systems)
        {
            world = systems.GetWorld();
            filter = world
                .Filter<FireComponent>().Inc<BurnedOutComponent>()
                .End();
        }
        
        public void Run(EcsSystems systems)
        {
            foreach (var entity in filter) 
                world.DelEntity(entity);
        }
    }
}