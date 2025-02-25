using System;
using WinterRose.Monogame.StatusSystem;

namespace WinterRose.Monogame.Weapons;

public class DefaultProjectileHitAction : ObjectComponent, IProjectileHitAction
{
    public void OnHit(Projectile bullet, WorldObject hitObject, CollisionSide hitPosition)
    {
        // bullets always hit something, so always do this
        HandleSingleBullet(bullet, hitObject, hitPosition);

        // but only if this bullet has an explosion/splash radius, do we calculate the splash damage
        if (bullet.SplashRadius > 0)
        {
            HandleSplashBullet(bullet, hitPosition);
        }
    }

    private void HandleSingleBullet(Projectile bullet, WorldObject hitObject, CollisionSide hitPosition)
    {
        IHittable[] hittable = hitObject.FetchComponents<IHittable>();
        hittable.Foreach(hit => hit.OnHit(bullet, bullet.Damage));

        if(hitObject.TryFetchComponent(out StatusEffector effector))
        {
            int guaranteedEffects = bullet.StatusChance / 100;
            if(bullet.StatusChance - guaranteedEffects * 100 > 0)
                if (Random.Shared.Next(0, 100) <= bullet.StatusChance)
                    guaranteedEffects++;

            guaranteedEffects.Repeat(i => effector.Apply(bullet.Damage.ConnectedStatusEffect));
        }
    }

    private void HandleSplashBullet(Projectile bullet, CollisionSide hitPosition)
    {
        throw new NotImplementedException("This feature is yet to be implemented. will work on it later when i have better physics.");
    }
}