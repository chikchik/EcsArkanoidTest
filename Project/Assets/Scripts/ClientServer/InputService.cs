using Fabros.Ecs.Utils;
using Fabros.EcsModules.Base.Components;
using Game.Ecs.ClientServer.Components.Input;
using Game.Fabros.Net.ClientServer;
using Game.Fabros.Net.ClientServer.Protocol;
using Leopotam.EcsLite;

namespace Game.ClientServer
{
    internal static class InputService
    {
        public static void ApplyInput(EcsWorld inputWorld, int playerID, UserInput input)
        {
            var inputEntity = inputWorld.NewEntity();
            inputEntity.EntityAddComponent<InputPlayerComponent>(inputWorld).PlayerID = playerID;
            inputEntity.EntityAddComponent<InputComponent>(inputWorld);

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