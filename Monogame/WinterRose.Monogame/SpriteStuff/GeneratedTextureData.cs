using Microsoft.Xna.Framework.Graphics;

namespace WinterRose.Monogame;

internal class GeneratedTextureData
{
    public string Name;
    public int Width;
    public int Height;
    public uint[] Pixels;

    public GeneratedTextureData(Texture2D texture)
    {
        Name = texture.Name;
        Width = texture.Width;
        Height = texture.Height;
        Pixels = new uint[Width * Height];
        texture.GetData(Pixels);
    }

    public GeneratedTextureData()
    {
        Name = "";
        Width = 0;
        Height = 0;
        Pixels = new uint[0];
    }

    public Texture2D MakeTexture()
    {
        return MonoUtils.CreateTexture(Width, Height, Pixels);
    }
}
