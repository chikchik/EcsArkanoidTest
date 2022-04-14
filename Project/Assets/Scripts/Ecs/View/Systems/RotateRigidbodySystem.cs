using Game.Ecs.Client.Components;
using Game.Ecs.ClientServer.Components;
using Game.Ecs.ClientServer.Components.Physics;
using Leopotam.EcsLite;
using UnityEngine;

namespace Game.Ecs.Client.Systems
{
    public class RotateRigidbodySystem : IEcsRunSystem
    {
        public void Run(EcsSystems systems)
        {
            var world = systems.GetWorld();
            var filter = world
                .Filter<RigidbodyComponent>()
                .Inc<TransformComponent>()
                .Exc<UnitComponent>()
                .End();

            var poolTransform = world.GetPool<TransformComponent>();
            var poolRigidbody = world.GetPool<RigidbodyComponent>();

            foreach (var entity in filter)
            {
                var transform = poolTransform.Get(entity).transform;
                var angle = poolRigidbody.Get(entity).angle;
                
                var eulerAngle = transform.eulerAngles;
                eulerAngle.y = Mathf.Rad2Deg * -angle;

                transform.eulerAngles = eulerAngle;   
            }
        }
    }
}
