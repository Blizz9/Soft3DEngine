using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SharpDX;

namespace Soft3DEngine
{
    public partial class MainWindow : Window
    {
        //private Device _device;
        //private Camera _camera;
        //private Mesh _mesh;
                                                                                                            
        private UnityDevice _device;
        private UnityCamera _camera;
        private List<UnityMesh> _meshes;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void loaded(object sender, RoutedEventArgs e)
        {
            WriteableBitmap screenBuffer = new WriteableBitmap(640, 480, 300, 300, PixelFormats.Bgra32, null);
            screen.Source = screenBuffer;
            RenderOptions.SetBitmapScalingMode(screen, BitmapScalingMode.NearestNeighbor);

            //_device = new Device(screenBuffer);
            _device = new UnityDevice(screenBuffer);

            //_camera = new Camera();
            _camera = new UnityCamera();
            _camera.Position = new Vector3(0, 0, 10);
            _camera.Target = Vector3.Zero;

            //_mesh = new Mesh("Cube", 8, 12);
            //_mesh = new UnityMesh("Cube", 8, 12);
            //_mesh.Vertices[0] = new Vector3(-1, 1, 1);
            //_mesh.Vertices[1] = new Vector3(1, 1, 1);
            //_mesh.Vertices[2] = new Vector3(-1, -1, 1);
            //_mesh.Vertices[3] = new Vector3(1, -1, 1);
            //_mesh.Vertices[4] = new Vector3(-1, 1, -1);
            //_mesh.Vertices[5] = new Vector3(1, 1, -1);
            //_mesh.Vertices[6] = new Vector3(1, -1, -1);
            //_mesh.Vertices[7] = new Vector3(-1, -1, -1);
            //_mesh.Faces[0] = new Face { A = 0, B = 1, C = 2 };
            //_mesh.Faces[1] = new Face { A = 1, B = 2, C = 3 };
            //_mesh.Faces[2] = new Face { A = 1, B = 3, C = 6 };
            //_mesh.Faces[3] = new Face { A = 1, B = 5, C = 6 };
            //_mesh.Faces[4] = new Face { A = 0, B = 1, C = 4 };
            //_mesh.Faces[5] = new Face { A = 1, B = 4, C = 5 };
            //_mesh.Faces[6] = new Face { A = 2, B = 3, C = 7 };
            //_mesh.Faces[7] = new Face { A = 3, B = 6, C = 7 };
            //_mesh.Faces[8] = new Face { A = 0, B = 2, C = 7 };
            //_mesh.Faces[9] = new Face { A = 0, B = 4, C = 7 };
            //_mesh.Faces[10] = new Face { A = 4, B = 5, C = 6 };
            //_mesh.Faces[11] = new Face { A = 4, B = 6, C = 7 };

            _meshes = loadModelJSON("Suzanne.model.json");

            CompositionTarget.Rendering += compositionTargetRendering;
        }

        private void compositionTargetRendering(object sender, EventArgs e)
        {
            _device.Clear(0, 0, 0, 255);

            foreach (UnityMesh mesh in _meshes)
                mesh.Rotation = new Vector3(mesh.Rotation.X, mesh.Rotation.Y + .02f, mesh.Rotation.Z);

            _device.Render(_camera, _meshes);
            _device.Present();
        }

        private List<UnityMesh> loadModelJSON(string fileName)
        {
            List<UnityMesh> meshes = new List<UnityMesh>();

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
                UnityMesh mesh = new UnityMesh(model.meshes[index].name.Value, vertexCount, faceCount);

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
