using UnityEngine;
using XFlow.Net.ClientServer.Ecs.Components;

namespace Game.Ecs.ClientServer.Components.Input
{
    public struct InputMoveToPointComponent:IInputComponent
    {
        public Vector3 Value;
    }
}