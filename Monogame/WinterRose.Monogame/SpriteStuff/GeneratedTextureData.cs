using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace WinterRose.Monogame;

/// <summary>
/// A data class that holds some information about a runtime created texture. Used for serialization
/// </summary>
public class GeneratedTextureData
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

    internal static GeneratedTextureData Create(Color color, int width, int height)
    {
        GeneratedTextureData data = new GeneratedTextureData
        {
            Name = $"Generated_{color}_{width}x{height}",
            Width = width,
            Height = height,
            Pixels = new uint[width * height]
        };

        uint packedColor = (uint)color.PackedValue;
        for (int i = 0; i < data.Pixels.Length; i++)
        {
            data.Pixels[i] = packedColor;
        }

        return data;
    }

}
