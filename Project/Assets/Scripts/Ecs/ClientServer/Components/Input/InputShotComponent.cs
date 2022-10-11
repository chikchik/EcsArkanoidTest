using UnityEngine;
using XFlow.Net.ClientServer.Ecs.Components;

namespace Game.Ecs.ClientServer.Components.Input
{
    public struct InputShotComponent:IInputComponent
    {
        public Vector3 Direction;
        public Vector3 Position;
    }
}