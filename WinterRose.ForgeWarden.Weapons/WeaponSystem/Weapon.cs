using System.Diagnostics;
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
    float time = 0;
    public void Update()
    {
        Trigger?.Update();
        //Console.WriteLine($"kb: {Input.HasKeyboardFocus}, mouse {Input.HasMouseFocus}");

        if (Magazine.ReloadBehavior.IsReloading)
            return;

        if(Input.IsPressed("reload"))
            Magazine.StartReload();

        if(Input.IsDown("fire"))
        {
            Console.WriteLine("fire" + time++);
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