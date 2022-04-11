using System.Collections.Generic;
using UnityEngine;

namespace Game.View
{
    public class GateView : MonoBehaviour
    {
        public List<GameObject> OpenedBy;

        public float OpenSpeed = 0.2f;
        public Vector3 moveOffset = Vector3.zero;

        public Vector3 StartPosition => transform.position;
        public Vector3 EndPosition => StartPosition - moveOffset;
    }
}