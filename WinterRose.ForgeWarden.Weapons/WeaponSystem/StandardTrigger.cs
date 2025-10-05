namespace WinterRose.ForgeWarden.DamageSystem.WeaponSystem;

public class StandardTrigger : Trigger
{
    float current = 0;

    public override bool CanFire()
    {
        return current <= 0;
    }

    public override bool TryFire()
    {
        if (current <= 0)
        {
            current = FireRate > 0 ? 1f / FireRate : 0f;
            return true;
        }
        return false;
    }

    public override void Update()
    {
        if (current > 0)
            current -= Time.deltaTime;
    }
}
