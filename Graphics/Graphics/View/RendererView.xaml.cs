using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
        private bool _dragInProgress;
        private Point _lastPosition;

        private DateTime _previousDate;
        private readonly Collection<double> _lastFpsValues = new Collection<double>();
        private string taskName;

        private Window window;

        private readonly Camera _camera = new Camera
        {
            Position = new Vector3(0, 0, 10.0f),
            Target = Vector3.Zero,
            Up = new Vector3(0, 1, 0)
        };

        public RendererView()
        {
            InitializeComponent();
        }

        private void RenderView_OnLoaded(object sender, EventArgs e)
        {
            _model = (RendererViewModel)DataContext;
            taskName = _model.TaskName;

            var window = Window.GetWindow(this);
            window.KeyDown += HandleKeyPress;
            window.MouseDown += WindowOnMouseDown;
            window.MouseUp += WindowOnMouseUp;
            window.MouseMove += WindowOnMouseMove;

            CompositionTarget.Rendering += CompositionTargetOnRendering;
            if (taskName == "Task6")
            {
                _meshes = new[] { JsonModelLoader.LoadMesh("Mesh/cylinder.babylon") };
                _meshes[0].Texture = new Texture("Mesh/yoba.png");
            }
            else
            {
                _meshes = new[] { new Plane(10, 10, 20, 20, (x, y) => (float)(Math.Cos(x) * Math.Cos(y))) };
                _meshes[0].Rotation = new Vector3((float)Math.PI / 2, 0, 0);
                _model.Wireframe = true;
            }
        }

        private void WindowOnMouseDown(object sender, MouseButtonEventArgs e)
        {
#if DEBUG
            if (e.ChangedButton == MouseButton.Right)
                _model.Wireframe = !_model.Wireframe;
#endif
            _dragInProgress = true;
            _lastPosition = e.GetPosition(this);
        }

        private void WindowOnMouseMove(object sender, MouseEventArgs e)
        {
            if (!_dragInProgress)
                return;
            var position = e.GetPosition(this);
            _camera.Rotate(new Vector2((float) (position.X - _lastPosition.X)/100, -(float) (position.Y - _lastPosition.Y)/100));
            _lastPosition = e.GetPosition(this);
        }

        private void WindowOnMouseUp(object sender, MouseButtonEventArgs e)
        {
            _dragInProgress = false;
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
                var totalValues = _lastFpsValues.Sum();

                var averageFps = totalValues / _lastFpsValues.Count;
                AverageFps.Text = $"average {averageFps:0.00} Fps";
            }

            _model.Clear(0, 0, 0, 255);

            if (taskName == "Task6")
                foreach (var mesh in _meshes)
                    //mesh.Rotation = new Vector3(mesh.Rotation.X + 0.01f, mesh.Rotation.Y + 0.01f, mesh.Rotation.Z);
                    mesh.Rotation = new Vector3(mesh.Rotation.X, mesh.Rotation.Y + 0.01f, mesh.Rotation.Z);

            _model.Render(_camera, _meshes);
            _model.Present();
        }

        private void RendererView_OnUnloaded(object sender, RoutedEventArgs e)
        {
            CompositionTarget.Rendering -= CompositionTargetOnRendering;
            if (window != null)
            {
                window.KeyDown -= HandleKeyPress;
                window.MouseDown -= WindowOnMouseDown;
                window.MouseUp -= WindowOnMouseUp;
                window.MouseMove -= WindowOnMouseMove;
            }
        }

        private void RendererView_OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            RendererView_OnUnloaded(sender, null);

            _model = (RendererViewModel)DataContext;
            if (_model == null)
                return;
            taskName = _model.TaskName;

            window = Window.GetWindow(this);

            window.KeyDown += HandleKeyPress;
            window.MouseDown += WindowOnMouseDown;
            window.MouseUp += WindowOnMouseUp;
            window.MouseMove += WindowOnMouseMove;

            CompositionTarget.Rendering += CompositionTargetOnRendering;
            if (taskName == "Task6")
            {
                _meshes = new[] { JsonModelLoader.LoadMesh("Mesh/cylinder.babylon") };
                _meshes[0].Texture = new Texture("Mesh/yoba.png");
            }
            else
            {
                _meshes = new[] { new Plane(10, 10, 20, 20, (x, y) => (float)(Math.Cos(x) * Math.Cos(y))) };
                _meshes[0].Rotation = new Vector3((float)Math.PI / 2, 0, 0);
                _model.Wireframe = true;
            }
        }
    }
}
