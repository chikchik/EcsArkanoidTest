#ifndef B2_W_MOTOR_JOINT_H
#define B2_W_MOTOR_JOINT_H

#ifdef _WIN32
#define DllExport __declspec (dllexport)
#elif __APPLE__ || defined(__ANDROID__)
#define DllExport __attribute__((visibility("default")))
#endif

#include <structs.h>
#include <b2_motor_joint.h>

DllExport void SetJointLinearOffset(b2Joint* joint, Vector2 linearOffset);

DllExport Vector2 GetJointLinearOffset(b2Joint* joint);

DllExport void SetJointAngularOffset(b2Joint* joint, float angularOffset);

DllExport float GetJointAngularOffset(b2Joint* joint);

DllExport void SetJointCorrectionFactor(b2Joint* joint, float factor);

DllExport float GetJointCorrectionFactor(b2Joint* joint);

#endif