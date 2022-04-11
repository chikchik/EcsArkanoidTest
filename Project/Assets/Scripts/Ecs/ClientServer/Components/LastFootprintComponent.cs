using System;
using UnityEngine;

namespace Game.Ecs.ClientServer.Components
{
    [Serializable]
    public struct LastFootprintComponent
    {
        public Vector3 position;
        public Vector3 direction;
        public bool isLeftHand;
    }
}