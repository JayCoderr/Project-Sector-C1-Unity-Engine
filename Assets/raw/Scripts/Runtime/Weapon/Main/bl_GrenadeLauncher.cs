using UnityEngine;

public class bl_GrenadeLauncher : bl_CustomGunBase
{
    public float explosionRadious = 10;
    private bl_Gun FPWeapon;
    private bl_NetworkGun TPWeapon;

    public override void Initialitate(bl_Gun gun)
    {
        FPWeapon = gun;
    }

    /// <summary>
    /// 
    /// </summary>
    public override void OnFire()
    {

    }

    /// <summary>
    /// 
    /// </summary>
    public override void OnFireDown()
    {
        if (!FPWeapon.FireRatePassed) return;

        FPWeapon.nextFireTime = Time.time;
        Shoot();
    }

    /// <summary>
    /// 
    /// </summary>
    public override void TPFire(bl_NetworkGun networkGun, ExitGames.Client.Photon.Hashtable data)
    {
        TPWeapon = networkGun;
        if (FPWeapon == null) { FPWeapon = TPWeapon.LocalGun; }

        Vector3 instancePosition = (Vector3)data["position"];
        Quaternion instanceRotation = (Quaternion)data["rotation"];

        GameObject projectile;
        if (FPWeapon.bulletInstanceMethod == BulletInstanceMethod.Pooled)
        {
            projectile = bl_ObjectPoolingBase.Instance.Instantiate(FPWeapon.BulletName, instancePosition, instanceRotation);
        }
        else
        {
            projectile = Instantiate(FPWeapon.bulletPrefab, instancePosition, instanceRotation) as GameObject;
        }

        var glp = projectile.GetComponent<bl_ProjectileBase>();
        TPWeapon.m_BulletData.Speed = TPWeapon.LocalGun.bulletSpeed;
        TPWeapon.m_BulletData.Position = TPWeapon.transform.position;
        TPWeapon.m_BulletData.AsNetworkBullet();

        glp.InitProjectile(TPWeapon.m_BulletData);
        TPWeapon.PlayLocalFireAudio();
    }

    /// <summary>
    /// 
    /// </summary>
    void Shoot()
    {
        Vector3 position = FPWeapon.muzzlePoint.position;
        Quaternion rotation = FPWeapon.PlayerCamera.transform.rotation;
        GameObject projectile;
        if (FPWeapon.bulletInstanceMethod == BulletInstanceMethod.Pooled)
        {
            projectile = bl_ObjectPoolingBase.Instance.Instantiate(FPWeapon.BulletName, position, rotation);
        }
        else
        {
            projectile = Instantiate(FPWeapon.bulletPrefab, position, rotation) as GameObject;
        }

        var glp = projectile.GetComponent<bl_ProjectileBase>();
        FPWeapon.BuildBulletData();
        FPWeapon.BulletSettings.ImpactForce = explosionRadious;
        glp.InitProjectile(FPWeapon.BulletSettings);
        FPWeapon.PlayFireAudio();
        FPWeapon.WeaponAnimation?.PlayFire(bl_WeaponAnimationBase.AnimationFlags.Aiming);
        if (FPWeapon.muzzleFlash != null) { FPWeapon.muzzleFlash.Play(); }
        FPWeapon.bulletsLeft--;
        FPWeapon.Kick();
        FPWeapon.UpdateUI();
        FPWeapon.CheckAutoReload(1);

        CustomProjectileData projectileData = new()
        {
            Origin = position,
            Rotation = rotation,
        };

        FPWeapon.PlayerNetwork.ReplicateCustomProjectile(projectileData);
        bl_EventHandler.DispatchLocalPlayerFire(FPWeapon.GunID);
    }
}