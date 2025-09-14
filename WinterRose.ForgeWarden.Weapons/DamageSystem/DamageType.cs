using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinterRose.ForgeWarden.HealthSystem;

namespace WinterRose.ForgeWarden.DamageSystem;
public abstract class DamageType
{
    /// <summary>
    /// Human-readable name for UI, debugging, etc.
    /// </summary>
    public abstract string Name { get; }

    /// <summary>
    /// Called when dealing damage to a target.
    /// </summary>
    public abstract void DealDamage(Vitality target, float amount);

    /// <summary>
    /// Optional: Apply any custom logic after damage (e.g., triggers, visual hooks)
    /// </summary>
    public virtual void OnDamageDealt(Vitality target, float amount) { }
}

