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
            RevoluteJoint = 1,
            PrismaticJoin = 2,
            DistanceJoint = 3,
            PulleyJoint = 4,
            MouseJoint = 5,
            GearJoint = 6,
            WheelJoint = 7,
            WeldJoint = 8,
            FrictionJoint = 9,
            MotorJoint = 10
        };
        
        public struct RaycastOutputReturnType
        {
            public IntPtr Body;
            public Vector2 Normal;
            public Vector2 Point;
            public float Fraction;
        }
        
        public delegate void DbgCallback(string str);

        public delegate void CollisionCallback(CollisionCallbackData callbackData);

        public delegate void DrawDbgCircleCallback(Vector2 center, float radius, Box2dColor color);

        public delegate void DrawDbgSegmentCallback(Vector2 v1, Vector2 v2, Box2dColor color);

        public delegate void DrawDbgTransformCallback(Vector2 v, Vector2 angle, Box2dColor color);

        public delegate void DrawDbgPolygonCallback(
            [MarshalAs(UnmanagedType.LPArray, SizeConst = 10)] [Out]
            Vector2[] v,
            int vCount, Box2dColor color);

        public delegate void ListOfPointersCallback(int count,
            [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)]
            IntPtr[] ptrArray);

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
            ref RaycastOutputReturnType hit, float distance, ushort layerMask = 0);

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
        public static extern void SetDebugDraw(IntPtr world, IntPtr debugDraw);
        
        [DllImport(DllName)]
        public static extern IntPtr CreateBox2dDebugDraw(IntPtr world, DrawDbgCircleCallback drawCircle,
                DrawDbgCircleCallback drawPoint, DrawDbgSegmentCallback drawSegment,
                DrawDbgTransformCallback drawTransform, DrawDbgPolygonCallback drawPolygon);


        [DllImport(DllName)]
        public static extern void DebugDraw(IntPtr world);

        [DllImport(DllName)]
        public static extern void SetFlagsForDebugDraw(IntPtr worldPtr, uint box2dDebugDrawFlags = 0);


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


// Common for ALL joints
        [DllImport(DllName)]
        public static extern Vector2 GetJointAnchorA(IntPtr joint);

        [DllImport(DllName)]
        public static extern Vector2 GetJointAnchorB(IntPtr joint);

        [DllImport(DllName)]
        public static extern Vector2 GetJointReactionForce(IntPtr joint, float inv_dt);

        [DllImport(DllName)]
        public static extern float GetJointReactionTorque(IntPtr joint, float inv_dt);

        [DllImport(DllName)]
        public static extern int GetJointType(IntPtr joint);

        [DllImport(DllName)]
        public static extern IntPtr GetJointBodyA(IntPtr joint);

        [DllImport(DllName)]
        public static extern IntPtr GetJointBodyB(IntPtr joint);

        [DllImport(DllName)]
        public static extern bool IsJointEnabled(IntPtr joint);

        [DllImport(DllName)]
        public static extern void ShiftJointOrigin(IntPtr joint, Vector2 newOrigin);

// common for mouse, distance
        [DllImport(DllName)]
        public static extern void SetJointStiffness(IntPtr joint, float stiffness);

        [DllImport(DllName)]
        public static extern float GetJointStiffness(IntPtr joint);

        [DllImport(DllName)]
        public static extern void SetJointDamping(IntPtr joint, float damping);

        [DllImport(DllName)]
        public static extern float GetJointDamping(IntPtr joint);

// common for revolute, friction, prismatic
        [DllImport(DllName)]
        public static extern Vector2 GetJointLocalAnchorA(IntPtr joint);

        [DllImport(DllName)]
        public static extern Vector2 GetJointLocalAnchorB(IntPtr joint);

// common for mouse, friction, motor
        [DllImport(DllName)]
        public static extern float GetJointMaxForce(IntPtr joint);

        [DllImport(DllName)]
        public static extern void SetJointMaxForce(IntPtr joint, float force);

// common for motor, friction
        [DllImport(DllName)]
        public static extern void SetJointMaxTorque(IntPtr joint, float torque);

        [DllImport(DllName)]
        public static extern float GetJointMaxTorque(IntPtr joint);

