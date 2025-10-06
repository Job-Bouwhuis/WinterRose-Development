namespace WinterRose.ForgeWarden.DamageSystem.WeaponSystem;

public class StandardTrigger : Trigger
{
    public override bool CanFire()
    {
        return timer <= 0;
    }

    public override bool TryFire()
    {
        if (timer <= 0)
        {
            timer = FireRate > 0 ? 1f / FireRate : 0f;
            return true;
        }
        return false;
    }

    public override void Update()
    {
        if (timer > 0)
            timer -= Time.deltaTime;
    }
}
