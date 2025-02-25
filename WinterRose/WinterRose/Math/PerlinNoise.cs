using System;

public class PerlinNoise
{
    public static PerlinNoise shared;

    private Random random = new();
    private readonly int[] permutation = new int[512];
    private int seed;

    static PerlinNoise()
    {
        shared = new PerlinNoise();
    }

    public PerlinNoise(int seed) => SetSeed(seed);
    public PerlinNoise() : this(Random.Shared.Next()) { }

    public void SetSeed(int newSeed)
    {
        seed = newSeed;
        random = new Random(seed);

        for (int i = 0; i < 256; i++)
        {
            permutation[i] = i;
            permutation[i + 256] = i;
        }

        // Shuffle the permutation array based on the seed
        for (int i = 0; i < permutation.Length; i++)
        {
            int j = random.Next(i, permutation.Length);
            int temp = permutation[i];
            permutation[i] = permutation[j];
            permutation[j] = temp;
        }
    }

    /// <summary>
    /// Generate 1D Perlin noise with octaves for added detail
    /// </summary>
    public float Generate(float x, int octaves = 4, float persistence = 0.5f, float frequency = 1.0f)
    {
        float total = 0;
        float amplitude = 1;
        float maxValue = 0; // Used for normalizing result

        for (int i = 0; i < octaves; i++)
        {
            float noise = Perlin(x * frequency);
            total += noise * amplitude;

            maxValue += amplitude;
            amplitude *= persistence;
            frequency *= 2;
        }

        return total / maxValue; // Normalized result
    }

    /// <summary>
    /// Generate 2D Perlin noise with octaves for added detail
    /// </summary>
    public float Generate(float x, float y, int octaves = 4, float persistence = 0.5f, float frequency = 1.0f)
    {
        float total = 0;
        float amplitude = 1;
        float maxValue = 0;

        for (int i = 0; i < octaves; i++)
        {
            float noise = Perlin(x * frequency, y * frequency);
            total += noise * amplitude;

            maxValue += amplitude;
            amplitude *= persistence;
            frequency *= 2;
        }

        return total / maxValue;
    }

    /// <summary>
    /// 1D Perlin noise base function
    /// </summary>
    private float Perlin(float x)
    {
        int X = (int)Math.Floor(x) & 255;
        x -= (float)Math.Floor(x);

        float u = Fade(x);

        int a = permutation[X];
        int b = permutation[X + 1];

        return Lerp(u, Grad(permutation[a], x), Grad(permutation[b], x - 1));
    }

    /// <summary>
    /// 2D Perlin noise base function
    /// </summary>
    private float Perlin(float x, float y)
    {
        int X = (int)Math.Floor(x) & 255;
        int Y = (int)Math.Floor(y) & 255;

        x -= (float)Math.Floor(x);
        y -= (float)Math.Floor(y);

        float u = Fade(x);
        float v = Fade(y);

        int a = permutation[X] + Y;
        int b = permutation[X + 1] + Y;

        return Lerp(v,
                    Lerp(u, Grad(permutation[a], x, y), Grad(permutation[b], x - 1, y)),
                    Lerp(u, Grad(permutation[a + 1], x, y - 1), Grad(permutation[b + 1], x - 1, y - 1)));
    }

    private float Fade(float t)
    {
        return t * t * t * (t * (t * 6 - 15) + 10);
    }

    private float Lerp(float t, float a, float b)
    {
        return a + t * (b - a);
    }

    private float Grad(int hash, float x)
    {
        int h = hash & 15;
        float grad = 1 + (h & 7); // Gradient value 1-8
        if ((h & 8) != 0)
            grad = -grad; // Randomly invert half of the gradients
        return grad * x; // Scale gradient to the given x coordinate
    }

    private float Grad(int hash, float x, float y)
    {
        int h = hash & 7; // Convert low 3 bits of hash code
        float u = (h < 4) ? x : y; // Use x or y depending on the hash
        float v = (h < 4) ? y : x; // Switch x and y based on the hash
        return ((h & 1) == 0 ? u : -u) + ((h & 2) == 0 ? v : -v); // Calculate gradient
    }
}
