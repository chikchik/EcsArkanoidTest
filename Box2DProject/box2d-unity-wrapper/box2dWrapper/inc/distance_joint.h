#ifndef B2_W_DISTANCE_JOINT_H
#define B2_W_DISTANCE_JOINT_H

#ifdef _WIN32
#define DllExport __declspec (dllexport)
#elif __APPLE__ || defined(__ANDROID__)
#define DllExport __attribute__((visibility("default")))
#endif

#include <structs.h>
#include <b2_distance_joint.h>

DllExport Vector2 GetJointAnchorA(b2Joint* joint);

DllExport Vector2 GetJointAnchorB(b2Joint* joint);

DllExport Vector2 GetJointReactionForce(b2Joint* joint, float inv_dt);

DllExport float GetJointReactionTorque(b2Joint* joint, float inv_dt);

DllExport float GetJointLength(b2Joint* joint);

DllExport float SetJointLength(b2Joint* joint, float length);

DllExport float GetJointMinLength(b2Joint* joint);

DllExport float SetJointMinLength(b2Joint* joint, float minLength);

DllExport float GetJointMaxLength(b2Joint* joint);

DllExport float SetJointMaxLength(b2Joint* joint, float maxLength);

DllExport float GetJointCurrentLength(b2Joint* joint);

DllExport void SetJointStiffness(b2Joint* joint, float stiffness);

DllExport float GetJointStiffness(b2Joint* joint);

DllExport void SetJointDamping(b2Joint* joint, float damping);

DllExport float GetJointDamping(b2Joint* joint);


#endif