using MFPS.Audio;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class bl_NetworkGun : bl_WeaponBase
{
    /// <summary>
    /// 
    /// </summary>
    public struct NetworkFireData
    {
        public Vector3 HitPosition;
        public Vector3 Inaccuracy;
        public object[] Params;
    }

    #region Public members
    [Header("Settings")]
    public bl_Gun LocalGun;
    public bool useCustomPlayerAnimations = false;
    public int customUpperAnimationID = 20;
    public string customFireAnimationName = "Fire";
    public int customFireAnimationsCount = 1;

    [Header("References")]
    public GameObject Bullet;
    public ParticleSystem MuzzleFlash;
    public Transform tpFirePoint;
    public GameObject DesactiveOnOffAmmo;
    public Transform LeftHandPosition;

    /// <summary>
    /// Automatically setup this weapon when enabled
    /// </summary>
    public bool AutoSetup
    {
        get;
        set;
    }
    #endregion

    #region Private members
    private AudioSource Source;
    private int WeaponID = -1;
    [System.NonSerialized]
    public BulletData m_BulletData = new();
    Vector3 bulletPosition = Vector3.zero;
    Quaternion bulletRotation = Quaternion.identity;
    #endregion

    /// <summary>
    /// 
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        SetupAudio();
    }

    /// <summary>
    /// Update type each is enable 
    /// </summary>
    protected override void OnEnable()
    {
        base.OnEnable();
        if (AutoSetup) ActiveThisWeapon();
    }

    /// <summary>
    /// Called when this weapon is activated.
    /// </summary>
    public virtual void ActiveThisWeapon()
    {
        if (PlayerRefs != null) PlayerRefs.playerNetwork.SetNetworkWeapon(Info.Type, this);
        if (LocalGun != null && LocalGun.customWeapon != null) LocalGun.customWeapon.Initialitate(LocalGun);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="weaponType"></param>
    /// <param name="fireData"></param>
    public virtual void OnNetworkFire(GunType weaponType, NetworkFireData fireData, FireReplicationFlag flag = FireReplicationFlag.None)
    {
        if (flag.IsByteEnumFlagPresent(FireReplicationFlag.BashAttack)) return;

        switch (weaponType)
        {
            case GunType.Machinegun:
                Fire(fireData);
                PlayerRefs.playerAnimations.PlayFireAnimation(GunType.Machinegun);
                break;
            case GunType.Pistol:
                Fire(fireData);
                PlayerRefs.playerAnimations.PlayFireAnimation(GunType.Pistol);
                break;
            case GunType.Sniper:
            case GunType.Shotgun:
                Fire(fireData);
                break;
            case GunType.Melee:
                KnifeFire();//if you need add your custom fire launcher in networkgun
                PlayerRefs.playerAnimations.PlayFireAnimation(GunType.Melee);
                break;
            case GunType.Launcher:
                FireProjectile(fireData);
                PlayerRefs.playerAnimations.PlayFireAnimation(GunType.Launcher);
                break;
            default:
                Debug.LogWarning("Not defined weapon type to sync bullets.");
                break;
        }

        PlayerRefs.onWeaponFire?.Invoke(new MFPSPlayerFireEventData()
        {
            AsLocalPlayer = false,
            GunID = GunID,
        });
    }

    /// <summary>
    /// Fire Sync in network player
    /// </summary>
    public virtual void Fire(NetworkFireData fireData)
    {
        if (LocalGun == null)
        {
            Debug.LogWarning($"The {gameObject.name} TPWeapon doesn't have assigned the FPWeapon.");
            return;
        }
        if (MuzzleFlash)
        {
            PlayMuzzleflash();
            bulletPosition = MuzzleFlash.transform.position;
        }
        else
        {
            bulletPosition = transform.position;
        }

        bulletRotation = Quaternion.LookRotation(fireData.HitPosition - bulletPosition);
        //bullet info is set up in start function
        GameObject newBullet = bl_ObjectPoolingBase.Instance.Instantiate(LocalGun.BulletName, bulletPosition, bulletRotation);
        m_BulletData.Speed = LocalGun.bulletSpeed;
        m_BulletData.Inaccuracity = fireData.Inaccuracy;
        m_BulletData.DropFactor = LocalGun.bulletDropFactor;
        m_BulletData.Position = PlayerRefs.transform.position;
        m_BulletData.ActorViewID = PlayerRefs.ViewID;
        m_BulletData.AsNetworkBullet();

        newBullet.GetComponent<bl_ProjectileBase>().InitProjectile(m_BulletData);
        PlayLocalFireAudio();
    }

    /// <summary>
    /// Play the fire visuals only, do not actually fire a bullet.
    /// </summary>
    public virtual void FireVisualOnly(FireReplicationFlag flag = FireReplicationFlag.None)
    {
        if (flag.IsByteEnumFlagPresent(FireReplicationFlag.BashAttack)) return;

        PlayMuzzleflash();
    }

    /// <summary>
    /// 
    /// </summary>
    public void FireProjectile(NetworkFireData fireData)
    {
        if (LocalGun == null)
        {
            Debug.LogWarning($"The {gameObject.name} TPWeapon doesn't have assigned the FPWeapon.");
            return;
        }

        GameObject projectileInstance = Instantiate(LocalGun.bulletPrefab, GetFirePoint().position, Quaternion.Euler(fireData.Inaccuracy)) as GameObject;

        if (projectileInstance.TryGetComponent<Rigidbody>(out var proRigid) && LocalGun.projectileDirectionMode != ProjectileDirectionMode.HandleByProjectile)
        {
            proRigid.AddForce(fireData.HitPosition, LocalGun.projectileForceMode);
        }

        m_BulletData.Speed = LocalGun.bulletSpeed;
        m_BulletData.Position = PlayerRefs.transform.position;
        m_BulletData.SetActor(PlayerRefs.ViewID);
        m_BulletData.AsNetworkBullet();

        var projectileScript = projectileInstance.GetComponent<bl_ProjectileBase>();
        projectileScript.InitProjectile(m_BulletData);

        PlayLocalFireAudio();
    }

    /// <summary>
    /// 
    /// </summary>
    public bool FireCustomLogic(ExitGames.Client.Photon.Hashtable data)
    {
        if (LocalGun != null && LocalGun.customWeapon != null)
        {
            LocalGun.customWeapon.TPFire(this, data);
            return true;
        }
        return false;
    }

    /// <summary>
    /// if grenade 
    /// </summary>
    /// <param name="s"></param>
    public virtual void GrenadeFire(float s, Vector3 position, Quaternion rotation, Vector3 direction, string id)
    {
        if (LocalGun != null)
        {
            //bullet info is set up in start function
            GameObject newBullet = Instantiate(Bullet, position, rotation) as GameObject; // create a bullet
            // set the gun's info into an array to send to the bullet
            m_BulletData.SetInaccuracity(s, LocalGun.spreadMinMax.y);
            m_BulletData.Speed = LocalGun.bulletSpeed;
            m_BulletData.Position = PlayerRefs.transform.position;
            m_BulletData.SetActor(PlayerRefs.ViewID);
            m_BulletData.AsNetworkBullet();

            if (newBullet.TryGetComponent<Rigidbody>(out var proRigid))
            {
                proRigid.AddForce(direction, ForceMode.Impulse);
            }

            var script = newBullet.GetComponent<bl_ProjectileBase>();
            script.InitProjectile(m_BulletData); //bl_Projectile.cs
            if (!string.IsNullOrEmpty(id))
            {
                script.SetUniqueID(id);
            }
            Source.clip = LocalGun.FireSound;
            Source.spread = Random.Range(1.0f, 1.5f);
            Source.Play();
        }
    }

    /// <summary>
    /// When is knife only reply sounds
    /// </summary>
    public virtual void KnifeFire()
    {
        if (LocalGun != null)
        {
            Source.clip = LocalGun.FireSound;
            Source.spread = Random.Range(1.0f, 1.5f);
            Source.Play();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void PlayMuzzleflash()
    {
        if (MuzzleFlash == null) return;

        MuzzleFlash.Play();
    }

    /// <summary>
    /// 
    /// </summary>
    public void PlayLocalFireAudio()
    {
        if (LocalGun == null) return;

        Source.clip = LocalGun.FireSound;
        Source.spread = Random.Range(1.0f, 1.5f);
        Source.Play();
    }

    /// <summary>
    /// 
    /// </summary>
    private void SetupAudio()
    {
        Source = GetComponent<AudioSource>();
        Source.playOnAwake = false;
        Source.spatialBlend = 1;
        if (bl_AudioController.Instance != null)
        {
            if (Info.Type != GunType.Melee)
            {
                Source.maxDistance = bl_AudioController.Instance.maxWeaponDistance;
                Source.minDistance = bl_AudioController.Instance.maxWeaponDistance * 0.09f;
            }
            else
            {
                Source.maxDistance = 5;
                Source.minDistance = 2;
            }
            Source.rolloffMode = bl_AudioController.Instance.audioRolloffMode;
        }
        Source.spatialize = true;
        Source.AssignMixerGroup("TP Weapon");
    }

    /// <summary>
    /// Returns the upper state ID
    /// That will be used to identify which upper body animations 
    /// will be played when this weapon is equipped
    /// </summary>
    /// <returns></returns>
    public int GetUpperStateID()
    {
        if (!useCustomPlayerAnimations || customUpperAnimationID <= 20) return (int)Info.Type;
        return customUpperAnimationID;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public override int GetGunID()
    {
        return GetWeaponID;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public override GunType GetWeaponType()
    {
        return Info.Type;
    }

    /// <summary>
    /// 
    /// </summary>
    public int GetWeaponID
    {
        get
        {
            if (LocalGun != null)
            {
                WeaponID = LocalGun.GunID;
            }
            return WeaponID;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public Transform GetFirePoint()
    {
        return tpFirePoint != null ? tpFirePoint : MuzzleFlash != null ? MuzzleFlash.transform : transform;
    }

    /// <summary>
    /// 
    /// </summary>
    private bl_GunInfo _info = null;
    public override bl_GunInfo Info
    {
        get
        {
            if (LocalGun != null)
            {
                _info ??= bl_GameData.Instance.GetWeapon(GetWeaponID);
                return _info;
            }
            else
            {
                Debug.LogError("This tpv weapon: " + gameObject.name + " has not been defined!");
                return bl_GameData.Instance.GetWeapon(0);
            }
        }
    }

    private bl_PlayerReferences _playerRefs = null;
    private bl_PlayerReferences PlayerRefs
    {
        get
        {
            if (_playerRefs == null)
            {
                var parentRef = transform.GetComponentInParent<IRealPlayerReference>();
                if (parentRef != null)
                {
                    _playerRefs = parentRef.GetReferenceRoot();
                }
            }
            return _playerRefs;
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (Application.isPlaying)
            return;

        if (LeftHandPosition != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(LeftHandPosition.position, 0.02f);
            Gizmos.DrawWireSphere(LeftHandPosition.position, 0.05f);
        }
    }
#endif
}