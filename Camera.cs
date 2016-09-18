using System;

namespace Soft3DEngine
{
    public class Camera
    {
        public float FieldOfView { get; set; }
        public float Aspect { get; set; }
        public float NearClipPlane { get; set; }
        public float FarClipPlane { get; set; }
        public Vector3 Position { get; set; }
        public Vector3 Target { get; set; }

        public Matrix4x4 CreateProjectionMatrix()
        {
            float yScale = (float)(1f / Math.Tan(FieldOfView * 0.5f));
            float q = FarClipPlane / (FarClipPlane - NearClipPlane);

            Matrix4x4 projection = new Matrix4x4();

            projection.M00 = yScale / Aspect;
            projection.M11 = yScale;
            projection.M22 = q;
            projection.M23 = 1.0f;
            projection.M32 = -q * NearClipPlane;

            return (projection);
        }

        public Matrix4x4 CreateWorldToCameraMatrix()
        {
            // Game Engine Architecture - page 478

            Vector3 zAxis = Target - Position;
            zAxis.Normalize();

            Vector3 xAxis = Vector3.Cross(Vector3.Up, zAxis);
            xAxis.Normalize();

            Vector3 yAxis = Vector3.Cross(zAxis, xAxis);

            Matrix4x4 worldToCamera = Matrix4x4.Identity;

            worldToCamera.M00 = xAxis.X;
            worldToCamera.M01 = yAxis.X;
            worldToCamera.M02 = zAxis.X;
            worldToCamera.M10 = xAxis.Y;
            worldToCamera.M11 = yAxis.Y;
            worldToCamera.M12 = zAxis.Y;
            worldToCamera.M20 = xAxis.Z;
            worldToCamera.M21 = yAxis.Z;
            worldToCamera.M22 = zAxis.Z;

            worldToCamera.M30 = Vector3.Dot(xAxis, Position);
            worldToCamera.M31 = Vector3.Dot(yAxis, Position);
            worldToCamera.M32 = Vector3.Dot(zAxis, Position);

            worldToCamera.M30 = -worldToCamera.M30;
            worldToCamera.M31 = -worldToCamera.M31;
            worldToCamera.M32 = -worldToCamera.M32;

            return (worldToCamera);
        }
    }
}
