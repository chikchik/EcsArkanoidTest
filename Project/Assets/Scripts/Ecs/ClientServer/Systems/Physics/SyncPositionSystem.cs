using Fabros.EcsModules.Base.Components;
using Game.Ecs.ClientServer.Components.Physics;
using Leopotam.EcsLite;
using UnityEngine;

namespace Game.Ecs.ClientServer.Systems.Physics
{
    public class SyncPhysicsPositionSystem : IEcsRunSystem
    {
        public void Run(EcsSystems systems)
        {
            var world = systems.GetWorld();
            var filter = world
                .Filter<RigidbodyComponent>()
                .Inc<PositionComponent>()
                .End();
            var poolRigidbody = world.GetPool<RigidbodyComponent>();
            var poolPosition = world.GetPool<PositionComponent>();
            
            foreach (var entity in filter)
            {
                var rigidbodyComponent = poolRigidbody.Get(entity);
                ref var positionComponent = ref poolPosition.GetRef(entity);
                // var position = new Vector3(rigidbodyComponent.position.x, positionComponent.value.y, rigidbodyComponent.position.y);
                
                // positionComponent.value = position;
            }
        }
    }
}
