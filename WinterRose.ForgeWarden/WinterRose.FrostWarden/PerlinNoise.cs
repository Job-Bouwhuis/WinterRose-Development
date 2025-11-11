using System;


namespace WinterRose.ForgeWarden;

public class PerlinNoise
{
    private readonly int seed;
    private readonly float scale;
    private readonly int octaves;
    private readonly float persistence;
    private readonly float lacunarity;

    private readonly Random rng;
    private readonly int[] permutation;

    public PerlinNoise(int seed, float scale = 1, int octaves = 1, float persistence = 0.5f, float lacunarity = 2)
    {
        this.seed = seed;
        this.scale = scale;
        this.octaves = octaves;
        this.persistence = persistence;
        this.lacunarity = lacunarity;

        rng = new Random(seed);
        permutation = new int[512];
        int[] p = new int[256];
        for (int i = 0; i < 256; i++) p[i] = i;
        for (int i = 0; i < 256; i++)
        {
            int swapIndex = rng.Next(256);
            (p[i], p[swapIndex]) = (p[swapIndex], p[i]);
        }
        for (int i = 0; i < 512; i++) permutation[i] = p[i & 255];
    }

    public float GetValue(float x, float y)
    {
        float amplitude = 1;
        float frequency = 1;
        float noiseHeight = 0;

        for (int i = 0; i < octaves; i++)
        {
            float sampleX = x / scale * frequency;
            float sampleY = y / scale * frequency;

            float perlinValue = Generate(sampleX, sampleY);
            noiseHeight += perlinValue * amplitude;

            amplitude *= persistence;
            frequency *= lacunarity;
        }

        return (noiseHeight + 1f) * 0.5f;
    }

    private float Generate(float x, float y)
    {
        int xi = (int)MathF.Floor(x) & 255;
        int yi = (int)MathF.Floor(y) & 255;

        float xf = x - MathF.Floor(x);
        float yf = y - MathF.Floor(y);

        float u = Fade(xf);
        float v = Fade(yf);

        int aa = permutation[permutation[xi] + yi];
        int ab = permutation[permutation[xi] + yi + 1];
        int ba = permutation[permutation[xi + 1] + yi];
        int bb = permutation[permutation[xi + 1] + yi + 1];

        float x1 = Lerp(Grad(aa, xf, yf), Grad(ba, xf - 1, yf), u);
        float x2 = Lerp(Grad(ab, xf, yf - 1), Grad(bb, xf - 1, yf - 1), u);

        return Lerp(x1, x2, v);
    }

    private static float Fade(float t) => t * t * t * (t * (t * 6 - 15) + 10);
    private static float Lerp(float a, float b, float t) => a + t * (b - a);
    private static float Grad(int hash, float x, float y)
    {
        int h = hash & 3;
        float u = h switch
        {
            0 => x,
            1 => -x,
            2 => y,
            _ => -y
        };
        float v = (h < 2) ? y : x;
        return (h & 1) == 0 ? u + v : u - v;
    }

    public PerlinNoise Combine(PerlinNoise other, Func<float, float, float> combineFunc)
    {
        return new PerlinNoise(seed + other.seed, scale, octaves, persistence, lacunarity)
        {
            combinedA = this,
            combinedB = other,
            combineFunc = combineFunc
        };
    }

    private PerlinNoise combinedA;
    private PerlinNoise combinedB;
    private Func<float, float, float> combineFunc;

    public float Get(float x, float y)
    {
        if (combineFunc == null) return GetValue(x, y);
        return combineFunc(combinedA.Get(x, y), combinedB.Get(x, y));
    }
}