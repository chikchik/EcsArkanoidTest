using Game.Ecs.ClientServer.Components;
using Flow.EcsLite;
using UnityEngine;
using XFlow.Ecs.ClientServer.Components;
using XFlow.EcsLite;
using XFlow.Net.ClientServer.Ecs.Components;

namespace Game.Ecs.ClientServer.Systems
{
    public class MoveByProgressSystem : IEcsRunSystem
    {
        public void Run(EcsSystems systems)
        {
            var world = systems.GetWorld();
            var filter = world
                .Filter<PositionComponent>()
                .Inc<MoveInfoComponent>()
                .Inc<ProgressComponent>()
                .Exc<PlayerComponent>()
                .End();
            var poolPosition = world.GetPool<PositionComponent>();
            var poolMoveInfo = world.GetPool<MoveInfoComponent>();
            var poolProgress = world.GetPool<ProgressComponent>();

            foreach (var entity in filter)
            {
                var moveInfoComponent = poolMoveInfo.Get(entity);
                var progressComponent = poolProgress.Get(entity);

                poolPosition.ReplaceIfChanged(entity, new PositionComponent{value = Vector3.Lerp(
                    moveInfoComponent.startPoint,
                    moveInfoComponent.endPoint,
                    progressComponent.progress)});
            }
        }
    }
}