
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
                var dest = pathComponent.Path[pathComponent.Current];
                var pos = poolPosition.Get(entity).Value;

                var dir = dest - pos;
                if (dir.magnitude < 0.001f)
                {
                    pathComponent.Current += 1;
                    if (pathComponent.Current > pathComponent.Path.Length)
                    {
                        pathComponent.Current = 0;
                    }
                }
            }
        }
    }
}