// common for prismatic, revolute, weld
        [DllImport(DllName)]
        public static extern float GetJointReferenceAngle(IntPtr joint);

// common for prismatic, wheel
        [DllImport(DllName)]
        public static extern Vector2 GetJointLocalAxisA(IntPtr joint);

        [DllImport(DllName)]
        public static extern float GetJointTranslation(IntPtr joint);

//common for prismatic, revolute
        [DllImport(DllName)]
        public static extern float GetJointSpeed(IntPtr joint);

//common for prismatic, revolute, wheel
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

// common for pulley, gear
        [DllImport(DllName)]
        public static extern float GetJointRatio(IntPtr joint);

// common for revolute, wheel
        [DllImport(DllName)]
        public static extern float GetJointAngle(IntPtr joint);

        [DllImport(DllName)]
        public static extern void SetJointMaxMotorTorque(IntPtr joint, float torque);

        [DllImport(DllName)]
        public static extern float GetJointMaxMotorTorque(IntPtr joint);

        [DllImport(DllName)]
        public static extern float GetJointMotorTorque(IntPtr joint, float inv_dt);

// GEAR JOINT
        [DllImport(DllName)]
        public static extern IntPtr GetJoint1(IntPtr joint);

        [DllImport(DllName)]
        public static extern IntPtr GetJoint2(IntPtr joint);

        [DllImport(DllName)]
        public static extern void SetJointRatio(IntPtr joint, float ratio);

        [DllImport(DllName)]
        public static extern int GetMotorJointTypeA(IntPtr joint);

        [DllImport(DllName)]
        public static extern int GetMotorJointTypeB(IntPtr joint);

        [DllImport(DllName)]
        public static extern IntPtr GetMotorJointBodyC(IntPtr joint);

        [DllImport(DllName)]
        public static extern IntPtr GetMotorJointBodyD(IntPtr joint);

// DISTANCE JOINT
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

// MOTOR JOINT
        [DllImport(DllName)]
        public static extern void SetJointLinearOffset(IntPtr joint, Vector2 linearOffset);

        [DllImport(DllName)]
        public static extern Vector2 GetJointLinearOffset(IntPtr joint);

        [DllImport(DllName)]
        public static extern void SetJointAngularOffset(IntPtr joint, float angularOffset);

        [DllImport(DllName)]
        public static extern float GetJointAngularOffset(IntPtr joint);

        [DllImport(DllName)]
        public static extern void SetJointCorrectionFactor(IntPtr joint, float factor);

        [DllImport(DllName)]
        public static extern float GetJointCorrectionFactor(IntPtr joint);

// MOUSE JOINT
        [DllImport(DllName)]
        public static extern void SetJointTarget(IntPtr joint, Vector2 target);

        [DllImport(DllName)]
        public static extern Vector2 GetJointTarget(IntPtr joint);

// PRISMATIC JOINT
        [DllImport(DllName)]
        public static extern void SetJointMaxMotorForce(IntPtr joint, float force);

        [DllImport(DllName)]
        public static extern float GetJointMaxMotorForce(IntPtr joint);

        [DllImport(DllName)]
        public static extern float GetJointMotorForce(IntPtr joint, float inv_dt);

// PULLEY JOINT
        [DllImport(DllName)]
        public static extern Vector2 GetJointGroundAnchorA(IntPtr joint);

        [DllImport(DllName)]
        public static extern Vector2 GetJointGroundAnchorB(IntPtr joint);

        [DllImport(DllName)]
        public static extern float GetJointLengthA(IntPtr joint);

        [DllImport(DllName)]
        public static extern float GetJointLengthB(IntPtr joint);

        [DllImport(DllName)]
        public static extern float GetJointCurrentLengthA(IntPtr joint);

        [DllImport(DllName)]
        public static extern float GetJointCurrentLengthB(IntPtr joint);

// WHEEL JOINT
        [DllImport(DllName)]
        public static extern float GetJointLinearSpeed(IntPtr joint);

        [DllImport(DllName)]
        public static extern float GetJointAngularSpeed(IntPtr joint);
    }
}