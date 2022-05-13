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
        public static extern IntPtr CreateBody(IntPtr worldPtr, int bodyType, Vector2 position, float angle, int entity);

        [DllImport(DllName)]
        public static extern void AddFixtureToBody(IntPtr body, IntPtr shape, float density,
            float friction, float restitution, float restitutionThreshold, bool isTrigger, B2Filter filter);

        [DllImport(DllName)]
        public static extern IntPtr CreateCircleShape(float radius);
        
        [DllImport(DllName)]
        public static extern IntPtr CreateBoxShape(Vector2 extents);

        [DllImport(DllName)]
        public static extern IntPtr CreatePolygonShape(Vector2[] vertices, int count);
        
        [DllImport(DllName)]
        public static extern IntPtr CreateChainShape(Vector2[] vertices, int count);

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
        public static extern void ApplyForceToCenter(IntPtr body, Vector2 force);
        
        [DllImport(DllName)]
        public static extern void ApplyLinearImpulse(IntPtr body, Vector2 force, Vector2 point);
        
        [DllImport(DllName)]
        public static extern void ApplyLinearImpulseToCenter(IntPtr body, Vector2 force);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool RayCastClosest(IntPtr world, Vector2 origin, Vector2 direction,
            ref RaycastOutputReturnType hit, float distance, UInt16 layerMask = 0);

        [DllImport(DllName)]
        public static extern void SetEnabled(IntPtr body, bool flag);

        [DllImport(DllName)]
        public static extern bool IsEnabled(IntPtr body);
        
        [DllImport(DllName)]
        public static extern void SetBullet(IntPtr body, bool flag);
        
        [DllImport(DllName)]
        public static extern float GetLinearDamping(IntPtr body);

        [DllImport(DllName)]
        public static extern float GetAngularDamping(IntPtr body);

        [DllImport(DllName)]
        public static extern void SetLinearDamping(IntPtr body, float val);

        [DllImport(DllName)]
        public static extern void SetAngularDamping(IntPtr body, float val);

        public delegate void CallbackDelegate(CollisionCallbackData callbackData);

        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern void SetBeginContactCallback(IntPtr worldPtr, CallbackDelegate callback);

        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern void SetEndContactCallback(IntPtr worldPtr, CallbackDelegate callback);

        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern void SetPreSolveCallback(IntPtr worldPtr, CallbackDelegate callback);

        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern void SetPostSolveCallback(IntPtr worldPtr, CallbackDelegate callback);

        [DllImport(DllName)]
        public static extern B2Filter GetBodyFixturesFilterData(IntPtr body);
        
    }
}