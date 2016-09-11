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

        // Game Engine Architecture - page 184
        public static readonly Matrix4x4 Identity = new Matrix4x4() { M00 = 1.0f, M11 = 1.0f, M22 = 1.0f, M33 = 1.0f };

        public Vector3 MultiplyPoint(Vector3 point)
        {
            // Game Engine Architecture - page 187

            Vector4 transformationVector = new Vector4();

            transformationVector.X = (point.X * M00) + (point.Y * M10) + (point.Z * M20) + M30;
            transformationVector.Y = (point.X * M01) + (point.Y * M11) + (point.Z * M21) + M31;
            transformationVector.Z = (point.X * M02) + (point.Y * M12) + (point.Z * M22) + M32;
            transformationVector.W = 1.0f / ((point.X * M03) + (point.Y * M13) + (point.Z * M23) + M33);

            return (new Vector3(transformationVector.X * transformationVector.W, transformationVector.Y * transformationVector.W, transformationVector.Z * transformationVector.W));
        }

        public static Matrix4x4 CreateRotation(float yaw, float pitch, float roll)
        {
            // Game Engine Architecture - page 205

            Quaternion rotation = Quaternion.Euler(yaw, pitch, roll);

            Matrix4x4 rotationMatrix = Matrix4x4.Identity;

            rotationMatrix.M00 = 1.0f - (2.0f * (rotation.Y * rotation.Y + rotation.Z * rotation.Z));
            rotationMatrix.M01 = 2.0f * (rotation.X * rotation.Y + rotation.Z * rotation.W);
            rotationMatrix.M02 = 2.0f * (rotation.X * rotation.Z - rotation.Y * rotation.W);
            rotationMatrix.M10 = 2.0f * (rotation.X * rotation.Y - rotation.Z * rotation.W);
            rotationMatrix.M11 = 1.0f - (2.0f * (rotation.X * rotation.X + rotation.Z * rotation.Z));
            rotationMatrix.M12 = 2.0f * (rotation.Y * rotation.Z + rotation.X * rotation.W);
            rotationMatrix.M20 = 2.0f * (rotation.X * rotation.Z + rotation.Y * rotation.W);
            rotationMatrix.M21 = 2.0f * (rotation.Y * rotation.Z - rotation.X * rotation.W);
            rotationMatrix.M22 = 1.0f - (2.0f * (rotation.X * rotation.X + rotation.Y * rotation.Y));

            return (rotationMatrix);
        }

        public static Matrix4x4 CreateTranslation(Vector3 position)
        {
            // Game Engine Architecture - page 186

            Matrix4x4 translation = Matrix4x4.Identity;

            translation.M30 = position.X;
            translation.M31 = position.Y;
            translation.M32 = position.Z;

            return (translation);
        }

        public static Matrix4x4 operator *(Matrix4x4 left, Matrix4x4 right)
        {
            // Game Engine Architecture - page 182

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
