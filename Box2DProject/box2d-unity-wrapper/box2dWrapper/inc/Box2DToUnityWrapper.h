#ifndef BOX2D_TO_UNITY_WRAPPER
#define BOX2D_TO_UNITY_WRAPPER

#include <stddef.h>
#include <vector>
#include <box2d.h>
#include <callbacks.h>
#include <raycast.h>
#include <debug_draw.h>
#include <contact_listener.h>

// SAFEARRAY
#include <comdef.h>

#ifdef _WIN32
#define DllExport __declspec (dllexport)
#elif __APPLE__ || defined(__ANDROID__)
#define DllExport __attribute__((visibility("default")))
#endif

extern "C"
{
    DllExport void SetBeginContactCallback(b2World* world, Callback callback);

    DllExport void SetEndContactCallback(b2World* world, Callback callback);

    DllExport void SetPreSolveCallback(b2World* world, Callback callback);

    DllExport void SetPostSolveCallback(b2World* world, Callback callback);

    DllExport void SetDebugDraw(b2World* world, DrawDbgCircleCallback drawCircle,
        DrawDbgCircleCallback drawPoint, DrawDbgSegmentCallback drawSegment,
        DrawDbgTransformCallback drawTransform, DrawDbgPolygonCallback drawPolygon);

    DllExport void DebugDraw(b2World* world);

    DllExport void SetFlagsForDebugDraw(b2World* world, uint32 mask);


    DllExport b2World* UpdateWorld(b2World* world,
        float timeStep, int velocityIterations, int positionIterations);

    DllExport void DestroyWorld(b2World* world);

    DllExport b2World* CloneWorld(void**& arrayOfReferences, int count, b2World* world);

    DllExport b2World* CreateWorld(Vector2 gravity);

    DllExport CollisionCallbackData TryGetContactInfoForBodies(b2Body* body1,
        b2Body* body2);

    DllExport void DestroyWorld(b2World* world);


    DllExport b2Shape* CreateChainShape(Vector2 vertices[], int count);

    DllExport b2Shape* CreatePolygonShape(Vector2 vertices[], int count);

    DllExport b2Shape* CreateCircleShape(float radius);

    DllExport b2Shape* CreateBoxShape(Vector2 extents);


    // BODY
    DllExport b2Body* CreateBody(b2World* world, int bodyType,
        Vector2 position, float angle, int entity);

    DllExport void AddFixtureToBody(b2Body* body, b2Shape* shape,
        float density, float friction, float restitution,
        float restitutionThreshold, bool isTrigger, b2Filter filter);
    
    DllExport int GetContactListCount(b2Body* body);

    DllExport void GetContactList(b2Body* body,
        SAFEARRAY*& dataArr, CallbackDebug cb);

    DllExport int GetEntityFromBody(b2Body* body);

    DllExport void DestroyBody(b2World* world, b2Body* body);

    DllExport Vector2 GetPosition(b2Body* body);

    DllExport void SetPosition(b2Body* body, Vector2 position, bool wake);

    DllExport float GetAngle(b2Body* body);

    DllExport void SetAngle(b2Body* body, float angle);

    DllExport void SetLinearVelocity(b2Body* body, Vector2 linearVelocity);

    DllExport void SetAngularVelocity(b2Body* body, float angularVelocity);

    DllExport Vector2 GetLinearVelocity(b2Body* body);

    DllExport float GetAngularVelocity(b2Body* body);

    DllExport void ApplyForce(b2Body* body, Vector2 force, Vector2 point, bool wake);

    DllExport void ApplyForceToCenter(b2Body* body, Vector2 force, bool wake);

    DllExport void ApplyTorque(b2Body* body, float torque, bool wake);

    DllExport void ApplyLinearImpulse(b2Body* body, Vector2 force, Vector2 point, bool wake);

    DllExport void ApplyLinearImpulseToCenter(b2Body* body, Vector2 force, bool wake);

    DllExport void ApplyAngularImpulse(b2Body* body, float impulse, bool wake);

    DllExport float GetLinearDamping(b2Body* body);

    DllExport void SetLinearDamping(b2Body* body, float val);

    DllExport float GetAngularDamping(b2Body* body);

    DllExport void SetAngularDamping(b2Body* body, float val);

    DllExport float GetGravityScale(b2Body* body);

    DllExport void SetGravityScale(b2Body* body, float scale);

    DllExport void SetType(b2Body* body, int type);

    DllExport int GetType(b2Body* body);

    DllExport void SetBullet(b2Body* body, bool flag);

    DllExport bool IsBullet(b2Body* body);

    DllExport void SetSleepingAllowed(b2Body* body, bool flag);

    DllExport bool IsSleepingAllowed(b2Body* body);

    DllExport void SetAwake(b2Body* body, bool flag);

    DllExport bool IsAwake(b2Body* body);

    DllExport void SetEnabled(b2Body* body, bool flag);

    DllExport bool IsEnabled(b2Body* body);

    DllExport void SetFixedRotation(b2Body* body, bool flag);

    DllExport bool IsFixedRotation(b2Body* body);

    DllExport b2World* GetWorldFromBody(b2Body* body);

    DllExport void SetEnabled(b2Body* body, bool flag);

    DllExport float GetMass(b2Body* body);

    DllExport float GetInertia(b2Body* body);

    DllExport BodyInfo GetBodyInfo(b2Body* body);

    DllExport void SetBodyInfo(b2Body* body, BodyInfo bodyInfo);

    DllExport B2FilterReturnType GetBodyFixturesFilterData(b2Body* body);

    DllExport bool RayCast(b2World* world, Vector2 origin, Vector2 direction,
        RaycastOutputReturnType* hit, float distance, uint16 layerMask);
}

#endif // !BOX2D_TO_UNITY_WRAPPER
