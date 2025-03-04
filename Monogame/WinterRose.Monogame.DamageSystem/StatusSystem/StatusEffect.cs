using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.Serialization;

namespace WinterRose.Monogame.StatusSystem;

/// <summary>
/// A status effect for the <see cref="StatusEffector"/>
/// </summary>
[method: DefaultArguments("New status effect")]
public abstract class StatusEffect() : ICloneable
{
    /// <summary>
    /// The description of this effect
    /// </summary>
    [IncludeWithSerialization]
    public abstract string Description { get; }
    /// <summary>
    /// When <see cref=""/>
    /// </summary>
    [IncludeWithSerialization]
    public abstract StatusEffectUpdateType UpdateType { get; }

    [IncludeWithSerialization]
    public abstract StatusEffectType EffectType { get; }

    /// <summary>
    /// The icon of this status effect. Can be null if not set.
    /// </summary>
    [IncludeWithSerialization]
    public Sprite Icon { get; set; }

    [IncludeWithSerialization]
    public int MaxStacks { get; set; } = 10;
    public int Stacks
    {
        get => stacks; 
        set
        {
            previousStacks = stacks;
            stacks = value;
            updateStacks = true;
        }
    }
    internal int previousStacks = 0;
    internal bool updateStacks = false;

    [IncludeWithSerialization]
    public float SecondsPerStack { get; set; } = 1f;
    [Show]
    protected internal float currentSeconds;
    private int stacks = 1;

    [IncludeWithSerialization]
    public bool RemoveAllStacksOntimeout { get; set; } = false;

    protected internal virtual void Update(StatusEffector effector) { }
    protected internal virtual void StacksUpdated(StatusEffector effector, int lastStacks, int currentStacks) { }

    public virtual object Clone()
    {
        var clone =  (StatusEffect)MemberwiseClone();
        clone.currentSeconds = 0;
        clone.previousStacks = 0;
        clone.updateStacks = false;
        clone.stacks = stacks;
        clone.MaxStacks = MaxStacks;
        clone.RemoveAllStacksOntimeout = RemoveAllStacksOntimeout;
        clone.SecondsPerStack = SecondsPerStack;
        return clone;
    }
}
