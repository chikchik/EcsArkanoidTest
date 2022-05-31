#include <wheel_joint.h>

DllExport float GetJointLinearSpeed(b2Joint* joint)
{
	return static_cast<b2WheelJoint*>(joint)->GetJointLinearSpeed();
}

DllExport float GetJointAngularSpeed(b2Joint* joint)
{
	return static_cast<b2WheelJoint*>(joint)->GetJointAngularSpeed();
}
