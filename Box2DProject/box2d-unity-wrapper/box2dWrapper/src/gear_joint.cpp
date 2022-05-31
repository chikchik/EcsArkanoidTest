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

DllExport int GetMotorJointTypeA(b2Joint* joint)
{
	return (int)static_cast<b2GearJoint*>(joint)->m_typeA;
}

DllExport int GetMotorJointTypeB(b2Joint* joint)
{
	return (int)static_cast<b2GearJoint*>(joint)->m_typeB;
}

DllExport b2Body* GetMotorJointBodyC(b2Joint* joint)
{
	return static_cast<b2GearJoint*>(joint)->m_bodyC;
}

DllExport b2Body* GetMotorJointBodyD(b2Joint* joint)
{
	return static_cast<b2GearJoint*>(joint)->m_bodyD;
}
