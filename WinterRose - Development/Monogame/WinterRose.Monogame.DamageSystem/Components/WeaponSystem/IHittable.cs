namespace WinterRose.Monogame.Weapons;

/// <summary>
/// Interface for objects that can be hit by projectile or their splash damage
/// </summary>
public interface IHittable
{
    /// <summary>
    /// Called when a projectile hits this object.
    /// </summary>
    /// <param name="bullet"></param>
    /// <param name="damage"></param>
    void OnHit(Projectile bullet, int damage);
}