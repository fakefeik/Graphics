using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Graphics.ViewModel;
using Color = System.Drawing.Color;
using Point = System.Drawing.Point;

namespace Graphics.View
{
    /// <summary>
    /// Interaction logic for PolyView.xaml
    /// </summary>
    public partial class PolyView : UserControl
    {
        private PolyViewModel _model;
        private Window _window;

        private bool _polyCompleted;
        private bool _differenceCompleted;

        private Point? _previousPoint;

        private List<Point> _poly1 = new List<Point>();
        private List<Point> _poly2 = new List<Point>(); 

        public PolyView()
        {
            InitializeComponent();
        }

        private void PolyView_OnLoaded(object sender, RoutedEventArgs e)
        {
            _model = (PolyViewModel)DataContext;
            _model.Draw(_poly1, _poly2);
            _window = Window.GetWindow(this);
            _window.MouseDown += WindowOnMouseDown;
        }

        private void WindowOnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Right)
            {
                if (!_polyCompleted)
                {
                    _polyCompleted = true;
                    _model.Draw(_poly1, _poly2);
                }
                else if (!_differenceCompleted)
                {
                    _model.Draw(_poly1, _poly2);
                    _differenceCompleted = true;
                }
                else
                {
                    _differenceCompleted = false;
                    _polyCompleted = false;
                    _poly1 = new List<Point>();
                    _poly2 = new List<Point>();
                    _model.Draw(_poly1, _poly2);
                }
                _previousPoint = null;
            }
            else
            {
                if (_differenceCompleted)
                    return;

                var actualWidth = Image.ActualWidth;
                var actualHeight = Image.ActualHeight;
                var widthRatio = _model.Width/actualWidth;
                var heightRatio = _model.Height/actualHeight;
                var position = e.GetPosition(Image);
                if (position.X >= actualWidth || position.X < 0 || position.Y >= actualHeight || position.Y < 0)
                    return;
                
                var bitmapPoint = new Point((int) (position.X*widthRatio), (int) (position.Y*heightRatio));
                if (!_differenceCompleted)
                {
                    
                    if (!_polyCompleted)
                        _poly1.Add(bitmapPoint);
                    else
                        _poly2.Add(bitmapPoint);
                    if (_previousPoint.HasValue)
                    {
                        Algorithm.Line(_previousPoint.Value.X, _previousPoint.Value.Y, bitmapPoint.X, bitmapPoint.Y, Color.Gray,
                            _model.SetPixel);
                        _model.Update();
                    }
                    _previousPoint = bitmapPoint;
                }
            }
        }

        private void PolyView_OnUnloaded(object sender, RoutedEventArgs e)
        {
            if (_window != null)
                _window.MouseDown -= WindowOnMouseDown;
        }
    }
}
