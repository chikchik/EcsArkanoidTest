using System;
using Fabros.EcsModules.Box2D.ClientServer.Api;
using UnityEngine;

namespace Fabros.EcsModules.Box2D.Client
{
    public class Box2dGizmosView : MonoBehaviour
    {
        private IntPtr box2d;
        
        [SerializeField]
        private bool draw = true;

        private IntPtr debugDraw;

        public void SetBox2D(IntPtr box2d)
        {
            if (debugDraw == default)
            {
                debugDraw = Box2DApi.CreateBox2dDebugDraw(DrawCircle,
                    DrawPoint, 
                    DrawSegment,
                    DrawTransform,
                    DrawPolygon);
            }

            Box2DApi.SetDebugDraw(box2d, debugDraw);
            this.box2d = box2d;
        }
        
        private void OnDrawGizmos()
        {
            if (box2d == default)
                return;
            if (!draw)
                return;

            Box2DApi.DebugDraw(box2d);
        }
        
        static void DrawPolygon(Vector2[] v, Int32 vCount, Box2dColor color)
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

        static void DrawCircle(Vector2 center, float radius, Box2dColor color)
        {
            Gizmos.color = new Color(color.R, color.Q, color.B, color.A);
            Gizmos.DrawWireSphere(new Vector3(center.x, radius, center.y), radius);
        }
        
        static void DrawPoint(Vector2 center, float radius, Box2dColor color)
        {
            Gizmos.color = new Color(color.R, color.Q, color.B, color.A);
            Gizmos.DrawSphere(new Vector3(center.x, radius, center.y), 0.1f);
        }
        
        static void DrawSegment(Vector2 v1, Vector2 v2, Box2dColor color)
        {
            Gizmos.color = new Color(color.R, color.Q, color.B, color.A);
            Gizmos.DrawLine(new Vector3(v1.x, 0.1f, v1.y), new Vector3(v2.x, 0.1f, v2.y));
        }

        static void DrawTransform(Vector2 v, Vector2 angleX, Box2dColor color)
        {
            Gizmos.color = new Color(color.R, color.Q, color.B, color.A);
            var v1 = new Vector3(v.x, 0.1f, v.y);
            var v2 = new Vector3(angleX.x, 0.1f, angleX.y);
            Gizmos.DrawLine(v1, v1 + v2);
        }
    }
}