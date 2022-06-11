using System;
using UnityEngine;

namespace Game.Ecs.ClientServer.Components.Input
{
    [Serializable]
    public struct InputMoveToPointComponent:IInputComponent
    {
        public Vector3 Value;
    }
}