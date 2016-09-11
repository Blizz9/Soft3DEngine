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
        private float[] _depthBuffer;

        public Device(WriteableBitmap renderTarget)
        {
            _renderTarget = renderTarget;
            _backBuffer = new byte[_renderTarget.PixelWidth * _renderTarget.PixelHeight * 4];
            _depthBuffer = new float[_renderTarget.PixelWidth * _renderTarget.PixelHeight];
        }

        public void Clear(Color color)
        {
            for (int index = 0; index < _backBuffer.Length; index += 4)
            {
                _backBuffer[index] = color.B;
                _backBuffer[index + 1] = color.G;
                _backBuffer[index + 2] = color.R;
                _backBuffer[index + 3] = color.A;
            }

            for (int index = 0; index < _depthBuffer.Length; index++)
                _depthBuffer[index] = float.MaxValue;
        }

        public void Render(Camera camera, List<Mesh> meshes)
        {
            Matrix4x4 viewMatrix = camera.CreateWorldToCameraMatrix();
            Matrix4x4 projectionMatrix = camera.CreateProjectionMatrix();

            foreach (Mesh mesh in meshes)
            {
                Matrix4x4 worldMatrix = Matrix4x4.CreateRotation(mesh.Rotation.Y, mesh.Rotation.X, mesh.Rotation.Z) * Matrix4x4.CreateTranslation(mesh.Position);
                Matrix4x4 transformationMatrix = worldMatrix * viewMatrix * projectionMatrix;

                int index = 0;
                foreach (Face face in mesh.Faces)
                {
                    Vector3 vertexA = mesh.Vertices[face.A];
                    Vector3 vertexB = mesh.Vertices[face.B];
                    Vector3 vertexC = mesh.Vertices[face.C];

                    Vector3 pointA = project(vertexA, transformationMatrix);
                    Vector3 pointB = project(vertexB, transformationMatrix);
                    Vector3 pointC = project(vertexC, transformationMatrix);

                    byte color = (byte)((0.25f + (index % mesh.Faces.Length) * 0.75f / mesh.Faces.Length) * 255);
                    drawTriangle(pointA, pointB, pointC, Color.FromArgb(255, color, color, color));

                    index++;
                }

                //foreach (Face face in mesh.Faces)
                //{
                //    Vector3 vertexA = mesh.Vertices[face.A];
                //    Vector3 vertexB = mesh.Vertices[face.B];
                //    Vector3 vertexC = mesh.Vertices[face.C];

                //    Vector3 pointA = project(vertexA, transformationMatrix);
                //    Vector3 pointB = project(vertexB, transformationMatrix);
                //    Vector3 pointC = project(vertexC, transformationMatrix);

                //    drawLine(pointA, pointB);
                //    drawLine(pointB, pointC);
                //    drawLine(pointC, pointA);
                //}
            }
        }

        public void Present()
        {
            int stride = _renderTarget.PixelWidth * ((_renderTarget.Format.BitsPerPixel + 7) / 8);
            _renderTarget.WritePixels(new Int32Rect(0, 0, _renderTarget.PixelWidth, _renderTarget.PixelHeight), _backBuffer, stride, 0);
        }

        private void putPixel(int x, int y, float z, Color color)
        {
            int index = x + y * _renderTarget.PixelWidth;
            int indexBy4 = index * 4;

            if (_depthBuffer[index] < z)
                return;

            _depthBuffer[index] = z;

            _backBuffer[indexBy4] = color.B;
            _backBuffer[indexBy4 + 1] = color.G;
            _backBuffer[indexBy4 + 2] = color.R;
            _backBuffer[indexBy4 + 3] = color.A;
        }

        private void drawPoint(Vector3 point, Color color)
        {
            // cull any points outside the render target
            if ((point.X >= 0) && (point.Y >= 0) && (point.X < _renderTarget.PixelWidth) && (point.Y < _renderTarget.PixelHeight))
                putPixel((int)point.X, (int)point.Y, point.Z, color);
        }

        public void drawLine(Vector3 point0, Vector3 point1)
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
                drawPoint(new Vector3(x0, y0, 0), Colors.Yellow);

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

        private Vector3 project(Vector3 pointCoordinates, Matrix4x4 transformationMatrix)
        {
            Vector3 point = transformationMatrix.MultiplyPoint(pointCoordinates);

            // offset the point from center to top-left of screen
            float offsetPointX = point.X * _renderTarget.PixelWidth + _renderTarget.PixelWidth / 2.0f;
            float offsetPointY = -point.Y * _renderTarget.PixelHeight + _renderTarget.PixelHeight / 2.0f;

            return (new Vector3(offsetPointX, offsetPointY, point.Z));
        }

        // drawing line between 2 points from left to right
        // papb -> pcpd
        // pa, pb, pc, pd must then be sorted before
        private void processScanLine(int y, Vector3 pa, Vector3 pb, Vector3 pc, Vector3 pd, Color color)
        {
            // Thanks to current Y, we can compute the gradient to compute others values like
            // the starting X (sx) and ending X (ex) to draw between
            // if pa.Y == pb.Y or pc.Y == pd.Y, gradient is forced to 1
            var gradient1 = pa.Y != pb.Y ? (y - pa.Y) / (pb.Y - pa.Y) : 1;
            var gradient2 = pc.Y != pd.Y ? (y - pc.Y) / (pd.Y - pc.Y) : 1;

            int sx = (int)Mathf.Lerp(pa.X, pb.X, gradient1);
            int ex = (int)Mathf.Lerp(pc.X, pd.X, gradient2);

            // starting Z & ending Z
            float z1 = Mathf.Lerp(pa.Z, pb.Z, gradient1);
            float z2 = Mathf.Lerp(pc.Z, pd.Z, gradient2);

            // drawing a line from left (sx) to right (ex) 
            for (var x = sx; x < ex; x++)
            {
                float gradient = (x - sx) / (float)(ex - sx);

                var z = Mathf.Lerp(z1, z2, gradient);
                drawPoint(new Vector3(x, y, z), color);
            }
        }

        private void drawTriangle(Vector3 p1, Vector3 p2, Vector3 p3, Color color)
        {
            // Sorting the points in order to always have this order on screen p1, p2 & p3
            // with p1 always up (thus having the Y the lowest possible to be near the top screen)
            // then p2 between p1 & p3
            if (p1.Y > p2.Y)
            {
                var temp = p2;
                p2 = p1;
                p1 = temp;
            }

            if (p2.Y > p3.Y)
            {
                var temp = p2;
                p2 = p3;
                p3 = temp;
            }

            if (p1.Y > p2.Y)
            {
                var temp = p2;
                p2 = p1;
                p1 = temp;
            }

            // inverse slopes
            float dP1P2, dP1P3;

            // http://en.wikipedia.org/wiki/Slope
            // Computing inverse slopes
            if (p2.Y - p1.Y > 0)
                dP1P2 = (p2.X - p1.X) / (p2.Y - p1.Y);
            else
                dP1P2 = 0;

            if (p3.Y - p1.Y > 0)
                dP1P3 = (p3.X - p1.X) / (p3.Y - p1.Y);
            else
                dP1P3 = 0;

            // First case where triangles are like that:
            // P1
            // -
            // -- 
            // - -
            // -  -
            // -   - P2
            // -  -
            // - -
            // -
            // P3
            if (dP1P2 > dP1P3)
            {
                for (var y = (int)p1.Y; y <= (int)p3.Y; y++)
                {
                    if (y < p2.Y)
                    {
                        processScanLine(y, p1, p3, p1, p2, color);
                    }
                    else
                    {
                        processScanLine(y, p1, p3, p2, p3, color);
                    }
                }
            }
            // First case where triangles are like that:
            //       P1
            //        -
            //       -- 
            //      - -
            //     -  -
            // P2 -   - 
            //     -  -
            //      - -
            //        -
            //       P3
            else
            {
                for (var y = (int)p1.Y; y <= (int)p3.Y; y++)
                {
                    if (y < p2.Y)
                    {
                        processScanLine(y, p1, p2, p1, p3, color);
                    }
                    else
                    {
                        processScanLine(y, p2, p3, p1, p3, color);
                    }
                }
            }
        }
    }
}
