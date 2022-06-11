using System;
using Fabros.EcsModules.Tick.Other;
using Game.Ecs.ClientServer.Components.Input;
using Game.Fabros.Net.ClientServer.Ecs.Components;
using UnityEngine;

namespace Game.Fabros.Net.ClientServer.Protocol
{
    public struct UserInput
    {
        public Tick time { get; set; }
        public int player;
        public IInputComponent data;
    }
}