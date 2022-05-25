using UnityEngine;

namespace Game.View
{
    public class ButtonView : MonoBehaviour
    {
        public float speed;
        public Vector3 moveOffset = Vector3.zero;

        public Vector3 StartPosition => transform.position;
        public Vector3 EndPosition => StartPosition - moveOffset;

        public bool Spawner;
    }
}