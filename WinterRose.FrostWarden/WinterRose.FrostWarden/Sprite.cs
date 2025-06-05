using Raylib_cs;
using System.Numerics;

namespace WinterRose.FrostWarden
{
    public class Sprite : IDisposable
    {
        public Texture2D Texture { get; private set; }
        public Vector2 Size => new Vector2(Texture.Width, Texture.Height);

        public int Width => (int)Size.X;
        public int Height => (int)Size.Y;

        private bool ownsTexture;

        public Sprite(Vector2 size, Color fillColor) : this((int)size.X, (int)size.Y, fillColor)
        { }

        public Sprite(int width, int height, Color fillColor)
        {
            Image img = Raylib.GenImageColor(width, height, fillColor);
            Texture2D tex = Raylib.LoadTextureFromImage(img);
            Raylib.UnloadImage(img);
            Texture = tex;
            this.ownsTexture = true;
        }

        public Sprite(string filePath)
        {
            Texture = Raylib.LoadTexture(filePath);
            ownsTexture = true;
        }

        // Use an existing Texture2D (generated or shared)
        public Sprite(Texture2D texture, bool ownsTexture = false)
        {
            Texture = texture;
            this.ownsTexture = ownsTexture;
        }

        public void Draw(Vector2 position, float rotation = 0f, float scale = 1f, Color? tint = null)
        {
            var color = tint ?? Color.White;
            Raylib.DrawTextureEx(Texture, position, rotation, scale, color);
        }

        public void Dispose()
        {
            if (ownsTexture && Texture.Id != 0)
            {
                Raylib.UnloadTexture(Texture);
            }
        }
    }
}
