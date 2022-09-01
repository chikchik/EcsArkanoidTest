using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.View
{
    public class RevoluteJointView : MonoBehaviour
    {
        public GameObject _connectedBody;
        public Vector2 _jointOffset;
        public Vector2 _connectedJointOffset;
        public bool _enableLimits;
        public float _upperAngleLimit;
        public float _lowerAngleLimit;
        public bool _collideConnected;
    }
}
