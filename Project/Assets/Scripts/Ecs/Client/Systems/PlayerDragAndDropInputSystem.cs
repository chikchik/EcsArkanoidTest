using System.Collections.Generic;
using Game.Ecs.Client.Components;
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
        
        private List<RaycastResult> _raycastResults = new List<RaycastResult>();
        
        public void Init(EcsSystems systems)
        {
            _world = systems.GetWorld();
        }
        
        public void Run(EcsSystems systems)
        {
            if (Input.GetMouseButtonUp(0) && _draggable != null)
            {
                _draggable = null;
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
            
            if (!Input.GetMouseButton(0))
                return;
            
            PointerEventData pointer = new PointerEventData(EventSystem.current);
            pointer.position = Input.mousePosition;
 
            EventSystem.current.RaycastAll(pointer, _raycastResults);

            if (_raycastResults.Count == 0)
                return;

            var go = _raycastResults[0];
            //skip ui
            if (go.gameObject.GetComponentInParent<Canvas>() != null)
                return;

            var draggable = go.gameObject.GetComponentInParent<BoxView>();
            if (draggable == null)
                return;
            
            if (!draggable.transform.TryGetLinkedEntity(_world, out int entity))
                return;
            
            _draggable = draggable.transform;
            _controlService.BeginDrag(entity);
            _world.GetOrCreateUniqueRef<MouseDownHandledComponent>();
        }
    }
}