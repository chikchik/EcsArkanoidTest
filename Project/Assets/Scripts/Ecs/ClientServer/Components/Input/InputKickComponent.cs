using UnityEngine;
using XFlow.Net.ClientServer.Ecs.Components;

namespace Game.Ecs.ClientServer.Components.Input
{
    public struct InputKickComponent: IInputComponent
    {
        public Vector3 dir;
    }
}