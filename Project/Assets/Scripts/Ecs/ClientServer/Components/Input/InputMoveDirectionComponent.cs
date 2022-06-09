using UnityEngine;

namespace Game.Ecs.ClientServer.Components.Input
{
    public struct InputMoveDirectionComponent:IInputComponent
    {
        public Vector3 Dir;
    }
}