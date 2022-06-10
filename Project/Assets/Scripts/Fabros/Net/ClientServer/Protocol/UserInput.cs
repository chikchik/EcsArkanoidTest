using System;
using Fabros.EcsModules.Tick.Other;
using Game.Ecs.ClientServer.Components.Input;
using UnityEngine;

namespace Game.Fabros.Net.ClientServer.Protocol
{
    [Serializable]
    public class UserInput
    {
        public enum InputType
        {
            Shot,
            Action,
            MoveToPoint,
            MoveToDirection
        }

        public Tick time;
        public int player;
        public InputType inputType;
        public IInputComponent data;


        public bool hasUnitPos;
        public Vector3 unitPos;
    }
}