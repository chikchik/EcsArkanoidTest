#ifndef B2_W_MYDEBUGDRAW_H
#define B2_W_MYDEBUGDRAW_H

#include <box2d.h>
#include <callbacks.h>
#include <structs.h>

class MyDebugDraw : public b2Draw
{
    public:
    DrawDbgCircleCallback m_drawCircle;
    DrawDbgCircleCallback m_drawPoint;
    DrawDbgSegmentCallback m_drawSegment;
    DrawDbgTransformCallback m_drawTransform;
    DrawDbgPolygonCallback m_drawPolygon;


    MyDebugDraw(DrawDbgCircleCallback drawCircle, DrawDbgCircleCallback drawPoint, DrawDbgSegmentCallback drawSegment,
        DrawDbgTransformCallback drawTransform, DrawDbgPolygonCallback drawPolygon);

    void DrawPolygon (const b2Vec2 *vertices, int32 vertexCount, const b2Color &color);
    void DrawSolidPolygon (const b2Vec2 *vertices, int32 vertexCount, const b2Color &color);
    void DrawCircle (const b2Vec2 &center, float radius, const b2Color &color);
    void DrawSolidCircle (const b2Vec2 &center, float radius, const b2Vec2 &axis, const b2Color &color);
    void DrawSegment (const b2Vec2 &p1, const b2Vec2 &p2, const b2Color &color);
    void DrawTransform (const b2Transform &xf);
    void DrawPoint (const b2Vec2 &p, float size, const b2Color &color);
};


#endif