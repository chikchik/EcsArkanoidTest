#include <pulley_joint.h>

DllExport Vector2 GetJointGroundAnchorA(b2Joint* joint)
{
    b2Vec2 vv = static_cast<b2PulleyJoint*>(joint)->GetGroundAnchorA();

    Vector2 v
    {
        vv.x,
        vv.y
    };
    return v;
}

DllExport Vector2 GetJointGroundAnchorB(b2Joint* joint)
{
    b2Vec2 vv = static_cast<b2PulleyJoint*>(joint)->GetGroundAnchorB();

    Vector2 v
    {
        vv.x,
        vv.y
    };
    return v;
}

DllExport float GetJointLengthA(b2Joint* joint)
{
	return static_cast<b2PulleyJoint*>(joint)->GetLengthA();
}

DllExport float GetJointLengthB(b2Joint* joint)
{
	return static_cast<b2PulleyJoint*>(joint)->GetLengthB();
}

DllExport float GetJointCurrentLengthA(b2Joint* joint)
{
	return static_cast<b2PulleyJoint*>(joint)->GetCurrentLengthA();
}

DllExport float GetJointCurrentLengthB(b2Joint* joint)
{
	return static_cast<b2PulleyJoint*>(joint)->GetCurrentLengthB();
}

