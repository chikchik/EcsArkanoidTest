using Fabros.EcsModules.Tick.Other;
using Game.Ecs.Client.Components;
using Leopotam.EcsLite;
using UnityEngine;

namespace Game.Ecs.Client.Systems
{
    public class CameraFollowSystem : IEcsRunSystem
    {
        private readonly Vector3 CameraOffset = new Vector3(-4, 8, 4) * 2;

        public void Run(EcsSystems systems)
        {
            var world = systems.GetWorld();

            if (!world.HasUnique<ClientPlayerComponent>()) return;

            var poolTransform = world.GetPool<TransformComponent>();
            var deltaTime = world.GetDeltaSeconds();

            var clientPlayerComponent = world.GetUnique<ClientPlayerComponent>();
            var targetEntityTransform = poolTransform.GetRef(clientPlayerComponent.entity).transform;
            var targetPosition = targetEntityTransform.position + CameraOffset;
            var camera = world.GetUniqueRef<ClientViewComponent>().Camera;

            camera.transform.position = Vector3.Lerp(
                camera.transform.position,
                targetPosition,
                2f * deltaTime);
        }
    }
}