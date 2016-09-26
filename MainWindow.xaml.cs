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

        private RenderMode _lastRenderMode;
        private string _lastLoadedModel;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void loaded(object sender, RoutedEventArgs e)
        {
            _lastRenderMode = RenderMode.Wireframe;
            _lastLoadedModel = "Cube.model.json";
            initializeComponents(640, 480);

            CompositionTarget.Rendering += compositionTargetRendering;
        }

        private void compositionTargetRendering(object sender, EventArgs e)
        {
            _device.Clear(Colors.Black);

            foreach (Mesh mesh in _meshes)
                mesh.Rotation = new Vector3(mesh.Rotation.X + (float)_modelRotationXSlider.Value, mesh.Rotation.Y + (float)_modelRotationYSlider.Value, mesh.Rotation.Z + (float)_modelRotationZSlider.Value);

            _device.Render(_camera, _meshes);
            _device.Present();
        }

        private void initializeComponents(int pixelWidth, int pixelHeight)
        {
            WriteableBitmap screenBuffer = new WriteableBitmap(pixelWidth, pixelHeight, 300, 300, PixelFormats.Bgra32, null);
            screen.Source = screenBuffer;
            RenderOptions.SetBitmapScalingMode(screen, BitmapScalingMode.NearestNeighbor);

            _device = new Device(screenBuffer);
            _device.RenderMode = _lastRenderMode;
            _device.LightPosition = new Vector3((float)_lightPositionXSlider.Value, (float)-_lightPositionYSlider.Value, (float)_lightPositionZSlider.Value);

            _camera = new Camera();
            _camera.FieldOfView = (float)_fieldOfViewSlider.Value;
            _camera.Aspect = (float)screenBuffer.PixelWidth / screenBuffer.PixelHeight;
            _camera.NearClipPlane = 0.01f;
            _camera.FarClipPlane = 1.0f;

            _meshes = loadModelJSON(_lastLoadedModel);
            _camera.Position = new Vector3((float)_cameraPositionXSlider.Value, (float)_cameraPositionYSlider.Value, (float)_cameraPositionZSlider.Value);
            _camera.Target = Vector3.Zero;
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

                    float xNormal = vertices[vertexIndex * vertexStep + 3];
                    float yNormal = vertices[vertexIndex * vertexStep + 4];
                    float zNormal = vertices[vertexIndex * vertexStep + 5];

                    mesh.Vertices[vertexIndex] = new Vertex { Coordinates = new Vector3(x, y, z), Normal = new Vector3(xNormal, yNormal, zNormal) };
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

        private void lowestResolutionRadioButtonChecked(object sender, RoutedEventArgs e)
        {
            initializeComponents(160, 120);
        }

        private void lowResolutionRadioButtonChecked(object sender, RoutedEventArgs e)
        {
            initializeComponents(320, 240);
        }

        private void mediumResolutionRadioButtonChecked(object sender, RoutedEventArgs e)
        {
            if (_lightPositionXSlider != null)
                initializeComponents(640, 480);
        }

        private void highResolutionRadioButtonChecked(object sender, RoutedEventArgs e)
        {
            initializeComponents(1024, 768);
        }

        private void pointRadioButtonChecked(object sender, RoutedEventArgs e)
        {
            if (_device != null)
            {
                _device.RenderMode = RenderMode.Point;
                _lastRenderMode = RenderMode.Point;
            }
        }

        private void wireframeRadioButtonChecked(object sender, RoutedEventArgs e)
        {
            if (_device != null)
            {
                _device.RenderMode = RenderMode.Wireframe;
                _lastRenderMode = RenderMode.Wireframe;
            }
        }

        private void flatShadingRadioButtonChecked(object sender, RoutedEventArgs e)
        {
            if (_device != null)
            {
                _device.RenderMode = RenderMode.FlatShading;
                _lastRenderMode = RenderMode.FlatShading;
            }
        }

        private void smoothShadingRadioButtonChecked(object sender, RoutedEventArgs e)
        {
            if (_device != null)
            {
                _device.RenderMode = RenderMode.SmoothShading;
                _lastRenderMode = RenderMode.SmoothShading;
            }
        }

        private void cubeRadioButtonChecked(object sender, RoutedEventArgs e)
        {
            _meshes = loadModelJSON("Cube.model.json");
            _lastLoadedModel = "Cube.model.json";
        }

        private void dominoRadioButtonChecked(object sender, RoutedEventArgs e)
        {
            _meshes = loadModelJSON("Domino.model.json");
            _lastLoadedModel = "Domino.model.json";
        }

        private void suzanneRadioButtonChecked(object sender, RoutedEventArgs e)
        {
            _meshes = loadModelJSON("Suzanne.model.json");
            _lastLoadedModel = "Suzanne.model.json";
        }

        private void arwingRadioButtonChecked(object sender, RoutedEventArgs e)
        {
            _meshes = loadModelJSON("Arwing.model.json");
            _lastLoadedModel = "Arwing.model.json";
        }

        private void deagleRadioButtonChecked(object sender, RoutedEventArgs e)
        {
            _meshes = loadModelJSON("Deagle.model.json");
            _lastLoadedModel = "Deagle.model.json";
        }

        private void sackboyRadioButtonChecked(object sender, RoutedEventArgs e)
        {
            _meshes = loadModelJSON("Sackboy.model.json");
            _lastLoadedModel = "Sackboy.model.json";
        }

        private void assaultRifleRadioButtonChecked(object sender, RoutedEventArgs e)
        {
            _meshes = loadModelJSON("AssaultRifle.model.json");
            _lastLoadedModel = "AssaultRifle.model.json";
        }

        private void marioRadioButtonChecked(object sender, RoutedEventArgs e)
        {
            _meshes = loadModelJSON("Mario.model.json");
            _lastLoadedModel = "Mario.model.json";
        }

        private void modelRotationSliderValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_modelRotationXTextBlock != null)
                _modelRotationXTextBlock.Text = _modelRotationXSlider.Value.ToString("n2");

            if (_modelRotationYTextBlock != null)
                _modelRotationYTextBlock.Text = _modelRotationYSlider.Value.ToString("n2");

            if (_modelRotationZTextBlock != null)
                _modelRotationZTextBlock.Text = _modelRotationZSlider.Value.ToString("n2");
        }

        private void cameraPositionSliderValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_cameraPositionXTextBlock != null)
                _cameraPositionXTextBlock.Text = _cameraPositionXSlider.Value.ToString("n2");

            if (_cameraPositionYTextBlock != null)
                _cameraPositionYTextBlock.Text = _cameraPositionYSlider.Value.ToString("n2");

            if (_cameraPositionZTextBlock != null)
                _cameraPositionZTextBlock.Text = _cameraPositionZSlider.Value.ToString("n2");

            if (_camera != null)
                _camera.Position = new Vector3((float)_cameraPositionXSlider.Value, (float)_cameraPositionYSlider.Value, (float)_cameraPositionZSlider.Value);
        }

        private void lightPositionSliderValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_lightPositionXTextBlock != null)
                _lightPositionXTextBlock.Text = _lightPositionXSlider.Value.ToString("n2");

            if (_lightPositionYTextBlock != null)
                _lightPositionYTextBlock.Text = _lightPositionYSlider.Value.ToString("n2");

            if (_lightPositionZTextBlock != null)
                _lightPositionZTextBlock.Text = _lightPositionZSlider.Value.ToString("n2");

            if (_device != null)
                _device.LightPosition = new Vector3((float)_lightPositionXSlider.Value, (float)-_lightPositionYSlider.Value, (float)_lightPositionZSlider.Value);
        }

        private void fieldOfViewSliderValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_fieldOfViewTextBlock != null)
                _fieldOfViewTextBlock.Text = _fieldOfViewSlider.Value.ToString("n2");

            if (_camera != null)
                _camera.FieldOfView = (float)_fieldOfViewSlider.Value;
        }
    }
}
