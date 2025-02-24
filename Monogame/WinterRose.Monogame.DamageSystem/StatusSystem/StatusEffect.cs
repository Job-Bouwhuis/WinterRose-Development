using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterRose.Monogame.StatusSystem;

/// <summary>
/// A status effect for the <see cref="StatusEffector"/>
/// </summary>
[method: DefaultArguments("New status effect")]
public abstract class StatusEffect : ICloneable
{
    /// <summary>
    /// The description of this effect
    /// </summary>
    public abstract string Description { get; }
    /// <summary>
    /// When <see cref=""/>
    /// </summary>
    public abstract StatusEffectUpdateType UpdateType { get; }

    /// <summary>
    /// The icon of this status effect. Can be null if not set.
    /// </summary>
    public Sprite Icon { get; set; }

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

    public float SecondsPerStack { get; set; } = 1f;
    [Show]
    protected internal float currentSeconds;
    private int stacks = 1;

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
        return clone;
    }
}
