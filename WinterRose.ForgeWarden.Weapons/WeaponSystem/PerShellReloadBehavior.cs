namespace WinterRose.ForgeWarden.DamageSystem.WeaponSystem;

public class PerShellReloadBehavior : ReloadBehavior
{
    private float timer;
    public bool UsePerShellTime { get; set; } = false; // false = total reload time distributed, true = per-shell time

    public override void StartReload()
    {
        IsReloading = true;
        timer = UsePerShellTime
            ? Magazine.ReloadTime
            : Magazine.ReloadTime / Magazine.MaxAmmo;
    }

    protected internal override void Update()
    {
        if (!IsReloading) return;
        timer -= Time.deltaTime;

        if (Magazine.AmmoReserves <= 0)
        {
            IsReloading = false;
            return;
        }

        if (timer <= 0 && Magazine.CurrentLoadedAmmo < Magazine.MaxAmmo && Magazine.AmmoReserves > 0)
        {
            Magazine.CurrentLoadedAmmo++;
            Magazine.ConsumeAmmo(1);

            timer = UsePerShellTime
                ? Magazine.ReloadTime
                : Magazine.ReloadTime / Magazine.MaxAmmo;
        }

        if (Magazine.CurrentLoadedAmmo >= Magazine.MaxAmmo)
            IsReloading = false;
    }
}