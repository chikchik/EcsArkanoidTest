using UnityEngine;

namespace Game.Ecs.ClientServer.Components
{
    public struct MovePathComponent
    {
        public Vector3[] path;
        public int current;
    }
}