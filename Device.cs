using System;
using System.Collections.Generic;
using System.Linq;
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

        public RenderMode RenderMode;
        public Vector3 LightPosition;

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

                foreach (Face face in mesh.Faces)
                {
                    Vertex vertexA = mesh.Vertices[face.A];
                    Vertex vertexB = mesh.Vertices[face.B];
                    Vertex vertexC = mesh.Vertices[face.C];

                    Vertex pointA = project(vertexA, worldMatrix, transformationMatrix);
                    Vertex pointB = project(vertexB, worldMatrix, transformationMatrix);
                    Vertex pointC = project(vertexC, worldMatrix, transformationMatrix);

                    if (RenderMode == RenderMode.Point)
                    {
                        drawPoint(pointA.Coordinates, Colors.Yellow);
                        drawPoint(pointB.Coordinates, Colors.Yellow);
                        drawPoint(pointC.Coordinates, Colors.Yellow);
                    }
                    else if (RenderMode == RenderMode.Wireframe)
                    {
                        drawLine(pointA.Coordinates, pointB.Coordinates);
                        drawLine(pointB.Coordinates, pointC.Coordinates);
                        drawLine(pointC.Coordinates, pointA.Coordinates);
                    }
                    else
                    {
                        drawTriangle(pointA, pointB, pointC, Colors.Black, RenderMode);
                    }
                }
            }
        }

        private Vertex project(Vertex vertex, Matrix4x4 worldMatrix, Matrix4x4 transformationMatrix)
        {
            Vector3 point = transformationMatrix.MultiplyPoint(vertex.Coordinates);

            // offset the point from center to top-left of screen
            float offsetPointX = point.X * _renderTarget.PixelWidth + _renderTarget.PixelWidth / 2.0f;
            float offsetPointY = -point.Y * _renderTarget.PixelHeight + _renderTarget.PixelHeight / 2.0f;

            Vertex projectedVertex = new Vertex();
            projectedVertex.Coordinates = new Vector3(offsetPointX, offsetPointY, point.Z);
            projectedVertex.Normal = worldMatrix.MultiplyPoint(vertex.Normal);
            projectedVertex.WorldCoordinates = worldMatrix.MultiplyPoint(vertex.Coordinates);

            return (projectedVertex);
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

        private void drawTriangle(Vertex vertex1, Vertex vertex2, Vertex vertex3, Color color, RenderMode renderMode)
        {
            // sort vertices by Y
            List<Vertex> vertices = new List<Vertex>() { vertex1, vertex2, vertex3 };
            IEnumerable<Vertex> sortedVertices = vertices.OrderBy(v => v.Coordinates.Y);
            vertex1 = sortedVertices.First();
            vertex2 = sortedVertices.ElementAt(1);
            vertex3 = sortedVertices.Last();

            float faceReflectivity = 0.0f;
            float vertex1Reflectivity = 0;
            float vertex2Reflectivity = 0;
            float vertex3Reflectivity = 0;
            if (RenderMode == RenderMode.FlatShading)
            {
                Vector3 faceNormal = (vertex1.Normal + vertex2.Normal + vertex3.Normal) / 3.0f;
                Vector3 faceCentroid = (vertex1.WorldCoordinates + vertex2.WorldCoordinates + vertex3.WorldCoordinates) / 3.0f;
                faceReflectivity = calculateReflectivity(faceCentroid, faceNormal);
            }
            else
            {
                vertex1Reflectivity = calculateReflectivity(vertex1.WorldCoordinates, vertex1.Normal);
                vertex2Reflectivity = calculateReflectivity(vertex2.WorldCoordinates, vertex2.Normal);
                vertex3Reflectivity = calculateReflectivity(vertex3.WorldCoordinates, vertex3.Normal);
            }

            float vertex1Vertex2Slope;
            if (vertex2.Coordinates.Y - vertex1.Coordinates.Y > 0)
                vertex1Vertex2Slope = (vertex2.Coordinates.X - vertex1.Coordinates.X) / (vertex2.Coordinates.Y - vertex1.Coordinates.Y);
            else
                vertex1Vertex2Slope = 0;

            float vertex1Vertex3Slope;
            if (vertex3.Coordinates.Y - vertex1.Coordinates.Y > 0)
                vertex1Vertex3Slope = (vertex3.Coordinates.X - vertex1.Coordinates.X) / (vertex3.Coordinates.Y - vertex1.Coordinates.Y);
            else
                vertex1Vertex3Slope = 0;

            // order the vertices when processing the scanline based on the comparisons of the slopes
            if (vertex1Vertex2Slope > vertex1Vertex3Slope)
                for (int y = (int)vertex1.Coordinates.Y; y <= (int)vertex3.Coordinates.Y; y++)
                    if (y < vertex2.Coordinates.Y)
                        if (renderMode == RenderMode.FlatShading)
                            processScanLine(y, vertex1, vertex3, vertex1, vertex2, faceReflectivity, color);
                        else
                            processScanLine(y, vertex1, vertex3, vertex1, vertex2, vertex1Reflectivity, vertex3Reflectivity, vertex1Reflectivity, vertex2Reflectivity, color);
                    else
                        if (renderMode == RenderMode.FlatShading)
                            processScanLine(y, vertex1, vertex3, vertex2, vertex3, faceReflectivity, color);
                        else
                            processScanLine(y, vertex1, vertex3, vertex2, vertex3, vertex1Reflectivity, vertex3Reflectivity, vertex2Reflectivity, vertex3Reflectivity, color);
            else
                for (int y = (int)vertex1.Coordinates.Y; y <= (int)vertex3.Coordinates.Y; y++)
                    if (y < vertex2.Coordinates.Y)
                        if (renderMode == RenderMode.FlatShading)
                            processScanLine(y, vertex1, vertex2, vertex1, vertex3, faceReflectivity, color);
                        else
                            processScanLine(y, vertex1, vertex2, vertex1, vertex3, vertex1Reflectivity, vertex2Reflectivity, vertex1Reflectivity, vertex3Reflectivity, color);
                    else
                        if (renderMode == RenderMode.FlatShading)
                            processScanLine(y, vertex2, vertex3, vertex1, vertex3, faceReflectivity, color);
                        else
                            processScanLine(y, vertex2, vertex3, vertex1, vertex3, vertex2Reflectivity, vertex3Reflectivity, vertex1Reflectivity, vertex3Reflectivity, color);
        }

        private void processScanLine(int y, Vertex edge1Vertex1, Vertex edge1Vertex2, Vertex edge2Vertex1, Vertex edge2Vertex2, float? faceReflectivity, float? edge1Vertex1Reflectivity, float? edge1Vertex2Reflectivity, float? edge2Vertex1Reflectivity, float? edge2Vertex2Reflectivity, Color color)
        {
            float edge1YProportion = edge1Vertex1.Coordinates.Y != edge1Vertex2.Coordinates.Y ? (y - edge1Vertex1.Coordinates.Y) / (edge1Vertex2.Coordinates.Y - edge1Vertex1.Coordinates.Y) : 1;
            float edge2YProportion = edge2Vertex1.Coordinates.Y != edge2Vertex2.Coordinates.Y ? (y - edge2Vertex1.Coordinates.Y) / (edge2Vertex2.Coordinates.Y - edge2Vertex1.Coordinates.Y) : 1;

            int startX = (int)Mathf.Lerp(edge1Vertex1.Coordinates.X, edge1Vertex2.Coordinates.X, edge1YProportion);
            int endX = (int)Mathf.Lerp(edge2Vertex1.Coordinates.X, edge2Vertex2.Coordinates.X, edge2YProportion);

            float startZ = Mathf.Lerp(edge1Vertex1.Coordinates.Z, edge1Vertex2.Coordinates.Z, edge1YProportion);
            float endZ = Mathf.Lerp(edge2Vertex1.Coordinates.Z, edge2Vertex2.Coordinates.Z, edge2YProportion);

            float startReflectivity = 0.0f;
            float endReflectivity = 0.0f;
            if (edge1Vertex1Reflectivity.HasValue)
            {
                startReflectivity = Mathf.Lerp(edge1Vertex1Reflectivity.Value, edge1Vertex2Reflectivity.Value, edge1YProportion);
                endReflectivity = Mathf.Lerp(edge2Vertex1Reflectivity.Value, edge2Vertex2Reflectivity.Value, edge2YProportion);
            }

            for (int x = startX; x < endX; x++)
            {
                float xProportion = (x - startX) / (float)(endX - startX);

                float z = Mathf.Lerp(startZ, endZ, xProportion);

                float reflectivity;
                if (faceReflectivity.HasValue)
                    reflectivity = faceReflectivity.Value;
                else
                    reflectivity = Mathf.Lerp(startReflectivity, endReflectivity, xProportion);

                drawPoint(new Vector3(x, y, z), (color * reflectivity));
            }
        }

        private void processScanLine(int y, Vertex edge1Vertex1, Vertex edge1Vertex2, Vertex edge2Vertex1, Vertex edge2Vertex2, float faceReflectivity, Color color)
        {
            processScanLine(y, edge1Vertex1, edge1Vertex2, edge2Vertex1, edge2Vertex2, faceReflectivity, null, null, null, null, color);
        }

        private void processScanLine(int y, Vertex edge1Vertex1, Vertex edge1Vertex2, Vertex edge2Vertex1, Vertex edge2Vertex2, float edge1Vertex1Reflectivity, float edge1Vertex2Reflectivity, float edge2Vertex1Reflectivity, float edge2Vertex2Reflectivity, Color color)
        {
            processScanLine(y, edge1Vertex1, edge1Vertex2, edge2Vertex1, edge2Vertex2, null, edge1Vertex1Reflectivity, edge1Vertex2Reflectivity, edge2Vertex1Reflectivity, edge2Vertex2Reflectivity, color);
        }

        private float calculateReflectivity(Vector3 position, Vector3 normal)
        {
            // Game Engine Architecture - page 473

            Vector3 lightDirection = LightPosition - position;

            normal.Normalize();
            lightDirection.Normalize();

            return (Mathf.Clamp01(Vector3.Dot(normal, lightDirection)));
        }
    }
}
