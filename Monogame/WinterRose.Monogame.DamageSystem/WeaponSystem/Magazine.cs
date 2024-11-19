using Microsoft.Xna.Framework;
using System;
using System.Collections;
using System.Threading.Tasks;

#nullable enable

namespace WinterRose.Monogame.Weapons
{
    public class Magazine : ObjectComponent
    {
        public Magazine() { }
        public Magazine(string bulletPrefabName) => BulletPrefab = new WorldObjectPrefab(bulletPrefabName);

        static Magazine()
        {
            Worlds.WorldTemplateObjectParsers.Add(typeof(Magazine), (instance, identifier) =>
            {
                var mag = (Magazine)instance;
                if (mag.bulletPrefab == null)
                    return $"{identifier}()";
                return $"{identifier}(\"{mag.bulletPrefab.Name}\")";
            });
        }

        /// <summary>
        /// The maximum amount of bullets that can be stored in the magazine.
        /// </summary>
        [IncludeInTemplateCreation]
        public int MaxBullets { get; set; } = 30;

        /// <summary>
        /// The current amount of bullets in the magazine.
        /// </summary>
        [IncludeInTemplateCreation]
        public int CurrentBullets { get; set; } = 30;

        /// <summary>
        /// The amount of pool of projectiles left when trying to reload
        /// </summary>
        [IncludeInTemplateCreation]
        public int PoolOfProjectiles { get; set; } = 60;

        /// <summary>
        /// The time it takes to reload the magazine in seconds
        /// </summary>
        [IncludeInTemplateCreation]
        public float ReloadTime { get; set; } = 2.0f;

        /// <summary>
        /// The amount of bullets that are fired per shot.
        /// </summary>
        [IncludeInTemplateCreation]
        public int BulletsPerShot { get; set; } = 1;

        /// <summary>
        /// The amount of bullets that are consumed per shot.
        /// </summary>
        [IncludeInTemplateCreation]
        public int BulletsConsumedPerShot { get; set; } = 1;

        /// <summary>
        /// The bullet this magazine shoots
        /// </summary>
        [IgnoreInTemplateCreation]
        public WorldObjectPrefab? BulletPrefab 
        { 
            get => bulletPrefab;
            set
            {
                bulletPrefab = value;
                if (bulletPrefab is null)
                    return;

                if(bulletPrefab!.LoadedObject is null)
                {
                    bulletPrefab.Load();
                }
            }
        }

        /// <summary>
        /// Whether the magazine is currently reloading
        /// </summary>
        [IgnoreInTemplateCreation]
        public bool IsReloading => isReloading;

        [Show, IgnoreInTemplateCreation]
        private bool isReloading = false;
        [IgnoreInTemplateCreation]
        private WorldObjectPrefab? bulletPrefab;
        [IgnoreInTemplateCreation]
        private int bulletsSpawned = 0;

        private void Awake()
        {
            if (BulletPrefab == null)
            {
                Debug.LogError("BulletPrefab is null on magazine " + owner.Name);
            }
        }

        /// <summary>
        /// Removes the amount of bullets per shot from the mag and creates them into the world
        /// </summary>
        /// <param name="bulletStartPos"></param>
        /// <param name="bulletStartRotation"></param>
        /// <returns>A reference to the <see cref="Projectile"/> component on each instantiated bullet</returns>
        public Projectile[] Take(Vector2 bulletStartPos, float bulletStartRotation)
        {
            if (CurrentBullets >= BulletsConsumedPerShot)
            {
                CurrentBullets -= BulletsConsumedPerShot;
                Projectile[] bullets = new Projectile[BulletsPerShot];

                foreach(int i in BulletsPerShot)
                {
                    WorldObject bullet = BulletPrefab.LoadIn(world);
                    bullet.Name += "_" + bulletsSpawned++;
                    bullet.transform.position = bulletStartPos;
                    bullet.transform.rotation = bulletStartRotation;
                    bullets[i] = bullet.FetchComponent<Projectile>()!;
                    if (bullets[i] is null)
                    {
                        Debug.LogException("Bullet does not have a Projectile component. This is strictly required.");
                        return [];
                    }
                }

                return bullets;
            }

            return [];
        }

        /// <summary>
        /// Reloads the magazine if it is not already reloading
        /// </summary>
        public void Reload()
        {
            if (CurrentBullets == MaxBullets || PoolOfProjectiles == 0)
                return;
            if(isReloading) return;
            isReloading = true;

            _ = Reloading();
        }

        private async Task Reloading()
        {
            await Task.Delay((int)(ReloadTime * 1000));

            int bulletsToReload = Math.Min(MaxBullets - CurrentBullets, PoolOfProjectiles);
            CurrentBullets += bulletsToReload;
            PoolOfProjectiles -= bulletsToReload;

            isReloading = false;
        }

        private void Close()
        {
            isReloading = false;
        }
    }
}