using System;
using Game.Fabros.EcsModules.Box2D.ClientServer.Api;
using Game.Fabros.EcsModules.Box2D.ClientServer.Components;
using Leopotam.EcsLite;
using UnityEngine;

namespace Game.Fabros.EcsModules.Box2D.Client.Systems
{
    public class Box2dDebugViewSystem : IEcsInitSystem, IEcsDestroySystem
    {
        private Box2DApi.DrawDbgCircleCallback _circleCallback;
        private Box2DApi.DrawDbgCircleCallback _pointCallback;
        private Box2DApi.DrawDbgSegmentCallback _segmentCallback;
        private Box2DApi.DrawDbgTransformCallback _transformCallback;
        private Box2DApi.DrawDbgPolygonCallback _polygonCallback;

        public void Init(EcsSystems systems)
        {
            var world = systems.GetWorld();

            if (!world.HasUnique<PhysicsWorldComponent>()) return;
            var physicsWorld = world.GetUnique<PhysicsWorldComponent>().WorldReference;

            _circleCallback = DrawCircle;
            _pointCallback = DrawPoint;
            _segmentCallback = DrawSegment;
            _transformCallback = DrawTransform;
            _polygonCallback = DrawPolygon;
            Box2DApi.SetDebugDraw(physicsWorld, _circleCallback, _pointCallback, _segmentCallback,
                _transformCallback, _polygonCallback);

            var box2dDebugDrawFlags = (UInt32) (Box2dDebugDrawFlags.ShapeBit | Box2dDebugDrawFlags.JointBit |
                                                Box2dDebugDrawFlags.AabbBit | Box2dDebugDrawFlags.PairBit |
                                                Box2dDebugDrawFlags.CenterOfMassBit | Box2dDebugDrawFlags.ContactBit);
            Box2DApi.SetFlagsForDebugDraw(physicsWorld, box2dDebugDrawFlags);
        }
        public void Destroy(EcsSystems systems)
        {
            //todo impl destroy
        }
        
        void DrawPolygon(Vector2[] v, Int32 vCount, Box2dColor color)
        {
            if (vCount < 2) return; 
            Gizmos.color = new Color(color.R, color.Q, color.B, color.A);
            for (int i = 1; i < vCount; i++)
            {
                var v1 = new Vector3(v[i].x, 0.1f, v[i].y);
                var v2 = new Vector3(v[i - 1].x, 0.1f, v[i - 1].y);
                Gizmos.DrawLine(v1, v2);
            }
            Gizmos.DrawLine(new Vector3(v[0].x, 0.1f, v[0].y),
                new Vector3(v[vCount - 1].x, 0.1f, v[vCount - 1].y));
        }

        void DrawCircle(Vector2 center, float radius, Box2dColor color)
        {
            Gizmos.color = new Color(color.R, color.Q, color.B, color.A);
            Gizmos.DrawWireSphere(new Vector3(center.x, radius, center.y), radius);
        }
        
        void DrawPoint(Vector2 center, float radius, Box2dColor color)
        {
            Gizmos.color = new Color(color.R, color.Q, color.B, color.A);
            Gizmos.DrawSphere(new Vector3(center.x, radius, center.y), 0.1f);
        }
        
        void DrawSegment(Vector2 v1, Vector2 v2, Box2dColor color)
        {
            Gizmos.color = new Color(color.R, color.Q, color.B, color.A);
            Gizmos.DrawLine(new Vector3(v1.x, 0.1f, v1.y), new Vector3(v2.x, 0.1f, v2.y));
        }

        void DrawTransform(Vector2 v, Vector2 angleX, Box2dColor color)
        {
            Gizmos.color = new Color(color.R, color.Q, color.B, color.A);
            var v1 = new Vector3(v.x, 0.1f, v.y);
            var v2 = new Vector3(angleX.x, 0.1f, angleX.y);
            Gizmos.DrawLine(v1, v1 + v2);
        }

    }
}