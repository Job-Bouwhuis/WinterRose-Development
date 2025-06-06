using Microsoft.Xna.Framework;
using System;
using WinterRose;
using WinterRose.Monogame;

namespace TopDownGame.Resources;
public abstract class Resource
{
    public virtual string Name { get; protected set; } = "Unnamed item";
    public virtual string Description { get; protected set; } = "Unnamed item";
    public virtual Rarity Rarity { get; protected set; }
    [IncludeWithSerialization]
    public Sprite ResourceSprite { get; set; }
    [IncludeWithSerialization]
    public DateTime? Timeout { get; set; }
    [IncludeWithSerialization]
    public Vector2 Scale { get; set; }
    [IncludeWithSerialization]
    public int Amount { get; set; }

    public Resource Clone()
    {
        Type t = GetType();
        var clone = ActivatorExtra.CreateInstance(t) as Resource;
        clone!.Name = Name;
        clone.Description = Description;
        clone.Rarity = Rarity;
        clone.ResourceSprite = ResourceSprite;
        clone.Timeout = Timeout;
        clone.Scale = Scale;
        clone.Amount = Amount;
        ConfigureClone(clone);
        return clone;
    }

    public virtual void ConfigureClone(Resource clone) { }
}
