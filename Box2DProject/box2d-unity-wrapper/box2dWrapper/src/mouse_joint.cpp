#include <mouse_joint.h>

DllExport void SetJointTarget(b2Joint* joint, Vector2 target)
{
	b2Vec2 vv
	{
		target.x,
		target.y
	};
	static_cast<b2MouseJoint*>(joint)->SetTarget(vv);
}

DllExport Vector2 GetJointTarget(b2Joint* joint)
{
	b2Vec2 v = static_cast<b2MouseJoint*>(joint)->GetTarget();

	Vector2 vv
	{
		v.x,
		v.y
	};
	return vv;
}