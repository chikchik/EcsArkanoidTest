using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Game.ClientServer.Physics
{
    public static class Box2DPhysics
    {
#if UNITY_IPHONE && !UNITY_EDITOR
    private const string DllName = "__Internal";
#else
        private const string DllName = "libbox2d";
#endif

        [DllImport(DllName)]
        public static extern IntPtr UpdateWorld(IntPtr world, float timeStep, int velocityIterations,
            int positionIterations);

        [DllImport(DllName)]
        public static extern IntPtr CreateWorld(Vector2 gravity);

        [DllImport(DllName)]
        public static extern void DestroyWorld(IntPtr world);

        [DllImport(DllName)]
        public static extern IntPtr CreateBody(IntPtr worldPtr, int bodyType, Vector2 position, Vector2 extents,
            float angle, float density, float friction, float restitution, float restitutionThreshold);

        [DllImport(DllName)]
        public static extern void DestroyBody(IntPtr world, IntPtr body);

        [DllImport(DllName)]
        public static extern Vector2 GetPosition(IntPtr bodyPtr);

        [DllImport(DllName)]
        public static extern void SetPosition(IntPtr body, Vector2 position);

        [DllImport(DllName)]
        public static extern float GetAngle(IntPtr body);

        [DllImport(DllName)]
        public static extern void SetAngle(IntPtr body, float angle);

        [DllImport(DllName)]
        public static extern Vector2 GetLinearVelocity(IntPtr body);

        [DllImport(DllName)]
        public static extern void SetLinearVelocity(IntPtr body, Vector2 linearVelocity);

        [DllImport(DllName)]
        public static extern float GetAngularVelocity(IntPtr body);

        [DllImport(DllName)]
        public static extern void SetAngularVelocity(IntPtr body, float angularVelocity);

        [DllImport(DllName)]
        public static extern BodyInfo GetBodyInfo(IntPtr body);

        [DllImport(DllName)]
        public static extern void SetBodyInfo(IntPtr body, BodyInfo bodyInfo);

        [DllImport(DllName)]
        public static extern void ApplyForce(IntPtr body, Vector2 force, Vector2 point);

        [DllImport(DllName)]
        public static extern bool RayCast(IntPtr world, Vector2 origin, Vector2 direction, float distance);
    }
}