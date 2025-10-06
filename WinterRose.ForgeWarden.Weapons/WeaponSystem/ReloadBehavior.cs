using WinterRose.StaticValueModifiers;

namespace WinterRose.ForgeWarden.DamageSystem.WeaponSystem;

public abstract class ReloadBehavior
{
    public bool IsReloading { get; protected set; }
    public virtual float ReloadProgress => IsReloading ? 1f - (timer / ReloadTime) : 1f;
    public StaticCombinedModifier<float> ReloadTime { get; set; } = 2;

    protected float timer = 0;
    protected internal Magazine Magazine { get; internal set; }

    public abstract void StartReload();
    internal protected abstract void Update();

    protected void ReloadImmediately()
    {
        int needed = Magazine.MaxAmmo - Magazine.CurrentLoadedAmmo;
        int load = Math.Min(needed, Magazine.AmmoReserves);
        Magazine.CurrentLoadedAmmo += load;
        Magazine.AmmoReserves -= load;
        IsReloading = false;
    }
}
