#ifndef B2_W_RAYCAST_H
#define B2_W_RAYCAST_H

#include <structs.h>
#include <misc.h>
#include <vector>

class RayCastClosestCallback : public b2RayCastCallback
{
public:
    RaycastOutputReturnType m_hit;
    uint16 m_layerMask;
    bool m_isHit = false;

    RayCastClosestCallback(uint16 layerMask);
    
    float ReportFixture (b2Fixture *fixture, const b2Vec2 &point, const b2Vec2 &normal, float fraction);
};

class RayCastAnyCallback : public b2RayCastCallback
{
public:
    float ReportFixture (b2Fixture *fixture, const b2Vec2 &point, const b2Vec2 &normal, float fraction);
};

class RayCastMultipleCallback : public b2RayCastCallback {
public:
    std::vector<RaycastOutputReturnType> foundBodies;
    
    float ReportFixture (b2Fixture *fixture,const b2Vec2 &point, const b2Vec2 &normal, float fraction);
};

#endif