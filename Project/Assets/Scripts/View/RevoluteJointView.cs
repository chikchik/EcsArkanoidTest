using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.View
{
    public class RevoluteJointView : MonoBehaviour
    {
        public GameObject ConnectedBody;
        public bool AutoCalculateOffsets;
        public Vector2 JointOffset;
        public Vector2 ConnectedJointOffset;
        public bool EnableLimits;
        public float UpperAngleLimit;
        public float LowerAngleLimit;
        public bool CollideConnected;
    }
}
