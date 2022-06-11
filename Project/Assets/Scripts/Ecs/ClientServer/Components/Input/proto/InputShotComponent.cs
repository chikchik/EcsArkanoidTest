using System;
using Game.Fabros.Net.ClientServer.Ecs.Components;
using UnityEngine;

namespace Game.Ecs.ClientServer.Components.Input
{
    public struct InputShotComponent:IInputComponent
    {
        public Vector3 dir;
    }
}