using Raylib_cs;
using System.Numerics;

namespace WinterRose.ForgeWarden
{
    public class Sprite : IDisposable
    {
        public string Source { get; internal protected set; }

        public virtual Texture2D Texture { get; protected set; }
        public virtual Vector2 Size => new Vector2(Texture.Width, Texture.Height);

        public int Width => (int)Size.X;
        public int Height => (int)Size.Y;

        protected internal bool OwnsTexture { get; protected set; } = false;

        public Sprite(string filePath)
        {
            Texture = SpriteCache.Get(filePath);
            Source = filePath;
        }

        public Sprite(string filePath, bool ownsTexture)
        {
            Texture = ray.LoadTexture(filePath);
            this.OwnsTexture = ownsTexture;
            Source = filePath;
        }

        public Sprite(Texture2D texture, bool ownsTexture)
        {
            Texture = texture;
            this.OwnsTexture = ownsTexture;
        }

        protected Sprite() { }

        // Factory method for a filled rectangle sprite
        public static Sprite CreateRectangle(int width, int height, Color fillColor)
        {
            Image img = Raylib.GenImageColor(width, height, fillColor);
            Texture2D tex = Raylib.LoadTextureFromImage(img);
            Raylib.UnloadImage(img);
            var sprite = new Sprite(tex, false);
            sprite.Source = $"Generated_{width}_{height}_{fillColor.R}{fillColor.G}{fillColor.B}{fillColor.A}";
            SpriteCache.RegisterSprite(sprite);
            return sprite;
        }

        // Factory method for a filled circle sprite
        public static Sprite CreateCircle(int diameter, Color fillColor)
        {
            Image img = Raylib.GenImageColor(diameter, diameter, ColorAlpha(0)); // transparent background

            // Draw the circle on the image manually:
            int radius = diameter / 2;
            for (int y = 0; y < diameter; y++)
            {
                for (int x = 0; x < diameter; x++)
                {
                    int dx = x - radius;
                    int dy = y - radius;
                    if (dx * dx + dy * dy <= radius * radius)
                    {
                        Raylib.ImageDrawPixel(ref img, x, y, fillColor);
                    }
                }
            }

            Texture2D tex = Raylib.LoadTextureFromImage(img);
            Raylib.UnloadImage(img);

            var sprite = new Sprite(tex, false);
            sprite.Source = $"Generated_Circle_{diameter}_{fillColor.R}{fillColor.G}{fillColor.B}{fillColor.A}";
            SpriteCache.RegisterSprite(sprite);
            return sprite;
        }

        private static Color ColorAlpha(byte alpha) => new Color(0, 0, 0, (int)alpha);

        public virtual void Dispose()
        {
            if (OwnsTexture && Texture.Id != 0)
            {
                Raylib.UnloadTexture(Texture);
            }
        }

        public static implicit operator Texture2D(Sprite s) => s.Texture;
    }
}
