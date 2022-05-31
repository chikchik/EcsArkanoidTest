#include <joint.h>

DllExport Vector2 GetJointAnchorA(b2Joint* joint)
{
    b2Vec2 vv = joint->GetAnchorA();

    Vector2 v
    {
        vv.x,
        vv.y
    };
    return v;
}

DllExport Vector2 GetJointAnchorB(b2Joint* joint)
{
    b2Vec2 vv = joint->GetAnchorB();


    Vector2 v
    {
        vv.x,
        vv.y
    };
    return v;
}

DllExport Vector2 GetJointReactionForce(b2Joint* joint, float inv_dt)
{
    b2Vec2 vv = joint->GetReactionForce(inv_dt);

    Vector2 v
    {
        vv.x,
        vv.y
    };
    return v;
}

DllExport float GetJointReactionTorque(b2Joint* joint, float inv_dt)
{
    return joint->GetReactionTorque(inv_dt);
}

DllExport int GetJointType(b2Joint* joint)
{
    return (int)joint->GetType();
}

DllExport b2Body* GetJointBodyA(b2Joint* joint)
{
    return joint->GetBodyA();
}

DllExport b2Body* GetJointBodyB(b2Joint* joint)
{
    return joint->GetBodyB();
}

DllExport bool IsJointEnabled(b2Joint* joint)
{
    return joint->IsEnabled();
}

DllExport void ShiftJointOrigin(b2Joint* joint, Vector2 newOrigin)
{
    b2Vec2 v
    {
        newOrigin.x,
        newOrigin.y
    };

    switch (joint->GetType())
    {
    case e_mouseJoint:
        static_cast<b2MouseJoint*>(joint)->ShiftOrigin(v);
        break;
    case e_pulleyJoint:
        static_cast<b2PulleyJoint*>(joint)->ShiftOrigin(v);
        break;
    default:
        joint->ShiftOrigin(v);
        break;
    }
}

DllExport void SetJointStiffness(b2Joint* joint, float stiffness)
{
    switch (joint->GetType())
    {
    case e_distanceJoint:
        static_cast<b2DistanceJoint*>(joint)->SetStiffness(stiffness);
        break;
    case e_mouseJoint:
        static_cast<b2MouseJoint*>(joint)->SetStiffness(stiffness);
        break;
    case e_weldJoint:
        static_cast<b2WeldJoint*>(joint)->SetStiffness(stiffness);
        break;
    default:
        break;
    }
}

DllExport float GetJointStiffness(b2Joint* joint)
{
    switch (joint->GetType())
    {
    case e_distanceJoint:
        return static_cast<b2DistanceJoint*>(joint)->GetStiffness();
    case e_mouseJoint:
        return static_cast<b2MouseJoint*>(joint)->GetStiffness();
    case e_weldJoint:
        return static_cast<b2WeldJoint*>(joint)->GetStiffness();

    default:
        return -1.0f;
    }
}

DllExport void SetJointDamping(b2Joint* joint, float damping)
{
    switch (joint->GetType())
    {
    case e_distanceJoint:
        static_cast<b2DistanceJoint*>(joint)->SetDamping(damping);
        break;
    case e_mouseJoint:
        static_cast<b2MouseJoint*>(joint)->SetDamping(damping);
        break;
    case e_weldJoint:
        static_cast<b2WeldJoint*>(joint)->SetDamping(damping);
        break;
    default:
        break;
    }
}

DllExport float GetJointDamping(b2Joint* joint)
{
    switch (joint->GetType())
    {
    case e_distanceJoint:
        return static_cast<b2DistanceJoint*>(joint)->GetDamping();
    case e_mouseJoint:
        return static_cast<b2MouseJoint*>(joint)->GetDamping();
    case e_weldJoint:
        return static_cast<b2WeldJoint*>(joint)->GetDamping();
    default:
        return -1.0;
    }
}

