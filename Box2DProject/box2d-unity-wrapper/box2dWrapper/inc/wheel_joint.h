#ifndef B2_W_JOINT_H
#define B2_W_JOINT_H

#ifdef _WIN32
#define DllExport __declspec (dllexport)
#elif __APPLE__ || defined(__ANDROID__)
#define DllExport __attribute__((visibility("default")))
#endif

#include <structs.h>
#include <b2_wheel_joint.h>

DllExport float GetJointLinearSpeed(b2Joint* joint);

DllExport float GetJointAngularSpeed(b2Joint* joint);


#endif