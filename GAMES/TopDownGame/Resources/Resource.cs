using Microsoft.Xna.Framework;
using System;
using TopDownGame.Inventories;
using WinterRose;
using WinterRose.Monogame;

namespace TopDownGame.Resources;
public abstract class Resource
{
    public abstract string Name { get; }
    public abstract string Description { get; }
    public abstract Rarity Rarity { get; }
    [IncludeWithSerialization]
    public Sprite ResourceSprite { get; set; }
    [IncludeWithSerialization]
    public DateTime Timeout { get; set; } = DateTime.MinValue;
    [IncludeWithSerialization]
    public Vector2 Scale { get; set; }
    [IncludeWithSerialization]
    public int Amount { get; set; }

    public Resource Clone()
    {
        Type t = GetType();
        var clone = ActivatorExtra.CreateInstance(t) as Resource;
        ConfigureClone(clone);
        return clone;
    }

    public abstract void ConfigureClone(Resource clone);
}
