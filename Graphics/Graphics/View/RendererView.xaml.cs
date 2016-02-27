using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Graphics.Model;
using Graphics.ViewModel;
using SharpDX;
using Plane = Graphics.Model.Plane;

namespace Graphics.View
{
    /// <summary>
    /// Interaction logic for RendererView.xaml
    /// </summary>
    public partial class RendererView : UserControl
    {
        private RendererViewModel _model;
        private Mesh[] _meshes;
        private Camera _camera = new Camera();
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
            CompositionTarget.Rendering += CompositionTargetOnRendering;
            _meshes = JsonModelLoader.LoadJsonFile("monkey.babylon");
            //_meshes = new List<Mesh>(_meshes) {new Plane(1, 1, 2, 2, (x, y) => (float) (Math.Cos(x) * Math.Cos(y)))}.ToArray();
            //_meshes = new[] { new Plane(2, 2, 10, 10, (x, y) => x * y) };
            //_meshes = new[] {new Plane(1, 1, 5, 5, (f, f1) => 0)};
            //_meshes[0].Rotation = Vector3.Down;
            _camera.Position = new Vector3(0, 0, 10.0f);
            _camera.Target = Vector3.Zero;
        }

        private void CompositionTargetOnRendering(object sender, EventArgs eventArgs)
        {
            var now = DateTime.Now;
            var currentFps = 1000.0 / (now - _previousDate).TotalMilliseconds;
            _previousDate = now;

            //fps.Text = string.Format("instant {0:0.00} fps", currentFps);

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
                //averageFps.Text = string.Format("average {0:0.00} fps", averageFps);
            }

            _model.Clear(0, 0, 0, 255);

            foreach (var mesh in _meshes)
            {
                // rotating slightly the meshes during each frame rendered
                //mesh.Rotation = new Vector3(mesh.Rotation.X + 0.01f, mesh.Rotation.Y + 0.01f, mesh.Rotation.Z);
                mesh.Rotation = new Vector3(mesh.Rotation.X, mesh.Rotation.Y + 0.01f, mesh.Rotation.Z);
                //mesh.Position = new Vector3(0, 0, (float)(5 * Math.Cos(alpha)));
            }

            // Doing the various matrix operations
            _model.Render(_camera, _meshes);
            // Flushing the back buffer into the front buffer
            _model.Present();
        }
    }
}
