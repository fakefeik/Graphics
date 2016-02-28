using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Graphics.Model;
using Graphics.ViewModel;
using SharpDX;
using Plane = Graphics.Model.Plane;
using Point = System.Windows.Point;

namespace Graphics.View
{
    /// <summary>
    /// Interaction logic for RendererView.xaml
    /// </summary>
    public partial class RendererView : UserControl
    {
        private RendererViewModel _model;
        private Mesh[] _meshes;
        private bool dragInProgress = false;
        private Point lastPosition;
        private Camera _camera = new Camera {Up = new Vector3(0, 1, 0)};
        private DateTime _previousDate;
        private Collection<double> _lastFpsValues = new Collection<double>();
        public RendererView()
        {
            _model = new RendererViewModel();
            DataContext = _model;
            InitializeComponent();
        }

        private void RendererView_OnLoaded(object sender, RoutedEventArgs e)
        {
            var window = Window.GetWindow(this);
            window.KeyDown += HandleKeyPress;
            window.MouseDown += WindowOnMouseDown;
            window.MouseUp += WindowOnMouseUp;
            window.MouseMove += WindowOnMouseMove;
            
            CompositionTarget.Rendering += CompositionTargetOnRendering;
            _meshes = JsonModelLoader.LoadJsonFile("monkey.babylon");
            //_meshes = new List<Mesh>(_meshes) {new Plane(1, 1, 2, 2, (x, y) => (float) (Math.Cos(x) * Math.Cos(y)))}.ToArray();
            //_meshes = new[] { new Plane(2, 2, 10, 10, (x, y) => x * y) };
            //_meshes = new[] {new Plane(1, 1, 5, 5, (f, f1) => 0)};
            //_meshes[0].Rotation = Vector3.Down;
            
            _camera.Position = new Vector3(0, 0, 10.0f);
            _camera.Target = Vector3.Zero;
            _camera.Move(Vector3.Zero);
            _camera.Rotate(Vector2.Zero);
        }

        private void WindowOnMouseDown(object sender, MouseButtonEventArgs e)
        {
            dragInProgress = true;
            lastPosition = e.GetPosition(this);
        }

        private void WindowOnMouseMove(object sender, MouseEventArgs e)
        {
            if (dragInProgress)
            {
                var position = e.GetPosition(this);
                _camera.Rotate(new Vector2((float) (position.X - lastPosition.X)/100, -(float) (position.Y - lastPosition.Y)/100));
                lastPosition = e.GetPosition(this);
            }
        }

        private void WindowOnMouseUp(object sender, MouseButtonEventArgs e)
        {
            dragInProgress = false;
        }

        private void HandleKeyPress(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.W)
                _camera.Move(new Vector3(1, 0, 0));
            else if (e.Key == Key.S)
                _camera.Move(new Vector3(-1, 0, 0));
            else if (e.Key == Key.A)
                _camera.Move(new Vector3(0, 1, 0));
            else if (e.Key == Key.D)
                _camera.Move(new Vector3(0, -1, 0));
        }

        private void CompositionTargetOnRendering(object sender, EventArgs eventArgs)
        {
            var now = DateTime.Now;
            var currentFps = 1000.0 / (now - _previousDate).TotalMilliseconds;
            _previousDate = now;

            Fps.Text = $"instant {currentFps:0.00} Fps";

            if (_lastFpsValues.Count < 60)
            {
                _lastFpsValues.Add(currentFps);
            }
            else
            {
                _lastFpsValues.RemoveAt(0);
                _lastFpsValues.Add(currentFps);
                var totalValues = 0d;
                for (var i = 0; i < _lastFpsValues.Count; i++)
                {
                    totalValues += _lastFpsValues[i];
                }

                var averageFps = totalValues / _lastFpsValues.Count;
                AverageFps.Text = $"average {averageFps:0.00} Fps";
            }

            _model.Clear(0, 0, 0, 255);

            foreach (var mesh in _meshes)
            {
                // rotating slightly the meshes during each frame rendered
                //mesh.Rotation = new Vector3(mesh.Rotation.X + 0.01f, mesh.Rotation.Y + 0.01f, mesh.Rotation.Z);
                mesh.Rotation = new Vector3(mesh.Rotation.X, mesh.Rotation.Y + 0.01f, mesh.Rotation.Z);
                //mesh.Position = new Vector3(0, 0, (float)(5 * Math.Cos(alpha)));
            }

            _model.Render(_camera, _meshes);
            _model.Present();
        }
    }
}
