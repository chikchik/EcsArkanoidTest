using Fabros.EcsModules.Base.Components;
using Game.Ecs.ClientServer.Components;
using Game.Fabros.Net.ClientServer.Ecs.Components;
using Leopotam.EcsLite;
using UnityEngine;

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