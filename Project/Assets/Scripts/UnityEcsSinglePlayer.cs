using System.Collections.Generic;
using Game.ClientServer;
using Game.Ecs.Client.Components;
using Game.Ecs.ClientServer.Components;
using Game.Ecs.View.Systems;

using Game.ClientServer.Services;
using Game.Ecs.ClientServer.Components.Inventory;
using Game.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using XFlow.Ecs.ClientServer;
using XFlow.EcsLite;
using XFlow.Modules.Inventory.ClientServer.Components;
using XFlow.Modules.Tick.ClientServer.Components;
using XFlow.Modules.Tick.ClientServer.Systems;
using XFlow.Modules.Tick.Other;
using XFlow.Net.Client;
using XFlow.Net.ClientServer;
using XFlow.Net.ClientServer.Ecs.Components;
using XFlow.Net.ClientServer.Ecs.Systems;
using XFlow.Utils;
using Zenject;

namespace Game
{
    public class UnityEcsSinglePlayer: MonoBehaviour
    {
        [Inject] private Camera _camera;
        [Inject] private Joystick _joystick;
        
        [Inject] private EcsWorld _world;
        [Inject(Id = EcsWorlds.Input)] private EcsWorld _inputWorld;
        [Inject(Id = EcsWorlds.Event)] private EcsWorld _eventWorld;
        [Inject(Id = EcsWorlds.Dead)]  private EcsWorld _deadWorld;
        
        [Inject] 
        private PlayerControlService _controlService;
        
        [Inject] 
        private EntityDestroyedListener _entityDestroyedListener;

        [Inject] 
        private CopyToDeadWorldListener _copyToDeadWorldListener;

        [Inject]
        private IEcsSystemsFactory _systemsFactory;
        [Inject]
        private IEcsViewSystemsFactory _viewSystemsFactory;

        private EcsSystems _systems;
        private EcsSystems _viewSystems;
        

        private int _unitEntity = -1;
        private int _playerId = 1;

        public void Start()
        {
            UnityEngine.Physics.autoSimulation = false;
            UnityEngine.Physics2D.simulationMode = SimulationMode2D.Script;

            _systems = new EcsSystems(_world, "systems");
            _systems.AddWorld(_inputWorld, EcsWorlds.Input);
            _systems.AddWorld(_eventWorld, EcsWorlds.Event);
            _systems.AddWorld(_deadWorld, EcsWorlds.Dead);
            
            _systemsFactory.AddNewSystems(_systems, 
                new IEcsSystemsFactory.Settings{AddClientSystems = true, AddServerSystems = true});
            _systems.Add(new TickSystem());
            _systems.Add(new DeleteDestroyedEntitiesSystem());
            
            _systems.PreInit();
            
            
            _world.EntityCreatedListeners.Add(new AllEntitiesAreReliableListener(_world));
            
            ClientServices.InitializeNewWorldFromScene(_world);
            
            _world.EntityDestroyedListeners.Add(_entityDestroyedListener);
            _world.EntityDestroyedListeners.Add(_copyToDeadWorldListener);

            _world.AddUnique<PrimaryWorldComponent>();
            
            _world.AddUnique(new TickDeltaComponent
            {
                Value = new TickDelta((int)(1f/Time.fixedDeltaTime))
            });

            _world.AddUnique(new TickComponent{Value = new Tick(0)});

            _unitEntity = UnitService.CreateUnitEntity(_world);
            _world.AddUnique(new ClientPlayerComponent{ entity = _unitEntity});
            
            
#if UNITY_EDITOR
            //systems.Add(new XFlow.EcsLite.UnityEditor.EcsWorldDebugSystem(bakeComponentsInName:true));
#endif
            

            _systems.Init();
            
            _viewSystems = new EcsSystems(_world, "viewSystems");
            _viewSystemsFactory.AddNewSystems(_viewSystems);
            
            _viewSystems.Init();
            
            _world.AddUnique(new MainPlayerIdComponent{value = _playerId});
            _unitEntity.EntityAdd<PlayerComponent>(_world).id = _playerId;

            var inventory = _world.NewEntity();
            inventory.EntityAdd<InventoryComponent>(_world).SlotCapacity = 10;
            
            var trash = _world.NewEntity();
            trash.EntityAdd<InventoryComponent>(_world).SlotCapacity = 10;

            _unitEntity.EntityAdd<InventoryLinkComponent>(_world).Inventory = _world.PackEntity(inventory);
            _unitEntity.EntityAdd<TrashLinkComponent>(_world).Trash = _world.PackEntity(trash);
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
            _viewSystems.Run();
        }


        

        public void FixedUpdate()
        {
            if (!Application.isPlaying)
                return;
            _systems.Run();
        }
        

        public void OnDrawGizmos()
        {
            if (!Application.isPlaying)
                return;
        
            EcsWorldDebugDraw.Draw(_world);
        }
    }
}