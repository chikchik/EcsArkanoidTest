#include <stddef.h>
#include <vector>
#include "box2d/headers/box2d.h"

#ifdef _WIN32
    #define DllExport __declspec (dllexport)
#elif __APPLE__ || defined(__ANDROID__)
    #define DllExport __attribute__((visibility("default")))
#endif

struct Vector2
{
    public : float x;
    public : float y;
};

struct RaycastOutputReturnType
{
    public : b2Body* body;
    public : Vector2 normal;
    public : Vector2 point;
    public : float fraction;
};

struct B2FilterReturnType
{
    public : uint16 categoryBits;
    public : uint16 maskBits;
    public : int16 groupIndex;
};

struct BodyInfo
{
    public : Vector2 position;
    public : Vector2 linearVelocity;
    public : float angularVelocity;
    public : float angle;
};

struct CollisionCallbackData
{
    public : b2World* physicsWorld;
    public : int entityA;
    public : int entityB;

    public : Vector2 velA;
    public : Vector2 velB;

    public : Vector2 contactPoints[b2_maxManifoldPoints];
    public : int32 contactPointCount;

    public : Vector2 normal;
};

struct Box2dColor
{
    public : float r;
    public : float g;
    public : float b;
    public : float a;
};

Vector2 GetUnityVectorFromB2d(const b2Vec2 &b2vec);

class RayCastClosestCallback : public b2RayCastCallback
{
public:
    RaycastOutputReturnType m_hit;
    uint16 m_layerMask;
    bool m_isHit = false;

    RayCastClosestCallback(uint16 layerMask)
    {
        m_layerMask = layerMask;
    }
    
    float ReportFixture (b2Fixture *fixture, const b2Vec2 &point, const b2Vec2 &normal, float fraction)
    {
        b2Filter filter = fixture->GetFilterData();
        
        if ((m_layerMask & filter.maskBits) != 0) return 1;

        m_hit.body = fixture->GetBody();
        m_hit.normal = GetUnityVectorFromB2d(normal);
        m_hit.point = GetUnityVectorFromB2d(point);
        m_hit.fraction = fraction;
        m_isHit = true;

        return fraction;
    }
};

class RayCastAnyCallback : public b2RayCastCallback
{
public:
    float ReportFixture (b2Fixture *fixture, const b2Vec2 &point, const b2Vec2 &normal, float fraction)
    {
        return 0.0f;
    }
};

class RayCastMultipleCallback : public b2RayCastCallback {
public:
    std::vector<RaycastOutputReturnType> foundBodies;
    
    float ReportFixture (b2Fixture *fixture,
        const b2Vec2 &point, const b2Vec2 &normal, float fraction) {
        RaycastOutputReturnType m_hit;
        m_hit.body = fixture->GetBody();
        m_hit.normal = GetUnityVectorFromB2d(normal);
        m_hit.point = GetUnityVectorFromB2d(point);
        m_hit.fraction = fraction;

        foundBodies.push_back(m_hit);

        return 1.0f;
    }
};

typedef void (__stdcall * Callback)(CollisionCallbackData);
typedef void (__stdcall * DrawDbgCircleCallback)(Vector2, float, Box2dColor);
typedef void (__stdcall * DrawDbgSegmentCallback)(Vector2, Vector2, Box2dColor);
typedef void (__stdcall * DrawDbgTransformCallback)(Vector2, Vector2, Box2dColor);
typedef void (__stdcall * DrawDbgPolygonCallback)(Vector2[b2_maxPolygonVertices], int32, Box2dColor);

class MyDebugDraw : public b2Draw
{
    private:
    DrawDbgCircleCallback m_drawCircle;
    DrawDbgCircleCallback m_drawPoint;
    DrawDbgSegmentCallback m_drawSegment;
    DrawDbgTransformCallback m_drawTransform;
    DrawDbgPolygonCallback m_drawPolygon;

    public:

