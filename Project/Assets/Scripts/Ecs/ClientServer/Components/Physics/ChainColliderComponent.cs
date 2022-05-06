using UnityEngine;

namespace Game.Ecs.ClientServer.Components.Physics
{
    [System.Serializable]
    public struct ChainColliderComponent
    {
        public Vector2[] points;
    }
}