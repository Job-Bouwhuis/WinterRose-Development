using WinterRose.StaticValueModifiers;

namespace WinterRose.ForgeWarden.DamageSystem.WeaponSystem;

public abstract class Trigger
{
    protected float timer = 0;
    /// <summary>
    /// FireRate in shots per second. eg at 1 shoots 1 shot per second
    /// </summary>
    public StaticCombinedModifier<float> FireRate { get; set; } = 0.5f;

    /// <summary>
    /// Returns true when the 
    /// </summary>
    /// <returns></returns>
    public abstract bool CanFire();
    public abstract void Update();
    public abstract bool TryFire();
}
