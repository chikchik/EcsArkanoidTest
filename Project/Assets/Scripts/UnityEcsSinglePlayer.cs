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
using XFlow.EcsLite;
using XFlow.Modules.Inventory.Client.Services;
using XFlow.Modules.Inventory.ClientServer.Components;
using XFlow.Modules.Inventory.Demo.UI;
using XFlow.Modules.Tick.ClientServer.Components;
using XFlow.Modules.Tick.ClientServer.Systems;
using XFlow.Modules.Tick.Other;
using XFlow.Net.ClientServer;
using XFlow.Net.ClientServer.Ecs.Components;
using XFlow.Utils;
using Zenject;

namespace Game
{
    public class UnityEcsSinglePlayer: MonoBehaviour
    {
        [Inject] private Camera camera;
        [Inject] private Global global;
        [Inject] private UI.UI ui;
        
        [Inject] private Joystick joystick;
        
        [Inject] private EcsWorld world;
        [Inject(Id = "input")] private EcsWorld inputWorld;
        
        [Inject] 
        private PlayerControlService controlService;
        
        [Inject] 
        private EntityDestroyedListener entityDestroyedListener;

        [Inject]
        private IEcsSystemsFactory systemsFactory;

        private EcsSystems systems;
        private EcsSystems viewSystems;
        

        private int unitEntity = -1;
        private int playerId = 1;

        public void Start()
        {
            UnityEngine.Physics.autoSimulation = false;
            UnityEngine.Physics2D.simulationMode = SimulationMode2D.Script;

            systems = new EcsSystems(world);
            systems.AddWorld(inputWorld, "input");
            ClientServices.InitializeNewWorldFromScene(world);
            
            world.EntityDestroyedListeners.Add(entityDestroyedListener);
            
            world.AddUnique(new TickDeltaComponent
            {
                Value = new TickDelta((int)(1f/Time.fixedDeltaTime))
            });

            world.AddUnique(new TickComponent{Value = new Tick(0)});

            unitEntity = UnitService.CreateUnitEntity(world);
            world.AddUnique(new ClientPlayerComponent{ entity = unitEntity});
            
            
#if UNITY_EDITOR
            systems.Add(new Flow.EcsLite.UnityEditor.EcsWorldDebugSystem(bakeComponentsInName:true));
#endif
           
            
            
            systemsFactory.AddNewSystems(systems, 
                new IEcsSystemsFactory.Settings{AddClientSystems = true, AddServerSystems = true});
            systems.Add(new TickSystem());
            

            systems.Init();
            
            viewSystems = new EcsSystems(world);
            viewSystems.Add(new SyncTransformSystem(true));
            viewSystems.Add(new RotateCharacterSystem());

            viewSystems.Add(new RotateRigidbodySystem());
            viewSystems.Add(new CameraFollowSystem(Camera.main));
            
            viewSystems.Init();
            
            world.AddUnique(new MainPlayerIdComponent{value = playerId});
            unitEntity.EntityAdd<PlayerComponent>(world).id = playerId;

            var inventory = world.NewEntity();
            inventory.EntityAddComponent<InventoryComponent>(world).SlotCapacity = 10;

            unitEntity.EntityAddComponent<InventoryLinkComponent>(world).Inventory = world.PackEntity(inventory);
            
            /*
            var trashEntity = world.NewEntity();
            trashEntity.EntityAddComponent<TrashComponent>(world);

            uiInventory.Init(inventory, trashEntity, world); 
            */
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
            /*
            UnityEcsClient.CheckInput(inputWorld, world, unitEntity, playerInput, camera, input =>
            {
                if (world.HasUnique<RootMotionComponent>())
                {
                    //todo, dublicated code
                    input.hasUnitPos = true;
                    input.unitPos = world.GetUnique<RootMotionComponent>().Position;
                }

                InputService.ApplyInput(inputWorld, playerId, input);
            });*/


            CheckInput(camera, joystick, controlService);
            
            viewSystems.Run();
        }


        

        public void FixedUpdate()
        {
            if (!Application.isPlaying)
                return;
            systems.Run();
        }
        

        public void OnDrawGizmos()
        {
            if (!Application.isPlaying)
                return;
        
            EcsWorldDebugDraw.Draw(world);
        }
    }
}