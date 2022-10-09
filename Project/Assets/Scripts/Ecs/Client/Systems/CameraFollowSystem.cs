using Game.ClientServer.Services;
using Game.Ecs.Client.Components;
using Game.UI;
using UnityEngine;
using XFlow.Ecs.Client.Components;
using XFlow.EcsLite;
using XFlow.Net.ClientServer;

namespace Game.Ecs.Client.Systems
{
    public class CameraFollowSystem : IEcsRunSystem
    {
        private readonly Vector3 _cameraOffset = new Vector3(-4, 8, 4) * 1.5f;

        private Camera _camera;
        public CameraFollowSystem(Camera camera)
        {
            this._camera = camera;
        }
        
        public void Run(EcsSystems systems)
        {
            var world = systems.GetWorld();
            
            if (!ClientBaseServices.TryGetControlledEntity(world, out int controlledEntity))
                return;
            
            var poolTransform = world.GetPool<TransformComponent>();
            var deltaTime = Time.deltaTime;
            
            //var controlledEntity = ApplyInputWorldService.GetControlledEntity(world, clientPlayerComponent.entity);
            var dist = 1f;
            //if (controlledEntity != clientPlayerComponent.entity)
            //    dist = 2.5f;

            var targetEntityTransform = poolTransform.Get(controlledEntity).Transform;
            var targetPosition = targetEntityTransform.position + _cameraOffset*dist;

            _camera.transform.position = Vector3.Lerp(
                _camera.transform.position,
                targetPosition,
                2f * deltaTime);
        }
    }
}