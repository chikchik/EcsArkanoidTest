#ifndef B2_W_REVOLUTE_JOINT_H
#define B2_W_REVOLUTE_JOINT_H

#ifdef _WIN32
#define DllExport __declspec (dllexport)
#elif __APPLE__ || defined(__ANDROID__)
#define DllExport __attribute__((visibility("default")))
#endif

#include <structs.h>
#include <b2_revolute_joint.h>

DllExport Vector2 GetJointLocalAnchorA(b2Joint* joint);

DllExport Vector2 GetJointLocalAnchorB(b2Joint* joint);

DllExport float GetJointReferenceAngle(b2Joint* joint);

DllExport float GetJointAngle(b2Joint* joint);

DllExport float GetJointSpeed(b2Joint* joint);

DllExport bool IsJointLimitEnabled(b2Joint* joint);

DllExport void EnableJointLimit(b2Joint* joint, bool flag);

DllExport float GetJointLowerLimit(b2Joint* joint);

DllExport float GetJointUpperLimit(b2Joint* joint);

DllExport void SetJointLimits(b2Joint* joint, float lower, float upper);

DllExport bool IsJointMotorEnabled(b2Joint* joint);

DllExport void EnableJointMotor(b2Joint* joint, bool flag);

DllExport void SetJointMotorSpeed(b2Joint* joint, float speed);

DllExport float GetJointMotorSpeed(b2Joint* joint);

DllExport void SetJointMaxMotorTorque(b2Joint* joint, float torque);

DllExport float GetJointMaxMotorTorque(b2Joint* joint);

DllExport float GetJointMotorTorque(b2Joint* joint, float inv_dt);

#endif