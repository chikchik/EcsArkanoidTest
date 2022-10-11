using UnityEngine;

namespace Game.Ecs.ClientServer.Components
{
    public struct MovePathComponent
    {
        public Vector3[] Path;
        public int Current;
    }
}