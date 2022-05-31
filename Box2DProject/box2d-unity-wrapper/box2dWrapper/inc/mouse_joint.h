#ifndef B2_W_MOUSE_JOINT_H
#define B2_W_MOUSE_JOINT_H

#ifdef _WIN32
#define DllExport __declspec (dllexport)
#elif __APPLE__ || defined(__ANDROID__)
#define DllExport __attribute__((visibility("default")))
#endif

#include <structs.h>
#include <b2_mouse_joint.h>


DllExport void SetTarget(b2Joint* joint, Vector2 target);

DllExport Vector2 GetTarget(b2Joint* joint);

DllExport void SetMaxForce(b2Joint* joint, float force);

DllExport float GetMaxForce(b2Joint* joint);

DllExport void SetStiffness(b2Joint* joint, float stiffness);

DllExport float GetStiffness(b2Joint* joint);

DllExport void SetDamping(b2Joint* joint, float damping);

DllExport float GetDamping(b2Joint* joint);


#endif
