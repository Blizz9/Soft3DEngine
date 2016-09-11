using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
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

        public void Clear(Color color)
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
            Matrix4x4 viewMatrix = camera.CreateWorldToCameraMatrix();
            Matrix4x4 projectionMatrix = camera.CreateProjectionMatrix();

            foreach (Mesh mesh in meshes)
            {
                Matrix4x4 worldMatrix = Matrix4x4.CreateRotation(mesh.Rotation.Y, mesh.Rotation.X, mesh.Rotation.Z) * Matrix4x4.CreateTranslation(mesh.Position);
                Matrix4x4 transformationMatrix = worldMatrix * viewMatrix * projectionMatrix;

                foreach (Face face in mesh.Faces)
                {
                    Vector3 vertexA = mesh.Vertices[face.A];
                    Vector3 vertexB = mesh.Vertices[face.B];
                    Vector3 vertexC = mesh.Vertices[face.C];

                    Vector2 pointA = project(vertexA, transformationMatrix);
                    Vector2 pointB = project(vertexB, transformationMatrix);
                    Vector2 pointC = project(vertexC, transformationMatrix);

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

        private void putPixel(int x, int y, Color color)
        {
            long index = (x + y * _renderTarget.PixelWidth) * 4;

            _backBuffer[index] = (byte)(color.B * 255);
            _backBuffer[index + 1] = (byte)(color.G * 255);
            _backBuffer[index + 2] = (byte)(color.R * 255);
            _backBuffer[index + 3] = (byte)(color.A * 255);
        }

        private void drawPoint(Vector2 point)
        {
            // cull any points outside the render target
            if ((point.X >= 0) && (point.Y >= 0) && (point.X < _renderTarget.PixelWidth) && (point.Y < _renderTarget.PixelHeight))
                putPixel((int)point.X, (int)point.Y, Color.FromArgb(1, 1, 1, 0));
        }

        public void drawLine(Vector2 point0, Vector2 point1)
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
                drawPoint(new Vector2(x0, y0));

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

        private Vector2 project(Vector3 pointCoordinates, Matrix4x4 transformationMatrix)
        {
            Vector3 point = transformationMatrix.MultiplyPoint(pointCoordinates);

            // offset the point from center to top-left of screen
            float offsetPointX = point.X * _renderTarget.PixelWidth + _renderTarget.PixelWidth / 2f;
            float offsetPointY = point.Y * _renderTarget.PixelHeight + _renderTarget.PixelHeight / 2f;

            return (new Vector2(offsetPointX, offsetPointY));
        }
    }
}
