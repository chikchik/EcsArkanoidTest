using Fabros.Ecs.ClientServer.Components;
using Fabros.Ecs.Utils;
using Game.Ecs.ClientServer.Components.Input;
using Game.Fabros.Net.ClientServer.Protocol;
using Leopotam.EcsLite;
using UnityEngine;

namespace Game.ClientServer
{
    internal static class InputService
    {
        public static void ApplyInput(EcsWorld inputWorld, int playerID, UserInput input)
        {
            if (!input.hasMove && !input.hasAction && !input.hasUnitPos && !input.hasShot)
                return;

//            Debug.Log($"apply input {input.time.Value}");

            var inputEntity = inputWorld.NewEntity();
            inputEntity.EntityAddComponent<InputPlayerComponent>(inputWorld).PlayerID = playerID;
            inputEntity.EntityAddComponent<InputComponent>(inputWorld);//.tick = input.time.Value;

            if (input.hasMove)
            {
                if (input.move.moveType == UserInput.MoveType.MoveToDirection)
                    inputEntity.EntityAddComponent<InputMoveDirectionComponent>(inputWorld).Dir = input.move.value;

                if (input.move.moveType == UserInput.MoveType.MoveToPoint)
                    inputEntity.EntityAddComponent<InputMoveToPointComponent>(inputWorld).Value = input.move.value;
            }

            if (input.hasAction) 
                inputEntity.EntityAddComponent<InputActionComponent>(inputWorld);
            
            if (input.hasShot) 
                inputEntity.EntityAddComponent<InputShotComponent>(inputWorld).dir = input.shot.direction;

            if (input.hasUnitPos)
                inputEntity.EntityAdd<PositionComponent>(inputWorld).value = input.unitPos;
        }

    }

    internal static class PlayerInputService
    {
        public static void AddShot(EcsWorld inputWorld)
        {
            var inputEntity = inputWorld.NewEntity();
            inputEntity.EntityAddComponent<InputComponent>(inputWorld);
            inputEntity.EntityAddComponent<InputShotComponent>(inputWorld);//.dir = dir;
        }
        
        public static void AddMoveToDirection(EcsWorld inputWorld, Vector3 dir)
        {
            var inputEntity = inputWorld.NewEntity();
            inputEntity.EntityAddComponent<InputComponent>(inputWorld);
            inputEntity.EntityAddComponent<InputMoveDirectionComponent>(inputWorld).Dir = dir;
        }
        
        public static void StopMoveToDirection(EcsWorld inputWorld)
        {
            var inputEntity = inputWorld.NewEntity();
            inputEntity.EntityAddComponent<InputComponent>(inputWorld);
            inputEntity.EntityAddComponent<InputMoveDirectionComponent>(inputWorld).Dir = Vector3.zero;
        }
        
        public static void AddMoveToPoint(EcsWorld inputWorld, Vector3 pos)
        {
            var inputEntity = inputWorld.NewEntity();
            inputEntity.EntityAddComponent<InputComponent>(inputWorld);
            inputEntity.EntityAddComponent<InputMoveToPointComponent>(inputWorld).Value = pos;
        }
    }
}