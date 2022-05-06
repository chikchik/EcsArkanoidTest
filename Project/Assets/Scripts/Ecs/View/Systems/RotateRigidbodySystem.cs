using Fabros.EcsModules.Base.Components;
using Game.ClientServer.Physics.Components;
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
                .Inc<RotationComponent>()
                .Exc<UnitComponent>()
                .End();

            var poolTransform = world.GetPool<TransformComponent>();
            // var poolRigidbody = world.GetPool<RigidbodyComponent>();
            var poolRotation = world.GetPool<RotationComponent>();

            foreach (var entity in filter)
            {
                var transform = poolTransform.Get(entity).transform;
                var angle = poolRotation.Get(entity).value;
                
                var eulerAngle = transform.eulerAngles;
                eulerAngle.y = Mathf.Rad2Deg * -angle;

                transform.eulerAngles = eulerAngle;   
            }
        }
    }
}
