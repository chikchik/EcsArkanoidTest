using System;
using UnityEngine;

namespace Game.Ecs.ClientServer.Components.Input
{
    [Serializable]
    public struct InputMoveDirectionComponent:IInputComponent
    {
        public Vector3 Dir;
    }
}