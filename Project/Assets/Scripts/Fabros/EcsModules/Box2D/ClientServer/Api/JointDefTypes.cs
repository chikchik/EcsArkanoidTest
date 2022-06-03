using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Fabros.EcsModules.Box2D.ClientServer.Api
{
    public static partial class Box2DApi
    {
        private const float FLT_MAX = 3.402823466e+38f;
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
            MotorJoint = int.MaxValue
        };
        

        [StructLayout(LayoutKind.Explicit)]
        public struct b2JointDef
        {
            [FieldOffset(0)] public JointType type;

            [FieldOffset(8)] public int userData;

            [FieldOffset(16)] public IntPtr bodyA;

            [FieldOffset(24)] public IntPtr bodyB;

            [FieldOffset(32)] public bool collideConnected;

            public static b2JointDef Null(
                JointType newType = JointType.UnknownJoint,
                int newUserData = -1,
                IntPtr newBodyA = default,
                IntPtr newBodyB = default,
                bool newCollideConnected = false
            ) => new b2JointDef
            {
                type = newType,
                userData = newUserData,
                bodyA = newBodyA,
                bodyB = newBodyB,
                collideConnected = newCollideConnected
            };
        };

        [StructLayout(LayoutKind.Sequential)]
        public struct b2DistanceJointDef
        {
            public b2JointDef baseClass;
            /// The local anchor point relative to bodyA's origin.
            public Vector2 localAnchorA;

            /// The local anchor point relative to bodyB's origin.
            public Vector2 localAnchorB;

            /// The rest length of this joint. Clamped to a stable minimum value.
            public float length;

            /// Minimum length. Clamped to a stable minimum value.
            public float minLength;

            /// Maximum length. Must be greater than or equal to the minimum length.
            public float maxLength;

            /// The linear stiffness in N/m.
            public float stiffness;

            /// The linear damping in N*s/m.
            public float damping;

            public static b2DistanceJointDef Null => new b2DistanceJointDef
            {
                baseClass = b2JointDef.Null(JointType.DistanceJoint),
                localAnchorA = Vector2.zero,
                localAnchorB = Vector2.zero,
                length = 1.0f,
                minLength = 0.0f,
                maxLength = float.MaxValue,
                stiffness = 0.0f,
                damping = 0.0f
            };
        };

        [StructLayout(LayoutKind.Sequential)]
        public struct b2FrictionJointDef
        {
            public b2JointDef baseClass;
            /// The local anchor point relative to bodyA's origin.
            public Vector2 localAnchorA;

            /// The local anchor point relative to bodyB's origin.
            public Vector2 localAnchorB;

            /// The maximum friction force in N.
            public float maxForce;

            /// The maximum friction torque in N-m.
            public float maxTorque;

            public static b2FrictionJointDef Null => new b2FrictionJointDef
            {
                baseClass = b2JointDef.Null(JointType.FrictionJoint),
                localAnchorA = Vector2.zero,
                localAnchorB = Vector2.zero,
                maxForce = 0.0f,
                maxTorque = 0.0f
            };
        };
        
        [StructLayout(LayoutKind.Sequential)]
        public struct b2GearJointDef
        {
            public b2JointDef baseClass;
            /// The first revolute/prismatic joint attached to the gear joint.
            public IntPtr joint1;

            /// The second revolute/prismatic joint attached to the gear joint.
            public IntPtr joint2;

            /// The gear ratio.
            public float ratio;

            public static b2GearJointDef Null => new b2GearJointDef
            {
                baseClass = b2JointDef.Null(JointType.GearJoint),
                joint1 = IntPtr.Zero,
                joint2 = IntPtr.Zero,
                ratio = 1.0f
            };
        };
        
        [StructLayout(LayoutKind.Sequential)]
        public struct b2MotorJointDef
        {
            public b2JointDef baseClass;
            /// Position of bodyB minus the position of bodyA, in bodyA's frame, in meters.
            public Vector2 linearOffset;

            /// The bodyB angle minus bodyA angle in radians.
            public float angularOffset;

            /// The maximum motor force in N.
            public float maxForce;

            /// The maximum motor torque in N-m.
            public float maxTorque;

            /// Position correction factor in the range [0,1].
            public float correctionFactor;
            
            public static b2MotorJointDef Null => new b2MotorJointDef
            {
                baseClass = b2JointDef.Null(JointType.MotorJoint),
                linearOffset = Vector2.zero,
                angularOffset = 0.0f,
                maxForce = 1.0f,
                maxTorque = 1.0f,
                correctionFactor = 0.3f
            };
        };
        
        [StructLayout(LayoutKind.Sequential)]
        public struct b2MouseJointDef
        {
            public b2JointDef baseClass;
            /// The initial world target point. This is assumed
            /// to coincide with the body anchor initially.
            public Vector2 target;

            /// The maximum constraint force that can be exerted
            /// to move the candidate body. Usually you will express
            /// as some multiple of the weight (multiplier * mass * gravity).
            public float maxForce;

            /// The linear stiffness in N/m
            public float stiffness;

            /// The linear damping in N*s/m
            public float damping;
            
            public static b2MouseJointDef Null => new b2MouseJointDef
            {
                baseClass = b2JointDef.Null(JointType.MouseJoint),
                target = Vector2.zero,
                maxForce = 0.0f,
                stiffness = 0.0f,
                damping = 0.0f
            };
        };
        
        [StructLayout(LayoutKind.Sequential)]
        public struct b2PrismaticJointDef
        {
            public b2JointDef baseClass;
            /// The local anchor point relative to bodyA's origin.
            public Vector2 localAnchorA;

            /// The local anchor point relative to bodyB's origin.
            public Vector2 localAnchorB;

            /// The local translation unit axis in bodyA.
            public Vector2 localAxisA;

            /// The constrained angle between the bodies: bodyB_angle - bodyA_angle.
            public float referenceAngle;

            /// Enable/disable the joint limit.
            public bool enableLimit;

            /// The lower translation limit, usually in meters.
            public float lowerTranslation;

            /// The upper translation limit, usually in meters.
            public float upperTranslation;

            /// Enable/disable the joint motor.
            public bool enableMotor;

            /// The maximum motor torque, usually in N-m.
            public float maxMotorForce;

            /// The desired motor speed in radians per second.
            public float motorSpeed;
            
            public static b2PrismaticJointDef Null => new b2PrismaticJointDef
            {
                baseClass = b2JointDef.Null(JointType.PrismaticJoin),
                localAnchorA = Vector2.zero,
                localAnchorB = Vector2.zero,
                localAxisA = new Vector2(1.0f, 0.0f),
                referenceAngle = 0.0f,
                enableLimit = false,
                lowerTranslation = 0.0f,
                upperTranslation = 0.0f,
                enableMotor = false,
                maxMotorForce = 0.0f,
                motorSpeed = 0.0f
            };
        };
        
        [StructLayout(LayoutKind.Sequential)]
        public struct b2PulleyJointDef
        {
            public b2JointDef baseClass;
            
            /// The first ground anchor in world coordinates. This point never moves.
            public Vector2 groundAnchorA;

            /// The second ground anchor in world coordinates. This point never moves.
            public Vector2 groundAnchorB;

            /// The local anchor point relative to bodyA's origin.
            public Vector2 localAnchorA;

            /// The local anchor point relative to bodyB's origin.
            public Vector2 localAnchorB;

            /// The a reference length for the segment attached to bodyA.
            public float lengthA;

            /// The a reference length for the segment attached to bodyB.
            public float lengthB;

            /// The pulley ratio, used to simulate a block-and-tackle.
            public float ratio;

            public static b2PulleyJointDef Null => new b2PulleyJointDef
            {
                baseClass = b2JointDef.Null(
                    JointType.PulleyJoint, newCollideConnected:true),
                groundAnchorA = new Vector2(-1.0f, 0.0f),
                groundAnchorB = new Vector2(1.0f, 0.0f),
                localAnchorA = new Vector2(-1.0f, 0.0f),
                localAnchorB = new Vector2(1.0f, 0.0f),
                lengthA = 0.0f,
                lengthB = 0.0f,
                ratio = 1.0f
            };
            
        };
        
        [StructLayout(LayoutKind.Sequential)]
        public struct b2RevoluteJointDef
        {
            public b2JointDef baseClass;
            /// The local anchor point relative to bodyA's origin.
            public Vector2 localAnchorA;

            /// The local anchor point relative to bodyB's origin.
            public Vector2 localAnchorB;

            /// The bodyB angle minus bodyA angle in the reference state (radians).
            public float referenceAngle;

            /// A flag to enable joint limits.
            public bool enableLimit;

            /// The lower angle for the joint limit (radians).
            public float lowerAngle;

            /// The upper angle for the joint limit (radians).
            public float upperAngle;

            /// A flag to enable the joint motor.
            public bool enableMotor;

            /// The desired motor speed. Usually in radians per second.
            public float motorSpeed;

            /// The maximum motor torque used to achieve the desired motor speed.
            /// Usually in N-m.
            public float maxMotorTorque;

            public static b2RevoluteJointDef Null => new b2RevoluteJointDef
            {
                baseClass = b2JointDef.Null(JointType.RevoluteJoint),
                localAnchorA = Vector2.zero,
                localAnchorB = Vector2.zero,
                referenceAngle = 0.0f,
                enableLimit = false,
                lowerAngle = 0.0f,
                upperAngle = 0.0f,
                enableMotor = false,
                motorSpeed = 0.0f,
                maxMotorTorque = 0.0f
            };
        };
        
        [StructLayout(LayoutKind.Sequential)]
        public struct b2WeldJointDef
        {
            public b2JointDef baseClass;
            /// The local anchor point relative to bodyA's origin.
            public Vector2 localAnchorA;

            /// The local anchor point relative to bodyB's origin.
            public Vector2 localAnchorB;

            /// The bodyB angle minus bodyA angle in the reference state (radians).
            public float referenceAngle;

            /// The rotational stiffness in N*m
            /// Disable softness with a value of 0
            public float stiffness;

            /// The rotational damping in N*m*s
            public float damping;

            public static b2WeldJointDef Null => new b2WeldJointDef
            {
                baseClass = b2JointDef.Null(JointType.WeldJoint),
                localAnchorA = Vector2.zero,
                localAnchorB = Vector2.zero,
                referenceAngle = 0.0f,
                stiffness = 0.0f,
                damping = 0.0f
            };            
        };
        
        [StructLayout(LayoutKind.Sequential)]
        public struct b2WheelJointDef
        {
            public b2JointDef baseClass;
            /// The local anchor point relative to bodyA's origin.
            public Vector2 localAnchorA;

            /// The local anchor point relative to bodyB's origin.
            public Vector2 localAnchorB;

            /// The local translation axis in bodyA.
            public Vector2 localAxisA;

            /// Enable/disable the joint limit.
            public bool enableLimit;

            /// The lower translation limit, usually in meters.
            public float lowerTranslation;

            /// The upper translation limit, usually in meters.
            public float upperTranslation;

            /// Enable/disable the joint motor.
            public bool enableMotor;

            /// The maximum motor torque, usually in N-m.
            public float maxMotorTorque;

            /// The desired motor speed in radians per second.
            public float motorSpeed;

            /// Suspension stiffness. Typically in units N/m.
            public float stiffness;

            /// Suspension damping. Typically in units of N*s/m.
            public float damping;

            public static b2WheelJointDef Null => new b2WheelJointDef
            {
                baseClass = b2JointDef.Null(JointType.WheelJoint),
                localAnchorA = Vector2.zero,
                localAnchorB = Vector2.zero,
                localAxisA = new Vector2(1.0f, 0.0f),
                enableLimit = false,
                lowerTranslation = 0.0f,
                upperTranslation = 0.0f,
                enableMotor = false,
                maxMotorTorque = 0.0f,
                motorSpeed = 0.0f,
                stiffness = 0.0f,
                damping = 0.0f
            };
        };
    }
}