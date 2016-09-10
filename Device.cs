using System;
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

        public void Clear(byte r, byte g, byte b, byte a)
        {
            for (long index = 0; index < _backBuffer.Length; index += 4)
            {
                _backBuffer[index] = b;
                _backBuffer[index + 1] = g;
                _backBuffer[index + 2] = r;
                _backBuffer[index + 3] = a;
            }
        }

        public void Render(Camera camera, params Mesh[] meshes)
        {
            Matrix viewMatrix = Matrix.LookAtLH(camera.Position, camera.Target, Vector3.UnitY);
            Matrix projectionMatrix = Matrix.PerspectiveFovRH(0.78f, (float)_renderTarget.PixelWidth / _renderTarget.PixelHeight, 0.01f, 1.0f);

            foreach (Mesh mesh in meshes)
            {
                Matrix worldMatrix = Matrix.RotationYawPitchRoll(mesh.Rotation.Y, mesh.Rotation.X, mesh.Rotation.Z) * Matrix.Translation(mesh.Position);
                Matrix transformationMatrix = worldMatrix * viewMatrix * projectionMatrix;

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

        private void putPixel(int x, int y, Color4 color)
        {
            long index = (x + y * _renderTarget.PixelWidth) * 4;

            _backBuffer[index] = (byte)(color.Blue * 255);
            _backBuffer[index + 1] = (byte)(color.Green * 255);
            _backBuffer[index + 2] = (byte)(color.Red * 255);
            _backBuffer[index + 3] = (byte)(color.Alpha * 255);
        }
        
        private void drawPoint(Vector2 point)
        {
            // cull any points outside the render target
            if ((point.X >= 0) && (point.Y >= 0) && (point.X < _renderTarget.PixelWidth) && (point.Y < _renderTarget.PixelHeight))
                putPixel((int)point.X, (int)point.Y, new Color4(1.0f, 1.0f, 0.0f, 1.0f));
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

        private Vector2 project(Vector3 pointCoordinates, Matrix transformationMatrix)
        {
            Vector3 point = Vector3.TransformCoordinate(pointCoordinates, transformationMatrix);

            // offset the point from center to top-left of screen
            float offsetPointX = point.X * _renderTarget.PixelWidth + _renderTarget.PixelWidth / 2.0f;
            float offsetPointY = -point.Y * _renderTarget.PixelHeight + _renderTarget.PixelHeight / 2.0f;

            return (new Vector2(offsetPointX, offsetPointY));
        }
    }
}
