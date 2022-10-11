using UnityEngine;
using XFlow.EcsLite;
using XFlow.Net.ClientServer.Ecs.Components;

namespace Game.Ecs.ClientServer.Components.Input
{
    public struct InputUpdateMouseDragComponent : IInputComponent
    {
        public Vector2 Position;
    }
}