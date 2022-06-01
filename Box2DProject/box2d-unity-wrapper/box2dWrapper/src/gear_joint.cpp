#include <gear_joint.h>

DllExport b2Joint* GetJoint1(b2Joint* joint)
{
	return static_cast<b2GearJoint*>(joint)->GetJoint1();
}

DllExport b2Joint* GetJoint2(b2Joint* joint)
{
	return static_cast<b2GearJoint*>(joint)->GetJoint2();
}

DllExport void SetJointRatio(b2Joint* joint, float ratio)
{
	static_cast<b2GearJoint*>(joint)->SetRatio(ratio);
}
