#include <prismatic_joint.h>

DllExport void SetJointMaxMotorForce(b2Joint* joint, float force)
{
	static_cast<b2PrismaticJoint*>(joint)->SetMaxMotorForce(force);
}

DllExport float GetJointMaxMotorForce(b2Joint* joint)
{
	return static_cast<b2PrismaticJoint*>(joint)->GetMaxMotorForce();
}

DllExport float GetJointMotorForce(b2Joint* joint, float inv_dt)
{
	return static_cast<b2PrismaticJoint*>(joint)->GetMotorForce(inv_dt);
}
