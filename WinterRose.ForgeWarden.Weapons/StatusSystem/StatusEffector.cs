using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.ForgeSignal;
using WinterRose.ForgeWarden.Entities;

namespace WinterRose.ForgeWarden.StatusSystem;

public class StatusEffector : Component, IUpdatable
{
    private readonly List<StatusEffect> effects = new();

    public void Apply(StatusEffect effect, int initialStacks = 1)
    {
        var existing = effects.FirstOrDefault(e => e.Name == effect.Name);
        if (existing != null)
        {
            existing.Apply(this, initialStacks);
        }
        else
        {
            var clone = effect.Clone();
            clone.Apply(this, initialStacks);
            effects.Add(clone);
        }
    }

    public void Update()
    {
        for (int i = effects.Count - 1; i >= 0; i--)
        {
            var effect = effects[i];
            effect.Update(this);
            if (effect.Stacks <= 0)
                effects.RemoveAt(i);
        }
    }

    public bool HasEffect(string name) => effects.Any(e => e.Name == name);

    public T? GetEffect<T>() where T : StatusEffect => effects.OfType<T>().FirstOrDefault();
}

