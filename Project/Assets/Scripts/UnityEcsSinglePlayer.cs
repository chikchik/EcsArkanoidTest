using System;
using Fabros.Ecs;
using Fabros.Ecs.ClientServer.Components;
using Fabros.Ecs.Utils;
using Fabros.EcsModules.Tick.Components;
using Fabros.EcsModules.Tick.Other;
using Game.ClientServer;
using Game.Ecs.Client.Components;
using Game.Ecs.Client.Systems;
using Game.Ecs.ClientServer.Components;
using Game.Fabros.Net.Client;
using Game.Fabros.Net.ClientServer;
using Game.Fabros.Net.ClientServer.Ecs.Components;
using Game.Fabros.Net.ClientServer.Protocol;
using Game.UI;
using Game.Utils;
using Leopotam.EcsLite;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

namespace Game.Client
{
    public class UnityEcsSinglePlayer: MonoBehaviour
    {
        [Inject] private Camera camera;
        [Inject] private Global global;
        [Inject] private UI ui;
        [Inject] private EcsWorld world;
        [Inject] private Joystick joystick;
        
        [Inject(Id = "input")] private EcsWorld inputWorld;
        
        [Inject] 
        private PlayerInputService playerInputService;
        
        [Inject] 
        private SingleInputService inputService;
        
        private EcsSystems systems;
        private EcsSystems viewSystems;
        

        private int unitEntity = -1;
        private int playerId = 1;

        public void Start()
        {
            UnityEngine.Physics.autoSimulation = false;
            UnityEngine.Physics2D.simulationMode = SimulationMode2D.Script;

            playerInputService = new PlayerInputService(inputWorld, world);
            playerInputService.SetInputService(inputService);
            
            systems = new EcsSystems(world);
            systems.AddWorld(inputWorld, "input");
            ClientServices.InitializeNewWorldFromScene(world);
            
            world.AddUnique(new TickDeltaComponent
            {
                Value = new TickDelta(1, (int)(1f/Time.fixedDeltaTime))
            });
            
            world.AddUnique(new TickComponent{Value = new Tick(0)});
            world.AddUnique(new ClientViewComponent
            {
                Global = global
            });

            
            unitEntity = UnitService.CreateUnitEntity(world);
            world.AddUnique(new ClientPlayerComponent{ entity = unitEntity});
            
            
#if UNITY_EDITOR
            systems.Add(new Leopotam.EcsLite.UnityEditor.EcsWorldDebugSystem(bakeComponentsInName:true));
#endif

            var systemsFactory = new EcsSystemsFactory(null);
            
            systemsFactory.AddNewSystems(systems, 
                new IEcsSystemsFactory.Settings{client = true, server = true});

            systems.Init();
            
            viewSystems = new EcsSystems(world);
            viewSystems.Add(new SyncTransformSystem(true));
            viewSystems.Add(new RotateCharacterSystem());

            viewSystems.Add(new RotateRigidbodySystem());
            viewSystems.Add(new CameraFollowSystem(Camera.main));
            
            viewSystems.Init();
            
            world.AddUnique(new MainPlayerIdComponent{value = playerId});
            unitEntity.EntityAdd<PlayerComponent>(world).id = playerId;
        }

        public static void CheckInput(Camera camera, Joystick joystick, PlayerInputService inputService)
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
                
                inputService.MoveToDirection(dir);
            }
            
            if (Input.GetMouseButtonDown(0) && !(EventSystem.current.IsPointerOverGameObject() &&
                                                 EventSystem.current.currentSelectedGameObject != null))
            {
                var ray = camera.ScreenPointToRay(Input.mousePosition);
                var plane = new Plane(new Vector3(0, 1, 0), 0);
                plane.Raycast(ray, out var dist);

                var point = ray.GetPoint(dist);
                
                inputService.MoveToPoint(point);
            }

            var hor = Input.GetAxis("Horizontal");
            var ver = Input.GetAxis("Vertical");
            
            if (Mathf.Abs(hor) > 0.01f || Mathf.Abs(ver) > 0.01f)
            {
                MoveDir(hor, ver);
            }
            else if (joystick.Direction.magnitude > 0.01f)
            {
                MoveDir(joystick.Direction.x, joystick.Direction.y);
            }else
            {
                inputService.StopMoveToDirection();
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


            CheckInput(camera, joystick, playerInputService);
            
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