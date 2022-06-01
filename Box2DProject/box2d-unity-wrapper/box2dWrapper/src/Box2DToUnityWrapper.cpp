#include <Box2DToUnityWrapper.h>

extern "C"
{
    DllExport void SetBeginContactCallback(b2World* world, CollisionCallback callback)
    {
        ((MyContactListener*)world -> GetContactListener()) -> m_callbackBeginContact = callback;
    }

    DllExport void SetEndContactCallback(b2World* world, CollisionCallback callback)
    {
        ((MyContactListener*)world -> GetContactListener()) -> m_callbackEndContact = callback;
    }

    DllExport void SetPreSolveCallback(b2World* world, CollisionCallback callback)
    {
        ((MyContactListener*)world -> GetContactListener()) -> m_callbackPreSolve = callback;
    }

    DllExport void SetPostSolveCallback(b2World* world, CollisionCallback callback)
    {
        ((MyContactListener*)world -> GetContactListener()) -> m_callbackPostSolve = callback;
    }

    DllExport void SetContactCallbacks(b2World* world, CollisionCallback beginContact,
        CollisionCallback endContact, CollisionCallback preSolve, CollisionCallback postSolve)
    {
        SetBeginContactCallback(world, beginContact);
        SetEndContactCallback(world, endContact);
        SetPreSolveCallback(world, preSolve);
        SetPostSolveCallback(world, postSolve);
    }

    DllExport void SetDebugDraw(b2World* world, b2Draw* debugDraw)
    {
        world -> SetDebugDraw(debugDraw);
    }

    DllExport b2Draw* CreateBox2dDebugDraw(DrawDbgCircleCallback drawCircle, DrawDbgCircleCallback drawPoint,
        DrawDbgSegmentCallback drawSegment, DrawDbgTransformCallback drawTransform, DrawDbgPolygonCallback drawPolygon)
    {
        return new MyDebugDraw(drawCircle, drawPoint, drawSegment, drawTransform, drawPolygon);
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

    DllExport b2World* CloneWorld(void**& arrayOfReferences, int count, b2World* world)
    {
        if (world->IsLocked())
        {
            return NULL;
        }
        CloneWorldService cloneService;

        // new World
        b2World* clonedWorld = new b2World(arrayOfReferences, count, *world, cloneService);

        // do we need to set all callbacks manually?
        MyContactListener* myContactListener = new MyContactListener(clonedWorld);
        clonedWorld->SetContactListener(myContactListener);

        return clonedWorld;
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

    DllExport int GetContactListCount(b2Body* body)
    {
        int size = 0;
        for (const b2ContactEdge* cE = body -> GetContactList(); cE; cE = cE->next)
        {
            size++;
        }
        return size;
    }

    DllExport int GetJointListCount(b2Body* body)
    {
        int size = 0;
        for (const b2JointEdge* cE = body->GetJointList(); cE; cE = cE->next)
        {
            size++;
        }
        return size;
    }

    DllExport void TryGetContactList(b2Body* body, ListOfPointersCallback successCb)
    {
        int count = GetContactListCount(body);
        if (count == 0) return;

        b2Body** ptrArray = new b2Body*[count];

        int i = 0;
        for (b2ContactEdge* cE = body -> GetContactList(); 
            cE; cE = cE->next, ++i)
        {
            ptrArray[i] = cE -> other;
        }

        successCb(count, (void**)ptrArray);
        delete ptrArray;
    }

    CollisionCallbackData GetContactInfoFromContact(b2Contact* contact)
    {
        CollisionCallbackData data
        {
            NULL,               //physicsWorld
            -1,                 //entityA
            -1,                 //entityB
            -1,                 //velA
            -1,                 //velB
            { 0, 0}, { 0, 0},   //contactPoints
            2,                  //contactPointCount
            {0, 0}              //normal
        };

        b2WorldManifold worldManifold;
        contact->GetWorldManifold(&worldManifold);

        data.physicsWorld = contact->GetFixtureA()->GetBody()->GetWorld();
        data.entityA = (int)contact->GetFixtureA()->GetBody()->GetUserData().pointer;
        data.entityB = (int)contact->GetFixtureB()->GetBody()->GetUserData().pointer;
        b2Vec2 b2dVelA = contact->GetFixtureA()->GetBody()->GetLinearVelocityFromWorldPoint(worldManifold.points[0]);
        Vector2 velA{
            b2dVelA.x,
            b2dVelA.y
        };
        data.velA = velA;

        b2Vec2 b2dVelB = contact->GetFixtureB()->GetBody()->GetLinearVelocityFromWorldPoint(worldManifold.points[1]);
        Vector2 velB{
            b2dVelB.x,
            b2dVelB.y
        };
        data.velB = velB;

        size_t length = (sizeof(worldManifold.points) / sizeof(*worldManifold.points));
        memcpy(data.contactPoints, worldManifold.points, length * sizeof(Vector2));
        data.contactPointCount = 2;

        Vector2 normal
        {
            worldManifold.normal.x,
            worldManifold.normal.y
        };
        data.normal = normal;

        return data;
    }

    DllExport void TryGetContactInfosForBody(b2Body* body, ListOfPointersCallback cb)
    {
        int count = GetContactListCount(body);
        if (count <= 0) return;

        b2Contact* myContact = NULL;
        CollisionCallbackData **collisionDataArray = new CollisionCallbackData*[count];

        int i = 0;
        for (b2ContactEdge* cE = body->GetContactList(); cE; cE = cE->next, ++i)
        {
            myContact = cE->contact;
            collisionDataArray[i] = &GetContactInfoFromContact(myContact);
        }


        cb(count, (void**)collisionDataArray);
        delete collisionDataArray;
    }

    DllExport void TryGetJointListFromBody(b2Body* body, ListOfPointersCallback cb)
    {
        int count = GetJointListCount(body);
        if (count <= 0) return;

        b2Joint** myJoint = new b2Joint*[count];

        int i = 0;
        for (b2JointEdge* j = body->GetJointList(); j; j = j->next, ++i)
        {
            myJoint[i] = j->joint;
        }


        cb(count, (void**)myJoint);
        delete myJoint;
    }

    DllExport void TryGetContactInfoForBodies(b2Body* body1, b2Body* body2, CollisionCallback cb)
    {
        b2Contact* myContact = NULL;

        for (b2ContactEdge* cE = body1 -> GetContactList(); cE; cE = cE->next)
		{
            if (cE -> other == body2)
            {
                myContact = cE -> contact;
                break;
            }
        }

        if (myContact == NULL) return;

        CollisionCallbackData data = GetContactInfoFromContact(myContact);

        cb(data);
    }

    DllExport void DestroyWorld(b2World* world)
    {
        b2ContactListener* myContactListener = world->GetContactListener();
        delete myContactListener;
        delete world;
    }

    DllExport b2Joint* CreateJoint(b2World* world, b2JointDef* def)
    {
        return world->CreateJoint(def);
    }

    DllExport void DestroyJoint(b2World* world, b2Joint* joint)
    {
        world->DestroyJoint(joint);
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
        b2Free(b2Vertices);
        return shape;
    }

    DllExport b2Shape* CreatePolygonShape(Vector2 vertices[], int count)
    {
        b2Vec2* b2Vertices = (b2Vec2*)b2Alloc(count * sizeof(b2Vec2));
        memcpy(b2Vertices, vertices, count * sizeof(b2Vec2));

        b2PolygonShape* shape = new b2PolygonShape();
        shape->Set(b2Vertices, count);
        b2Free(b2Vertices);
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

    DllExport float GetMass(b2Body* body)
    {
        return body->GetMass();
    }

    DllExport float GetInertia(b2Body* body)
    {
        return body->GetInertia();
    }

    DllExport void SetFixedRotation(b2Body* body, bool flag)
    {
        body->SetFixedRotation(flag);
    }

    DllExport bool IsFixedRotation(b2Body* body)
    {
        return body->IsFixedRotation();
    }

    DllExport bool IsAwake(b2Body* body)
    {
        return body->IsAwake();
    }

    DllExport bool IsSleepingAllowed(b2Body* body)
    {
        return body->IsSleepingAllowed();
    }

    DllExport void SetSleepingAllowed(b2Body* body, bool flag)
    {
        body->SetSleepingAllowed(flag);
    }

    DllExport bool IsBullet(b2Body* body)
    {
        return body->IsBullet();
    }

    DllExport int GetType(b2Body* body)
    {
        return body->GetType();
    }

    DllExport void SetType(b2Body* body, int type)
    {
        if (body->GetWorld()->m_locked)
        {
            return;
        }
        body->SetType((b2BodyType)type);
    }

    DllExport void SetGravityScale(b2Body* body, float scale)
    {
        body->SetGravityScale(scale);
    }

    DllExport float GetGravityScale(b2Body* body)
    {
        return body->GetGravityScale();
    }

    DllExport void SetAwake(b2Body* body, bool flag)
    {
        body->SetAwake(flag);
    }

    DllExport void ApplyTorque(b2Body* body, float torque, bool wake)
    {
        body->ApplyTorque(torque, wake);
    }

    DllExport void ApplyAngularImpulse(b2Body* body, float impulse, bool wake)
    {
        body->ApplyAngularImpulse(impulse, wake);
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

    DllExport void SetPosition(b2Body* body, Vector2 position, bool wake)
    {
        if (body->GetWorld()->m_locked)
        {
            return;
        }
        float angle = body -> GetAngle();
        b2Vec2 bPosition(position.x, position.y);

        body -> SetTransform(bPosition, angle);
        body -> SetAwake(wake);
    }

    DllExport float GetAngle(b2Body* body)
    {
        return body -> GetAngle();
    }

    DllExport void SetAngle(b2Body* body, float angle)
    {
        if (body->GetWorld()->m_locked)
        {
            return;
        }
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

    DllExport void ApplyForce(b2Body* body, Vector2 force, Vector2 point, bool wake)
    {
        b2Vec2 bForce(force.x, force.y);
        b2Vec2 bPoint(point.x, point.y);

        body -> ApplyForce(bForce, bPoint, wake);
    }

    DllExport void ApplyForceToCenter(b2Body* body, Vector2 force, bool wake)
    {
        b2Vec2 bForce(force.x, force.y);

        body -> ApplyForceToCenter(bForce, wake);
    }

    DllExport void ApplyLinearImpulse(b2Body* body, Vector2 force, Vector2 point, bool wake)
    {
        b2Vec2 bForce(force.x, force.y);
        b2Vec2 bPoint(point.x, point.y);

        body -> ApplyLinearImpulse(bForce, bPoint, wake);
    }

    DllExport void ApplyLinearImpulseToCenter(b2Body* body, Vector2 force, bool wake)
    {
        b2Vec2 bForce(force.x, force.y);

        body -> ApplyLinearImpulseToCenter(bForce, wake);
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
        SetPosition(body, bodyInfo.position, bodyInfo.awake);
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

    // JOINTS

}