using System;
using UnityEngine;

namespace Game.ClientServer.Physics
{
    public struct RaycastOutputReturnType
    {
        public IntPtr Body;
        public Vector2 Normal;
        public Vector2 Point;
        public float Fraction;
    }
}