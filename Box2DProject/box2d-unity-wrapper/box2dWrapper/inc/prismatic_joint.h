#ifndef B2_W_JOINT_H
#define B2_W_JOINT_H

#ifdef _WIN32
#define DllExport __declspec (dllexport)
#elif __APPLE__ || defined(__ANDROID__)
#define DllExport __attribute__((visibility("default")))
#endif

#include <structs.h>
#include <b2_prismatic_joint.h>

DllExport void SetJointMaxMotorForce(b2Joint* joint, float force);

DllExport float GetJointMaxMotorForce(b2Joint* joint);

DllExport float GetJointMotorForce(b2Joint* joint, float inv_dt);

#endif