using Game.ClientServer;
using Game.ClientServer.Physics;
using Game.Ecs.ClientServer.Components.Physics;
using Leopotam.EcsLite;

namespace Game.Ecs.ClientServer.Systems.Physics
{
    public class InitPhysicsSystem : IEcsWorldChangedSystem
    {
        public void WorldChanged(EcsWorld world)
        {
            if (!world.HasUnique<PhysicsWorldComponent>())
            {
                ref var physicsWorldComponent = ref world.AddUnique<PhysicsWorldComponent>();
                physicsWorldComponent.worldReference = Box2DPhysics.CreateWorld(Config.GRAVITY);   
            }
        }
    }
}
