
using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.FrostWarden
{
    public static class GifLoader
    {
        public static Texture2D[] LoadGifFrames(string path)
        {
            using var gif = System.Drawing.Image.FromFile(path);
            var dimension = new System.Drawing.Imaging.FrameDimension(gif.FrameDimensionsList[0]);
            int frameCount = gif.GetFrameCount(dimension);

            var textures = new List<Texture2D>();

            for (int i = 0; i < frameCount; i++)
            {
                gif.SelectActiveFrame(dimension, i);

                using System.Drawing.Bitmap frameBmp = new(gif.Width, gif.Height);
                using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(frameBmp))
                {
                    g.DrawImage(gif, new System.Drawing.Rectangle(0, 0, gif.Width, gif.Height));
                }

                // Lock bitmap bits for fast pixel access
                System.Drawing.Imaging.BitmapData bmpData = frameBmp.LockBits(
                    new System.Drawing.Rectangle(0, 0, frameBmp.Width, frameBmp.Height),
                    System.Drawing.Imaging.ImageLockMode.ReadOnly,
                    System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                int size = bmpData.Stride * bmpData.Height;
                byte[] pixels = new byte[size];
                System.Runtime.InteropServices.Marshal.Copy(bmpData.Scan0, pixels, 0, size);
                frameBmp.UnlockBits(bmpData);

                // Raylib Image expects RGBA, but Windows bitmaps are BGRA, so swap channels:
                for (int p = 0; p < size; p += 4)
                {
                    byte b = pixels[p + 0];
                    byte g = pixels[p + 1];
                    byte r = pixels[p + 2];
                    byte a = pixels[p + 3];

                    pixels[p + 0] = r;
                    pixels[p + 1] = g;
                    pixels[p + 2] = b;
                    pixels[p + 3] = a;
                }

                unsafe
                {
                    // Create Raylib Image
                    Image raylibImage = new Image
                    {
                        Data = System.Runtime.InteropServices.Marshal.AllocHGlobal(size).ToPointer(),
                        Width = frameBmp.Width,
                        Height = frameBmp.Height,
                        Mipmaps = 1,
                        Format = PixelFormat.UncompressedR8G8B8A8
                    };

                    System.Runtime.InteropServices.Marshal.Copy(pixels, 0, (nint)raylibImage.Data, size);

                    Texture2D tex = Raylib.LoadTextureFromImage(raylibImage);

                    textures.Add(tex);
                }
            }

            return textures.ToArray();
        }
    }
}
