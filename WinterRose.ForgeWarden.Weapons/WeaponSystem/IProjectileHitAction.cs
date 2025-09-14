using System.Numerics;

namespace WinterRose.ForgeWarden.DamageSystem.WeaponSystem;

public interface IProjectileHitAction
{
    void OnProjectileHit(Vector2 point);
}
