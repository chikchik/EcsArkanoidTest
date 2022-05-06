using System.Collections.Generic;
using UnityEngine;

namespace Game.Ecs.ClientServer.Components.Physics
{
    [System.Serializable]
    public struct PolygonColliderComponent
    {
        public int[] anchors;
        public List<Vector2> vertices;
    }
}