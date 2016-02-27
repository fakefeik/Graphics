using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Graphics.Model;
using Color = System.Drawing.Color;
using FlowDirection = System.Windows.Forms.FlowDirection;
using PixelFormat = System.Windows.Media.PixelFormat;
using Point = System.Drawing.Point;

namespace Graphics.ViewModel
{
    public class ChartViewModel : BaseViewModel
    {
        public ICommand ScaleCommand { get; private set; }
        public ICommand ChangeResolutionCommand { get; private set; }
        public ICommand MoveCommand { get; private set; }

        private Action<Color> Draw;

        private int PixelsHorizontal;
        private int PixelsVertical;
        private Point Center;

        public List<ParamsViewModel> Params { get; private set; }
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


        public ChartViewModel(string s)
        {
            if (s == "Task1")
            {
                InitializeParams(new[] {"A", "B"});
                Draw = DrawFunction;
            }
            else if (s == "Task2")
            {
                InitializeParams(new[] {"A"});
                Draw = DrawPolarFunction;
            }
            else if (s == "Task3")
            {
                InitializeParams(new string[0]);
                Draw = DrawParabola;
            }
            ImageName = $"../Images/{s}.png";
            InitializeBitmapResolution(512, 512);
            PixelsHorizontal = 10;
            PixelsVertical = 10;

            InitializeCommands();
            DrawChart();
        }

        private void InitializeParams(string[] names)
        {
            Params =
                names.Select(
                    x => new ParamsViewModel(new RelayCommand(Increase), new RelayCommand(Decrease), new Parameter(x)))
                    .ToList();
        }

        private void Increase(object o)
        {
            var p = (Parameter)o;
            if (p.Value + p.Step < p.MaxValue)
                p.Value += p.Step;
            DrawChart();
            for (int i = 0; i < Params.Count; i++)
                Params[i].Parameter = Params[i].Parameter;
        }

        private void Decrease(object o)
        {
            var p = (Parameter)o;
            if (p.Value + p.Step < p.MaxValue)
                p.Value -= p.Step;
            DrawChart();
            for (int i = 0; i < Params.Count; i++)
                Params[i].Parameter = Params[i].Parameter;
        }

        private void InitializeBitmapResolution(int width, int height)
        {
            this.width = width;
            this.height = height;
            Center = new Point(width/2, height/2);
            rawStride = (width * pf.BitsPerPixel + 7) / 8;
            pixelData = new byte[rawStride * height];
        }

