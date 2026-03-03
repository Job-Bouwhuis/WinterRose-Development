using BulletSharp;
using System.Diagnostics;
using WinterRose.ForgeGuardChecks;
using WinterRose.ForgeWarden.Entities;
using WinterRose.ForgeWarden.Input;
using WinterRose.ForgeWarden.Physics;
using WinterRose.StaticValueModifiers;

namespace WinterRose.ForgeWarden.DamageSystem.WeaponSystem;

[RequireComponent<Magazine>]
public class Weapon : Component, IUpdatable
{
    [InjectFromSelf]
    public Magazine Magazine { get; set; }
    public Trigger Trigger { get; set; }

    public StaticCombinedModifier<float> Spread { get; set; } = 0;

    public List<Trigger> AvailableTriggers { get; set; } = [];
    float time = 0;

    protected override void Awake()
    {
        if (Trigger == null)
        {
            if (AvailableTriggers.Count > 0)
                Trigger = AvailableTriggers[0];
        }
    }

    public void Update()
    {
        if (Trigger is null)
        {
            log.Error("Trigger not assigned!");
            return;
        }

        Trigger.Update();

        if (Magazine.ReloadBehavior.IsReloading)
            return;

        if (Input.IsPressed("reload"))
            Magazine.StartReload();

        if (Input.IsDown("fire"))
        {
            if (Trigger.CanFire() && Magazine.CanFire())
            {
                if (Trigger.TryFire())
                {
                    var projectiles = Magazine.TakeProjectiles();

                    CreateProjectile();
                }
            }
        }
    }

    private Entity CreateProjectile()
    {
        Entity e = owner.world.CreateEntity("Projectile");
        e.AddComponent<SpriteRenderer>(Magazine.Projectile.ProjectileTexture);
        var texBounds = Magazine.Projectile.ProjectileTexture.Size;
        var col = e.AddComponent<Collider>(new Box2DShape(texBounds.X / 2, texBounds.Y / 2, 0.0001f));
        e.AddComponent<RigidBodyComponent>(col, 10);

        Projectile p = e.AddComponent<Projectile>();
        p.Stats = Magazine.Projectile;

        return e;
    }
}


