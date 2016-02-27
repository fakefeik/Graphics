﻿using SharpDX;

namespace Graphics.Model
{
    public class Texture
    {
        private byte[] internalBuffer;
        private int width;
        private int height;

        public Texture(string filename, int width, int height)
        {
            this.width = width;
            this.height = height;
            Load(filename);
        }

        async void Load(string filename)
        {
            //var file = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFileAsync(filename);

            //using (var stream = await file.OpenReadAsync())
            //{
            //    var bmp = new WriteableBitmap(width, height);
            //    bmp.SetSource(stream);
            //    internalBuffer = bmp.PixelBuffer.ToArray();
            //}
        }

        public Color4 Map(float tu, float tv)
        {
            //// Image is not loaded yet
            //if (internalBuffer == null)
            //{
            //    return Color4.White;
            //}
            //// using a % operator to cycle/repeat the texture if needed
            //int u = Math.Abs((int) (tu*width) % width);
            //int v = Math.Abs((int) (tv*height) % height);

            //int pos = (u + v * width) * 4;
            //byte b = internalBuffer[pos];
            //byte g = internalBuffer[pos + 1];
            //byte r = internalBuffer[pos + 2];
            //byte a = internalBuffer[pos + 3];

            //return new Color4(r / 255.0f, g / 255.0f, b / 255.0f, a / 255.0f);
            //TODO: find PixelBuffer in WriteableBitmap
            return new Color4(1, 1, 1, 1);
        }
    }
}
