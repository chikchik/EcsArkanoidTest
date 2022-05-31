#include <distance_joint.h>

DllExport float GetJointLength(b2Joint* joint)
{
    return static_cast<b2DistanceJoint*>(joint)->GetLength();
}

DllExport float SetJointLength(b2Joint* joint, float length)
{
    return static_cast<b2DistanceJoint*>(joint)->SetLength(length);
}

DllExport float GetJointMinLength(b2Joint* joint)
{
    return static_cast<b2DistanceJoint*>(joint)->GetMinLength();
}

DllExport float SetJointMinLength(b2Joint* joint, float minLength)
{
    return static_cast<b2DistanceJoint*>(joint)->SetMinLength(minLength);
}

DllExport float GetJointMaxLength(b2Joint* joint)
{
    return static_cast<b2DistanceJoint*>(joint)->GetMaxLength();
}

DllExport float SetJointMaxLength(b2Joint* joint, float maxLength)
{
    return static_cast<b2DistanceJoint*>(joint)->SetMaxLength(maxLength);
}

DllExport float GetJointCurrentLength(b2Joint* joint)
{
    return static_cast<b2DistanceJoint*>(joint)->GetCurrentLength();
}