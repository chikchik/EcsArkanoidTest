using Game.Fabros.Net.ClientServer.Ecs.Components;
using UnityEngine;

namespace Game.Ecs.ClientServer.Components.Input.proto
{
    public struct InputMoveDirectionComponent:IInputComponent
    {
        public Vector3 Dir;
    }
}