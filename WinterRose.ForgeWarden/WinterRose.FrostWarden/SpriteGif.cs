using Raylib_cs;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.PixelFormats;
using Image = SixLabors.ImageSharp.Image;

namespace WinterRose.ForgeWarden
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

            using var gif = Image.Load<Rgba32>(path);

            foreach (var frame in gif.Frames)
            {
                int delay = frame.Metadata.GetGifMetadata().FrameDelay * 10; // in ms
                frameDurations.Add(delay);

                var imageData = new byte[frame.Width * frame.Height * 4];
                frame.CopyPixelDataTo(imageData);

                unsafe
                {
                    var rayImage = new Raylib_cs.Image
                    {
                        Data = System.Runtime.InteropServices.Marshal.AllocHGlobal(imageData.Length).ToPointer(),
                        Width = frame.Width,
                        Height = frame.Height,
                        Mipmaps = 1,
                        Format = PixelFormat.UncompressedR8G8B8A8
                    };

                    System.Runtime.InteropServices.Marshal.Copy(imageData, 0, (nint)rayImage.Data, imageData.Length);
                    var texture = Raylib.LoadTextureFromImage(rayImage);
                    ray.SetTextureFilter(texture, TextureFilter.Point);

                    frames.Add(texture);
                    SpriteCache.RegisterTexture2D(path + frames.Count, texture);
                }
            }

            SpriteCache.RegisterSprite(this);
        }
    }
}
