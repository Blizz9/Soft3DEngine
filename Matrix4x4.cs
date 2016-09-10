namespace Soft3DEngine
{
    public struct Matrix4x4
    {
        public float M00;
        public float M01;
        public float M02;
        public float M03;
        public float M10;
        public float M11;
        public float M12;
        public float M13;
        public float M20;
        public float M21;
        public float M22;
        public float M23;
        public float M30;
        public float M31;
        public float M32;
        public float M33;

        public static readonly Matrix4x4 Identity = new Matrix4x4() { M00 = 1, M11 = 1, M22 = 1, M33 = 1 };

        public static Matrix4x4 operator *(Matrix4x4 left, Matrix4x4 right)
        {
            Matrix4x4 multiplied = new Matrix4x4();

            multiplied.M00 = (left.M00 * right.M00) + (left.M01 * right.M10) + (left.M02 * right.M20) + (left.M03 * right.M30);
            multiplied.M01 = (left.M00 * right.M01) + (left.M01 * right.M11) + (left.M02 * right.M21) + (left.M03 * right.M31);
            multiplied.M02 = (left.M00 * right.M02) + (left.M01 * right.M12) + (left.M02 * right.M22) + (left.M03 * right.M32);
            multiplied.M03 = (left.M00 * right.M03) + (left.M01 * right.M13) + (left.M02 * right.M23) + (left.M03 * right.M33);
            multiplied.M10 = (left.M10 * right.M00) + (left.M11 * right.M10) + (left.M12 * right.M20) + (left.M13 * right.M30);
            multiplied.M11 = (left.M10 * right.M01) + (left.M11 * right.M11) + (left.M12 * right.M21) + (left.M13 * right.M31);
            multiplied.M12 = (left.M10 * right.M02) + (left.M11 * right.M12) + (left.M12 * right.M22) + (left.M13 * right.M32);
            multiplied.M13 = (left.M10 * right.M03) + (left.M11 * right.M13) + (left.M12 * right.M23) + (left.M13 * right.M33);
            multiplied.M20 = (left.M20 * right.M00) + (left.M21 * right.M10) + (left.M22 * right.M20) + (left.M23 * right.M30);
            multiplied.M21 = (left.M20 * right.M01) + (left.M21 * right.M11) + (left.M22 * right.M21) + (left.M23 * right.M31);
            multiplied.M22 = (left.M20 * right.M02) + (left.M21 * right.M12) + (left.M22 * right.M22) + (left.M23 * right.M32);
            multiplied.M23 = (left.M20 * right.M03) + (left.M21 * right.M13) + (left.M22 * right.M23) + (left.M23 * right.M33);
            multiplied.M30 = (left.M30 * right.M00) + (left.M31 * right.M10) + (left.M32 * right.M20) + (left.M33 * right.M30);
            multiplied.M31 = (left.M30 * right.M01) + (left.M31 * right.M11) + (left.M32 * right.M21) + (left.M33 * right.M31);
            multiplied.M32 = (left.M30 * right.M02) + (left.M31 * right.M12) + (left.M32 * right.M22) + (left.M33 * right.M32);
            multiplied.M33 = (left.M30 * right.M03) + (left.M31 * right.M13) + (left.M32 * right.M23) + (left.M33 * right.M33);

            return (multiplied);
        }
    }
}
