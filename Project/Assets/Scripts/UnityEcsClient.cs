using System;
using System.Collections.Generic;
using Fabros.Ecs.Utils;
using Fabros.EcsModules.Tick.Other;
using Game.Ecs.Client.Components;
using Game.Ecs.ClientServer.Components;
using Game.Fabros.EcsModules.Fire.Client.Components;
using Game.Fabros.Net.Client;
using Game.Fabros.Net.ClientServer;
using Game.Fabros.Net.ClientServer.Protocol;
using Game.Utils;
using Leopotam.EcsLite;
using UnityEngine;
using Zenject;

namespace Game.Client
{
    public class UnityEcsClient : MonoBehaviour
    {
        public Camera Camera;

        private NetClient client;
        
        [Inject] private Global global;
        [Inject] private PlayerInput.PlayerInput playerInput;
        [Inject] private UI.MainUI ui;
        [Inject] private EcsWorld world;

        void Start()
        {
            client = new NetClient(world);

            client.InitWorld = world =>
            {
                var viewComponent = new ClientViewComponent();
                viewComponent.Camera = Camera.main;
                viewComponent.MainUI = ui;
                viewComponent.Global = global;

                world.AddUnique<ClientViewComponent>() = viewComponent;
            };

            client.LinkUnits = (world) => { ClientServices.LinkUnits(world); };

            client.DeleteEntities = (world, entities) =>
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
        }
        
        private void Update()
        {
            if (!client.Connected)
                return;

            client.Update();

            //проверям инпуты от нашего игрока, если был то шлем  на сервер и добавляем в общий список, которые выполняется чуть позже 
            CheckInput(playerInput, client.GetContexts(), client.GetWorld(), client.GetContexts().Inputs,
                client.GetPlayerID(), packet => { client.Input(packet); });
        }

        private void OnDestroy()
        {
            client.OnDestroy();
        }

        private void OnGUI()
        {
            client.OnGUI();
        }


        public void CheckInput(PlayerInput.PlayerInput playerInput, LeoContexts Leo, EcsWorld world,
            List<UserInput> pendingInputs, int playerID, Action<Packet> send = null)
        {
            /*
             * внутриигровая логика, проверка ввода
             * ввод игрока добавляется в pendingInputs лист и передается внутрь Action send
             * на клиенте и сервере send обрабатывается по разному
             * 
             */

            //эти расчеты имеют смысл когда на клиенте и сервере разный tickrate
            //например 20 на сервере, 60 на клиенте
            //тогда сервер за один свой апдейт прибавляет +60/20 = +3 тика
            //клиент же по +1
            //потому надо сделать snap значения ввода чтоб оно попало на корректный серверный тик 
            var dt = Leo.GetConfig(world).clientTickrate / Leo.GetConfig(world).serverTickrate;
            var a = Leo.GetCurrentTick(world).Value / dt + 1;
            var tick = new Tick(a * dt);

            var entity = BaseServices.GetUnitEntityByPlayerId(world, playerID);
            if (entity == -1)
                return;

            var forward = Camera.transform.forward;
            forward.y = 0;
            forward.Normalize();

            var right = Camera.transform.right;
            right.y = 0;
            right.Normalize();

            var moveDirection = playerInput.Movement;
            moveDirection = forward * moveDirection.z + right * moveDirection.x;

            if (playerInput.HasInteraction)
            {
                var input = new UserInput
                    {time = tick, player = playerID, hasInteraction = true, action = new UserInput.Action()};

                pendingInputs.Add(input);
                send?.Invoke(new Packet {input = input});
            }


            if (playerInput.HasTouch)
            {
                var ray = Camera.ScreenPointToRay(playerInput.TouchPosition);
                var plane = new Plane(new Vector3(0, 1, 0), 0);
                plane.Raycast(ray, out var dist);

                var point = ray.GetPoint(dist);

                var input = new UserInput
                {
                    time = tick,
                    player = playerID,
                    hasMove = true,
                    move = new UserInput.Move {value = point, moveType = UserInput.MoveType.MoveToPoint}
                };

                pendingInputs.Add(input);
                send?.Invoke(new Packet {input = input});
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
                    time = tick,
                    player = playerID,
                    hasMove = true,
                    move = new UserInput.Move {value = moveDirection, moveType = UserInput.MoveType.MoveToDirection}
                };

                pendingInputs.Add(input);
                send?.Invoke(new Packet {input = input});
            }
        }
    }
}