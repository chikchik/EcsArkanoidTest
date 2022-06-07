using System;
using Fabros.Ecs.Utils;
using Game.ClientServer;
using Game.Ecs.ClientServer.Components;
using Game.Fabros.Net.ClientServer.Protocol;
using Leopotam.EcsLite;
using UnityEngine;
using Zenject;

namespace Game.Client
{
    /*
    public class KeyboardController
    {
        private EcsWorld world;
        private EcsWorld inputWorld;
        
        public KeyboardController(
            EcsWorld world, 
            [Inject(Id = "input")] EcsWorld inputWorld)
        {
            this.world = world;
            this.inputWorld = inputWorld;
        }
        
        public static void CheckInput(EcsWorld world, 
            int unitEntity, 
            PlayerInput.PlayerInput playerInput,
            Camera camera, Action<UserInput> addUserInput
        )
        {
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
                if (unitEntity.EntityHas<TargetPositionComponent>(world))
                    if (moveDirection.magnitude < 0.001f)
                        return;

                var input = new UserInput
                {
                    hasMove = true,
                    move = new UserInput.Move {value = moveDirection, moveType = UserInput.MoveType.MoveToDirection}
                };

                addUserInput(input);
            }
        }
        
        public void Update(int unitEntity, PlayerInput.PlayerInput playerInput, Camera camera, int playerId)
        {
            CheckInput(world, unitEntity, playerInput, camera, input =>
            {
                if (world.HasUnique<RootMotionComponent>())
                {
                    //todo, dublicated code
                    input.hasUnitPos = true;
                    input.unitPos = world.GetUnique<RootMotionComponent>().Position;
                }

                InputService.ApplyInput(inputWorld, playerId, input);
            });

            if (Input.GetKeyDown(KeyCode.Space))
            {
                var input = new UserInput{player = playerId, hasInteraction = true};
                InputService.ApplyInput(inputWorld, playerId, input);
            }
        }
    }*/
}