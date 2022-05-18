#include <debug_draw.h>

MyDebugDraw::MyDebugDraw(DrawDbgCircleCallback drawCircle, DrawDbgCircleCallback drawPoint, DrawDbgSegmentCallback drawSegment,
    DrawDbgTransformCallback drawTransform, DrawDbgPolygonCallback drawPolygon)
{
    m_drawCircle = drawCircle;
    m_drawPoint = drawPoint;
    m_drawSegment = drawSegment;
    m_drawTransform = drawTransform;
    m_drawPolygon = drawPolygon;
}

void MyDebugDraw::DrawPolygon (const b2Vec2 *vertices, int32 vertexCount, const b2Color &color)
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
void MyDebugDraw::DrawSolidPolygon (const b2Vec2 *vertices, int32 vertexCount, const b2Color &color)
{
    DrawPolygon(vertices, vertexCount, color);
}

void MyDebugDraw::DrawCircle (const b2Vec2 &center, float radius, const b2Color &color)
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

void MyDebugDraw::DrawSolidCircle (const b2Vec2 &center, float radius, const b2Vec2 &axis, const b2Color &color)
{
    DrawCircle(center, radius, color);
}
void MyDebugDraw::DrawSegment (const b2Vec2 &p1, const b2Vec2 &p2, const b2Color &color)
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
void MyDebugDraw::DrawTransform (const b2Transform &xf)
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
void MyDebugDraw::DrawPoint (const b2Vec2 &p, float size, const b2Color &color)
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