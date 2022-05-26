using System;
using UnityEngine;

namespace Game.ClientServer.Box2D
{
    public struct RaycastOutputReturnType
    {
        public IntPtr Body;
        public Vector2 Normal;
        public Vector2 Point;
        public float Fraction;
    }
}