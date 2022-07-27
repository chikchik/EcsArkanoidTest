using Flow.EcsLite;
using Game.Ecs.ClientServer.Components;
using UnityEngine;
using XFlow.Ecs.ClientServer.Components;
using XFlow.EcsLite;

namespace Game.Ecs.ClientServer.Systems
{
    public class MovePathSystem : IEcsRunSystem
    {
        public void Run(EcsSystems systems)
        {
            var world = systems.GetWorld();
            var filter = world
                .Filter<MovePathComponent>()
                .Inc<PositionComponent>()
                .End();

            var poolMovePath = world.GetPool<MovePathComponent>();
            var poolPosition = world.GetPool<PositionComponent>();
            
            foreach (var entity in filter)
            {
                ref var pathComponent = ref poolMovePath.GetRef(entity);
                var dest = pathComponent.path[pathComponent.current];
                var pos = poolPosition.Get(entity).value;

                var dir = dest - pos;
                if (dir.magnitude < 0.001f)
                {
                    pathComponent.current += 1;
                    if (pathComponent.current > pathComponent.path.Length)
                    {
                        pathComponent.current = 0;
                    }
                }
            }
        }
    }
}