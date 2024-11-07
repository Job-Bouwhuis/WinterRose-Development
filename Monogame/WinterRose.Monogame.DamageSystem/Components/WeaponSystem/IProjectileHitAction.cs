namespace WinterRose.Monogame.Weapons;

public interface IProjectileHitAction
{
    /// <summary>
    /// Called when a projectile hits this object. Returns whether the object can pass through the object or not.
    /// </summary>
    /// <param name="usedBullet"></param>
    /// <param name="hitObject"></param>
    /// <param name="hitPosition"></param>
    /// <returns></returns>
    void OnHit(Projectile usedBullet, WorldObject hitObject, CollisionSide hitPosition);
}