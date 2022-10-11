using UnityEngine;
using XFlow.EcsLite;
using XFlow.Net.ClientServer.Ecs.Components;

namespace Game.Ecs.ClientServer.Components.Input
{
    public struct InputBeginMouseDragComponent : IInputComponent
    {
        public EcsPackedEntity Entity;
        public Vector2 MouseOffset;
    }
}