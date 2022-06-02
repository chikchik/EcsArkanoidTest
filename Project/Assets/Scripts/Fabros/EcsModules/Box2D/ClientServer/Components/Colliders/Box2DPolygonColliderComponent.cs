using UnityEngine;

namespace Fabros.EcsModules.Box2D.ClientServer.Components.Colliders
{
    [System.Serializable]
    public struct Box2DPolygonColliderComponent
    {
        public int[] Anchors;
        //public List<Vector2> Vertices;//todo, refactor to array[]
        public Vector2[] Vertices;
    }
}