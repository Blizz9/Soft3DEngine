using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media.Imaging;
using SharpDX;

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
            Matrix viewMatrix = createLHLookAt(camera.Position, camera.Target, UnityVector3.Up);
            Matrix projectionMatrix = createRHPerspective(.78f, (float)_renderTarget.PixelWidth / _renderTarget.PixelHeight, .01f, 1);

            foreach (Mesh mesh in meshes)
            {
                Vector3 tempSharpDXVector3 = new Vector3(mesh.Position.X, mesh.Position.Y, mesh.Position.Z);
                Matrix worldMatrix = Matrix.RotationYawPitchRoll(mesh.Rotation.Y, mesh.Rotation.X, mesh.Rotation.Z) * Matrix.Translation(tempSharpDXVector3);
                //Matrix worldMatrix = Matrix.RotationYawPitchRoll(mesh.Rotation.Y, mesh.Rotation.X, mesh.Rotation.Z) * Matrix.Translation(mesh.Position);
                Matrix transformationMatrix = worldMatrix * viewMatrix * projectionMatrix;

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

        private UnityVector2 project(UnityVector3 pointCoordinates, Matrix transformationMatrix)
        {
            UnityVector4 transformationVector = new UnityVector4();
            transformationVector.X = (pointCoordinates.X * transformationMatrix.M11) + (pointCoordinates.Y * transformationMatrix.M21) + (pointCoordinates.Z * transformationMatrix.M31) + transformationMatrix.M41;
            transformationVector.Y = (pointCoordinates.X * transformationMatrix.M12) + (pointCoordinates.Y * transformationMatrix.M22) + (pointCoordinates.Z * transformationMatrix.M32) + transformationMatrix.M42;
            transformationVector.Z = (pointCoordinates.X * transformationMatrix.M13) + (pointCoordinates.Y * transformationMatrix.M23) + (pointCoordinates.Z * transformationMatrix.M33) + transformationMatrix.M43;
            transformationVector.W = 1f / ((pointCoordinates.X * transformationMatrix.M14) + (pointCoordinates.Y * transformationMatrix.M24) + (pointCoordinates.Z * transformationMatrix.M34) + transformationMatrix.M44);

            UnityVector3 point = new UnityVector3(transformationVector.X * transformationVector.W, transformationVector.Y * transformationVector.W, transformationVector.Z * transformationVector.W);

            // offset the point from center to top-left of screen
            float offsetPointX = point.X * _renderTarget.PixelWidth + _renderTarget.PixelWidth / 2f;
            float offsetPointY = point.Y * _renderTarget.PixelHeight + _renderTarget.PixelHeight / 2f;

            return (new UnityVector2(offsetPointX, offsetPointY));
        }

        private Matrix createLHLookAt(UnityVector3 position, UnityVector3 target, UnityVector3 up)
        {
            UnityVector3 xAxis;
            UnityVector3 yAxis;
            UnityVector3 zAxis;

            zAxis = target - position;
            zAxis.Normalize();

            xAxis = UnityVector3.Cross(up, zAxis);
            xAxis.Normalize();

            yAxis = UnityVector3.Cross(zAxis, xAxis);

            Matrix lhLookAT = Matrix.Identity;
            lhLookAT.M11 = xAxis.X;
            lhLookAT.M12 = yAxis.X;
            lhLookAT.M13 = zAxis.X;
            lhLookAT.M21 = xAxis.Y;
            lhLookAT.M22 = yAxis.Y;
            lhLookAT.M23 = zAxis.Y;
            lhLookAT.M31 = xAxis.Z;
            lhLookAT.M32 = yAxis.Z;
            lhLookAT.M33 = zAxis.Z;

            lhLookAT.M41 = UnityVector3.Dot(xAxis, position);
            lhLookAT.M42 = UnityVector3.Dot(yAxis, position);
            lhLookAT.M43 = UnityVector3.Dot(zAxis, position);

            lhLookAT.M41 = -lhLookAT.M41;
            lhLookAT.M42 = -lhLookAT.M42;
            lhLookAT.M43 = -lhLookAT.M43;

            return (lhLookAT);
        }

        private Matrix createRHPerspective(float fieldOfView, float aspect, float nearClipPlane, float farClipPlane)
        {
            float yScale = (float)(1f / Math.Tan(fieldOfView * .5f));
            float q = farClipPlane / (nearClipPlane - farClipPlane);

            Matrix rhPerspective = new Matrix();
            rhPerspective.M11 = yScale / aspect;
            rhPerspective.M22 = yScale;
            rhPerspective.M33 = q;
            rhPerspective.M34 = -1;
            rhPerspective.M43 = q * nearClipPlane;

            return (rhPerspective);
        }
    }
}