DllExport Vector2 GetJointLocalAnchorA(b2Joint* joint)
{
    b2Vec2 vv;

    switch (joint->GetType())
    {
    case e_revoluteJoint:
        vv = static_cast<b2RevoluteJoint*>(joint)->GetLocalAnchorA();
        break;
    case e_frictionJoint:
        vv = static_cast<b2FrictionJoint*>(joint)->GetLocalAnchorA();
        break;
    case e_prismaticJoint:
        vv = static_cast<b2PrismaticJoint*>(joint)->GetLocalAnchorA();
        break;
    case e_weldJoint:
        vv = static_cast<b2WeldJoint*>(joint)->GetLocalAnchorA();
        break;
    case e_wheelJoint:
        vv = static_cast<b2WheelJoint*>(joint)->GetLocalAnchorA();
        break;
    }

    Vector2 v
    {
        vv.x,
        vv.y
    };
    return v;
}

DllExport Vector2 GetJointLocalAnchorB(b2Joint* joint)
{
    b2Vec2 vv;

    switch (joint->GetType())
    {
    case e_revoluteJoint:
        vv = static_cast<b2RevoluteJoint*>(joint)->GetLocalAnchorB();
        break;
    case e_frictionJoint:
        vv = static_cast<b2FrictionJoint*>(joint)->GetLocalAnchorB();
        break;
    case e_prismaticJoint:
        vv = static_cast<b2PrismaticJoint*>(joint)->GetLocalAnchorB();
        break;
    case e_weldJoint:
        vv = static_cast<b2WeldJoint*>(joint)->GetLocalAnchorB();
        break;
    case e_wheelJoint:
        vv = static_cast<b2WheelJoint*>(joint)->GetLocalAnchorB();
        break;
    }

    Vector2 v
    {
        vv.x,
        vv.y
    };
    return v;
}

DllExport void SetJointMaxForce(b2Joint* joint, float force)
{
    switch (joint->GetType())
    {
    case e_frictionJoint:
        static_cast<b2MouseJoint*>(joint)->SetMaxForce(force);
        break;
    case e_mouseJoint:
        static_cast<b2MouseJoint*>(joint)->SetMaxForce(force);
        break;
    case e_motorJoint:
        static_cast<b2MotorJoint*>(joint)->SetMaxForce(force);
        break;
    }
}

DllExport float GetJointMaxForce(b2Joint* joint)
{
    switch (joint->GetType())
    {
    case e_frictionJoint:
        return static_cast<b2MouseJoint*>(joint)->GetMaxForce();
    case e_mouseJoint:
        return static_cast<b2MouseJoint*>(joint)->GetMaxForce();
    case e_motorJoint:
        return static_cast<b2MotorJoint*>(joint)->GetMaxForce();
    default:
        return -1.0;
    }
}

DllExport void SetJointMaxTorque(b2Joint* joint, float torque)
{
    switch (joint->GetType())
    {
    case e_frictionJoint:
        static_cast<b2FrictionJoint*>(joint)->SetMaxTorque(torque);
        break;
    case e_motorJoint:
        static_cast<b2MotorJoint*>(joint)->SetMaxTorque(torque);
        break;
    }
}

DllExport float GetJointMaxTorque(b2Joint* joint)
{
    switch (joint->GetType())
    {
    case e_frictionJoint:
        return static_cast<b2FrictionJoint*>(joint)->GetMaxTorque();
    case e_motorJoint:
        return static_cast<b2MotorJoint*>(joint)->GetMaxTorque();
    default:
        return -1.0;
    }
}

DllExport float GetJointReferenceAngle(b2Joint* joint)
{
    switch (joint->GetType())
    {
    case e_revoluteJoint:
        return static_cast<b2RevoluteJoint*>(joint)->GetReferenceAngle();
    case e_weldJoint:
        return static_cast<b2WeldJoint*>(joint)->GetReferenceAngle();
    case e_prismaticJoint:
        return static_cast<b2PrismaticJoint*>(joint)->GetReferenceAngle();
    default:
        return -1.0;
    }
}

