#ifndef B2_W_JOINT_H
#define B2_W_JOINT_H

#ifdef _WIN32
#define DllExport __declspec (dllexport)
#elif __APPLE__ || defined(__ANDROID__)
#define DllExport __attribute__((visibility("default")))
#endif

#include <structs.h>
#include <b2_joint.h>

DllExport Vector2 GetJointAnchorA(b2Joint* joint);

DllExport Vector2 GetJointAnchorB(b2Joint* joint);

DllExport Vector2 GetJointReactionForce(b2Joint* joint, float inv_dt);

DllExport float GetJointReactionTorque(b2Joint* joint, float inv_dt);

DllExport int GetJointType(b2Joint* joint);

DllExport b2Body* GetJointBodyA(b2Joint* joint);

DllExport b2Body* GetJointBodyB(b2Joint* joint);

DllExport bool IsJointEnabled(b2Joint* joint);

DllExport void ShiftJointOrigin(b2Joint* joint, Vector2 newOrigin);

#endif