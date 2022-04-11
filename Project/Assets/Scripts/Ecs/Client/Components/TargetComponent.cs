using System;
using UnityEngine;

namespace Game.Ecs.Client.Components
{
    [Serializable]
    public struct TargetComponent
    {
        public Transform target;
    }
}