DllExport Vector2 GetJointLocalAxisA(b2Joint* joint)
{
    b2Vec2 vv;
    switch (joint->GetType())
    {
    case e_wheelJoint:
        vv = static_cast<b2WheelJoint*>(joint)->GetLocalAxisA();
    case e_prismaticJoint:
        vv = static_cast<b2PrismaticJoint*>(joint)->GetLocalAxisA();
    }

    Vector2 v
    {
        vv.x,
        vv.y
    };
    return v;
}

DllExport float GetJointTranslation(b2Joint* joint)
{
    switch (joint->GetType())
    {
    case e_wheelJoint:
        return static_cast<b2WheelJoint*>(joint)->GetJointTranslation();
    case e_prismaticJoint:
        return static_cast<b2PrismaticJoint*>(joint)->GetJointTranslation();
    default:
        return -1.0;
    }
}

DllExport float GetJointSpeed(b2Joint* joint)
{
    switch (joint->GetType())
    {
    case e_revoluteJoint:
        return static_cast<b2RevoluteJoint*>(joint)->GetJointSpeed();
    case e_prismaticJoint:
        return static_cast<b2PrismaticJoint*>(joint)->GetJointSpeed();
    default:
        return -1.0;
    }
}

DllExport bool IsJointLimitEnabled(b2Joint* joint)
{
    switch (joint->GetType())
    {
    case e_revoluteJoint:
        return static_cast<b2RevoluteJoint*>(joint)->IsLimitEnabled();
    case e_prismaticJoint:
        return static_cast<b2PrismaticJoint*>(joint)->IsLimitEnabled();
    case e_wheelJoint:
        return static_cast<b2WheelJoint*>(joint)->IsLimitEnabled();
    default:
        return 0;
    }
}

DllExport void EnableJointLimit(b2Joint* joint, bool flag)
{
    switch (joint->GetType())
    {
    case e_revoluteJoint:
        static_cast<b2RevoluteJoint*>(joint)->EnableLimit(flag);
        break;
    case e_prismaticJoint:
        static_cast<b2PrismaticJoint*>(joint)->EnableLimit(flag);
        break;
    case e_wheelJoint:
        static_cast<b2WheelJoint*>(joint)->EnableLimit(flag);
        break;
    }
}

DllExport float GetJointLowerLimit(b2Joint* joint)
{
    switch (joint->GetType())
    {
    case e_revoluteJoint:
        return static_cast<b2RevoluteJoint*>(joint)->GetLowerLimit();
    case e_prismaticJoint:
        return static_cast<b2PrismaticJoint*>(joint)->GetLowerLimit();
    case e_wheelJoint:
        return static_cast<b2WheelJoint*>(joint)->GetLowerLimit();
    default:
        return -1.0;
    }
}

DllExport float GetJointUpperLimit(b2Joint* joint)
{
    switch (joint->GetType())
    {
    case e_revoluteJoint:
        return static_cast<b2RevoluteJoint*>(joint)->GetUpperLimit();
    case e_prismaticJoint:
        return static_cast<b2PrismaticJoint*>(joint)->GetUpperLimit();
    case e_wheelJoint:
        return static_cast<b2WheelJoint*>(joint)->GetUpperLimit();
    default:
        return -1.0;
    }
}

DllExport void SetJointLimits(b2Joint* joint, float lower, float upper)
{
    switch (joint->GetType())
    {
    case e_revoluteJoint:
        static_cast<b2RevoluteJoint*>(joint)->SetLimits(lower, upper);
        break;
    case e_prismaticJoint:
        static_cast<b2PrismaticJoint*>(joint)->SetLimits(lower, upper);
        break;
    case e_wheelJoint:
        static_cast<b2WheelJoint*>(joint)->SetLimits(lower, upper);
        break;
    }
}

