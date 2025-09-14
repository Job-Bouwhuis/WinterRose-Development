namespace WinterRose.ForgeWarden.DamageSystem.WeaponSystem;

public class PerShellReloadBehavior : ReloadBehavior
{
    private float timer;

    public override void StartReload()
    {
        IsReloading = true;
        timer = 0;
    }

    protected internal override void Update()
    {
        if (!IsReloading) return;
        timer -= Time.deltaTime;

        if(Magazine.AmmoReserves <= 0)
        {
            IsReloading = false;
            return;
        }

        if (timer <= 0 && Magazine.CurrentLoadedAmmo < Magazine.MaxAmmo && Magazine.AmmoReserves > 0)
        {
            Magazine.CurrentLoadedAmmo++;
            Magazine.AmmoReserves--;
            timer = Magazine.ReloadTime;
        }


        if (Magazine.CurrentLoadedAmmo >= Magazine.MaxAmmo)
            IsReloading = false;
    }
}