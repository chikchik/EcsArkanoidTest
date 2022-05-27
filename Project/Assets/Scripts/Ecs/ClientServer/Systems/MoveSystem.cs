using Fabros.Ecs.Utils;
using Fabros.EcsModules.Base.Components;
using Fabros.EcsModules.Tick.Other;
using Game.Ecs.Client.Systems;
using Game.Ecs.ClientServer.Components;
using Game.Ecs.ClientServer.Components.Physics;
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
                .Inc<AverageSpeedComponent>()
                //.Inc<AIPlayerComponent>()
                .End();
            var deltaTime = world.GetDeltaSeconds();
            var poolPosition = world.GetPool<PositionComponent>();
            var poolMoveDirection = world.GetPool<MoveDirectionComponent>();
            var poolSpeed = world.GetPool<SpeedComponent>();
            var poolMoving = world.GetPool<MovingComponent>();

            foreach (var entity in filter)
            {
                var moveDirectionComponent = poolMoveDirection.Get(entity);
                //var speedComponent = poolSpeed.Get(entity);

                var speed = entity.EntityGetComponent<AverageSpeedComponent>(world).Value;
                //var dir = moveDirectionComponent.value * deltaTime * speedComponent.speed;
                var dir = moveDirectionComponent.value * deltaTime * speed; //speedComponent.speed;
                if (dir.sqrMagnitude > 0)
                {
                    poolPosition.GetRef(entity).value += dir;
                    poolMoving.Replace(entity, new MovingComponent());
                }
                else
                    poolMoving.Del(entity);
            }
        }
    }

    public class AddLerpSystem : IEcsRunSystem
    {
        public void Run(EcsSystems systems)
        {
            var world = systems.GetWorld();
            var filter = world.Filter<PositionComponent>().IncAdded<BodyReferenceComponent>().End();
            var poolLerp = world.GetPool<LerpComponent>();
            
            foreach (var entity in filter)
            {
                if (entity.EntityHas<LerpComponent>(world))
                    continue;
                
                poolLerp.Add(entity).value = 0.25f;
            }
        }
    }
}