    MyDebugDraw(DrawDbgCircleCallback drawCircle, DrawDbgCircleCallback drawPoint, DrawDbgSegmentCallback drawSegment,
        DrawDbgTransformCallback drawTransform, DrawDbgPolygonCallback drawPolygon)
    {
        m_drawCircle = drawCircle;
        m_drawPoint = drawPoint;
        m_drawSegment = drawSegment;
        m_drawTransform = drawTransform;
        m_drawPolygon = drawPolygon;
    }

    void DrawPolygon (const b2Vec2 *vertices, int32 vertexCount, const b2Color &color)
    {
        Vector2 vS[b2_maxPolygonVertices];

        for (int i = 0; i < vertexCount; i++) {
            Vector2 v1
            {
                vertices[i].x,
                vertices[i].y
            };
            vS[i] = v1;
        }

        Box2dColor c
        {
            color.r,
            color.g,
            color.b,
            color.a
        };

        m_drawPolygon(vS, vertexCount, c);
    }
    void DrawSolidPolygon (const b2Vec2 *vertices, int32 vertexCount, const b2Color &color)
    {
        DrawPolygon(vertices, vertexCount, color);
    }

    void DrawCircle (const b2Vec2 &center, float radius, const b2Color &color)
    {
        Vector2 cen
        {
            center.x,
            center.y
        };

        Box2dColor c
        {
            color.r,
            color.g,
            color.b,
            color.a
        };
        m_drawCircle(cen, radius, c);
    }

    void DrawSolidCircle (const b2Vec2 &center, float radius, const b2Vec2 &axis, const b2Color &color)
    {
        DrawCircle(center, radius, color);
    }
    void DrawSegment (const b2Vec2 &p1, const b2Vec2 &p2, const b2Color &color)
    {
        Vector2 v1
        {
            p1.x,
            p1.y
        };

        Vector2 v2
        {
            p2.x,
            p2.y
        };

        Box2dColor c
        {
            color.r,
            color.g,
            color.b,
            color.a
        };
        m_drawSegment(v1, v2, c);
    }
    void DrawTransform (const b2Transform &xf)
    {
        Vector2 v
        {
            xf.p.x,
            xf.p.y
        };
        Vector2 r
        {
            xf.q.GetXAxis().x,
            xf.q.GetXAxis().y
        };
        Vector2 u
        {
            xf.q.GetYAxis().x,
            xf.q.GetYAxis().y
        };

        Box2dColor cG
        {
            0,
            1,
            0,
            1
        };

        Box2dColor cR
        {
            1,
            0,
            0,
            1
        };
        m_drawTransform(v, r, cG);
        m_drawTransform(v, u, cR);
    }
    void DrawPoint (const b2Vec2 &p, float size, const b2Color &color)
    {
        Vector2 cen
        {
            p.x,
            p.y
        };
        Box2dColor c
        {
            color.r,
            color.g,
            color.b,
            color.a
        };
        m_drawPoint(cen, size, c);
    }
};

class MyContactListener : public b2ContactListener
{
    private:

    b2World* m_world = NULL;

    void GetEntityContacts(CollisionCallbackData* data, b2Contact* contact)
    {
        data->entityA = (int)contact->GetFixtureA()->GetBody()->GetUserData().pointer;
        data->entityB = (int)contact->GetFixtureB()->GetBody()->GetUserData().pointer;

        b2WorldManifold worldManifold;
        contact -> GetWorldManifold(&worldManifold);

        GetContactVelocity(data, &worldManifold, contact);

        GetNormal(data, &worldManifold);

        GetContactPoints(data, &worldManifold);
    }

    void GetContactVelocity(CollisionCallbackData* data, b2WorldManifold* worldManifold, b2Contact* contact)
    {
        b2Vec2 v1 = contact->GetFixtureA()->GetBody()->GetLinearVelocityFromWorldPoint(worldManifold->points[0]);
        
        Vector2 vec1 {
            v1.x,
            v1.y
        };

        data->velA = vec1;

        b2Vec2 v2 = contact->GetFixtureB()->GetBody()->GetLinearVelocityFromWorldPoint(worldManifold->points[0]);
        
        Vector2 vec2 {
            v2.x,
            v2.y
        };

        data->velB = vec2;
    }

