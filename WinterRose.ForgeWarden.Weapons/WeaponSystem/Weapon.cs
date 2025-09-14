using WinterRose.ForgeGuardChecks;
using WinterRose.ForgeWarden.Input;

namespace WinterRose.ForgeWarden.DamageSystem.WeaponSystem;

[RequireComponent<Magazine>]
public class Weapon : Component, IUpdatable
{
    [InjectFromSelf]
    public Magazine Magazine { get; set; }

    [InjectFromSelf(ThrowWhenAbsent = false)]
    public Trigger Trigger { get; set; }

    public List<Trigger> AvailableTriggers { get; set; } = [];

    public InputBinding FireInput { get; set; }
    public InputBinding ReloadInput { get; set; }
    public InputBinding ChangeTriggerModeInput { get; set; }

    public void Update()
    {
        Trigger?.Update();

        if (Magazine.ReloadBehavior.IsReloading)
            return;

        Magazine.StartReload();

        if(Input.IsDown("fire"))
        {
            if(Trigger.CanFire())
            {
                if(Trigger.TryFire())
                {
                    var projectiles = Magazine.TakeProjectiles();
                }
            }
        }
    }
}