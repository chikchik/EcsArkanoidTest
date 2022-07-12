using Game.Ecs.ClientServer.Components;
using Flow.EcsLite;

namespace Game.Ecs.ClientServer.Systems
{
    public class LookDirectionSystem : IEcsRunSystem
    {
        public void Run(EcsSystems systems)
        {
            var world = systems.GetWorld();
            var filter = world.Filter<MoveDirectionComponent>().End();
            var poolMoveDirection = world.GetPool<MoveDirectionComponent>();
            var poolLookDirection = world.GetPool<LookDirectionComponent>();
            foreach (var entity in filter)
            {
                var dir = poolMoveDirection.Get(entity).value;
                if (dir.sqrMagnitude > 0.001f)
                {
                    poolLookDirection.GetOrCreateRef(entity).value = dir.normalized;
                }
            }
        }
    }
}