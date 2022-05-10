using Fabros.EcsModules.Base.Components;
using Fabros.EcsModules.Tick.Other;
using Game.Ecs.ClientServer.Components;
using Leopotam.EcsLite;

namespace Game.Ecs.ClientServer.Systems
{
    public class MoveSystem : IEcsRunSystem
    {
        public void Run(EcsSystems systems)
        {
            var world = systems.GetWorld();
            var filter = world
                .Filter<MoveDirectionComponent>()
                .Inc<PositionComponent>()
                .Inc<SpeedComponent>()
                .Exc<PlayerComponent>()
                .Exc<AIPlayerComponent>()
                .End();
            var deltaTime = world.GetDeltaSeconds();
            var poolPosition = world.GetPool<PositionComponent>();
            var poolMoveDirection = world.GetPool<MoveDirectionComponent>();
            var poolSpeed = world.GetPool<SpeedComponent>();
            
            foreach (var entity in filter)
            {
                ref var positionComponent = ref poolPosition.GetRef(entity);
                var moveDirectionComponent = poolMoveDirection.Get(entity);
                var speedComponent = poolSpeed.Get(entity);

                positionComponent.value += moveDirectionComponent.value * deltaTime * speedComponent.speed;
            }
        }
    }
}