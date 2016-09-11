using System;

namespace Soft3DEngine
{
    public struct Vector3
    {
        private const float ZERO_TOLERANCE = 1e-6f;

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

        public static readonly Vector3 Up = new Vector3(0, 1, 0);

        private bool isZero(float value)
        {
            return (Math.Abs(value) < ZERO_TOLERANCE);
        }

        public float Magnitude()
        {
            return ((float)Math.Sqrt((X * X) + (Y * Y) + (Z * Z)));
        }

        public void Normalize()
        {
            float magnitude = Magnitude();

            if (!isZero(magnitude))
            {
                float inverseMagnitude = 1.0f / magnitude;

                X *= inverseMagnitude;
                Y *= inverseMagnitude;
                Z *= inverseMagnitude;
            }
        }

        public static Vector3 Cross(Vector3 left, Vector3 right)
        {
            return (new Vector3((left.Y * right.Z) - (left.Z * right.Y), (left.Z * right.X) - (left.X * right.Z), (left.X * right.Y) - (left.Y * right.X)));
        }

        public static float Dot(Vector3 left, Vector3 right)
        {
            return ((left.X * right.X) + (left.Y * right.Y) + (left.Z * right.Z));
        }

        public static Vector3 operator -(Vector3 left, Vector3 right)
        {
            return (new Vector3(left.X - right.X, left.Y - right.Y, left.Z - right.Z));
        }
    }
}
