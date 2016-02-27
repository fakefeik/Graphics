using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Graphics.Model;
using Color = System.Drawing.Color;
using PixelFormat = System.Windows.Media.PixelFormat;

namespace Graphics.ViewModel
{
    public class ChartViewModel : BaseViewModel
    {
        public List<Parameter> Paramers { get; set; }
        private Func<double, List<double>, double> Function; 

        public string Text { get; private set; }

        private BitmapSource _imageSource;
        public BitmapSource ImageSource
        {
            get
            {
                if (_imageSource == null)
                    _imageSource = BitmapSource.Create(2, 2, 300, 300, PixelFormats.Indexed8, BitmapPalettes.Gray256, pixelData, 2);
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
            var model = new ChartModel();
            var function = model.Functions[5];
            Paramers = function.Params;
            Function = function.Func;

            width = 500;
            height = 500;
            rawStride = (width * pf.BitsPerPixel + 7) / 8;
            pixelData = new byte[rawStride * height];
            Clear(Color.White);
            DrawAxis(Color.Black, Color.Gray, 10, 10);
            DrawChart(Color.Brown, 10, 10);
            ImageSource = BitmapSource.Create(width, height,
                96, 96, pf, null, pixelData, rawStride);
            
        }

        private void SetPixel(int x, int y, Color color)
        {
            if (x >= 0 && x < width && y >= 0 && y < height)
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

        private void DrawChart(Color chartColor, int pixelsHorizontal, int pixelsVertical)
        {
            var previousY = 0;
            for (int i = 0; i < width; i++)
            {
                var x = (i - width/2.0)/(double)pixelsHorizontal;
                var param = Paramers.Select(p => (double) p.InitialValue).ToList();
                var res = Function(x, param);
                var y = (int) (res*pixelsVertical+height/2);
                SetPixel(i, y, chartColor);
                if (i != 0)
                    Line(i - 1, previousY, i, y, chartColor);
                previousY = y;
            }
            //Line(0, 0, width, height, chartColor);
        }

        private void DrawAxis(Color axisColor, Color linesColor, int pixelsHorizontal, int pixelsVertical)
        {
            for (int i = width/2%pixelsHorizontal; i < width; i += pixelsHorizontal)
                Line(i, 0, i, height, linesColor);
            for (int i = height/2%pixelsVertical; i < height; i += pixelsVertical)
                Line(0, i, width, i, linesColor);
            Line(0, height/2.0f, width, height/2.0f, axisColor);
            Line(width/2.0f, 0, width/2.0f, height, axisColor);
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
