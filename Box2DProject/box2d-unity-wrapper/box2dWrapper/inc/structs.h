#ifndef B2_W_STRUCTS_H
#define B2_W_STRUCTS_H

#include <box2d.h>

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

#endif