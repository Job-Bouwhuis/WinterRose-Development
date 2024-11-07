using Microsoft.Xna.Framework;
using System.Linq;

#nullable enable

namespace WinterRose.Monogame.Weapons;

/// <summary>
/// A weapon that can shoot projectiles.
/// </summary>
[RequireComponent<Magazine>]
public class Weapon : ObjectBehavior
{
    /// <summary>
    /// The fire rate of the weapon (how many bullets per second)
    /// </summary>
    [field: Show]
    public float FireRate { get; set; } = 10.0f;

    /// <summary>
    /// The transform to take the rotation from when creating a new bullet. when null uses the rotation value of the owner of this component
    /// </summary>
    public Transform? RotationInheritor = null;

    /// <summary>
    /// The transform to take the position from when creating a new bullet. when null uses the position of the owner of this component
    /// </summary>
    public Transform? PositionInheritor = null;

    /// <summary>
    /// The type of fire mode the weapon has
    /// </summary>
    public WeaponFireingMode AvailableFireingMode { get; set; } = WeaponFireingMode.Single;
    /// <summary>
    /// The current fire mode of the weapon
    /// </summary>
    public WeaponFireingMode CurrentFiringMode => currentFireMode; 

    /// <summary>
    /// The amount of times the gun fires on burst mode
    /// </summary>
    public int BurstAmount { get; set; } = 3;

    /// <summary>
    /// The magazine of the weapon
    /// </summary>
    public Magazine Magazine { get; private set; }


    [Show]
    private float fireRateTimer = 0.0f;

    [Show]
    private int timesFired = 0;

    [Show]
    bool shooting = false;

    [Show]
    WeaponFireingMode currentFireMode = WeaponFireingMode.Single;

    private void Awake()
    {
        if (!TryFetchComponent(out Magazine mag))
            Debug.LogError("Magazine is null on weapon " + owner.Name);

        Magazine = mag;
    }

    // Update is called once per frame
    void Update()
    {
        // Handle reloading
        Reload();

        // handle switching firemodes
        SwitchFiringMode();

        /// handle firerate timeout. Do not continue if the firerate is not ready
        if (fireRateTimer > 0.0f)
        {
            fireRateTimer -= Time.SinceLastFrame;
            return;
        }

        // if the weapon is reloading, do not continue to prevent shooting while reloading in case the magazine is not empty.
        if (Magazine.IsReloading)
            return;

        // Handle shooting
        Shoot();
    }

    private void Shoot()
    {
        if (Input.MouseLeft)
        {
            shooting = true;

            if (currentFireMode == WeaponFireingMode.Single)
            {
                if (timesFired > 0)
                    return;
                timesFired++;
            }
            if (currentFireMode == WeaponFireingMode.Burst)
            {
                if (timesFired >= BurstAmount)
                    return;
                timesFired++;
            }

            Transform pos = PositionInheritor ?? transform;
            Vector2 startPos = pos.position + pos.up;
            Transform rot = RotationInheritor ?? transform;
            float startRotation = rot.rotation;

            Projectile[] bullets = Magazine.Take(startPos, startRotation);
            foreach (Projectile bullet in bullets)
            {
                bullet.Fire();
                fireRateTimer = 1.0f / FireRate;
            }
        }
        else
        {
            shooting = false;
            timesFired = 0;
        }
    }

    private void Reload()
    {
        if (Input.RDown)
        {
            Magazine.Reload();
        }
    }

    private void SwitchFiringMode()
    {
        if (Input.BDown)
        {
            // cycle through the firemodes that are available
            var valuesTemp = System.Enum.GetValues(typeof(WeaponFireingMode));
            WeaponFireingMode[] values = new WeaponFireingMode[valuesTemp.Length];
            valuesTemp.CopyTo(values, 0);

            values = values.Where(value => AvailableFireingMode.HasFlag(value)).ToArray();

            int currentIndex = System.Array.IndexOf(values, currentFireMode);
            currentIndex++;
            if (currentIndex >= values.Length)
                currentIndex = 0;

            currentFireMode = values[currentIndex];
        }
    }
}