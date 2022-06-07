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
        public bool hasAction;

        public Move move;
        public bool hasMove;


        public Shot shot;
        public bool hasShot;
        
        [Serializable]
        public class Action
        {
        }
        
        [Serializable]
        public class Shot
        {
            //public Vector3 direction;
        }

        [Serializable]
        public class Move
        {
            public Vector3 value;
            public MoveType moveType;
        }

        public bool hasUnitPos;
        public Vector3 unitPos;
    }
}