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
            _camera.Position = new Vector3(0, 0, 10.0f);
            _camera.Target = Vector3.Zero;

            //_mesh = new Mesh("Cube", 8);
            _mesh = new UnityMesh("Cube", 8);
            _mesh.Vertices[0] = new Vector3(-1, 1, 1);
            _mesh.Vertices[1] = new Vector3(1, 1, 1);
            _mesh.Vertices[2] = new Vector3(-1, -1, 1);
            _mesh.Vertices[3] = new Vector3(-1, -1, -1);
            _mesh.Vertices[4] = new Vector3(-1, 1, -1);
            _mesh.Vertices[5] = new Vector3(1, 1, -1);
            _mesh.Vertices[6] = new Vector3(1, -1, 1);
            _mesh.Vertices[7] = new Vector3(1, -1, -1);

            CompositionTarget.Rendering += compositionTargetRendering;
        }

        private void compositionTargetRendering(object sender, EventArgs e)
        {
            _device.Clear(0, 0, 0, 255);

            _mesh.Rotation = new Vector3(_mesh.Rotation.X + 0.01f, _mesh.Rotation.Y + 0.01f, _mesh.Rotation.Z);

            _device.Render(_camera, _mesh);
            _device.Present();
        }
    }
}