    void GetContactPoints(CollisionCallbackData* data, b2WorldManifold* worldManifold)
    {
        size_t length = (sizeof(worldManifold->points)/sizeof(*worldManifold->points));
        memcpy(data->contactPoints, worldManifold->points, length * sizeof(Vector2));
    }

    void GetNormal(CollisionCallbackData* data, b2WorldManifold* worldManifold)
    {
        Vector2 vec2 {
            worldManifold->normal.x,
            worldManifold->normal.y
        };
        data->normal = vec2;
    }

    void BeginContact(b2Contact* contact)
    {
        if (m_callbackBeginContact == NULL) return;

        CollisionCallbackData data;
        GetEntityContacts(&data, contact);

        m_callbackBeginContact(data);
    }

    void EndContact(b2Contact* contact)
    {
        if (m_callbackEndContact == NULL) return;
        CollisionCallbackData data;
        GetEntityContacts(&data, contact);
        m_callbackEndContact(data);
    }

    void PreSolve(b2Contact* contact, const b2Manifold* oldManifold)
    {
        if (m_callbackPreSolve == NULL) return;
        CollisionCallbackData data;
        GetEntityContacts(&data, contact);
        m_callbackPreSolve(data);
    }

    void PostSolve(b2Contact* contact, const b2ContactImpulse* impulse)
    {
        if (m_callbackPostSolve == NULL) return;
        CollisionCallbackData data;
        GetEntityContacts(&data, contact);
        m_callbackPostSolve(data);
    }

    public:

    MyContactListener(b2World* world) : m_world(world) {};

    Callback m_callbackBeginContact = NULL;
    Callback m_callbackEndContact = NULL;
    Callback m_callbackPreSolve = NULL;
    Callback m_callbackPostSolve = NULL;
};

Vector2 GetUnityVectorFromB2d(const b2Vec2 &b2vec)
{
    Vector2 vec
    {
        b2vec.x,
        b2vec.y
    };
    return vec;
}

