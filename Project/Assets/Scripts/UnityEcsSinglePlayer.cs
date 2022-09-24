using System.Collections.Generic;
using Game.Ecs.Client.Components;
using Game.Ecs.ClientServer.Components;
using Game.Ecs.View.Systems;

using Game.ClientServer.Services;
using Game.Ecs.ClientServer.Components.Inventory;
using Game.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using XFlow.EcsLite;
using XFlow.Modules.Inventory.ClientServer.Components;
using XFlow.Net.ClientServer;
using XFlow.Net.ClientServer.Ecs.Components;
using XFlow.Utils;
using Zenject;

namespace Game
{
    public class UnityEcsSinglePlayer: MonoBehaviour
    {
        [Inject] private Camera _camera;
        [Inject] private Joystick _joystick;
        
        [Inject] private DiContainer _diContainer;
        [Inject] private EcsWorld _world;
       
        [Inject] 
        private PlayerControlService _controlService;

        [Inject]
        private SingleGame _game;
        

        private int _unitEntity = -1;
        private int _playerId = 1;

        public void Start()
        {
            _game.PreInit();
            
            ClientServices.InitializeNewWorldFromScene(_world);
            
            _unitEntity = UnitService.CreateUnitEntity(_world);
            _world.AddUnique(new ClientPlayerComponent{ entity = _unitEntity});
            
            _world.AddUnique(new MainPlayerIdComponent{value = _playerId});
            _unitEntity.EntityAdd<PlayerComponent>(_world).id = _playerId;

            var inventory = _world.NewEntity();
            inventory.EntityAdd<InventoryComponent>(_world).SlotCapacity = 10;
            
            var trash = _world.NewEntity();
            trash.EntityAdd<InventoryComponent>(_world).SlotCapacity = 10;

            _unitEntity.EntityAdd<InventoryLinkComponent>(_world).Inventory = _world.PackEntity(inventory);
            _unitEntity.EntityAdd<TrashLinkComponent>(_world).Trash = _world.PackEntity(trash);
            
            _game.Init();
        }

        public static bool IsPointerOverUIObject()
        {
            PointerEventData pointer = new PointerEventData(EventSystem.current);
            pointer.position = Input.mousePosition;
 
            List<RaycastResult> raycastResults = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointer, raycastResults);
 
            if (raycastResults.Count > 0)
            {
                if (raycastResults[0].gameObject.GetComponentInParent<Canvas>() != null)
                    return true;

            }

            return false;
        }
        
        public static void CheckInput(Camera camera, Joystick joystick, PlayerControlService controlService)
        {
            void MoveDir(float hor, float ver)
            {
                var forward = -Vector3.Cross(Vector3.up,camera.transform.right) ;
                forward.y = 0;
                forward.Normalize();

                var right = camera.transform.right;
                right.y = 0;
                right.Normalize();

                var dir = forward * ver + right * hor;
                
                controlService.MoveToDirection(dir);
            }
           
            
            if (Input.GetMouseButtonDown(0) && !IsPointerOverUIObject())
            {
                //EventSystem.current.
                var ray = camera.ScreenPointToRay(Input.mousePosition);
                var plane = new Plane(new Vector3(0, 1, 0), 0);
                plane.Raycast(ray, out var dist);

                var point = ray.GetPoint(dist);
                
                controlService.MoveToPoint(point);
                return;
            }

            var hor = Input.GetAxis("Horizontal");
            var ver = Input.GetAxis("Vertical");
            //Debug.Log(joystick.Direction);
            
            if (Mathf.Abs(hor) > 0.01f || Mathf.Abs(ver) > 0.01f)
            {
                MoveDir(hor, ver);
            }
            else if (joystick.Direction.magnitude > 0.01f)
            {
                MoveDir(joystick.Direction.x, joystick.Direction.y);
            }else
            {
                controlService.StopMoveToDirection();
            }
        }
        public void Update()
        {
            CheckInput(_camera, _joystick, _controlService);
            _game.Update();
        }


        

        public void FixedUpdate()
        {
            if (!Application.isPlaying)
                return;
            _game.FixedUpdate();
        }
        

        public void OnDrawGizmos()
        {
            if (!Application.isPlaying)
                return;
        
            EcsWorldDebugDraw.Draw(_world);
        }
    }
}