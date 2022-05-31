#include <motor_joint.h>

DllExport void SetJointLinearOffset(b2Joint* joint, Vector2 linearOffset)
{
	b2Vec2 vv
	{
		linearOffset.x,
		linearOffset.y
	};
	static_cast<b2MotorJoint*>(joint)->SetLinearOffset(vv);
}

DllExport Vector2 GetJointLinearOffset(b2Joint* joint)
{
	b2Vec2 vv = static_cast<b2MotorJoint*>(joint)->GetLinearOffset();

	Vector2 v
	{
		vv.x,
		vv.y
	};
	return v;
}

DllExport void SetJointAngularOffset(b2Joint* joint, float angularOffset)
{
	static_cast<b2MotorJoint*>(joint)->SetAngularOffset(angularOffset);
}

DllExport float GetJointAngularOffset(b2Joint* joint)
{
	return static_cast<b2MotorJoint*>(joint)->GetAngularOffset();
}

DllExport void SetJointCorrectionFactor(b2Joint* joint, float factor)
{
	static_cast<b2MotorJoint*>(joint)->SetAngularOffset(factor);
}

DllExport float GetJointCorrectionFactor(b2Joint* joint)
{
	return static_cast<b2MotorJoint*>(joint)->GetCorrectionFactor();
}
