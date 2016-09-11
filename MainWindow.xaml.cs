using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Soft3DEngine
{
    public partial class MainWindow : Window
    {                                                                                                    
        private Device _device;
        private Camera _camera;
        private List<Mesh> _meshes;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void loaded(object sender, RoutedEventArgs e)
        {
            WriteableBitmap screenBuffer = new WriteableBitmap(640, 480, 300, 300, PixelFormats.Bgra32, null);
            screen.Source = screenBuffer;
            RenderOptions.SetBitmapScalingMode(screen, BitmapScalingMode.NearestNeighbor);

            _device = new Device(screenBuffer);

            _camera = new Camera();
            _camera.FieldOfView = .78f;
            _camera.Aspect = (float)screenBuffer.PixelWidth / screenBuffer.PixelHeight;
            _camera.NearClipPlane = .01f;
            _camera.FarClipPlane = 1;

            //_meshes = loadModelJSON("Suzanne.model.json");
            _meshes = loadModelJSON("Domino.model.json");
            _camera.Position = new Vector3(0, 0, 10);
            _camera.Target = Vector3.Zero;

            //_meshes = loadModelJSON("Mario.model.json");
            //_camera.Position = new Vector3(0, 1.8f, 10);
            //_camera.Target = new Vector3(0, 1.8f, 0);

            CompositionTarget.Rendering += compositionTargetRendering;
        }

        private void compositionTargetRendering(object sender, EventArgs e)
        {
            _device.Clear(Colors.Black);

            foreach (Mesh mesh in _meshes)
                mesh.Rotation = new Vector3(mesh.Rotation.X + .01f, mesh.Rotation.Y + .01f, mesh.Rotation.Z);

            _device.Render(_camera, _meshes);
            _device.Present();
        }

        private List<Mesh> loadModelJSON(string fileName)
        {
            List<Mesh> meshes = new List<Mesh>();

            dynamic model = JsonConvert.DeserializeObject(File.ReadAllText(fileName));

            for (int index = 0; index < model.meshes.Count; index++)
            {
                float[] vertices = ((JArray)model.meshes[index].vertices).Select(v => (float)v).ToArray();
                int[] faces = ((JArray)model.meshes[index].indices).Select(i => (int)i).ToArray();
                int uvCount = (int)model.meshes[index].uvCount.Value;

                int vertexStep = 1;
                switch (uvCount)
                {
                    case 0:
                        vertexStep = 6;
                        break;

                    case 1:
                        vertexStep = 8;
                        break;

                    case 2:
                        vertexStep = 10;
                        break;
                }

                int vertexCount = vertices.Length / vertexStep;
                int faceCount = faces.Length / 3;
                Mesh mesh = new Mesh(model.meshes[index].name.Value, vertexCount, faceCount);

                for (int vertexIndex = 0; vertexIndex < vertexCount; vertexIndex++)
                {
                    float x = vertices[vertexIndex * vertexStep];
                    float y = vertices[vertexIndex * vertexStep + 1];
                    float z = vertices[vertexIndex * vertexStep + 2];

                    mesh.Vertices[vertexIndex] = new Vector3(x, y, z);
                }

                for (int faceIndex = 0; faceIndex < faceCount; faceIndex++)
                {
                    int a = faces[faceIndex * 3];
                    int b = faces[faceIndex * 3 + 1];
                    int c = faces[faceIndex * 3 + 2];

                    mesh.Faces[faceIndex] = new Face(a, b, c);
                }

                float[] positionCoordinates = ((JArray)model.meshes[index].position).Select(pc => (float)pc).ToArray();
                mesh.Position = new Vector3(positionCoordinates[0], positionCoordinates[1], positionCoordinates[2]);
                
                meshes.Add(mesh);
            }

            return (meshes);
        }
    }
}
