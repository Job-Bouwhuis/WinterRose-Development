using WinterRose.StaticValueModifiers;

namespace WinterRose.ForgeWarden.DamageSystem.WeaponSystem;

public class Magazine : Component, IUpdatable
{
    public ProjectileStats Projectile { get; set; }
    public ReloadBehavior ReloadBehavior
    {
        get; set
        {
            if (value is null)
                field.Magazine = null!;
            field = value!;
            field?.Magazine = this;
        }
    }

    public StaticCombinedModifier<float> Multishot { get; set; } = 1;
    public StaticCombinedModifier<int> AmmoConsumedPerShot { get; set; } = 1;
    public StaticCombinedModifier<int> MaxAmmo { get; set; } = 25;
    public int AmmoReserves { get; set; } = 1000;
    public int CurrentLoadedAmmo { get; set; }

    public void StartReload() => ReloadBehavior.StartReload();
    public void Update() => ReloadBehavior.Update();

    public int TakeProjectiles()
    {
        int ammoCost = AmmoConsumedPerShot;

        if (CurrentLoadedAmmo < ammoCost)
            return 0;

        CurrentLoadedAmmo -= ammoCost;

        float multishotValue = Multishot;

        int guaranteedProjectiles = (int)MathF.Floor(multishotValue);
        float fractional = multishotValue - guaranteedProjectiles;

        int totalProjectiles = guaranteedProjectiles;

        if (fractional > 0f)
        {
            float roll = Random.Shared.NextSingle();
            if (roll < fractional)
                totalProjectiles++;
        }

        return totalProjectiles;
    }

    public Magazine(ProjectileStats projectile, ReloadBehavior reloadBehavior)
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
