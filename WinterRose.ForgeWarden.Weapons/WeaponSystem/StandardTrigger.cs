namespace WinterRose.ForgeWarden.DamageSystem.WeaponSystem;

public class StandardTrigger : Trigger
{
    float current = 0;

    public override bool CanFire()
    {
        return current == 0;
    }
    public override bool TryFire()
    {
        if (current <= 0)
        {
            current = FireRate; // lock firing until cooldown is over
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
