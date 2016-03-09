using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Newtonsoft.Json;
using Brushes = System.Windows.Media.Brushes;
using Color = System.Drawing.Color;
using FlowDirection = System.Windows.Forms.FlowDirection;
using PixelFormat = System.Windows.Media.PixelFormat;
using Point = System.Drawing.Point;

namespace Graphics.ViewModel
{
    public class PolyViewModel : BaseViewModel
    {
        public ICommand ScaleCommand { get; private set; }
        public ICommand ChangeResolutionCommand { get; private set; }
        public ICommand MoveCommand { get; private set; }

        private string _poly1 = "[[-40, 10], [-20, 30], [30, 20], [-5, 0]]";
        private string _poly2 = "[[-25, -3], [-10, 41], [20, -10], [-5, 15]]";

        public string Poly1
        {
            get { return _poly1; }
            set
            {
                _poly1 = value;
                DrawChart();
                OnPropertyChanged("Poly1");
            }
        }

        public string Poly2
        {
            get { return _poly2; }
            set
            {
                _poly2 = value;
                DrawChart();
                OnPropertyChanged("Poly2");
            }
        }

        private int PixelsHorizontal;
        private int PixelsVertical;
        private Point Center;

        public string ImageName { get; private set; }

        private DrawingImage _imageSource;
        public DrawingImage ImageSource
        {
            get
            {
                return _imageSource;
            }
            private set
            {
                _imageSource = value;
                OnPropertyChanged("ImageSource");
            }
        }

        byte[] pixelData;
        PixelFormat pf = PixelFormats.Rgb24;
        int width, height, rawStride;

        public PolyViewModel(string s)
        {
            ImageName = $"../Images/{s}.png";
            InitializeBitmapResolution(512, 512);
            PixelsHorizontal = 10;
            PixelsVertical = 10;

            InitializeCommands();
            DrawChart();
        }

        private void InitializeBitmapResolution(int width, int height)
        {
            this.width = width;
            this.height = height;
            Center = new Point(width / 2, height / 2);
            rawStride = (width * pf.BitsPerPixel + 7) / 8;
            pixelData = new byte[rawStride * height];
        }

        private void InitializeCommands()
        {
            ChangeResolutionCommand = new RelayCommand(o =>
            {
                if ((string)o == "-1")
                {
                    if (width >= 256 && height >= 256)
                    {
                        InitializeBitmapResolution(width / 2, height / 2);
                        DrawChart();
                    }
                }
                else
                {
                    if (width < 1024 && height < 1024)
                    {
                        InitializeBitmapResolution(width * 2, height * 2);
                        DrawChart();
                    }
                }
            });
            ScaleCommand = new RelayCommand(o =>
            {
                if ((string)o == "-1")
                {
                    if (PixelsHorizontal <= 120)
                        PixelsHorizontal += 10;
                    if (PixelsVertical <= 120)
                        PixelsVertical += 10;
                }
                else
                {
                    if (PixelsHorizontal > 10)
                        PixelsHorizontal -= 10;
                    if (PixelsVertical > 10)
                        PixelsVertical -= 10;
                }
                DrawChart();
            });
            MoveCommand = new RelayCommand(o =>
            {
                if ((string)o == "left")
                    Center.X += 10;
                else if ((string)o == "right")
                    Center.X -= 10;
                else if ((string)o == "up")
                    Center.Y += 10;
                else
                    Center.Y -= 10;
                DrawChart();
            });
        }

        private void DrawChart()
        {
            Clear(Color.White);
            DrawAxis(Color.Black, Color.AliceBlue);
            Draw(Color.Brown);
            DrawUi();
        }

        private void DrawUi()
        {
            var bitmapSource = BitmapSource.Create(width, height,
                96, 96, pf, null, pixelData, rawStride);
            var visual = new DrawingVisual();
            using (var graphics = visual.RenderOpen())
            {
                graphics.DrawImage(bitmapSource, new Rect(0, 0, width, height));
                for (
                    int i = Center.X % PixelsHorizontal,
                        j = -Center.X / PixelsHorizontal;
                    i < width;
                    i += PixelsHorizontal)
                    graphics.DrawText(
                        new FormattedText($"{j++}", CultureInfo.InvariantCulture,
                            (System.Windows.FlowDirection)FlowDirection.LeftToRight,
                            new Typeface("Segoe UI"), PixelsHorizontal * 0.7, Brushes.Black),
                        new System.Windows.Point(i, -1));
                for (
                    int i = Center.Y % PixelsVertical,
                        j = Center.Y / PixelsVertical;
                    i < width;
                    i += PixelsVertical)
                    graphics.DrawText(
                        new FormattedText($"{j--}", CultureInfo.InvariantCulture,
                            (System.Windows.FlowDirection)FlowDirection.LeftToRight,
                            new Typeface("Segoe UI"), PixelsHorizontal * 0.7, Brushes.Black),
                        new System.Windows.Point(-1, i));
            }
            ImageSource = new DrawingImage(visual.Drawing);
        }

        private void SetPixel(int x, int y, Color color)
        {
            if (IsInBounds(x, y))
            {
                var xIndex = x * 3;
                var yIndex = y * rawStride;
                pixelData[xIndex + yIndex] = color.R;
                pixelData[xIndex + yIndex + 1] = color.G;
                pixelData[xIndex + yIndex + 2] = color.B;
            }
        }

        private void Clear(Color color)
        {
            for (int i = 0; i < width; i++)
                for (int j = 0; j < height; j++)
                    SetPixel(i, j, color);
        }

        private bool IsInBounds(int x, int y)
        {
            return x >= 0 && x < width && y >= 0 && y < height;
        }

        private List<PointF> TryGetPoly(string s)
        {
            try
            {
                return JsonConvert.DeserializeObject<dynamic[]>(s).Select(x => new PointF(x[0].ToObject<float>(), x[1].ToObject<float>())).ToList();
            }
            catch (Exception e)
            {
                return new List<PointF>();
            }
        }

        private void Draw(Color color)
        {
            var p1 = TryGetPoly(Poly1);
            var p2 = TryGetPoly(Poly2);

            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                {
                    var xActual = (x - Center.X)/(float) PixelsHorizontal;
                    var yActual = -(y - Center.Y)/(float) PixelsVertical;

                    var insidePoly1 = Algorithm.IsInPolygon(p1, xActual, yActual);
                    var insidePoly2 = Algorithm.IsInPolygon(p2, xActual, yActual);
                    if (insidePoly1 && insidePoly2)
                        SetPixel(x, y, Color.Brown);
                    else if (insidePoly1)
                        SetPixel(x, y, Color.Blue);
                    else if (insidePoly2)
                        SetPixel(x, y, Color.Green);
                }
        }

        private void DrawAxis(Color axisColor, Color linesColor)
        {
            for (int i = Center.X % PixelsHorizontal; i < width; i += PixelsHorizontal)
                Algorithm.Line(i, 0, i, height, linesColor, SetPixel);
            for (int i = Center.Y % PixelsVertical; i < height; i += PixelsVertical)
                Algorithm.Line(0, i, width, i, linesColor, SetPixel);
            Algorithm.Line(0, Center.Y, width, Center.Y, axisColor, SetPixel);
            Algorithm.Line(Center.X, 0, Center.X, height, axisColor, SetPixel);
        }
    }
}
