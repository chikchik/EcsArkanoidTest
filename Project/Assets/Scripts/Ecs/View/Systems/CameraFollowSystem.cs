using Game.Ecs.Client.Components;

using Game.ClientServer;
using Game.ClientServer.Services;
using UnityEngine;
using XFlow.Ecs.Client.Components;
using XFlow.EcsLite;

namespace Game.Ecs.View.Systems
{
    public class CameraFollowSystem : IEcsRunSystem
    {
        private readonly Vector3 CameraOffset = new Vector3(-4, 8, 4) * 1.5f;

        private Camera camera;
        public CameraFollowSystem(Camera camera)
        {
            this.camera = camera;
        }
        
        public void Run(EcsSystems systems)
        {
            var world = systems.GetWorld();

            if (!world.HasUnique<ClientPlayerComponent>()) return;

            var poolTransform = world.GetPool<TransformComponent>();
            var deltaTime = Time.deltaTime;

            var clientPlayerComponent = world.GetUnique<ClientPlayerComponent>();
            

            var controlledEntity = ApplyInputWorldService.GetControlledEntity(world, clientPlayerComponent.entity);
            var dist = 1f;
            if (controlledEntity != clientPlayerComponent.entity)
                dist = 2.5f;

            var targetEntityTransform = poolTransform.Get(controlledEntity).Transform;
            var targetPosition = targetEntityTransform.position + CameraOffset*dist;

            camera.transform.position = Vector3.Lerp(
                camera.transform.position,
                targetPosition,
                2f * deltaTime);
        }
    }
}