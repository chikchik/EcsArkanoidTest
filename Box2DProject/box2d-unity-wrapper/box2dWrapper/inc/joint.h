#ifndef B2_W_JOINT_H
#define B2_W_JOINT_H

#ifdef _WIN32
#define DllExport __declspec (dllexport)
#elif __APPLE__ || defined(__ANDROID__)
#define DllExport __attribute__((visibility("default")))
#endif

#include <structs.h>
#include <b2_joint.h>

// common for all joints
DllExport Vector2 GetJointAnchorA(b2Joint* joint);

DllExport Vector2 GetJointAnchorB(b2Joint* joint);

DllExport Vector2 GetJointReactionForce(b2Joint* joint, float inv_dt);

DllExport float GetJointReactionTorque(b2Joint* joint, float inv_dt);

DllExport int GetJointType(b2Joint* joint);

DllExport b2Body* GetJointBodyA(b2Joint* joint);

DllExport b2Body* GetJointBodyB(b2Joint* joint);

DllExport bool IsJointEnabled(b2Joint* joint);

DllExport void ShiftJointOrigin(b2Joint* joint, Vector2 newOrigin);

// common for mouse, distance
DllExport void SetJointStiffness(b2Joint* joint, float stiffness);

DllExport float GetJointStiffness(b2Joint* joint);

DllExport void SetJointDamping(b2Joint* joint, float damping);

DllExport float GetJointDamping(b2Joint* joint);

// common for revolute, friction, prismatic
DllExport Vector2 GetJointLocalAnchorA(b2Joint* joint);

DllExport Vector2 GetJointLocalAnchorB(b2Joint* joint);

// common for mouse, friction, motor
DllExport float GetJointMaxForce(b2Joint* joint);

DllExport void SetJointMaxForce(b2Joint* joint, float force);

// common for motor, friction
DllExport void SetJointMaxTorque(b2Joint* joint, float torque);

DllExport float GetJointMaxTorque(b2Joint* joint);

// common for prismatic, revolute, weld
DllExport float GetJointReferenceAngle(b2Joint* joint);

// common for prismatic, wheel
DllExport Vector2 GetJointLocalAxisA(b2Joint* joint);

DllExport float GetJointTranslation(b2Joint* joint);

//common for prismatic, revolute
DllExport float GetJointSpeed(b2Joint* joint);

//common for prismatic, revolute, wheel
DllExport bool IsJointLimitEnabled(b2Joint* joint);

DllExport void EnableJointLimit(b2Joint* joint, bool flag);

DllExport float GetJointLowerLimit(b2Joint* joint);

DllExport float GetJointUpperLimit(b2Joint* joint);

DllExport void SetJointLimits(b2Joint* joint, float lower, float upper);

DllExport bool IsJointMotorEnabled(b2Joint* joint);

DllExport void EnableJointMotor(b2Joint* joint, bool flag);

DllExport void SetJointMotorSpeed(b2Joint* joint, float speed);

DllExport float GetJointMotorSpeed(b2Joint* joint);

// common for pulley, gear
DllExport float GetJointRatio(b2Joint* joint);

// common for revolute, wheel
DllExport float GetJointAngle(b2Joint* joint);

DllExport void SetJointMaxMotorTorque(b2Joint* joint, float torque);

DllExport float GetJointMaxMotorTorque(b2Joint* joint);

DllExport float GetJointMotorTorque(b2Joint* joint, float inv_dt);


#endif