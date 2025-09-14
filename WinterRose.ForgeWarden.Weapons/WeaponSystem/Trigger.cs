using WinterRose.StaticValueModifiers;

namespace WinterRose.ForgeWarden.DamageSystem.WeaponSystem;

public abstract class Trigger
{
    public StaticCombinedModifier<float> FireRate { get; set; } = 0.5f;

    public abstract bool CanFire();
    public abstract void Update();
    public abstract bool TryFire();
}
