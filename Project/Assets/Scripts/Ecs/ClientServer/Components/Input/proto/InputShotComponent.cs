using System;
using UnityEngine;

namespace Game.Ecs.ClientServer.Components.Input
{
    [Serializable]
    public struct InputShotComponent:IInputComponent
    {
        public Vector3 dir;
    }
}