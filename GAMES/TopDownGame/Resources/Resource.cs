using System;
using WinterRose.Monogame;

namespace TopDownGame.Resources;
public abstract class Resource
{
    public abstract string Name { get; }
    public abstract string Description { get; }
    public abstract Rarity Rarity { get; }
    public abstract Sprite ResourceSprite { get; set; }
    public DateTime? Timeout { get; set; }
    public int Amount { get; set; }
}
