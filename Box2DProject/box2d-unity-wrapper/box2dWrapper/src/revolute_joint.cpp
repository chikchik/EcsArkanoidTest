#include <revolute_joint.h>

DllExport Vector2 GetJointLocalAnchorA(b2Joint* joint)
{
    b2Vec2 vv = static_cast<b2RevoluteJoint*>(joint)->GetLocalAnchorA();

    Vector2 v
    {
        vv.x,
        vv.y
    };
	return v;
}

DllExport Vector2 GetJointLocalAnchorB(b2Joint* joint)
{
    b2Vec2 vv = static_cast<b2RevoluteJoint*>(joint)->GetLocalAnchorA();

    Vector2 v
    {
        vv.x,
        vv.y
    };
    return v;
}

DllExport float GetJointReferenceAngle(b2Joint* joint)
{
    return static_cast<b2RevoluteJoint*>(joint)->GetReferenceAngle();
}

DllExport float GetJointAngle(b2Joint* joint)
{
    return static_cast<b2RevoluteJoint*>(joint)->GetJointAngle();
}

DllExport float GetJointSpeed(b2Joint* joint)
{
    return static_cast<b2RevoluteJoint*>(joint)->GetJointSpeed();
}

DllExport bool IsJointLimitEnabled(b2Joint* joint)
{
    return static_cast<b2RevoluteJoint*>(joint)->IsLimitEnabled();
}

DllExport void EnableJointLimit(b2Joint* joint, bool flag)
{
    static_cast<b2RevoluteJoint*>(joint)->EnableLimit(flag);
}

DllExport float GetJointLowerLimit(b2Joint* joint)
{
    return static_cast<b2RevoluteJoint*>(joint)->GetLowerLimit();
}

DllExport float GetJointUpperLimit(b2Joint* joint)
{
    return static_cast<b2RevoluteJoint*>(joint)->GetUpperLimit();
}

DllExport void SetJointLimits(b2Joint* joint, float lower, float upper)
{
    static_cast<b2RevoluteJoint*>(joint)->SetLimits(lower, upper);
}

DllExport bool IsJointMotorEnabled(b2Joint* joint)
{
    return static_cast<b2RevoluteJoint*>(joint)->IsMotorEnabled();
}

DllExport void EnableJointMotor(b2Joint* joint, bool flag)
{
    static_cast<b2RevoluteJoint*>(joint)->EnableMotor(flag);
}

DllExport void SetJointMotorSpeed(b2Joint* joint, float speed)
{
    static_cast<b2RevoluteJoint*>(joint)->SetMotorSpeed(speed);
}

DllExport float GetJointMotorSpeed(b2Joint* joint)
{
    return static_cast<b2RevoluteJoint*>(joint)->GetMotorSpeed();
}

DllExport void SetJointMaxMotorTorque(b2Joint* joint, float torque)
{
    static_cast<b2RevoluteJoint*>(joint)->SetMaxMotorTorque(torque);
}

DllExport float GetJointMaxMotorTorque(b2Joint* joint)
{
    return static_cast<b2RevoluteJoint*>(joint)->GetMaxMotorTorque();
}

DllExport float GetJointMotorTorque(b2Joint* joint, float inv_dt)
{
    return static_cast<b2RevoluteJoint*>(joint)->GetMotorTorque(inv_dt);
}
