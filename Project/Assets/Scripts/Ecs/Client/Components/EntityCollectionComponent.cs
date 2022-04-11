using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Ecs.Client.Components
{
    [Serializable]
    public struct EntityCollectionComponent
    {
        public Dictionary<GameObject, int> entities;
    }
}