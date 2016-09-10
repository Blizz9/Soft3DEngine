using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Soft3DEngine
{
    public class Device
    {
        private WriteableBitmap _renderTarget;
        private byte[] _backBuffer;

        public Device(WriteableBitmap renderTarget)
        {
            _renderTarget = renderTarget;
            _backBuffer = new byte[_renderTarget.PixelWidth * _renderTarget.PixelHeight * 4];
        }

        public void Clear(System.Windows.Media.Color color)
        {
            for (long index = 0; index < _backBuffer.Length; index += 4)
            {
                _backBuffer[index] = color.B;
                _backBuffer[index + 1] = color.G;
                _backBuffer[index + 2] = color.R;
                _backBuffer[index + 3] = color.A;
            }
        }

        public void Render(Camera camera, List<Mesh> meshes)
        {
            Matrix4x4 viewMatrix = createLHLookAt(camera.Position, camera.Target, UnityVector3.Up);
            Matrix4x4 projectionMatrix = createRHPerspective(.78f, (float)_renderTarget.PixelWidth / _renderTarget.PixelHeight, .01f, 1);

            foreach (Mesh mesh in meshes)
            {
                //Vector3 tempSharpDXVector3 = new Vector3(mesh.Position.X, mesh.Position.Y, mesh.Position.Z);
                //Matrix worldMatrix = Matrix.RotationYawPitchRoll(mesh.Rotation.Y, mesh.Rotation.X, mesh.Rotation.Z) * Matrix.Translation(tempSharpDXVector3);
                Matrix4x4 worldMatrix = rotationYawPitchRoll(mesh.Rotation.Y, mesh.Rotation.X, mesh.Rotation.Z) * translation(mesh.Position);
                Matrix4x4 transformationMatrix = worldMatrix * viewMatrix * projectionMatrix;

                foreach (Face face in mesh.Faces)
                {
                    UnityVector3 vertexA = mesh.Vertices[face.A];
                    UnityVector3 vertexB = mesh.Vertices[face.B];
                    UnityVector3 vertexC = mesh.Vertices[face.C];

                    UnityVector2 pointA = project(vertexA, transformationMatrix);
                    UnityVector2 pointB = project(vertexB, transformationMatrix);
                    UnityVector2 pointC = project(vertexC, transformationMatrix);

                    drawLine(pointA, pointB);
                    drawLine(pointB, pointC);
                    drawLine(pointC, pointA);
                }
            }
        }

        public void Present()
        {
            int stride = _renderTarget.PixelWidth * ((_renderTarget.Format.BitsPerPixel + 7) / 8);
            _renderTarget.WritePixels(new Int32Rect(0, 0, _renderTarget.PixelWidth, _renderTarget.PixelHeight), _backBuffer, stride, 0);
        }

        private void putPixel(int x, int y, System.Windows.Media.Color color)
        {
            long index = (x + y * _renderTarget.PixelWidth) * 4;

            _backBuffer[index] = (byte)(color.B * 255);
            _backBuffer[index + 1] = (byte)(color.G * 255);
            _backBuffer[index + 2] = (byte)(color.R * 255);
            _backBuffer[index + 3] = (byte)(color.A * 255);
        }

        private void drawPoint(UnityVector2 point)
        {
            // cull any points outside the render target
            if ((point.X >= 0) && (point.Y >= 0) && (point.X < _renderTarget.PixelWidth) && (point.Y < _renderTarget.PixelHeight))
                putPixel((int)point.X, (int)point.Y, System.Windows.Media.Color.FromArgb(1, 1, 1, 0));
        }

        public void drawLine(UnityVector2 point0, UnityVector2 point1)
        {
            // Bresenham's line algorithm

            int x0 = (int)point0.X;
            int y0 = (int)point0.Y;
            int x1 = (int)point1.X;
            int y1 = (int)point1.Y;

            int dx = Math.Abs(x1 - x0);
            int dy = Math.Abs(y1 - y0);
            int sx = (x0 < x1) ? 1 : -1;
            int sy = (y0 < y1) ? 1 : -1;
            int error = dx - dy;

            while (true)
            {
                drawPoint(new UnityVector2(x0, y0));

                if ((x0 == x1) && (y0 == y1))
                    break;

                int doubleError = 2 * error;

                if (doubleError > -dy)
                {
                    error -= dy;
                    x0 += sx;
                }

                if (doubleError < dx)
                {
                    error += dx;
                    y0 += sy;
                }
            }
        }

        private UnityVector2 project(UnityVector3 pointCoordinates, Matrix4x4 transformationMatrix)
        {
            UnityVector4 transformationVector = new UnityVector4();
            transformationVector.X = (pointCoordinates.X * transformationMatrix.M00) + (pointCoordinates.Y * transformationMatrix.M10) + (pointCoordinates.Z * transformationMatrix.M20) + transformationMatrix.M30;
            transformationVector.Y = (pointCoordinates.X * transformationMatrix.M01) + (pointCoordinates.Y * transformationMatrix.M11) + (pointCoordinates.Z * transformationMatrix.M21) + transformationMatrix.M31;
            transformationVector.Z = (pointCoordinates.X * transformationMatrix.M02) + (pointCoordinates.Y * transformationMatrix.M12) + (pointCoordinates.Z * transformationMatrix.M22) + transformationMatrix.M32;
            transformationVector.W = 1f / ((pointCoordinates.X * transformationMatrix.M03) + (pointCoordinates.Y * transformationMatrix.M13) + (pointCoordinates.Z * transformationMatrix.M23) + transformationMatrix.M33);

            UnityVector3 point = new UnityVector3(transformationVector.X * transformationVector.W, transformationVector.Y * transformationVector.W, transformationVector.Z * transformationVector.W);

            // offset the point from center to top-left of screen
            float offsetPointX = point.X * _renderTarget.PixelWidth + _renderTarget.PixelWidth / 2f;
            float offsetPointY = point.Y * _renderTarget.PixelHeight + _renderTarget.PixelHeight / 2f;

            return (new UnityVector2(offsetPointX, offsetPointY));
        }

        private Matrix4x4 createLHLookAt(UnityVector3 position, UnityVector3 target, UnityVector3 up)
        {
            UnityVector3 xAxis;
            UnityVector3 yAxis;
            UnityVector3 zAxis;

            zAxis = target - position;
            zAxis.Normalize();

            xAxis = UnityVector3.Cross(up, zAxis);
            xAxis.Normalize();

            yAxis = UnityVector3.Cross(zAxis, xAxis);

            Matrix4x4 lhLookAT = Matrix4x4.Identity;
            lhLookAT.M00 = xAxis.X;
            lhLookAT.M01 = yAxis.X;
            lhLookAT.M02 = zAxis.X;
            lhLookAT.M10 = xAxis.Y;
            lhLookAT.M11 = yAxis.Y;
            lhLookAT.M12 = zAxis.Y;
            lhLookAT.M20 = xAxis.Z;
            lhLookAT.M21 = yAxis.Z;
            lhLookAT.M22 = zAxis.Z;

            lhLookAT.M30 = UnityVector3.Dot(xAxis, position);
            lhLookAT.M31 = UnityVector3.Dot(yAxis, position);
            lhLookAT.M32 = UnityVector3.Dot(zAxis, position);

            lhLookAT.M30 = -lhLookAT.M30;
            lhLookAT.M31 = -lhLookAT.M31;
            lhLookAT.M32 = -lhLookAT.M32;

            return (lhLookAT);
        }

        private Matrix4x4 createRHPerspective(float fieldOfView, float aspect, float nearClipPlane, float farClipPlane)
        {
            float yScale = (float)(1f / Math.Tan(fieldOfView * .5f));
            float q = farClipPlane / (nearClipPlane - farClipPlane);

            Matrix4x4 rhPerspective = new Matrix4x4();
            rhPerspective.M00 = yScale / aspect;
            rhPerspective.M11 = yScale;
            rhPerspective.M22 = q;
            rhPerspective.M23 = -1;
            rhPerspective.M32 = q * nearClipPlane;

            return (rhPerspective);
        }

        private Matrix4x4 rotationQuaternion(UnityQuaternion rotation)
        {
            float xx = rotation.X * rotation.X;
            float yy = rotation.Y * rotation.Y;
            float zz = rotation.Z * rotation.Z;
            float xy = rotation.X * rotation.Y;
            float zw = rotation.Z * rotation.W;
            float zx = rotation.Z * rotation.X;
            float yw = rotation.Y * rotation.W;
            float yz = rotation.Y * rotation.Z;
            float xw = rotation.X * rotation.W;

            Matrix4x4 rotationMatrix = Matrix4x4.Identity;
            rotationMatrix.M00 = 1.0f - (2.0f * (yy + zz));
            rotationMatrix.M01 = 2.0f * (xy + zw);
            rotationMatrix.M02 = 2.0f * (zx - yw);
            rotationMatrix.M10 = 2.0f * (xy - zw);
            rotationMatrix.M11 = 1.0f - (2.0f * (zz + xx));
            rotationMatrix.M12 = 2.0f * (yz + xw);
            rotationMatrix.M20 = 2.0f * (zx + yw);
            rotationMatrix.M21 = 2.0f * (yz - xw);
            rotationMatrix.M22 = 1.0f - (2.0f * (yy + xx));

            return (rotationMatrix);
        }

        private Matrix4x4 rotationYawPitchRoll(float yaw, float pitch, float roll)
        {
            UnityQuaternion quaternion = quaternionRotationYawPitchRoll(yaw, pitch, roll);

            return (rotationQuaternion(quaternion));
        }

        private UnityQuaternion quaternionRotationYawPitchRoll(float yaw, float pitch, float roll)
        {
            float halfRoll = roll * .5f;
            float halfPitch = pitch * .5f;
            float halfYaw = yaw * .5f;

            float sinRoll = (float)Math.Sin(halfRoll);
            float cosRoll = (float)Math.Cos(halfRoll);
            float sinPitch = (float)Math.Sin(halfPitch);
            float cosPitch = (float)Math.Cos(halfPitch);
            float sinYaw = (float)Math.Sin(halfYaw);
            float cosYaw = (float)Math.Cos(halfYaw);

            UnityQuaternion quaternion = new UnityQuaternion();
            quaternion.X = (cosYaw * sinPitch * cosRoll) + (sinYaw * cosPitch * sinRoll);
            quaternion.Y = (sinYaw * cosPitch * cosRoll) - (cosYaw * sinPitch * sinRoll);
            quaternion.Z = (cosYaw * cosPitch * sinRoll) - (sinYaw * sinPitch * cosRoll);
            quaternion.W = (cosYaw * cosPitch * cosRoll) + (sinYaw * sinPitch * sinRoll);

            return (quaternion);
        }

        private Matrix4x4 translation(UnityVector3 value)
        {
            Matrix4x4 translationMatrix = Matrix4x4.Identity;
            translationMatrix.M30 = value.X;
            translationMatrix.M31 = value.Y;
            translationMatrix.M32 = value.Z;

            return (translationMatrix);
        }
    }
}
