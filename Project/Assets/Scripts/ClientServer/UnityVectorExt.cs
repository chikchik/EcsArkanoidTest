using UnityEngine;

namespace Game.ClientServer
{
    public static partial class UnityVectorExt
    {
        public static Vector2 WithX(this Vector2 v, float x)
        {
            return new Vector2(x, v.y);
        }

        public static Vector3 ToVector3XZ(this Vector2 v)
        {
            return new Vector3(v.x, 0, v.y);
        }

        public static Vector2 WithY(this Vector2 v, float y)
        {
            return new Vector2(v.x, y);
        }

        public static Vector2 WithAddedToX(this Vector2 v, float x)
        {
            return new Vector2(v.x + x, v.y);
        }

        public static Vector2 WithAddedToY(this Vector2 v, float y)
        {
            return new Vector2(v.x, v.y + y);
        }

        public static Vector3 WithX(this Vector3 v, float x)
        {
            return new Vector3(x, v.y, v.z);
        }

        public static Vector3 DivInverted(this Vector3 v)
        {
            return new Vector3(1f / v.x, 1f / v.y, 1f / v.z);
        }

        public static Vector3 WithAddedToX(this Vector3 v, float s)
        {
            return new Vector3(v.x + s, v.y, v.z);
        }

        public static Vector3 WithAddedToY(this Vector3 v, float s)
        {
            return new Vector3(v.x, v.y + s, v.z);
        }

        public static Vector3 WithAddedToZ(this Vector3 v, float s)
        {
            return new Vector3(v.x, v.y, v.z + s);
        }

        public static Vector3 WithMultipliedX(this Vector3 v, float s)
        {
            return new Vector3(v.x * s, v.y, v.z);
        }

        public static Vector2 ToVector2XZ(this Vector3 v)
        {
            return new Vector2(v.x, v.z);
        }

        public static Vector2 ToVector2XY(this Vector3 v)
        {
            return new Vector2(v.x, v.y);
        }

        public static Vector3 WithMultiplied(this Vector3 v, float s)
        {
            return new Vector3(v.x * s, v.y * s, v.z * s);
        }

        public static Vector3 WithMultipliedY(this Vector3 v, float s)
        {
            return new Vector3(v.x, v.y * s, v.z);
        }

        public static Vector2 WithMultipliedX(this Vector2 v, float s)
        {
            return new Vector2(v.x * s, v.y);
        }

        public static Vector2 WithMultipliedY(this Vector2 v, float s)
        {
            return new Vector2(v.x, v.y * s);
        }

        public static Vector2 WithMultipliedXY(this Vector2 v, float sx, float sy)
        {
            return new Vector2(v.x * sx, v.y * sy);
        }

        public static Vector3 WithMultipliedZ(this Vector3 v, float s)
        {
            return new Vector3(v.x, v.y, v.z * s);
        }

        public static Vector3 WithY(this Vector3 v, float y)
        {
            return new Vector3(v.x, y, v.z);
        }

        public static Vector3 WithAdded(this Vector3 v, float x, float y, float z)
        {
            return new Vector3(v.x + x, v.y + y, v.z + z);
        }

        public static Vector3Int WithAdded(this Vector3Int v, int x, int y, int z)
        {
            return new Vector3Int(v.x + x, v.y + y, v.z + z);
        }

        public static Vector3 WithZ(this Vector3 v, float z)
        {
            return new Vector3(v.x, v.y, z);
        }

        public static Vector3 WithZ(this Vector2 v, float z)
        {
            return new Vector3(v.x, v.y, z);
        }

        public static Vector3 WithSwappedYZ(this Vector3 v)
        {
            return new Vector3(v.x, v.z, v.y);
        }

        public static Vector2 ToVector2(this Vector3 v)
        {
            return new Vector2(v.x, v.y);
        }

        public static float Dot(this Vector2 v1, Vector2 v2)
        {
            return v1.x * v2.x + v1.y * v2.y;
        }
    }
}