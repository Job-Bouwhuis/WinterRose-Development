namespace WinterRose.ForgeWarden.DamageSystem.WeaponSystem;

public class FullReloadBehavior : ReloadBehavior
{
    private float timer;

    public override void StartReload()
    {
        IsReloading = true;
        timer = Magazine.ReloadTime;
    }

    protected internal override void Update()
    {
        if (!IsReloading) return;
        timer -= Time.deltaTime;
        if (timer <= 0)
            ReloadImmediately();
    }
}
