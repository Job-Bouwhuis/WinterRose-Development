using Raylib_cs;
using System.Numerics;

namespace WinterRose.FrostWarden
{
    public class Sprite : IDisposable
    {
        public string Source { get; private set; }

        public virtual Texture2D Texture { get; private set; }
        public virtual Vector2 Size => new Vector2(Texture.Width, Texture.Height);

        public int Width => (int)Size.X;
        public int Height => (int)Size.Y;

        private bool ownsTexture;

        public Sprite(string filePath)
        {
            Texture = SpriteCache.Get(filePath);
            Source = filePath;
        }

        public Sprite(Texture2D texture, bool ownsTexture)
        {
            Texture = texture;
            this.ownsTexture = ownsTexture;
        }

        protected Sprite() { }

        // Factory method for a filled rectangle sprite
        public static Sprite CreateRectangle(int width, int height, Color fillColor)
        {
            Image img = Raylib.GenImageColor(width, height, fillColor);
            Texture2D tex = Raylib.LoadTextureFromImage(img);
            Raylib.UnloadImage(img);
            var sprite = new Sprite(tex, true);
            sprite.Source = $"Generated_{width}_{height}_{fillColor.R}{fillColor.G}{fillColor.B}{fillColor.A}";
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

            var sprite = new Sprite(tex, ownsTexture: true);
            sprite.Source = $"Generated_Circle_{diameter}_{fillColor.R}{fillColor.G}{fillColor.B}{fillColor.A}";
            return sprite;
        }

        private static Color ColorAlpha(byte alpha) => new Color(0, 0, 0, (int)alpha);

        public void Draw(Vector2 position, float rotation = 0f, float scale = 1f, Color? tint = null)
        {
            var color = tint ?? Color.White;
            Raylib.DrawTextureEx(Texture, position, rotation, scale, color);
        }

        public virtual void Dispose()
        {
            if (ownsTexture && Texture.Id != 0)
            {
                Raylib.UnloadTexture(Texture);
            }
        }

        public static implicit operator Texture2D(Sprite s) => s.Texture;
    }
}
