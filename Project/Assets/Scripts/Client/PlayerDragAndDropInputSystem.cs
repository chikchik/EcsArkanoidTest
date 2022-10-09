using System.Collections.Generic;
using Game.View;
using UnityEngine;
using UnityEngine.EventSystems;
using XFlow.Ecs.Client;
using XFlow.EcsLite;
using Zenject;

namespace Game.Client
{
    public class PlayerDragAndDropInputSystem : IEcsRunSystem, IEcsInitSystem
    {
        [Inject]private Camera _camera;
        [Inject]private PlayerControlService _controlService;

        private Transform _draggable;
        private EcsWorld _world;

        private EcsPackedEntity _entity;
        
        public void Init(EcsSystems systems)
        {
            _world = systems.GetWorld();
        }
        
        public void Run(EcsSystems systems)
        {
            if (Input.GetMouseButtonUp(0) && _draggable != null)
            {
                _controlService.EndDrag();
                return;
            }

            if (Input.GetMouseButton(0) && _draggable != null)
            {
                var ray = _camera.ScreenPointToRay(Input.mousePosition);
                var plane = new Plane(new Vector3(0, 1, 0), 0);
                plane.Raycast(ray, out var dist);

                var point = ray.GetPoint(dist);
                
                _controlService.UpdateDrag(point);
                return;
            }
            
            PointerEventData pointer = new PointerEventData(EventSystem.current);
            pointer.position = Input.mousePosition;
 
            List<RaycastResult> raycastResults = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointer, raycastResults);

            if (raycastResults.Count > 0)
            {
                if (raycastResults[0].gameObject.GetComponentInParent<Canvas>() != null)
                    return;

                var go = raycastResults[0].gameObject.GetComponentInParent<BoxView>();
                if (go != null)
                {
                    _draggable = go.transform;
                    if (_draggable.TryGetLinkedEntity(_world, out int entity))
                    {
                        _controlService.BeginDrag(entity);
                    }
                }
            }

            return;

        }

    }
}