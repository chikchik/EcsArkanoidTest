#ifndef B2_W_MOUSE_JOINT_H
#define B2_W_MOUSE_JOINT_H

#ifdef _WIN32
#define DllExport __declspec (dllexport)
#elif __APPLE__ || defined(__ANDROID__)
#define DllExport __attribute__((visibility("default")))
#endif

#include <structs.h>
#include <b2_mouse_joint.h>


DllExport void SetJointTarget(b2Joint* joint, Vector2 target);

DllExport Vector2 GetJointTarget(b2Joint* joint);

#endif
