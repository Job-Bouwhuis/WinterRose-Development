using System;

namespace WinterRose.Monogame.Weapons;

public class DefaultProjectileHitAction : ObjectComponent, IProjectileHitAction
{
    public void OnHit(Projectile bullet, WorldObject hitObject, CollisionSide hitPosition)
    {
        if (bullet.SplashRadius > 0)
        {
            HandleSplashBullet(bullet, hitPosition);
        }
        else
        {
            HandleSingleBullet(bullet, hitObject, hitPosition);
        }
    }

    private void HandleSingleBullet(Projectile bullet, WorldObject hitObject, CollisionSide hitPosition)
    {
        IHittable hittable = hitObject.FetchComponent<IHittable>();
        hittable?.OnHit(bullet, bullet.Damage);
    }

    private void HandleSplashBullet(Projectile bullet, CollisionSide hitPosition)
    {
        throw new NotImplementedException("This feature is yet to be implemented. will work on it later when i have better physics.");
    }
}