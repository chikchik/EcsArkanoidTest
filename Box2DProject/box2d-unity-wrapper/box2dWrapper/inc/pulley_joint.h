#ifndef B2_W_JOINT_H
#define B2_W_JOINT_H

#ifdef _WIN32
#define DllExport __declspec (dllexport)
#elif __APPLE__ || defined(__ANDROID__)
#define DllExport __attribute__((visibility("default")))
#endif

#include <structs.h>
#include <b2_pulley_joint.h>

DllExport Vector2 GetJointGroundAnchorA(b2Joint* joint);

DllExport Vector2 GetJointGroundAnchorB(b2Joint* joint);

DllExport float GetJointLengthA(b2Joint* joint);

DllExport float GetJointLengthB(b2Joint* joint);

DllExport float GetJointCurrentLengthA(b2Joint* joint);

DllExport float GetJointCurrentLengthB(b2Joint* joint);

#endif
