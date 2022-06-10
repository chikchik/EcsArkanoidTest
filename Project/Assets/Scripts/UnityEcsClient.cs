using System;
using Fabros.Ecs;
using Fabros.Ecs.Client.Components;
using Fabros.Ecs.ClientServer.Components;
using Fabros.Ecs.Utils;
using Game.ClientServer;
using Game.Ecs.Client.Components;
using Game.Ecs.Client.Systems;
using Game.Ecs.ClientServer.Components;
using Game.Fabros.EcsModules.Fire.Client.Components;
using Game.Fabros.Net.Client;
using Game.Fabros.Net.ClientServer;
using Game.Fabros.Net.ClientServer.Protocol;
using Game.UI;
using Game.Utils;
using Leopotam.EcsLite;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

namespace Game.Client
{
    public class UnityEcsClient : MonoBehaviour
    {
        private NetClient client;

        [Inject] private Camera camera;
        [Inject] private Global global;
        [Inject] private UI ui;
        [Inject] private Joystick joystick;
        
        [Inject] private MPInputService inputService;
        [Inject] private PlayerInputService playerInputService;
        
        [Inject] private EcsWorld world;
        [Inject(Id="input")] private EcsWorld inputWorld;

        private EcsSystems viewSystems;

        private void Start()
        {
            UnityEngine.Physics.autoSimulation = false;
            UnityEngine.Physics2D.simulationMode = SimulationMode2D.Script;

            playerInputService.SetInputService(inputService);
            
            var pool = SharedComponents.CreateComponentsPool();
            client = new NetClient(world, pool, new EcsSystemsFactory(pool), inputService);
            
            viewSystems = new EcsSystems(world);
            viewSystems.Add(new SyncTransformSystem(false));
            viewSystems.Add(new RotateCharacterSystem());

            viewSystems.Add(new RotateRigidbodySystem());
            viewSystems.Add(new CameraFollowSystem(Camera.main));
            
#if UNITY_EDITOR
            viewSystems.Add(new Leopotam.EcsLite.UnityEditor.EcsWorldDebugSystem(bakeComponentsInName:true));
#endif
            
            inputService.SetNetClient(client);
            
            client.ConnectedAction = () =>
            {
                viewSystems.Init();
            };
            
            client.InitWorldAction = world =>
            {
                var viewComponent = new ClientViewComponent();
                viewComponent.Global = global;

                world.AddUnique<ClientViewComponent>() = viewComponent;
            };


            client.DeleteEntitiesAction = (world, entities) =>
            {
                foreach (var entity in entities)
                {
                    if (entity.EntityHasComponent<TransformComponent>(world))
                    {
                        var go = entity.EntityGetComponent<TransformComponent>(world).Transform.gameObject;
                        Destroy(go);
                    }

                    if (entity.EntityHasComponent<FireViewComponent>(world))
                    {
                        var go = entity.EntityGetComponent<FireViewComponent>(world).view.gameObject;
                        Destroy(go);
                    }
                };
            };

            client.Start();
            
            
            /*
            ui.ApplyInputAction = (input) =>
            {
                var id = world.GetUnique<MainPlayerIdComponent>().value;
                var unitEntity = BaseServices.GetUnitEntityByPlayerId(world, id);
                if (input.hasShot)
                    input.shot = new UserInput.Shot {direction = unitEntity.EntityGet<LookDirectionComponent>(world).value};
                
                InputService.ApplyInput(inputWorld, id, input);
                client.AddUserInput(input);
            };*/
        }

        private void Update()
        {
            #if UNITY_EDITOR
            if (!Application.isPlaying)
                return;
            #endif
            
            if (!client.Connected)
                return;

            client.Update();
            
            UnityEcsSinglePlayer.CheckInput(camera, joystick, playerInputService);
            
            /*
            var unitEntity = BaseServices.GetUnitEntityByPlayerId(world, client.GetPlayerID());
            CheckInput(inputWorld, world, 
                unitEntity, playerInput, camera,
                input => client.AddUserInput(input));


            if (Input.GetMouseButtonDown(0) && !(EventSystem.current.IsPointerOverGameObject() &&
                                                 EventSystem.current.currentSelectedGameObject != null))
            {
                PlayerInputService.AddMoveToPoint(inputWorld, Input.mousePosition);
            }
            */
            
            viewSystems.Run();
        }

        private void OnDestroy()
        {
            if (client == null)
                return;
            client.OnDestroy();
        }

        private void OnGUI()
        {
            client.OnGUI();
        }

        private void OnDrawGizmos()
        {
            if (!Application.isPlaying)
                return;
            EcsWorldDebugDraw.Draw(world);
        }


        public static void CheckInput(EcsWorld inputWorld, EcsWorld world, 
            int unitEntity, 
            Camera camera, Action<UserInput> addUserInput
            )
        {
            /*
            if (unitEntity == -1)
                return;
            
            var forward = camera.transform.forward;
            forward.y = 0;
            forward.Normalize();

            var right = camera.transform.right;
            right.y = 0;
            right.Normalize();

            var moveDirection = playerInput.Movement;
            moveDirection = forward * moveDirection.z + right * moveDirection.x;


            if (playerInput.HasTouch)
            {
                var ray = camera.ScreenPointToRay(playerInput.TouchPosition);
                var plane = new Plane(new Vector3(0, 1, 0), 0);
                plane.Raycast(ray, out var dist);

                var point = ray.GetPoint(dist);

                var input = new UserInput
                {
                    hasMove = true,
                    move = new UserInput.Move {value = point, moveType = UserInput.MoveType.MoveToPoint}
                };

                addUserInput(input);
                return;
            }

            var lastDirection = unitEntity.EntityGetComponent<MoveDirectionComponent>(world).value;

            if (moveDirection != lastDirection)
            {
                PlayerInputService.AddMoveToDirection(inputWorld, moveDirection);
            }*/
        }
    }
}