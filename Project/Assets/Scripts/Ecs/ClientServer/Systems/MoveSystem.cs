using Fabros.Ecs.Utils;
using Fabros.EcsModules.Base.Components;
using Fabros.EcsModules.Tick.Other;
using Game.Ecs.Client.Components;
using Game.Ecs.ClientServer.Components;
using Leopotam.EcsLite;
using UnityEngine;

namespace Game.Ecs.ClientServer.Systems
{
    public struct MovingComponent
    {
    }

    public class MoveSystem : IEcsRunSystem
    {
        public void Run(EcsSystems systems)
        {
            var world = systems.GetWorld();
            var filter = world
                .Filter<MoveDirectionComponent>()
                .Inc<PositionComponent>()
                .Inc<SpeedComponent>()
                .End();
            var deltaTime = world.GetDeltaSeconds();
            var poolPosition = world.GetPool<PositionComponent>();
            var poolMoveDirection = world.GetPool<MoveDirectionComponent>();
            var poolSpeed = world.GetPool<SpeedComponent>();
            var poolMoving = world.GetPool<MovingComponent>();
            

            foreach (var entity in filter)
            {
                ref var positionComponent = ref poolPosition.GetRef(entity);
                var moveDirectionComponent = poolMoveDirection.Get(entity);
                var speedComponent = poolSpeed.Get(entity);

                var speed = entity.EntityGetComponent<AverageSpeedComponent>(world).value.magnitude;
                //var dir = moveDirectionComponent.value * deltaTime * speedComponent.speed;
                var dir = moveDirectionComponent.value * deltaTime * speed;//speedComponent.speed;
                positionComponent.value += dir;
                
                if (dir.magnitude > 0)
                    poolMoving.Replace(entity, new MovingComponent());
                else
                    poolMoving.Del(entity);

               // var animator = entity.EntityGet<AnimatorComponent>(world).animator;
              //  var clip = animator.GetCurrentAnimatorClipInfo(0)[0].clip;
                //var speed = clip.averageSpeed;
            }
        }
    }
}