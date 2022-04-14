using Fabros.Ecs.Utils;
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
using Zenject;

namespace Game.Client
{
    public class UnityEcsClient : MonoBehaviour
    {
        private NetClient client;

        [Inject] private Camera camera;
        [Inject] private Global global;
        [Inject] private PlayerInput.PlayerInput playerInput;
        [Inject] private MainUI ui;
        [Inject] private EcsWorld world;

        private EcsSystems viewSystems;

        private void Start()
        {
            client = new NetClient(world);

            viewSystems = new EcsSystems(world);

            viewSystems.Add(new SyncTransformSystem());
            viewSystems.Add(new RotateCharacterSystem());
            viewSystems.Add(new RotateRigidbodySystem());
            viewSystems.Add(new CameraFollowSystem());


            client.ConnectedAction = () =>
            {
                viewSystems.Init();
            };
            
            client.InitWorldAction = world =>
            {
                var viewComponent = new ClientViewComponent();
                viewComponent.Camera = Camera.main;
                viewComponent.MainUI = ui;
                viewComponent.Global = global;

                world.AddUnique<ClientViewComponent>() = viewComponent;
            };

            client.LinkUnitsAction = world => { ClientServices.LinkUnits(world); };

            client.DeleteEntitiesAction = (world, entities) =>
            {
                entities.ForEach(entity =>
                {
                    if (entity.EntityHasComponent<GameObjectComponent>(world))
                    {
                        var go = entity.EntityGetComponent<GameObjectComponent>(world).GameObject;
                        Destroy(go);
                    }

                    if (entity.EntityHasComponent<FireViewComponent>(world))
                    {
                        var go = entity.EntityGetComponent<FireViewComponent>(world).view.gameObject;
                        Destroy(go);
                    }
                });
            };

            client.Start();


            ui.InteractionButton.onClick.AddListener(() =>
            {
                var input = new UserInput
                {
                    hasInteraction = true,
                    action = new UserInput.Action()
                };

                client.AddUserInput(input);
            });
        }

        private void Update()
        {
            if (!client.Connected)
                return;

            client.Update();
            CheckInput();
            
            viewSystems.Run();
        }

        private void OnDestroy()
        {
            client.OnDestroy();
        }

        private void OnGUI()
        {
            client.OnGUI();
        }


        public void CheckInput()
        {
            var entity = BaseServices.GetUnitEntityByPlayerId(world, client.GetPlayerID());
            if (entity == -1)
                return;

            var playerID = client.GetPlayerID();
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

                client.AddUserInput(input);
                return;
            }

            var lastDirection = entity.EntityGetComponent<MoveDirectionComponent>(world).value;

            if (moveDirection != lastDirection)
            {
                if (entity.EntityHas<TargetPositionComponent>(world))
                    if (moveDirection.magnitude < 0.001f)
                        return;

                var input = new UserInput
                {
                    hasMove = true,
                    move = new UserInput.Move {value = moveDirection, moveType = UserInput.MoveType.MoveToDirection}
                };

                client.AddUserInput(input);
            }
        }
    }
}