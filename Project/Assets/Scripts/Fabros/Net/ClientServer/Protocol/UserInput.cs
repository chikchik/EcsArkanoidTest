using System;
using Fabros.EcsModules.Tick.Other;
using UnityEngine;

namespace Game.Fabros.Net.ClientServer.Protocol
{
    [Serializable]
    public class UserInput
    {
        public enum MoveType
        {
            MoveToPoint,
            MoveToDirection
        }

        public Tick time;
        public int player;

        public Action action;
        public bool hasInteraction;

        public Move move;
        public bool hasMove;

        [Serializable]
        public class Action
        {
        }

        [Serializable]
        public class Move
        {
            public Vector3 value;
            public MoveType moveType;
        }
    }
}