using System;

namespace Soft3DEngine
{
    public static class Mathf
    {
        private const float ZERO_TOLERANCE = 1e-6f;

        public static float Clamp(float value, float min, float max)
        {
            return (Math.Max(min, Math.Min(value, max)));
        }

        public static float Clamp01(float value)
        {
            return (Clamp(value, 0.0f, 1.0f));
        }

        public static bool IsZero(float value)
        {
            return (Math.Abs(value) < ZERO_TOLERANCE);
        }

        public static float Lerp(float start, float end, float beta)
        {
            return (start + (end - start) * Mathf.Clamp01(beta));
        }
    }
}