extern "C"
{
    DllExport void SetBeginContactCallback(b2World* world, Callback callback)
    {
        ((MyContactListener*)world -> GetContactListener()) -> m_callbackBeginContact = callback;
    }

    DllExport void SetEndContactCallback(b2World* world, Callback callback)
    {
        ((MyContactListener*)world -> GetContactListener()) -> m_callbackEndContact = callback;
    }

    DllExport void SetPreSolveCallback(b2World* world, Callback callback)
    {
        ((MyContactListener*)world -> GetContactListener()) -> m_callbackPreSolve = callback;
    }

    DllExport void SetPostSolveCallback(b2World* world, Callback callback)
    {
        ((MyContactListener*)world -> GetContactListener()) -> m_callbackPostSolve = callback;
    }

    DllExport void SetDebugDraw(b2World* world, DrawDbgCircleCallback drawCircle, DrawDbgCircleCallback drawPoint,
        DrawDbgSegmentCallback drawSegment, DrawDbgTransformCallback drawTransform, DrawDbgPolygonCallback drawPolygon)
    {
        MyDebugDraw* dbgDraw = new MyDebugDraw(drawCircle, drawPoint, drawSegment, drawTransform, drawPolygon);

        world -> SetDebugDraw(dbgDraw);
    }

    DllExport void DebugDraw(b2World* world)
    {
        if (world != NULL)
            world -> DebugDraw();
    }

    DllExport void SetFlagsForDebugDraw(b2World* world, uint32 mask)
    {
        world -> GetDebugDraw() -> SetFlags(mask);
    }

    DllExport b2World* UpdateWorld(b2World* world, float timeStep, int velocityIterations, int positionIterations)
    {
        world -> Step(timeStep, velocityIterations, positionIterations);

        return world;
    }

    DllExport b2World* CreateWorld(Vector2 gravity)
    {
        b2Vec2 bGravity(gravity.x, gravity.y);
        b2World* world = new b2World(bGravity);

        MyContactListener* myContactListener = new MyContactListener(world);

        world -> SetContactListener(myContactListener);

        return world;
    }

    DllExport b2World* GetWorldFromBody(b2Body* body)
    {
        return body -> GetWorld();
    }

    DllExport void DestroyWorld(b2World* world)
    {
        delete world -> GetContactListener();
        delete world;
        world = nullptr;
    }

    DllExport b2Body* CreateBody(b2World* world, int bodyType, Vector2 position, float angle, int entity)
    {
        b2BodyDef bodyDef;
        bodyDef.type = (b2BodyType)bodyType;
        bodyDef.position.Set(position.x, position.y);
        bodyDef.angle = angle;
        bodyDef.userData.pointer = entity;

        b2Body* body = world -> CreateBody(&bodyDef);
        body -> SetFixedRotation(false);

        return body;
    }

    DllExport int GetEntityFromBody(b2Body* body)
    {
        return (int)body -> GetUserData().pointer;
    }

    DllExport b2Shape* CreateChainShape(Vector2 vertices[], int count)
    {
        b2Vec2* b2Vertices = (b2Vec2*)b2Alloc(count * sizeof(b2Vec2));
        memcpy(b2Vertices, vertices, count * sizeof(b2Vec2));

        b2ChainShape* shape = new b2ChainShape();
        shape->CreateLoop(b2Vertices, count);

        return shape;
    }

    DllExport b2Shape* CreatePolygonShape(Vector2 vertices[], int count)
    {
        b2Vec2* b2Vertices = (b2Vec2*)b2Alloc(count * sizeof(b2Vec2));
        memcpy(b2Vertices, vertices, count * sizeof(b2Vec2));

        b2PolygonShape* shape = new b2PolygonShape();
        shape->Set(b2Vertices, count);
        return shape;
    }

    DllExport b2Shape* CreateCircleShape(float radius)
    {
        b2CircleShape* shape = new b2CircleShape();
        shape->m_radius = radius;
        return shape;
    }

    DllExport b2Shape* CreateBoxShape(Vector2 extents)
    {
        b2PolygonShape* shape = new b2PolygonShape();
        shape->SetAsBox(extents.x, extents.y);
        return shape;
    }

    DllExport void AddFixtureToBody(b2Body* body, b2Shape* shape, float density, float friction, float restitution,
        float restitutionThreshold, bool isTrigger, b2Filter filter)
    {
        b2FixtureDef fixtureDef;
        fixtureDef.shape = shape;
        fixtureDef.density = density;
        fixtureDef.friction = friction;
        fixtureDef.restitution = restitution;
        fixtureDef.restitutionThreshold = restitutionThreshold;
        fixtureDef.isSensor = isTrigger;
        fixtureDef.filter = filter;

        body -> CreateFixture(&fixtureDef);
        b2Free(shape);
    }

    DllExport float GetLinearDamping(b2Body* body)
    {
        return body -> GetLinearDamping();
    }

    DllExport float GetAngularDamping(b2Body* body)
    {
        return body -> GetAngularDamping();
    }

    DllExport void SetLinearDamping(b2Body* body, float val)
    {
        body -> SetLinearDamping(val);
    }

    DllExport void SetAngularDamping(b2Body* body, float val)
    {
        body -> SetAngularDamping(val);
    }

    DllExport void DestroyBody(b2World* world, b2Body* body)
    {
        world -> DestroyBody(body);
    }

    DllExport Vector2 GetPosition(b2Body* body)
    {
        const b2Vec2& position = body -> GetPosition();

        Vector2 sharedPos {
            position.x,
            position.y
        };

        return sharedPos;
    }

    DllExport void SetPosition(b2Body* body, Vector2 position)
    {
        float angle = body -> GetAngle();
        b2Vec2 bPosition(position.x, position.y);

        body -> SetTransform(bPosition, angle);
        body -> SetAwake(true);
    }

    DllExport float GetAngle(b2Body* body)
    {
        return body -> GetAngle();
    }

    DllExport void SetAngle(b2Body* body, float angle)
    {
        b2Vec2 position = body -> GetPosition();

        body -> SetTransform(position, angle);
    }

    DllExport void SetLinearVelocity(b2Body* body, Vector2 linearVelocity)
    {
        b2Vec2 bLinearVelocity(linearVelocity.x, linearVelocity.y);

        body -> SetLinearVelocity(bLinearVelocity);
    }

    DllExport void SetAngularVelocity(b2Body* body, float angularVelocity)
    {
        body -> SetAngularVelocity(angularVelocity);
    }

    DllExport Vector2 GetLinearVelocity(b2Body* body)
    {
        const b2Vec2 linearVelocity =  body -> GetLinearVelocity();

        struct Vector2 vec {
            linearVelocity.x,
            linearVelocity.y
        };

        return vec;
    }

    DllExport float GetAngularVelocity(b2Body* body)
    {
        return  body -> GetAngularVelocity();
    }

    DllExport void ApplyForce(b2Body* body, Vector2 force, Vector2 point)
    {
        b2Vec2 bForce(force.x, force.y);
        b2Vec2 bPoint(point.x, point.y);

        body -> ApplyForce(bForce, bPoint, true);
    }

    DllExport void ApplyForceToCenter(b2Body* body, Vector2 force)
    {
        b2Vec2 bForce(force.x, force.y);

        body -> ApplyForceToCenter(bForce, true);
    }

    DllExport void ApplyLinearImpulse(b2Body* body, Vector2 force, Vector2 point)
    {
        b2Vec2 bForce(force.x, force.y);
        b2Vec2 bPoint(point.x, point.y);

        body -> ApplyLinearImpulse(bForce, bPoint, true);
    }

    DllExport void ApplyLinearImpulseToCenter(b2Body* body, Vector2 force)
    {
        b2Vec2 bForce(force.x, force.y);

        body -> ApplyLinearImpulseToCenter(bForce, true);
    }

    DllExport void SetEnabled(b2Body* body, bool flag)
    {
        body -> SetEnabled(flag);
    }

    DllExport bool IsEnabled(b2Body* body)
    {
        return body -> IsEnabled();
    }

    DllExport BodyInfo GetBodyInfo(b2Body* body)
    {
        BodyInfo bodyInfo;
        bodyInfo.position = GetPosition(body);
        bodyInfo.linearVelocity = GetLinearVelocity(body);
        bodyInfo.angularVelocity = GetAngularVelocity(body);
        bodyInfo.angle = GetAngle(body);

        return bodyInfo;
    }

    DllExport void SetBodyInfo(b2Body* body, BodyInfo bodyInfo)
    {
        SetAngle(body, bodyInfo.angle);
        SetPosition(body, bodyInfo.position);
        SetLinearVelocity(body, bodyInfo.linearVelocity);
        SetAngularVelocity(body, bodyInfo.angularVelocity);
    }

    DllExport void SetBullet(b2Body* body, bool flag)
    {
        body -> SetBullet(flag);
    }

    DllExport B2FilterReturnType GetBodyFixturesFilterData(b2Body* body)
    {
        b2Fixture fixture = body -> GetFixtureList()[0];
        b2Filter filter = fixture.GetFilterData();
        B2FilterReturnType filterResult
        {
            filterResult.categoryBits = filter.categoryBits,
            filterResult.maskBits = filter.maskBits,
            filterResult.groupIndex = filter.groupIndex
        };
        
        return filterResult;
    }

    DllExport bool RayCast(b2World* world, Vector2 origin, Vector2 direction,
        RaycastOutputReturnType* hit, float distance, uint16 layerMask)
    {
        b2Vec2 position(origin.x, origin.y);
        
        b2RayCastInput ray;
        ray.p1.Set(position.x, position.y);
        ray.p2.Set(position.x + direction.x * distance, position.y + direction.y * distance);
        ray.maxFraction = distance;

        RayCastClosestCallback callback(layerMask);
        world -> RayCast(&callback, ray.p1, ray.p2);

        if (!callback.m_isHit) return false;

        hit -> body = callback.m_hit.body;
        hit -> normal = callback.m_hit.normal;
        hit -> point = callback.m_hit.point;
        hit -> fraction = callback.m_hit.fraction;

        return true;
    }
}