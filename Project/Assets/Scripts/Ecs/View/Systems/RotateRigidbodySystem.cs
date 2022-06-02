using Fabros.Ecs.Client.Components;
using Fabros.EcsModules.Box2D.ClientServer.Components;
using Fabros.EcsModules.Tick.Other;
using Game.Ecs.Client.Components;
using Game.Ecs.ClientServer.Components;
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
                .Filter<Box2DRigidbodyComponent>()
                .Inc<TransformComponent>()
                .Inc<Rotation2DComponent>()
                .Exc<UnitComponent>()
                .End();

            var poolTransform = world.GetPool<TransformComponent>();
            var poolLerp = world.GetPool<LerpComponent>();
            var poolRotation = world.GetPool<Rotation2DComponent>();

            foreach (var entity in filter)
            {
                var transform = poolTransform.Get(entity).Transform;
                var angle = poolRotation.Get(entity).Angle;
                
                var eulerAngle = transform.eulerAngles;

                var viewAngle = eulerAngle.y;
                
                var lerp = poolLerp.GetNullable(entity)?.value??1f;
                viewAngle = Mathf.LerpAngle(viewAngle, Mathf.Rad2Deg * -angle, lerp);

                //viewAngle = Mathf.Rad2Deg * -angle;

                eulerAngle.y = viewAngle;//Mathf.Rad2Deg * -angle;
                
                //var ang = eulerAngle

                transform.eulerAngles = eulerAngle;   
            }
        }
    }
}
