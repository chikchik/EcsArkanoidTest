#ifndef B2_W_GEAR_JOINT_H
#define B2_W_GEAR_JOINT_H

#ifdef _WIN32
#define DllExport __declspec (dllexport)
#elif __APPLE__ || defined(__ANDROID__)
#define DllExport __attribute__((visibility("default")))
#endif

#include <structs.h>
#include <b2_gear_joint.h>

DllExport b2Joint* GetJoint1(b2Joint* joint);

DllExport b2Joint* GetJoint2(b2Joint* joint);

DllExport void SetJointRatio(b2Joint* joint, float ratio);

DllExport int GetMotorJointTypeA(b2Joint* joint);

DllExport int GetMotorJointTypeB(b2Joint* joint);

DllExport b2Body* GetMotorJointBodyC(b2Joint* joint);

DllExport b2Body* GetMotorJointBodyD(b2Joint* joint);

#endif