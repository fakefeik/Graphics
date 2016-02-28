using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using SharpDX;
using Rectangle = System.Drawing.Rectangle;

namespace Graphics.Model
{
    public class Texture
    {
        private readonly byte[] _internalBuffer;
        private readonly int _width;
        private readonly int _height;
        private readonly Color4 _color;

        public Texture() : this(Color4.White)
        {
        }

        public Texture(Color4 color)
        {
            _color = color;
        }

        public Texture(string filename)
        {
            var bmp = new Bitmap(filename);
            _width = bmp.Width;
            _height = bmp.Height;
            var rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            var bmpData = bmp.LockBits(rect, ImageLockMode.ReadWrite, bmp.PixelFormat);

            var ptr = bmpData.Scan0;

            var bytes = Math.Abs(bmpData.Stride) * bmp.Height;
            _internalBuffer = new byte[bytes];

            Marshal.Copy(ptr, _internalBuffer, 0, bytes);
        }

        public Color4 Map(float tu, float tv)
        {
            if (_internalBuffer == null)
                return _color;

            var u = Math.Abs((int)(tu * _width) % _width);
            var v = Math.Abs((int)(tv * _height) % _height);

            var pos = (u + v * _width) * 3;
            var b = _internalBuffer[pos];
            var g = _internalBuffer[pos + 1];
            var r = _internalBuffer[pos + 2];

            return new Color4(r / 255.0f, g / 255.0f, b / 255.0f, 1);
        }
    }
}
