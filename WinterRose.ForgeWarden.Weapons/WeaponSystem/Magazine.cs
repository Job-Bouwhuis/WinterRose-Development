using WinterRose.StaticValueModifiers;

namespace WinterRose.ForgeWarden.DamageSystem.WeaponSystem;

public class Magazine : Component, IUpdatable
{
    public Projectile Projectile { get; set; }
    public ReloadBehavior ReloadBehavior { get; set; }

    public StaticCombinedModifier<float> Multishot { get; set; } = 1;
    public StaticCombinedModifier<int> AmmoConsumedPerShot { get; set; } = 1;
    public StaticCombinedModifier<int> MaxAmmo { get; set; } = 25;
    public int AmmoReserves { get; set; } = 1000;
    public int CurrentLoadedAmmo { get; set; }
    public StaticCombinedModifier<float> ReloadTime { get; set; } = 2;

    public void StartReload() => ReloadBehavior.StartReload();
    public void Update() => ReloadBehavior.Update();

    public IReadOnlyList<Projectile> TakeProjectiles()
    {
        CurrentLoadedAmmo -= AmmoConsumedPerShot;
        return new List<Projectile>();
    }

    public Magazine(Projectile projectile, ReloadBehavior reloadBehavior)
    {
        Projectile = projectile;
        ReloadBehavior = reloadBehavior;
        reloadBehavior.Magazine = this;
    }

    private Magazine() { } // for serialization

    public void ConsumeAmmo(int amount)
    {
        AmmoReserves -= amount;
        if(AmmoReserves < 0)
            AmmoReserves = 0;
    }

    public virtual bool CanFire() => CurrentLoadedAmmo >= AmmoConsumedPerShot;
}
