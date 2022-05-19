using System.Collections.Generic;
using UnityEngine;

namespace Game.Ecs.ClientServer.Components.Physics
{
    [System.Serializable]
    public struct PolygonColliderComponent
    {
        public int[] Anchors;
        public List<Vector2> Vertices;//todo, refactor to array[]
    }
}