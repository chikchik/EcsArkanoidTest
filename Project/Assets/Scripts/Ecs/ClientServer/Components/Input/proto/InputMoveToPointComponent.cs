using Game.Fabros.Net.ClientServer.Ecs.Components;
using UnityEngine;

namespace Game.Ecs.ClientServer.Components.Input.proto
{
    public struct InputMoveToPointComponent:IInputComponent
    {
        public Vector3 Value;
    }
}