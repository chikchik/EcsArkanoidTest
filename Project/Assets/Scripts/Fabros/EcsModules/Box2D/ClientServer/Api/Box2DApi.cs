using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Fabros.EcsModules.Box2D.ClientServer.Api
{
    public static class Box2DApi
    {
#if UNITY_IPHONE && !UNITY_EDITOR
        private const string DllName = "__Internal";
#else
        private const string DllName = "libbox2d";
#endif
        
        public enum JointType
        {
            UnknownJoint = 0,
            RevoluteJoint,
            PrismaticJoint,
            DistanceJoint,
            PulleyJoint,
            MouseJoint,
            GearJoint,
            WheelJoint,
            WeldJoint,
            FrictionJoint,
            MotorJoint
        };
        
        public delegate void DbgCallback(string str);
        public delegate void CollisionCallback(CollisionCallbackData callbackData);
        public delegate void DrawDbgCircleCallback(Vector2 center, float radius, Box2dColor color);
        public delegate void DrawDbgSegmentCallback(Vector2 v1, Vector2 v2, Box2dColor color);
        public delegate void DrawDbgTransformCallback(Vector2 v, Vector2 angle, Box2dColor color);
        
        public delegate void DrawDbgPolygonCallback(
            [MarshalAs(UnmanagedType.LPArray, SizeConst = 10)] [Out] Vector2[] v,
            Int32 vCount, Box2dColor color);
        
        public delegate void ListOfPointersCallback(Int32 count, 
            [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] IntPtr[] ptrArray);
        
        [DllImport(DllName)]
        public static extern void SetContactCallbacks(IntPtr world, CollisionCallback beginContact,
            CollisionCallback endContact, CollisionCallback preSolve, CollisionCallback postSolve);
        
        [DllImport(DllName)]
        public static extern IntPtr UpdateWorld(IntPtr world, float timeStep, int velocityIterations,
            int positionIterations);

        [DllImport(DllName)]
        public static extern IntPtr CreateWorld(Vector2 gravity);

        [DllImport(DllName)]
        public static extern void DestroyWorld(IntPtr world);

        [DllImport(DllName)]
        public static extern IntPtr CreateBody(IntPtr world, int bodyType, Vector2 position, float angle, int entity);

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
        public static extern Vector2 GetPosition(IntPtr body);

        [DllImport(DllName)]
        public static extern void SetPosition(IntPtr body, Vector2 position, bool wake);

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
        public static extern bool RayCast(IntPtr world, Vector2 origin, Vector2 direction,
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
        
        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern void SetBeginContactCallback(IntPtr worldPtr, CollisionCallback callback);

        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern void SetEndContactCallback(IntPtr worldPtr, CollisionCallback callback);

        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern void SetPreSolveCallback(IntPtr worldPtr, CollisionCallback callback);

        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern void SetPostSolveCallback(IntPtr worldPtr, CollisionCallback callback);

        [DllImport(DllName)]
        public static extern B2Filter GetBodyFixturesFilterData(IntPtr body);

        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern void SetDebugDraw(IntPtr world, DrawDbgCircleCallback drawCircle,
            DrawDbgCircleCallback drawPoint, DrawDbgSegmentCallback drawSegment,
            DrawDbgTransformCallback drawTransform, DrawDbgPolygonCallback drawPolygon);


        [DllImport(DllName)]
        public static extern void DebugDraw(IntPtr world);

        [DllImport(DllName)]
        public static extern void SetFlagsForDebugDraw(IntPtr worldPtr, UInt32 box2dDebugDrawFlags = 0);


        [DllImport(DllName)]
        public static extern IntPtr CloneWorld(ref IntPtr[] dataArr, int count, IntPtr world);
        
        [DllImport(DllName)]
        public static extern float GetMass(IntPtr body);

        [DllImport(DllName)]
        public static extern float GetInertia(IntPtr body);

        [DllImport(DllName)]
        public static extern void SetFixedRotation(IntPtr body, bool flag);

        [DllImport(DllName)]
        public static extern bool IsFixedRotation(IntPtr body);

        [DllImport(DllName)]
        public static extern bool IsAwake(IntPtr body);

        [DllImport(DllName)]
        public static extern bool IsSleepingAllowed(IntPtr body);

        [DllImport(DllName)]
        public static extern void SetSleepingAllowed(IntPtr body, bool flag);

        [DllImport(DllName)]
        public static extern bool IsBullet(IntPtr body);

        [DllImport(DllName)]
        public static extern int GetType(IntPtr body);

        [DllImport(DllName)]
        public static extern void SetType(IntPtr body, int type);

        [DllImport(DllName)]
        public static extern void SetGravityScale(IntPtr body, float scale);

        [DllImport(DllName)]
        public static extern float GetGravityScale(IntPtr body);

        [DllImport(DllName)]
        public static extern void SetAwake(IntPtr body, bool flag);

        [DllImport(DllName)]
        public static extern void ApplyTorque(IntPtr body, float torque, bool wake);

        [DllImport(DllName)]
        public static extern void ApplyAngularImpulse(IntPtr body, float impulse, bool wake);

        [DllImport(DllName)]
        public static extern void TryGetContactList(IntPtr body,
            ListOfPointersCallback success, DbgCallback cb);
        
        [DllImport(DllName)]
        public static extern void TryGetContactInfoForBodies(IntPtr body1,
            IntPtr body2, CollisionCallback success);

        // Marshal.PtrToStructure<CollisionCallbackData>(ptr)
        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern void TryGetContactInfosForBody(IntPtr body,
            ListOfPointersCallback success);
        
        
        // JOINTS COMMON
        [DllImport(DllName)]
        public static extern IntPtr CreateJoint(IntPtr world, int jointType,
            IntPtr bodyA, IntPtr bodyB, bool isCollideConnected);
        
        [DllImport(DllName)]
        public static extern void DestroyJoint(IntPtr world, IntPtr joint);
        
        [DllImport(DllName)]
        public static extern int GetJointListCount(IntPtr body);
        
        [DllImport(DllName)]
        public static extern void TryGetJointListFromBody(IntPtr body, ListOfPointersCallback cb);
                    
        [DllImport(DllName)]
        public static extern Vector2 GetJointAnchorA(IntPtr joint);

        [DllImport(DllName)]
        public static extern Vector2 GetJointAnchorB(IntPtr joint);

        [DllImport(DllName)]
        public static extern Vector2 GetJointReactionForce(IntPtr joint, float inv_dt);

        [DllImport(DllName)]
        public static extern float GetJointReactionTorque(IntPtr joint, float inv_dt);
        
        [DllImport(DllName)]
        public static extern IntPtr GetJointBodyA(IntPtr joint);

        [DllImport(DllName)]
        public static extern IntPtr GetJointBodyB(IntPtr joint);

        [DllImport(DllName)]
        public static extern bool IsJointEnabled(IntPtr joint);

        [DllImport(DllName)]
        public static extern void ShiftJointOrigin(IntPtr joint, Vector2 newOrigin);

        // DISTANCE JOINTS
        [DllImport(DllName)]
        public static extern float GetJointLength(IntPtr joint);

        [DllImport(DllName)]
        public static extern float SetJointLength(IntPtr joint, float length);

        [DllImport(DllName)]
        public static extern float GetJointMinLength(IntPtr joint);

        [DllImport(DllName)]
        public static extern float SetJointMinLength(IntPtr joint, float minLength);

        [DllImport(DllName)]
        public static extern float GetJointMaxLength(IntPtr joint);

        [DllImport(DllName)]
        public static extern float SetJointMaxLength(IntPtr joint, float maxLength);

        [DllImport(DllName)]
        public static extern float GetJointCurrentLength(IntPtr joint);

        [DllImport(DllName)]
        public static extern void SetJointStiffness(IntPtr joint, float stiffness);

        [DllImport(DllName)]
        public static extern float GetJointStiffness(IntPtr joint);

        [DllImport(DllName)]
        public static extern void SetJointDamping(IntPtr joint, float damping);

        [DllImport(DllName)]
        public static extern float GetJointDamping(IntPtr joint);
        
        
        // REVOLUTE JOINTS
        [DllImport(DllName)]
        public static extern Vector2 GetJointLocalAnchorA(IntPtr joint);

        [DllImport(DllName)]
        public static extern Vector2 GetJointLocalAnchorB(IntPtr joint);

        [DllImport(DllName)]
        public static extern float GetJointReferenceAngle(IntPtr joint);

        [DllImport(DllName)]
        public static extern float GetJointAngle(IntPtr joint);

        [DllImport(DllName)]
        public static extern float GetJointSpeed(IntPtr joint);

        [DllImport(DllName)]
        public static extern bool IsJointLimitEnabled(IntPtr joint);

        [DllImport(DllName)]
        public static extern void EnableJointLimit(IntPtr joint, bool flag);

        [DllImport(DllName)]
        public static extern float GetJointLowerLimit(IntPtr joint);

        [DllImport(DllName)]
        public static extern float GetJointUpperLimit(IntPtr joint);

        [DllImport(DllName)]
        public static extern void SetJointLimits(IntPtr joint, float lower, float upper);

        [DllImport(DllName)]
        public static extern bool IsJointMotorEnabled(IntPtr joint);

        [DllImport(DllName)]
        public static extern void EnableJointMotor(IntPtr joint, bool flag);

        [DllImport(DllName)]
        public static extern void SetJointMotorSpeed(IntPtr joint, float speed);

        [DllImport(DllName)]
        public static extern float GetJointMotorSpeed(IntPtr joint);

        [DllImport(DllName)]
        public static extern void SetJointMaxMotorTorque(IntPtr joint, float torque);

        [DllImport(DllName)]
        public static extern float GetJointMaxMotorTorque(IntPtr joint);
        
        [DllImport(DllName)]
        public static extern float GetJointMotorTorque(IntPtr joint, float inv_dt);
        
        // MOUSE JOINT
        [DllImport(DllName)]
        public static extern void SetTarget(IntPtr joint, Vector2 target);

        [DllImport(DllName)]
        public static extern Vector2 GetTarget(IntPtr joint);

        [DllImport(DllName)]
        public static extern void SetMaxForce(IntPtr joint, float force);

        [DllImport(DllName)]
        public static extern float GetMaxForce(IntPtr joint);

        [DllImport(DllName)]
        public static extern void SetStiffness(IntPtr joint, float stiffness);

        [DllImport(DllName)]
        public static extern float GetStiffness(IntPtr joint);

        [DllImport(DllName)]
        public static extern void SetDamping(IntPtr joint, float damping);

        [DllImport(DllName)]
        public static extern float GetDamping(IntPtr joint);

    }
}