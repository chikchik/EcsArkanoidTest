#include <raycast.h>

RayCastClosestCallback::RayCastClosestCallback(uint16 layerMask)
{
    m_layerMask = layerMask;
}

float RayCastClosestCallback::ReportFixture (b2Fixture *fixture, const b2Vec2 &point, const b2Vec2 &normal, float fraction)
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

float RayCastAnyCallback::ReportFixture (b2Fixture *fixture, const b2Vec2 &point, const b2Vec2 &normal, float fraction)
{
    return 0.0f;
}

float RayCastMultipleCallback::ReportFixture (b2Fixture *fixture,
    const b2Vec2 &point, const b2Vec2 &normal, float fraction) {
    RaycastOutputReturnType m_hit;
    m_hit.body = fixture->GetBody();
    m_hit.normal = GetUnityVectorFromB2d(normal);
    m_hit.point = GetUnityVectorFromB2d(point);
    m_hit.fraction = fraction;

    foundBodies.push_back(m_hit);

    return 1.0f;
}
