using UnityEngine;

namespace Game.Fabros.EcsModules.Box2D.ClientServer.Components
{
    [System.Serializable]
    public struct PolygonColliderComponent
    {
        public int[] Anchors;
        //public List<Vector2> Vertices;//todo, refactor to array[]
        public Vector2[] Vertices;
    }
}