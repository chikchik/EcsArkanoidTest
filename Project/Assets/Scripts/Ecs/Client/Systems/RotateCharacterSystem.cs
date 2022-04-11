using Game.Ecs.Client.Components;
using Game.Ecs.ClientServer.Components;
using Leopotam.EcsLite;
using UnityEngine;

namespace Game.Ecs.Client.Systems
{
    public class RotateCharacterSystem : IEcsRunSystem
    {
        public void Run(EcsSystems systems)
        {
            var world = systems.GetWorld();
            var filter = world
                .Filter<UnitComponent>()
                .Inc<MoveDirectionComponent>()
                .Inc<TransformComponent>()
                .End();

            var poolTransform = world.GetPool<TransformComponent>();
            var poolMoveDirection = world.GetPool<MoveDirectionComponent>();

            foreach (var entity in filter)
            {
                ref var transformComponent = ref poolTransform.GetRef(entity);
                var moveDirectionComponent = poolMoveDirection.Get(entity);

                if (Mathf.Approximately(moveDirectionComponent.value.magnitude, 0)) continue;

                transformComponent.transform.forward = moveDirectionComponent.value.normalized;
            }
        }
    }
}