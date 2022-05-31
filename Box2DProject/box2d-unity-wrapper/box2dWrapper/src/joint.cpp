#include <joint.h>

DllExport Vector2 GetJointAnchorA(b2Joint* joint)
{
    b2Vec2 vv = joint->GetAnchorA();

    Vector2 v
    {
        vv.x,
        vv.y
    };
    return v;
}

DllExport Vector2 GetJointAnchorB(b2Joint* joint)
{
    b2Vec2 vv = joint->GetAnchorB();


    Vector2 v
    {
        vv.x,
        vv.y
    };
    return v;
}

DllExport Vector2 GetJointReactionForce(b2Joint* joint, float inv_dt)
{
    b2Vec2 vv = joint->GetReactionForce(inv_dt);

    Vector2 v
    {
        vv.x,
        vv.y
    };
    return v;
}

DllExport float GetJointReactionTorque(b2Joint* joint, float inv_dt)
{
    return joint->GetReactionTorque(inv_dt);
}

DllExport int GetJointType(b2Joint* joint)
{
    return (int)joint->GetType();
}

DllExport b2Body* GetJointBodyA(b2Joint* joint)
{
    return joint->GetBodyA();
}

DllExport b2Body* GetJointBodyB(b2Joint* joint)
{
    return joint->GetBodyB();
}

DllExport bool IsJointEnabled(b2Joint* joint)
{
    return joint->IsEnabled();
}

DllExport void ShiftJointOrigin(b2Joint* joint, Vector2 newOrigin)
{
    b2Vec2 v
    {
        newOrigin.x,
        newOrigin.y
    };
    joint->ShiftOrigin(v);
}
