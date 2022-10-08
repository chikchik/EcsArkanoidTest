using Game.Ecs.ClientServer.Components;

using UnityEngine;
using XFlow.Ecs.Client.Components;
using XFlow.Ecs.ClientServer.Components;
using XFlow.EcsLite;
using XFlow.Modules.Box2D.ClientServer.Components;
using XFlow.Utils;

namespace Game.Ecs.View.Systems
{
    public class RotateRigidbodySystem : IEcsRunSystem
    {
        public void Run(EcsSystems systems)
        {
            var world = systems.GetWorld();
            var filter = world
                .Filter<Box2DBodyComponent>()
                .Inc<TransformComponent>()
                .Inc<Rotation2DComponent>()
                .Exc<UnitComponent>()
                .Exc<LookDirectionComponent>()
                .End();

            var poolTransform = world.GetPool<TransformComponent>();
            var poolLerp = world.GetPool<LerpComponent>();
            var poolRotation = world.GetPool<Rotation2DComponent>();

            foreach (var entity in filter)
            {
                var transform = poolTransform.Get(entity).Transform;
                
                var lerp = poolLerp.GetNullable(entity)?.Value??1f;
                
                var destAngle = poolRotation.Get(entity).AngleRadians * -Mathf.Rad2Deg;
                var angle = Mathf.LerpAngle(transform.eulerAngles.y, destAngle, lerp);

                transform.eulerAngles = transform.eulerAngles.WithY(angle);   
            }
        }
    }
}
