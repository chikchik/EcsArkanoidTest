using Fabros.EcsModules.Tick.Components;
using Game.ClientServer;
using Game.ClientServer.Physics;
using Game.Ecs.ClientServer.Components.Physics;
using Leopotam.EcsLite;

namespace Game.Ecs.ClientServer.Systems.Physics
{
    public class UpdatePhysicsWorldSystem : IEcsRunSystem
    {
        public void Run(EcsSystems systems)
        {
            var world = systems.GetWorld();
            if(!world.HasUnique<PhysicsWorldComponent>()) return;
            
            var physicsWorld = world.GetUnique<PhysicsWorldComponent>();
            var deltaTime = world.GetUnique<TickDeltaComponent>().Value.Seconds;

            Box2DPhysics.UpdateWorld(
                physicsWorld.worldReference, 
                deltaTime, 
                Config.POSITION_ITERATIONS, 
                Config.VELOCITY_ITERATIONS);

            var filter = world
                .Filter<BodyReferenceComponent>()
                .Inc<RigidbodyComponent>()
                .End();
            var bodyReferenceComponent = world.GetPool<BodyReferenceComponent>();
            var poolRigidBodyComponent = world.GetPool<RigidbodyComponent>();
            
            foreach (var entity in filter)
            {
                var bodyReference = bodyReferenceComponent.Get(entity).bodyReference;
                var bodyInfo = Box2DPhysics.GetBodyInfo(bodyReference);
                ref var rigidBodyComponent = ref poolRigidBodyComponent.GetRef(entity);
                rigidBodyComponent.position = bodyInfo.position;
                rigidBodyComponent.linearVelocity = bodyInfo.linearVelocity;
                rigidBodyComponent.angularVelocity = bodyInfo.angularVelocity;
                rigidBodyComponent.angle = bodyInfo.angle;
            }
        }
    }
}
