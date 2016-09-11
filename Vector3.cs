using System;

namespace Soft3DEngine
{
    public struct Vector3
    {
        public float X;
        public float Y;
        public float Z;

        public Vector3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public static readonly Vector3 Zero = new Vector3();

        public static readonly Vector3 Up = new Vector3(0.0f, 1.0f, 0.0f);

        public float Magnitude()
        {
            // Game Engine Architecture - page 171
            return ((float)Math.Sqrt((X * X) + (Y * Y) + (Z * Z)));
        }

        public void Normalize()
        {
            // Game Engine Architecture - page 173

            float magnitude = Magnitude();

            if (!Mathf.IsZero(magnitude))
            {
                float inverseMagnitude = 1.0f / magnitude;

                X *= inverseMagnitude;
                Y *= inverseMagnitude;
                Z *= inverseMagnitude;
            }
        }

        public static Vector3 Cross(Vector3 left, Vector3 right)
        {
            // Game Engine Architecture - page 176
            return (new Vector3((left.Y * right.Z) - (left.Z * right.Y), (left.Z * right.X) - (left.X * right.Z), (left.X * right.Y) - (left.Y * right.X)));
        }

        public static float Dot(Vector3 left, Vector3 right)
        {
            // Game Engine Architecture - page 173
            return ((left.X * right.X) + (left.Y * right.Y) + (left.Z * right.Z));
        }

        public static Vector3 operator -(Vector3 left, Vector3 right)
        {
            // Game Engine Architecture - page 171
            return (new Vector3(left.X - right.X, left.Y - right.Y, left.Z - right.Z));
        }
    }
}
