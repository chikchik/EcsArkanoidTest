﻿using Fabros.Ecs.ClientServer.Components;
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
            if (!input.hasMove && !input.hasInteraction && !input.hasUnitPos)
                return;

            Debug.Log($"apply input {input.time.Value}");

            var inputEntity = inputWorld.NewEntity();
            inputEntity.EntityAddComponent<InputPlayerComponent>(inputWorld).PlayerID = playerID;
            inputEntity.EntityAddComponent<InputComponent>(inputWorld).tick = input.time.Value;

            if (input.hasMove)
            {
                if (input.move.moveType == UserInput.MoveType.MoveToDirection)
                    inputEntity.EntityAddComponent<InputMoveComponent>(inputWorld).Dir = input.move.value;

                if (input.move.moveType == UserInput.MoveType.MoveToPoint)
                    inputEntity.EntityAddComponent<InputMoveToPointComponent>(inputWorld).Value = input.move.value;
            }

            if (input.hasInteraction) 
                inputEntity.EntityAddComponent<InputActionComponent>(inputWorld);

            if (input.hasUnitPos)
            {
                inputEntity.EntityAdd<PositionComponent>(inputWorld).value = input.unitPos;
            }
        }
    }
}