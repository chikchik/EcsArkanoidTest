using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Fabros.EcsModules.Box2D.ClientServer.Api
{
    public static partial class Box2DApi
    {
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
        
        [StructLayout(LayoutKind.Explicit)]
        public class b2JointDef
        {
            [FieldOffset(0)] public Box2DApi.JointType type = JointType.UnknownJoint;

            [FieldOffset(8)] public int userData = -1;

            [FieldOffset(16)] public IntPtr bodyA = IntPtr.Zero;

            [FieldOffset(24)] public IntPtr bodyB = IntPtr.Zero;

            [FieldOffset(32)] public bool collideConnected = false;

            public b2JointDef(Box2DApi.JointType type, IntPtr bodyA,
                IntPtr bodyB, bool collideConnected)
            {
                this.type = type;
                this.bodyA = bodyA;
                this.bodyB = bodyB;
                this.collideConnected = collideConnected;
            }
        };

        [StructLayout(LayoutKind.Sequential)]
        public class b2DistanceJointDef : b2JointDef
        {
            /// The local anchor point relative to bodyA's origin.
            public Vector2 localAnchorA = Vector2.zero;

            /// The local anchor point relative to bodyB's origin.
            public Vector2 localAnchorB = Vector2.zero;

            /// The rest length of this joint. Clamped to a stable minimum value.
            public float length = 0.0f;

            /// Minimum length. Clamped to a stable minimum value.
            public float minLength = 5f;

            /// Maximum length. Must be greater than or equal to the minimum length.
            public float maxLength = 5;

            /// The linear stiffness in N/m.
            public float stiffness = 0.0f;

            /// The linear damping in N*s/m.
            public float damping = 0.0f;

            public b2DistanceJointDef(IntPtr bodyA,
                IntPtr bodyB, bool collideConnected)
                : base(JointType.DistanceJoint, bodyA, bodyB, collideConnected)
            {
            }
        };

        [StructLayout(LayoutKind.Sequential)]
        public class b2FrictionJointDef : b2JointDef
        {
            /// The local anchor point relative to bodyA's origin.
            public Vector2 localAnchorA = Vector2.zero;

            /// The local anchor point relative to bodyB's origin.
            public Vector2 localAnchorB = Vector2.zero;

            /// The maximum friction force in N.
            public float maxForce = 0.0f;

            /// The maximum friction torque in N-m.
            public float maxTorque = 0.0f;

            public b2FrictionJointDef(IntPtr bodyA,
                IntPtr bodyB, bool collideConnected)
                : base(JointType.FrictionJoint, bodyA, bodyB, collideConnected) {}
        };
        
        [StructLayout(LayoutKind.Sequential)]
        public class b2GearJointDef : b2JointDef
        {
            /// The first revolute/prismatic joint attached to the gear joint.
            public IntPtr joint1 = IntPtr.Zero;

            /// The second revolute/prismatic joint attached to the gear joint.
            public IntPtr joint2 = IntPtr.Zero;

            /// The gear ratio.
            public float ratio = 1.0f;
            
            public b2GearJointDef(IntPtr bodyA,
                IntPtr bodyB, bool collideConnected)
                : base(JointType.GearJoint, bodyA, bodyB, collideConnected) {}
        };
        
        [StructLayout(LayoutKind.Sequential)]
        public class b2MotorJointDef : b2JointDef
        {
            /// Position of bodyB minus the position of bodyA, in bodyA's frame, in meters.
            public Vector2 linearOffset = Vector2.zero;

            /// The bodyB angle minus bodyA angle in radians.
            public float angularOffset= 0.0f;

            /// The maximum motor force in N.
            public float maxForce = 1.0f;

            /// The maximum motor torque in N-m.
            public float maxTorque = 1.0f;

            /// Position correction factor in the range [0,1].
            public float correctionFactor = 0.3f;
            
            public b2MotorJointDef(IntPtr bodyA,
                IntPtr bodyB, bool collideConnected)
                : base(JointType.MotorJoint, bodyA, bodyB, collideConnected) {}
        };
        
        [StructLayout(LayoutKind.Sequential)]
        public class b2MouseJointDef : b2JointDef
        {
            /// The initial world target point. This is assumed
            /// to coincide with the body anchor initially.
            public Vector2 target = Vector2.zero;

            /// The maximum constraint force that can be exerted
            /// to move the candidate body. Usually you will express
            /// as some multiple of the weight (multiplier * mass * gravity).
            public float maxForce = 0.0f;

            /// The linear stiffness in N/m
            public float stiffness = 0.0f;

            /// The linear damping in N*s/m
            public float damping = 0.0f;
            
            public b2MouseJointDef(IntPtr bodyA,
                IntPtr bodyB, bool collideConnected)
                : base(JointType.MouseJoint, bodyA, bodyB, collideConnected) {}
        };
        
        [StructLayout(LayoutKind.Sequential)]
        public class b2PrismaticJointDef : b2JointDef
        {
            /// The local anchor point relative to bodyA's origin.
            public Vector2 localAnchorA = Vector2.zero;

            /// The local anchor point relative to bodyB's origin.
            public Vector2 localAnchorB = Vector2.zero;

            /// The local translation unit axis in bodyA.
            public Vector2 localAxisA = Vector2.zero;

            /// The constrained angle between the bodies: bodyB_angle - bodyA_angle.
            public float referenceAngle = 0.0f;

            /// Enable/disable the joint limit.
            public bool enableLimit = false;

            /// The lower translation limit, usually in meters.
            public float lowerTranslation = 0.0f;

            /// The upper translation limit, usually in meters.
            public float upperTranslation = 0.0f;

            /// Enable/disable the joint motor.
            public bool enableMotor = false;

            /// The maximum motor torque, usually in N-m.
            public float maxMotorForce = 0.0f;

            /// The desired motor speed in radians per second.
            public float motorSpeed = 0.0f;
            
            public b2PrismaticJointDef(IntPtr bodyA,
                IntPtr bodyB, bool collideConnected)
                : base(JointType.PrismaticJoin, bodyA, bodyB, collideConnected) {}
        };
        
        [StructLayout(LayoutKind.Sequential)]
        public class b2PulleyJointDef : b2JointDef
        {
            /// The first ground anchor in world coordinates. This point never moves.
            Vector2 groundAnchorA = Vector2.zero;

            /// The second ground anchor in world coordinates. This point never moves.
            Vector2 groundAnchorB = Vector2.zero;

            /// The local anchor point relative to bodyA's origin.
            Vector2 localAnchorA = Vector2.zero;

            /// The local anchor point relative to bodyB's origin.
            Vector2 localAnchorB = Vector2.zero;

            /// The a reference length for the segment attached to bodyA.
            float lengthA = 0.0f;

            /// The a reference length for the segment attached to bodyB.
            float lengthB = 0.0f;

            /// The pulley ratio, used to simulate a block-and-tackle.
            float ratio = 1.0f;
            
            public b2PulleyJointDef(IntPtr bodyA,
                IntPtr bodyB, bool collideConnected)
                : base(JointType.PulleyJoint, bodyA, bodyB, collideConnected) {}
        };
        
        [StructLayout(LayoutKind.Sequential)]
        public class b2RevoluteJointDef : b2JointDef
        {
            /// The local anchor point relative to bodyA's origin.
            Vector2 localAnchorA = Vector2.zero;

            /// The local anchor point relative to bodyB's origin.
            Vector2 localAnchorB = Vector2.zero;

            /// The bodyB angle minus bodyA angle in the reference state (radians).
            float referenceAngle = 0.0f;

            /// A flag to enable joint limits.
            bool enableLimit = false;

            /// The lower angle for the joint limit (radians).
            float lowerAngle = 0.0f;

            /// The upper angle for the joint limit (radians).
            float upperAngle = 0.0f;

            /// A flag to enable the joint motor.
            bool enableMotor = false;

            /// The desired motor speed. Usually in radians per second.
            float motorSpeed = 0.0f;

            /// The maximum motor torque used to achieve the desired motor speed.
            /// Usually in N-m.
            float maxMotorTorque = 0.0f;
            
            public b2RevoluteJointDef(IntPtr bodyA,
                IntPtr bodyB, bool collideConnected)
                : base(JointType.RevoluteJoint, bodyA, bodyB, collideConnected) {}
        };
        
        [StructLayout(LayoutKind.Sequential)]
        public class b2WeldJointDef : b2JointDef
        {
            /// The local anchor point relative to bodyA's origin.
            Vector2 localAnchorA = Vector2.zero;

            /// The local anchor point relative to bodyB's origin.
            Vector2 localAnchorB = Vector2.zero;

            /// The bodyB angle minus bodyA angle in the reference state (radians).
            float referenceAngle = 0.0f;

            /// The rotational stiffness in N*m
            /// Disable softness with a value of 0
            float stiffness = 0.0f;

            /// The rotational damping in N*m*s
            float damping = 0.0f;
            
            public b2WeldJointDef(IntPtr bodyA,
                IntPtr bodyB, bool collideConnected)
                : base(JointType.WeldJoint, bodyA, bodyB, collideConnected) {}
        };
        
        [StructLayout(LayoutKind.Sequential)]
        public class b2WheelJointDef : b2JointDef
        {
        /// The local anchor point relative to bodyA's origin.
	        Vector2 localAnchorA = Vector2.zero;

	        /// The local anchor point relative to bodyB's origin.
	        Vector2 localAnchorB = Vector2.zero;

	        /// The local translation axis in bodyA.
	        Vector2 localAxisA = Vector2.zero;

	        /// Enable/disable the joint limit.
	        bool enableLimit = false;

	        /// The lower translation limit, usually in meters.
	        float lowerTranslation = 0.0f;

	        /// The upper translation limit, usually in meters.
	        float upperTranslation = 0.0f;

	        /// Enable/disable the joint motor.
	        bool enableMotor = false;

	        /// The maximum motor torque, usually in N-m.
	        float maxMotorTorque = 0.0f;

	        /// The desired motor speed in radians per second.
	        float motorSpeed = 0.0f;

	        /// Suspension stiffness. Typically in units N/m.
	        float stiffness = 0.0f;

	        /// Suspension damping. Typically in units of N*s/m.
	        float damping = 0.0f;
    
            public b2WheelJointDef(IntPtr bodyA,
                IntPtr bodyB, bool collideConnected)
                : base(JointType.WheelJoint, bodyA, bodyB, collideConnected) {}
        };  
    }
}