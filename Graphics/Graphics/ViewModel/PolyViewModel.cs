using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Drawing;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Color = System.Drawing.Color;

namespace Graphics.ViewModel
{
    public class PolyViewModel : BaseViewModel
    {
        public ICommand ChangeResolutionCommand { get; private set; }
        public string ImageName { get; set; }

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

        byte[] pixelData;
        PixelFormat pf = PixelFormats.Rgb24;
        int rawStride;

        public int Width { get; private set; }
        public int Height { get; private set; }

        public PolyViewModel(string s)
        {
            ImageName = $"../Images/{s}.png";
            ChangeResolutionCommand = new RelayCommand(o =>
            {
                if ((string)o == "-1")
                {
                    if (Width >= 256 && Height >= 256)
                    {
                        InitializeBitmapResolution(Width / 2, Height / 2);
                        //DrawChart();
                    }
                }
                else
                {
                    if (Width < 1024 && Height < 1024)
                    {
                        InitializeBitmapResolution(Width * 2, Height * 2);
                        //DrawChart();
                    }
                }
            });
            InitializeBitmapResolution(512, 512);
            Update();
        }

        private void InitializeBitmapResolution(int width, int height)
        {
            Width = width;
            Height = height;
            rawStride = (width * pf.BitsPerPixel + 7) / 8;
            pixelData = new byte[rawStride * height];
        }

        public void SetPixel(int x, int y, Color color)
        {
            var xIndex = x * 3;
            var yIndex = y * rawStride;
            pixelData[xIndex + yIndex] = color.R;
            pixelData[xIndex + yIndex + 1] = color.G;
            pixelData[xIndex + yIndex + 2] = color.B;
        }

        public void Clear(Color color)
        {
            for (int i = 0; i < Width; i++)
                for (int j = 0; j < Height; j++)
                    SetPixel(i, j, color);
        }

        public void Draw(List<Point> poly1, List<Point> poly2)
        {
            Clear(Color.White);
            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                {
                    var insidePoly1 = Algorithm.IsInBoundingBox(poly1, x, y) && Algorithm.IsInPolygon(poly1, x, y);
                    var insidePoly2 = Algorithm.IsInBoundingBox(poly2, x, y) && Algorithm.IsInPolygon(poly2, x, y);
                    if (insidePoly1 && insidePoly2)
                        SetPixel(x, y, Color.Brown);
                    else if (insidePoly1)
                        SetPixel(x, y, Color.Blue);
                    else if (insidePoly2)
                        SetPixel(x, y, Color.Green);
                }
            Update();
        }

        public void Update()
        {
            ImageSource = BitmapSource.Create(Width, Height,
                96, 96, pf, null, pixelData, rawStride);
        }
    }
}