        private void InitializeCommands()
        {
            ChangeResolutionCommand = new RelayCommand(o =>
            {
                if ((string) o == "-1")
                {
                    if (width >= 256 && height >= 256)
                    { 
                        InitializeBitmapResolution(width/2, height/2);
                        DrawChart();
                    }
                }
                else
                {
                    if (width < 1024 && height < 1024)
                    {
                        InitializeBitmapResolution(width*2, height*2);
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
                if ((string) o == "left")
                    Center.X += 10;
                else if ((string) o == "right")
                    Center.X -= 10;
                else if ((string) o == "up")
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
                    int i = Center.X%PixelsHorizontal + PixelsHorizontal,
                        j = -(width - Center.X%PixelsHorizontal)/PixelsHorizontal/2;
                    i < width;
                    i += PixelsHorizontal)
                    graphics.DrawText(
                        new FormattedText($"{j++}", CultureInfo.InvariantCulture,
                            (System.Windows.FlowDirection) FlowDirection.LeftToRight,
                            new Typeface("Segoe UI"), PixelsHorizontal*0.7, Brushes.Black),
                        new System.Windows.Point(i, -1));
                for (
                    int i = Center.Y%PixelsVertical + PixelsVertical,
                        j = (height - Center.Y%PixelsVertical)/PixelsVertical/2;
                    i < width;
                    i += PixelsVertical)
                    graphics.DrawText(
                        new FormattedText($"{j--}", CultureInfo.InvariantCulture,
                            (System.Windows.FlowDirection) FlowDirection.LeftToRight,
                            new Typeface("Segoe UI"), PixelsHorizontal*0.7, Brushes.Black),
                        new System.Windows.Point(-1, i));
            }
            ImageSource = new DrawingImage(visual.Drawing);
        }

        private void SetPixel(int x, int y, Color color)
        {
            if (IsInBounds(x, y))
            { 
                var xIndex = x*3;
                var yIndex = y*rawStride;
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

        private void DrawFunction(Color chartColor)
        {
            Func<double, List<double>, double> function = (x, p) =>
            {
                try
                {
                    var subsquare = p[0]*x*x/(x + p[1]);
                    return Math.Sign(subsquare)*Math.Pow(Math.Abs(subsquare), 1/3.0);
                }
                catch (Exception)
                {
                    return 0;
                }
            };
            var previousY = 0;
            var param = Params.Select(p => (double)p.Parameter.Value).ToList();
            for (int i = 0; i < width; i++)
            {
                var x = (i - Center.X)/(double)PixelsHorizontal;
                var res = -function(x, param);
                int y;
                var secondResult = 0.0;
                if (double.IsNaN(res))
                    continue;
                if (double.IsInfinity(res))
                {
                    secondResult = -function(x - 0.001, param);
                    y = secondResult > 0 ? height-1 : 0;
                }
                else
                    y = (int) (res*PixelsVertical+Center.Y);
                if (!IsInBounds(0, previousY) && !IsInBounds(0, y))
                {
                    previousY = y;
                    continue;
                }
                SetPixel(i, y, chartColor);
                if (i != 0)
                    Line(i - 1, previousY, i, y, chartColor);
                previousY = double.IsInfinity(res) ? (secondResult > 0 ? 0 : height - 1) : y;
            }
        }

        private void DrawPolarFunction(Color chartColor)
        {
            var previousX = Center.X;
            var previousY = Center.Y;
            var a = Params.Select(x => x.Parameter.Value).First();
            for (int i = 0; i < 360; i++)
            {
                // r = a*sin(3φ)
                // x = r*cos(φ)
                // y = r*sin(φ)

                // x = a*sin(3φ)*cos(φ)
                // y = a*sin(3φ)*sin(φ)
                var phi = i/180.0*Math.PI;
                var x = a*Math.Sin(3*phi)*Math.Cos(phi);
                var y = a*Math.Sin(3*phi)*Math.Sin(phi);
                var xActual = (int) (x*PixelsHorizontal + Center.X);
                var yActual = (int) (y*PixelsVertical + Center.Y);

                SetPixel(xActual, yActual, chartColor);
                Line(previousX, previousY, xActual, yActual, chartColor);
                previousX = xActual;
                previousY = yActual;
            }
        }

        private void DrawParabola(Color color)
        {
            int p = 2;
            int x0 = 0;
            int y0 = 0;
            int Sh, Sv, Sd;
            int y = y0;
            int x = x0;
            Sd = (y + 1) * (y + 1) - 2 * p * (x + 1);
            Sv = (y + 1) * (y + 1) - 2 * p * x;
            Sh = y * y - 2 * p * (x + 1);
            SetPixel(x0 * PixelsHorizontal + width / 2, y0 * PixelsVertical + height / 2, color);
            while (x + x0 < width/PixelsHorizontal/2) //пока полотно не кончится
            {
                if (Math.Abs(Sh) - Math.Abs(Sv) <= 0)
                {
                    if (Math.Abs(Sd) - Math.Abs(Sh) < 0)
                        y++;
                    x++;
                }
                else
                {
                    if (Math.Abs(Sv) - Math.Abs(Sd) > 0)
                        x++;
                    y++;
                }

                SetPixel((x + x0) * PixelsHorizontal + width / 2, (y + y0) * PixelsVertical + height / 2, color);
                SetPixel((x + x0) * PixelsHorizontal + width / 2, -(y + y0) * PixelsVertical + height / 2, color);
                Sd = (y + 1) * (y + 1) - 2 * p * (x + 1);
                Sv = (y + 1) * (y + 1) - 2 * p * x;
                Sh = y * y - 2 * p * (x + 1);
            }
        }

        private void DrawAxis(Color axisColor, Color linesColor)
        {
            for (int i = Center.X%PixelsHorizontal; i < width; i += PixelsHorizontal)
                Line(i, 0, i, height, linesColor);
            for (int i = Center.Y%PixelsVertical; i < height; i += PixelsVertical)
                Line(0, i, width, i, linesColor);
            Line(0, Center.Y, width, Center.Y, axisColor);
            Line(Center.X, 0, Center.X, height, axisColor);
        }

        private void Line(float x1, float y1, float x2, float y2, Color color)
        {
            // Bresenham's line algorithm
            var steep = Math.Abs(y2 - y1) > Math.Abs(x2 - x1);
            float temp;
            if (steep)
            {
                temp = x1;
                x1 = y1;
                y1 = temp;

                temp = x2;
                x2 = y2;
                y2 = temp;
            }

            if (x1 > x2)
            {
                temp = x1;
                x1 = x2;
                x2 = temp;

                temp = y1;
                y1 = y2;
                y2 = temp;
            }

            var dx = x2 - x1;
            var dy = Math.Abs(y2 - y1);

            var error = dx/2.0f;
            var ystep = y1 < y2 ? 1 : -1;
            var y = (int) y1;

            var maxX = (int) x2;

            for (int x = (int) x1; x < maxX; x++)
            {
                if (steep)
                    SetPixel(y, x, color);
                else
                    SetPixel(x, y, color);

                error -= dy;

                if (error < 0)
                {
                    y += ystep;
                    error += dx;
                }
            }
        }
    }
}
