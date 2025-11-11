using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.ForgeWarden.TileMaps;
public static class BiomeRegistry
{
    private static readonly Dictionary<Biome, float> Biomes = new();

    public static void AddBiome(Biome biome, float weight)
    {
        if (weight <= 0)
            throw new ArgumentException("Biome weight must be positive.", nameof(weight));

        Biomes[biome] = weight;
    }

    public static Biome Get(float rng)
    {
        if (Biomes.Count == 0)
            throw new InvalidOperationException("No biomes registered.");

        float totalWeight = 0;
        foreach (var kv in Biomes)
            totalWeight += kv.Value;

        rng = Math.Clamp(rng, 0f, 1f);

        float target = rng * totalWeight;
        float cumulative = 0;

        foreach (var kv in Biomes)
        {
            cumulative += kv.Value;
            if (target <= cumulative)
                return kv.Key;
        }

        return Biomes.Keys.Last();
    }
}
