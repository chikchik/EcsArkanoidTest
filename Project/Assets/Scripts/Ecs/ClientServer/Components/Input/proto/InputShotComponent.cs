using Game.Fabros.Net.ClientServer.Ecs.Components;
using UnityEngine;

namespace Game.Ecs.ClientServer.Components.Input.Proto
{
    public struct InputShotComponent:IInputComponent
    {
        public Vector3 dir;
        public Vector3 pos;
    }
}