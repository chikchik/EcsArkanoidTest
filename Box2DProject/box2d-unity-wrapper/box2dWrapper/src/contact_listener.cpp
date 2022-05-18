#include <contact_listener.h>

void MyContactListener::GetEntityContacts(CollisionCallbackData* data, b2Contact* contact)
{
    data->entityA = (int)contact->GetFixtureA()->GetBody()->GetUserData().pointer;
    data->entityB = (int)contact->GetFixtureB()->GetBody()->GetUserData().pointer;

    b2WorldManifold worldManifold;
    contact -> GetWorldManifold(&worldManifold);

    GetContactVelocity(data, &worldManifold, contact);

    GetNormal(data, &worldManifold);

    GetContactPoints(data, &worldManifold);
}

void MyContactListener::GetContactVelocity(CollisionCallbackData* data, b2WorldManifold* worldManifold, b2Contact* contact)
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

void MyContactListener::GetContactPoints(CollisionCallbackData* data, b2WorldManifold* worldManifold)
{
    size_t length = (sizeof(worldManifold->points)/sizeof(*worldManifold->points));
    memcpy(data->contactPoints, worldManifold->points, length * sizeof(Vector2));
}

void MyContactListener::GetNormal(CollisionCallbackData* data, b2WorldManifold* worldManifold)
{
    Vector2 vec2 {
        worldManifold->normal.x,
        worldManifold->normal.y
    };
    data->normal = vec2;
}

void MyContactListener::BeginContact(b2Contact* contact)
{
    if (m_callbackBeginContact == NULL) return;

    CollisionCallbackData data;
    GetEntityContacts(&data, contact);

    m_callbackBeginContact(data);
}

void MyContactListener::EndContact(b2Contact* contact)
{
    if (m_callbackEndContact == NULL) return;
    CollisionCallbackData data;
    GetEntityContacts(&data, contact);
    m_callbackEndContact(data);
}

void MyContactListener::PreSolve(b2Contact* contact, const b2Manifold* oldManifold)
{
    if (m_callbackPreSolve == NULL) return;
    CollisionCallbackData data;
    GetEntityContacts(&data, contact);
    m_callbackPreSolve(data);
}

void MyContactListener::PostSolve(b2Contact* contact, const b2ContactImpulse* impulse)
{
    if (m_callbackPostSolve == NULL) return;
    CollisionCallbackData data;
    GetEntityContacts(&data, contact);
    m_callbackPostSolve(data);
}