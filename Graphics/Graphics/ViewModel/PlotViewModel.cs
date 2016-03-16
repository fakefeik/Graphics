using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Color = System.Drawing.Color;

namespace Graphics.ViewModel
{
    public class PlotViewModel : BaseViewModel
    {
        public ICommand ChangeResolutionCommand { get; private set; }

        public string ImageName { get; private set; }

        private BitmapSource _imageSource;

        public BitmapSource ImageSource
        {
            get { return _imageSource; }
            private set
            {
                _imageSource = value;
                OnPropertyChanged("ImageSource");
            }
        }

        private string _param1 = "-5";
        private string _param2 = "-5";
        private string _param3 = "5";
        private string _param4 = "5";

        public string Param1
        {
            get { return _param1; }
            set
            {
                _param1 = value;
                Draw();
                OnPropertyChanged("Poly1");
            }
        }

        public string Param2
        {
            get { return _param2; }
            set
            {
                _param2 = value;
                Draw();
                OnPropertyChanged("Poly2");
            }
        }

        public string Param3
        {
            get { return _param3; }
            set
            {
                _param3 = value;
                Draw();
                OnPropertyChanged("Poly3");
            }
        }

        public string Param4
        {
            get { return _param4; }
            set
            {
                _param4 = value;
                Draw();
                OnPropertyChanged("Poly4");
            }
        }

        byte[] pixelData;
        PixelFormat pf = PixelFormats.Rgb24;
        int width, height, rawStride;

        public PlotViewModel(string s)
        {
            ImageName = $"../Images/{s}.png";
            InitializeBitmapResolution(512, 512);
            InitializeCommands();
            Draw();
        }

        private void InitializeBitmapResolution(int width, int height)
        {
            this.width = width;
            this.height = height;
            rawStride = (width*pf.BitsPerPixel + 7)/8;
            pixelData = new byte[rawStride*height];
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
                        Draw();
                    }
                }
                else
                {
                    if (width < 1024 && height < 1024)
                    {
                        InitializeBitmapResolution(width*2, height*2);
                        Draw();
                    }
                }
            });
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

        private double TryParse(string s, double d)
        {
            try
            {
                return double.Parse(s);
            }
            catch (Exception)
            {
                return d;
            }
        }

        private void Draw()
        {
            Clear(Color.White);
            DrawPlot(width - 1, height - 1, TryParse(Param1, -5), TryParse(Param2, -5), TryParse(Param3, 5), TryParse(Param4, 5),
                (x, y) => Math.Cos(x)*Math.Cos(y));
            ImageSource = BitmapSource.Create(width, height,
                96, 96, pf, null, pixelData, rawStride);
        }

        private void DrawPlot(int w, int h, double x1, double y1, double x2, double y2,
            Func<double, double, double> function)
        {
            var n = 50;
            var m = w*6;

            var top = new int[w + 1];
            var bottom = new int[w + 1];

            var minx = 10.0;
            var maxx = -minx;
            var miny = minx;
            var maxy = maxx;

            for (int i = 0; i <= n; i++)
            {
                var x = x2 + i*(x1 - x2)/n;
                for (int j = 0; j <= n; j++)
                {
                    var y = y2 + j*(y1 - y2)/n;
                    var z = function(x, y);
                    var xx = CoordX(x, y, z);
                    var yy = CoordY(x, y, z);
                    if (xx > maxx)
                        maxx = xx;
                    if (yy > maxy)
                        maxy = yy;
                    if (xx < minx)
                        minx = xx;
                    if (yy < miny)
                        miny = yy;
                }
            }

            for (int i = 0; i < w; i++)
            {
                top[i] = h;
                bottom[i] = 0;
            }

            for (int i = 0; i <= n; i++)
            {
                var x = x2 + i*(x1 - x2)/n;
                for (int j = 0; j <= m; j++)
                {
                    var y = y2 + j*(y1 - y2)/m;
                    var z = function(x, y);
                    var xx = CoordX(x, y, z);
                    var yy = CoordY(x, y, z);
                    xx = (xx - minx)/(maxx - minx)*w;
                    yy = (yy - miny)/(maxy - miny)*h;

                    if (x >= x1 && x <= x2 && y >= y1 && y <= y2)
                    {
                        if (yy > bottom[(int) xx])
                        {
                            SetPixel((int) xx, (int) yy, Color.Black);
                            bottom[(int) xx] = (int) yy;
                        }

                        if (yy < top[(int) xx])
                        {
                            SetPixel((int) xx, (int) yy, Color.Black);
                            top[(int) xx] = (int) yy;
                        }
                    }
                }
            }
        }

        private static double CoordX(double x, double y, double z)
        {
            return (y - x)*Math.Sqrt(3.0)/2;
        }

        private static double CoordY(double x, double y, double z)
        {
            return (x + y)/2 - z;
        }
    }
}
