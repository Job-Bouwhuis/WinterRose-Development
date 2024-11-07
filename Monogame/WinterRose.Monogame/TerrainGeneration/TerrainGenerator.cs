using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.Marshalling;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WinterRose.Monogame.TerrainGeneration;

public class TerrainGenerator
{
    private GraphicsDevice graphicsDevice; // Graphics device for creating textures
    private Dictionary<float, Color> colorMapping; // Manual color mapping

    public TerrainGenerator(GraphicsDevice graphicsDevice)
    {
        this.graphicsDevice = graphicsDevice;
        colorMapping = new Dictionary<float, Color>
        {
            { 1, new Color(255, 255, 255) }, // White for Snow
            { 0.8f, new Color(100, 100, 100) }, // Gray for Stone
            { 0.6f, new Color(139, 69, 19) }, // Brown for dirt
            { 0f, new Color(34, 139, 34) }, // Green for grass
            { -0.3f, new Color(194, 178, 128) }, // Sand
            { -1f, new Color(135, 206, 235) } // Blue for water
        };
    }

    public Texture2D CreateMasterSprite(int width, int height, float frequency, float bias, float colorNoiseIntensity)
    {
        Texture2D masterSprite = new Texture2D(graphicsDevice, width, height);
        Color[] textureData = new Color[width * height];
        PerlinNoise noise = new();
        PerlinNoise colorNoise = new();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float noiseValue = noise.Generate(x * frequency, y * frequency, 10, 1, frequency) * bias;
                Color baseColor = GetColorFromNoiseValue(noiseValue);

                float colorNoiseValue = colorNoise.Generate(x * colorNoiseIntensity, y * colorNoiseIntensity);
                Color finalColor = ApplyColorOffset(baseColor, colorNoiseValue);

                textureData[y * width + x] = finalColor;
            }
        }

        masterSprite.SetData(textureData);
        return masterSprite;
    }

    public Texture2D CreateSubSprite(int width, int height, float frequency, float bias, float colorNoiseIntensity, int offsetX, int offsetY, int seed)
    {
        Texture2D masterSprite = new Texture2D(graphicsDevice, width, height);
        Color[] textureData = new Color[width * height];
        PerlinNoise noise = new(seed);
        PerlinNoise colorNoise = new(seed);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float noiseValue = noise.Generate((x + offsetX) * frequency, (y + offsetY) * frequency, 10, 1f, frequency) * bias;
                Color baseColor = GetColorFromNoiseValue(noiseValue);

                float colorNoiseValue = colorNoise.Generate((x + offsetX) * colorNoiseIntensity, (y + offsetY) * colorNoiseIntensity);
                Color finalColor = ApplyColorOffset(baseColor, colorNoiseValue);

                textureData[y * width + x] = finalColor;
            }
        }

        masterSprite.SetData(textureData);
        return masterSprite;
    }

    public Texture2D CreateSubSpriteGrayScale(int width, int height, float frequency, float bias, int offsetX, int offsetY, int seed)
    {
        Texture2D masterSprite = new Texture2D(graphicsDevice, width, height);
        Color[] textureData = new Color[width * height];
        PerlinNoise noise = new(seed); 

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float noiseValue = noise.Generate((x + offsetX) * frequency, (y + offsetY) * frequency, 10, 0.5f, frequency) * bias;

                byte grayValue = (byte)Math.Clamp(noiseValue * 255, 0, 255);
                textureData[y * width + x] = new Color(grayValue, grayValue, grayValue);
            }
        }

        masterSprite.SetData(textureData);
        return masterSprite;
    }

    private Color ApplyColorOffset(Color baseColor, float noiseValue)
    {
        noiseValue = (noiseValue - 0.5f) * 0.2f;
        return new Color(
            Math.Clamp(baseColor.R + (int)(noiseValue * 255), 0, 255),
            Math.Clamp(baseColor.G + (int)(noiseValue * 255), 0, 255),
            Math.Clamp(baseColor.B + (int)(noiseValue * 255), 0, 255)
        );
    }


    private Color GetColorFromNoiseValue(float value)
    {
        float closestValue = float.MaxValue;
        Color closestColor = Color.White;

        foreach (var kvp in colorMapping)
        {
            float key = kvp.Key;
            Color color = kvp.Value;

            if (Math.Abs(key - value) < Math.Abs(closestValue - value))
            {
                closestValue = key;
                closestColor = color; 
            }
        }

        return closestColor; 
    }

    public void AddColorMapping(float value, Color color)
    {
        if (!colorMapping.ContainsKey(value))
        {
            colorMapping.Add(value, color);
        }
    }
}

