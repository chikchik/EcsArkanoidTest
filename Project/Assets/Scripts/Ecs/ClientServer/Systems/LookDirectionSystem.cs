using Game.Ecs.ClientServer.Components;

using XFlow.Ecs.ClientServer.Components;
using XFlow.EcsLite;

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
                var dir = poolMoveDirection.Get(entity).Value;
                if (dir.sqrMagnitude > 0.001f)
                {
                    poolLookDirection.GetOrCreateRef(entity).Value = dir.normalized;
                }
            }
        }
    }
}