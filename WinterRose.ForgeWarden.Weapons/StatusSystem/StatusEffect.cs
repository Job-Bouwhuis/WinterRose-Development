using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.StaticValueModifiers;

namespace WinterRose.ForgeWarden.StatusSystem;
public abstract class StatusEffect
{
    public abstract string Name { get; }

    public StaticCombinedModifier<int> MaxStacks { get; protected set; } = 10;
    private int stacks = 1;
    public int Stacks
    {
        get => stacks;
        set
        {
            int previous = stacks;
            stacks = Math.Clamp(value, 0, MaxStacks);
            OnStacksUpdated(previous, stacks);
        }
    }

    public StaticCombinedModifier<float> SecondsPerStack { get; protected set; } = -1f;
    public bool RemoveAllStacksOnTimeout { get; protected set; } = false;

    protected float currentSeconds = 0f;

    /// <summary>
    /// Called every update tick by the effector
    /// </summary>
    public void Update(StatusEffector effector)
    {
        if (!Validate(effector) || SecondsPerStack < 0 || Stacks <= 0)
            return;

        currentSeconds += Time.deltaTime;

        if (currentSeconds >= SecondsPerStack)
        {
            if (RemoveAllStacksOnTimeout)
                Stacks = 0;
            else
                Stacks--;

            currentSeconds = 0;

            if (Stacks > 0)
                OnUpdate(effector);
        }
    }

    /// <summary>Check if this effect is valid on the given effector.</summary>
    public abstract bool Validate(StatusEffector effector);

    protected virtual void OnStacksUpdated(int previousStacks, int newStacks) { }

    protected abstract void OnUpdate(StatusEffector effector);

    public virtual void Apply(StatusEffector effector, int initialStacks = 1)
    {
        if (!Validate(effector))
            return;

        Stacks += initialStacks;
        OnApply(effector);
    }

    protected virtual void OnApply(StatusEffector effector) { }

    public abstract StatusEffect Clone();
}

