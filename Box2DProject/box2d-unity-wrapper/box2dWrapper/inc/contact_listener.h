#ifndef B2_W_CONTACTLISTENER_H
#define B2_W_CONTACTLISTENER_H

#include <box2d.h>
#include <callbacks.h>
#include <structs.h>

class MyContactListener : public b2ContactListener
{
    private:

    b2World* m_world = NULL;

    void GetEntityContacts(CollisionCallbackData* data, b2Contact* contact);

    void GetContactVelocity(CollisionCallbackData* data, b2WorldManifold* worldManifold, b2Contact* contact);

    void GetContactPoints(CollisionCallbackData* data, b2WorldManifold* worldManifold);

    void GetNormal(CollisionCallbackData* data, b2WorldManifold* worldManifold);

    void BeginContact(b2Contact* contact);

    void EndContact(b2Contact* contact);

    void PreSolve(b2Contact* contact, const b2Manifold* oldManifold);

    void PostSolve(b2Contact* contact, const b2ContactImpulse* impulse);

    public:

    MyContactListener(b2World* world) : m_world(world) {};

    CollisionCallback m_callbackBeginContact = NULL;
    CollisionCallback m_callbackEndContact = NULL;
    CollisionCallback m_callbackPreSolve = NULL;
    CollisionCallback m_callbackPostSolve = NULL;
};

#endif