using System;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using static WinterRose.Monogame.Sprite;

namespace WinterRose.Monogame;

public static class NoiseGenerator
{
    public static Texture2D GenerateNoiseTexture(GraphicsDevice graphicsDevice, NoiseType noiseType, int width, int height, float frequency, float amplitude, float offsetX, float offsetY)
    {
        if (width <= 0 || height <= 0)
        {
            throw new ArgumentException("Width and height must be positive values.");
        }

        Color[] colorData = new Color[width * height];

        switch (noiseType)
        {
            case NoiseType.Perlin:
                GeneratePerlinNoise(colorData, width, height, frequency, amplitude, offsetX, offsetY);
                break;

            case NoiseType.PerlinWithAlpha:
                GeneratePerlinNoiseWithAlpha(colorData, width, height, frequency, amplitude, offsetX, offsetY);
                break;

            case NoiseType.Random:
                GenerateRandomNoise(colorData, width, height, amplitude);
                break;

            default:
                throw new ArgumentException("Unsupported noise type.");
        }

        // Create a Texture2D from color data
        Texture2D noiseTexture = new Texture2D(graphicsDevice, width, height);
        noiseTexture.SetData(colorData);

        return noiseTexture;
    }

    private static void GeneratePerlinNoise(Color[] colorData, int width, int height, float frequency, float amplitude, float offsetX, float offsetY)
    {
        float scale = 1.0f / (frequency * 1f);

        float scaleX = 1.0f / frequency;
        float scaleY = 1.0f / (frequency * 0.8f);

        PerlinNoise noise = new();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float xCoord = (x + offsetX) * scaleX;
                float yCoord = (y + offsetY) * scaleY;
                // Generate Perlin noise value and normalize to [0, 1]
                float perlinValue = (noise.Generate(xCoord + yCoord, yCoord - xCoord, 4, 0.5f, 1) + 1) / 2;

                // Scale perlinValue to a grayscale value between 0 and 255
                int grayscaleValue = (int)(perlinValue * 255); // 0 to 255 range
                grayscaleValue = Math.Clamp(grayscaleValue, 0, 255); // Ensure it's in the range 0-255

                // Set pixel color based on the grayscale value
                int index = y * width + x; // Convert (x, y) to 1D index
                colorData[index] = new Color(grayscaleValue, grayscaleValue, grayscaleValue, 255); // Grayscale color with full alpha
            }
        }
    }

    private static void GeneratePerlinNoiseWithAlpha(Color[] colorData, int width, int height, float frequency, float amplitude, float offsetX, float offsetY)
    {
        float scale = 1.0f / frequency;

        for (int y = 0; y < height; y++)
        {
            PerlinNoise noise = new();
            for (int x = 0; x < width; x++)
            {
                // Generate Perlin noise value for the color and normalize to [0, 1]
                float perlinValue = (noise.Generate((x + offsetX) * scale, (y + offsetY) * scale) + 1) / 2;

                // Generate Perlin noise value for the alpha and normalize to [0, 1]
                float perlinAlphaValue = (noise.Generate((x + offsetX + 100) * scale, (y + offsetY + 100) * scale) + 1) / 2;

                // Scale color perlin value to grayscale (0 to 255)
                int value = (int)(perlinValue * amplitude * 255);
                value = Math.Clamp(value, 0, 255);

                // Scale alpha perlin value (0 to 255)
                int alpha = (int)(perlinAlphaValue * 255); // Can add an alpha amplitude in the future
                alpha = Math.Clamp(alpha, 0, 255);

                // Set pixel color based on grayscale and perlin alpha
                int index = y * width + x;
                colorData[index] = new Color(value, value, value, alpha); // Grayscale color with Perlin noise alpha
            }
        }
    }

    private static void GenerateRandomNoise(Color[] colorData, int width, int height, float amplitude)
    {
        // Example: Using random noise in MonoGame
        Random random = new Random();

        for (int i = 0; i < width * height; i++)
        {
            int grayscaleValue = random.Next(0, (int)(amplitude * 255));

            // Set pixel color based on the grayscale value
            colorData[i] = new Color(grayscaleValue, grayscaleValue, grayscaleValue);
        }
    }

}