DllExport bool IsJointMotorEnabled(b2Joint* joint)
{
    switch (joint->GetType())
    {
    case e_revoluteJoint:
        return static_cast<b2RevoluteJoint*>(joint)->IsMotorEnabled();
    case e_prismaticJoint:
        return static_cast<b2PrismaticJoint*>(joint)->IsMotorEnabled();
    case e_wheelJoint:
        return static_cast<b2WheelJoint*>(joint)->IsMotorEnabled();
    default:
        return 0;
    }
}

DllExport void EnableJointMotor(b2Joint* joint, bool flag)
{
    switch (joint->GetType())
    {
    case e_revoluteJoint:
        static_cast<b2RevoluteJoint*>(joint)->EnableMotor(flag);
        break;
    case e_prismaticJoint:
        static_cast<b2PrismaticJoint*>(joint)->EnableMotor(flag);
        break;
    case e_wheelJoint:
        static_cast<b2WheelJoint*>(joint)->EnableMotor(flag);
        break;
    }
}

DllExport void SetJointMotorSpeed(b2Joint* joint, float speed)
{
    switch (joint->GetType())
    {
    case e_revoluteJoint:
        static_cast<b2RevoluteJoint*>(joint)->SetMotorSpeed(speed);
        break;
    case e_prismaticJoint:
        static_cast<b2PrismaticJoint*>(joint)->SetMotorSpeed(speed);
        break;
    case e_wheelJoint:
        static_cast<b2WheelJoint*>(joint)->SetMotorSpeed(speed);
        break;
    }
}

DllExport float GetJointMotorSpeed(b2Joint* joint)
{
    switch (joint->GetType())
    {
    case e_revoluteJoint:
        return static_cast<b2RevoluteJoint*>(joint)->GetMotorSpeed();
    case e_prismaticJoint:
        return static_cast<b2PrismaticJoint*>(joint)->GetMotorSpeed();
    case e_wheelJoint:
        return static_cast<b2WheelJoint*>(joint)->GetMotorSpeed();
    default:
        return -1.0;
    }
}

DllExport float GetJointRatio(b2Joint* joint)
{
    switch (joint->GetType())
    {
    case e_pulleyJoint:
        return static_cast<b2PulleyJoint*>(joint)->GetRatio();
    case e_gearJoint:
        return static_cast<b2GearJoint*>(joint)->GetRatio();
    default:
        return -1.0;
    }
}

DllExport float GetJointAngle(b2Joint* joint)
{
    switch (joint->GetType())
    {
    case e_revoluteJoint:
        return static_cast<b2RevoluteJoint*>(joint)->GetJointAngle();
    case e_wheelJoint:
        return static_cast<b2WheelJoint*>(joint)->GetJointAngle();
    default:
        return -1.0;
    }
}

DllExport void SetJointMaxMotorTorque(b2Joint* joint, float torque)
{
    switch (joint->GetType())
    {
    case e_revoluteJoint:
        static_cast<b2RevoluteJoint*>(joint)->SetMaxMotorTorque(torque);
        break;
    case e_wheelJoint:
        static_cast<b2WheelJoint*>(joint)->SetMaxMotorTorque(torque);
        break;
    }
}

DllExport float GetJointMaxMotorTorque(b2Joint* joint)
{
    switch (joint->GetType())
    {
    case e_revoluteJoint:
        return static_cast<b2RevoluteJoint*>(joint)->GetMaxMotorTorque();
    case e_wheelJoint:
        return static_cast<b2WheelJoint*>(joint)->GetMaxMotorTorque();
    default:
        return -1.0;
    }
}

DllExport float GetJointMotorTorque(b2Joint* joint, float inv_dt)
{
    switch (joint->GetType())
    {
    case e_revoluteJoint:
        return static_cast<b2RevoluteJoint*>(joint)->GetMotorTorque(inv_dt);
    case e_wheelJoint:
        return static_cast<b2WheelJoint*>(joint)->GetMotorTorque(inv_dt);
    default:
        return -1.0;
    }
}


