using Raylib_cs;

namespace WinterRose.FrostWarden
{
    public class SpriteGif : Sprite
    {
        public override Texture2D Texture => GetNext();
        public override Vector2 Size => new Vector2(frames[currentFrame].Width, frames[currentFrame].Height);

        public IReadOnlyList<Texture2D> Frames => frames;
        public IReadOnlyList<int> FrameDurationsMs => frameDurations;

        private readonly List<Texture2D> frames = new();
        private readonly List<int> frameDurations = new();

        public int FrameCount => frames.Count;

        int currentFrame = 0;
        float timer = 0;

        public Texture2D GetNext()
        {
            float frameDurationMs = frameDurations[currentFrame];

            timer += Time.deltaTime;
            if (timer > frameDurationMs / 1000)
            {
                currentFrame = (currentFrame + 1) % frames.Count;
                timer = 0;
            }

            return frames[currentFrame];
        }

        public SpriteGif(string path)
        {
            Source = path;
            var gif = System.Drawing.Image.FromFile(path);
            var dimension = new System.Drawing.Imaging.FrameDimension(gif.FrameDimensionsList[0]);
            int frameCount = gif.GetFrameCount(dimension);

            // Extract delay info
            const int PROPERTY_TAG_FRAME_DELAY = 0x5100;
            var delaysProp = gif.GetPropertyItem(PROPERTY_TAG_FRAME_DELAY);
            var delays = new int[frameCount];
            for (int i = 0; i < frameCount; i++)
                delays[i] = BitConverter.ToInt32(delaysProp.Value, i * 4) * 10; // to ms

            for (int i = 0; i < frameCount; i++)
            {
                gif.SelectActiveFrame(dimension, i);

                using var frameBmp = new System.Drawing.Bitmap(gif.Width, gif.Height);
                using (var g = System.Drawing.Graphics.FromImage(frameBmp))
                    g.DrawImage(gif, new System.Drawing.Rectangle(0, 0, gif.Width, gif.Height));

                System.Drawing.Imaging.BitmapData bmpData = frameBmp.LockBits(
                    new System.Drawing.Rectangle(0, 0, frameBmp.Width, frameBmp.Height),
                    System.Drawing.Imaging.ImageLockMode.ReadOnly,
                    System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                int size = bmpData.Stride * bmpData.Height;
                byte[] pixels = new byte[size];
                System.Runtime.InteropServices.Marshal.Copy(bmpData.Scan0, pixels, 0, size);
                frameBmp.UnlockBits(bmpData);

                for (int p = 0; p < size; p += 4)
                    (pixels[p], pixels[p + 2]) = (pixels[p + 2], pixels[p]); // BGR → RGB

                unsafe
                {
                    var rayImage = new Image
                    {
                        Data = System.Runtime.InteropServices.Marshal.AllocHGlobal(size).ToPointer(),
                        Width = frameBmp.Width,
                        Height = frameBmp.Height,
                        Mipmaps = 1,
                        Format = PixelFormat.UncompressedR8G8B8A8
                    };

                    System.Runtime.InteropServices.Marshal.Copy(pixels, 0, (nint)rayImage.Data, size);
                    var texture = Raylib.LoadTextureFromImage(rayImage);
                    ray.SetTextureFilter(texture, TextureFilter.Point);

                    frames.Add(texture);
                    SpriteCache.RegisterTexture2D(path + frames.Count, texture);
                    frameDurations.Add(delays[i]);
                }
            }

            gif.Dispose();
            SpriteCache.RegisterSprite(this);
        }
    }
}
