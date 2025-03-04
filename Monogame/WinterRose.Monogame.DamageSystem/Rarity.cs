using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Security.Policy;

namespace WinterRose.Monogame;

public sealed class Rarity
{
    public int Level { get; private set; }
    public string Name { get; private set; }
    public Color Color { get; private set; }

    private Rarity() { }

    private static Dictionary<int, Rarity> RarityList { get; } = [];

    public static Rarity CreateRarity(int level, string name, Color color)
    {
        if (RarityList.TryGetValue(level, out Rarity value))
            return value;

        var newRarity = new Rarity()
        {
            Name = name,
            Level = level,
            Color = color
        };

        RarityList[level] = newRarity;
        return newRarity;
    }

    public static Rarity GetRarity(int level) => RarityList[level];

}