#include <mouse_joint.h>

DllExport void SetTarget(b2Joint* joint, Vector2 target)
{
	b2Vec2 vv
	{
		target.x,
		target.y
	};
	static_cast<b2MouseJoint*>(joint)->SetTarget(vv);
}

DllExport Vector2 GetTarget(b2Joint* joint)
{
	b2Vec2 v = static_cast<b2MouseJoint*>(joint)->GetTarget();

	Vector2 vv
	{
		v.x,
		v.y
	};
	return vv;
}

DllExport void SetMaxForce(b2Joint* joint, float force)
{
	static_cast<b2MouseJoint*>(joint)->SetMaxForce(force);
}

DllExport float GetMaxForce(b2Joint* joint)
{
	return static_cast<b2MouseJoint*>(joint)->GetMaxForce();
}

DllExport void SetStiffness(b2Joint* joint, float stiffness)
{
	static_cast<b2MouseJoint*>(joint)->SetStiffness(stiffness);
}

DllExport float GetStiffness(b2Joint* joint)
{
	return static_cast<b2MouseJoint*>(joint)->GetStiffness();
}

DllExport void SetDamping(b2Joint* joint, float damping)
{
	static_cast<b2MouseJoint*>(joint)->SetDamping(damping);
}

DllExport float GetDamping(b2Joint* joint)
{
	return static_cast<b2MouseJoint*>(joint)->GetDamping();
}