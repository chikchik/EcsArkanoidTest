using System;
using UnityEngine;

namespace Fabros.EcsModules.Box2D.ClientServer.Api
{
    public struct RaycastOutputReturnType
    {
        public IntPtr Body;
        public Vector2 Normal;
        public Vector2 Point;
        public float Fraction;
    }
}