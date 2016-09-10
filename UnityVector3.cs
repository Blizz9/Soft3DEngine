using System;

namespace Soft3DEngine
{
    public struct UnityVector3
    {
        private const float ZERO_TOLERANCE = 1e-6f;

        public float X;
        public float Y;
        public float Z;

        public UnityVector3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public static readonly UnityVector3 Zero = new UnityVector3();

        public static readonly UnityVector3 Up = new UnityVector3(0, 1, 0);

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

        public static UnityVector3 Cross(UnityVector3 left, UnityVector3 right)
        {
            return (new UnityVector3((left.Y * right.Z) - (left.Z * right.Y), (left.Z * right.X) - (left.X * right.Z), (left.X * right.Y) - (left.Y * right.X)));
        }

        public static float Dot(UnityVector3 left, UnityVector3 right)
        {
            return ((left.X * right.X) + (left.Y * right.Y) + (left.Z * right.Z));
        }

        public static UnityVector3 operator -(UnityVector3 left, UnityVector3 right)
        {
            return (new UnityVector3(left.X - right.X, left.Y - right.Y, left.Z - right.Z));
        }
    }
}
