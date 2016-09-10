using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
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
        private UnityMesh _mesh;

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
            _mesh = new UnityMesh(8, 12);
            _mesh.Vertices[0] = new Vector3(-1, 1, 1);
            _mesh.Vertices[1] = new Vector3(1, 1, 1);
            _mesh.Vertices[2] = new Vector3(-1, -1, 1);
            _mesh.Vertices[3] = new Vector3(1, -1, 1);
            _mesh.Vertices[4] = new Vector3(-1, 1, -1);
            _mesh.Vertices[5] = new Vector3(1, 1, -1);
            _mesh.Vertices[6] = new Vector3(1, -1, -1);
            _mesh.Vertices[7] = new Vector3(-1, -1, -1);
            _mesh.Faces[0] = new Face { A = 0, B = 1, C = 2 };
            _mesh.Faces[1] = new Face { A = 1, B = 2, C = 3 };
            _mesh.Faces[2] = new Face { A = 1, B = 3, C = 6 };
            _mesh.Faces[3] = new Face { A = 1, B = 5, C = 6 };
            _mesh.Faces[4] = new Face { A = 0, B = 1, C = 4 };
            _mesh.Faces[5] = new Face { A = 1, B = 4, C = 5 };
            _mesh.Faces[6] = new Face { A = 2, B = 3, C = 7 };
            _mesh.Faces[7] = new Face { A = 3, B = 6, C = 7 };
            _mesh.Faces[8] = new Face { A = 0, B = 2, C = 7 };
            _mesh.Faces[9] = new Face { A = 0, B = 4, C = 7 };
            _mesh.Faces[10] = new Face { A = 4, B = 5, C = 6 };
            _mesh.Faces[11] = new Face { A = 4, B = 6, C = 7 };

            CompositionTarget.Rendering += compositionTargetRendering;
        }

        private void compositionTargetRendering(object sender, EventArgs e)
        {
            _device.Clear(0, 0, 0, 255);

            _mesh.Rotation = new Vector3(_mesh.Rotation.X + .01f, _mesh.Rotation.Y + .01f, _mesh.Rotation.Z);

            _device.Render(_camera, _mesh);
            _device.Present();
        }
    }
}
