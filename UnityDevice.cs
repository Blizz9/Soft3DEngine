using System;
using System.Windows;
using System.Windows.Media.Imaging;
using SharpDX;

namespace Soft3DEngine
{
    public class UnityDevice
    {
        private WriteableBitmap _renderTarget;
        private byte[] _backBuffer;

        public UnityDevice(WriteableBitmap renderTarget)
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

        public void Render(UnityCamera camera, params UnityMesh[] meshes)
        {
            Matrix viewMatrix = createLHLookAt(camera.Position, camera.Target, Vector3.UnitY);
            Matrix projectionMatrix = createLHPerspective(0.78f, (float)_renderTarget.PixelWidth / _renderTarget.PixelHeight, 0.01f, 1.0f);

            foreach (UnityMesh mesh in meshes)
            {
                Matrix worldMatrix = Matrix.RotationYawPitchRoll(mesh.Rotation.Y, mesh.Rotation.X, mesh.Rotation.Z) * Matrix.Translation(mesh.Position);
                Matrix transformationMatrix = worldMatrix * viewMatrix * projectionMatrix;

                foreach (Vector3 vertex in mesh.Vertices)
                {
                    Vector2 point = project(vertex, transformationMatrix);
                    drawPoint(point);
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

        private Vector2 project(Vector3 pointCoordinates, Matrix transformationMatrix)
        {
            Vector4 transformationVector = new Vector4();
            transformationVector.X = (pointCoordinates.X * transformationMatrix.M11) + (pointCoordinates.Y * transformationMatrix.M21) + (pointCoordinates.Z * transformationMatrix.M31) + transformationMatrix.M41;
            transformationVector.Y = (pointCoordinates.X * transformationMatrix.M12) + (pointCoordinates.Y * transformationMatrix.M22) + (pointCoordinates.Z * transformationMatrix.M32) + transformationMatrix.M42;
            transformationVector.Z = (pointCoordinates.X * transformationMatrix.M13) + (pointCoordinates.Y * transformationMatrix.M23) + (pointCoordinates.Z * transformationMatrix.M33) + transformationMatrix.M43;
            transformationVector.W = 1f / ((pointCoordinates.X * transformationMatrix.M14) + (pointCoordinates.Y * transformationMatrix.M24) + (pointCoordinates.Z * transformationMatrix.M34) + transformationMatrix.M44);

            Vector3 point = new Vector3(transformationVector.X * transformationVector.W, transformationVector.Y * transformationVector.W, transformationVector.Z * transformationVector.W);

            // offset the point from center to top-left of screen
            float offsetPointX = point.X * _renderTarget.PixelWidth + _renderTarget.PixelWidth / 2.0f;
            float offsetPointY = -point.Y * _renderTarget.PixelHeight + _renderTarget.PixelHeight / 2.0f;

            return (new Vector2(offsetPointX, offsetPointY));
        }

        private Matrix createLHLookAt(Vector3 cameraPosition, Vector3 target, Vector3 up)
        {
            Vector3 xaxis, yaxis, zaxis;
            Vector3.Subtract(ref target, ref cameraPosition, out zaxis); zaxis.Normalize();
            Vector3.Cross(ref up, ref zaxis, out xaxis); xaxis.Normalize();
            Vector3.Cross(ref zaxis, ref xaxis, out yaxis);

            Matrix lhLookAT = Matrix.Identity;
            lhLookAT.M11 = xaxis.X; lhLookAT.M21 = xaxis.Y; lhLookAT.M31 = xaxis.Z;
            lhLookAT.M12 = yaxis.X; lhLookAT.M22 = yaxis.Y; lhLookAT.M32 = yaxis.Z;
            lhLookAT.M13 = zaxis.X; lhLookAT.M23 = zaxis.Y; lhLookAT.M33 = zaxis.Z;

            Vector3.Dot(ref xaxis, ref cameraPosition, out lhLookAT.M41);
            Vector3.Dot(ref yaxis, ref cameraPosition, out lhLookAT.M42);
            Vector3.Dot(ref zaxis, ref cameraPosition, out lhLookAT.M43);

            lhLookAT.M41 = -lhLookAT.M41;
            lhLookAT.M42 = -lhLookAT.M42;
            lhLookAT.M43 = -lhLookAT.M43;

            return (lhLookAT);
        }

        private Matrix createLHPerspective(float fieldOfView, float aspect, float nearClipPlane, float farClipPlane)
        {
            float yScale = (float)(1.0f / Math.Tan(fieldOfView * 0.5f));
            float q = farClipPlane / (farClipPlane - nearClipPlane);

            Matrix lhPerspective = new Matrix();
            lhPerspective.M11 = yScale / aspect;
            lhPerspective.M22 = yScale;
            lhPerspective.M33 = q;
            lhPerspective.M34 = 1.0f;
            lhPerspective.M43 = -q * nearClipPlane;

            return (lhPerspective);
        }
    }
}
