using System;
using UnityEngine;

namespace Game.Ecs.Client.Components
{
    [Serializable]
    public struct CollectableTargetComponent
    {
        public GameObject targetObject;
    }
}