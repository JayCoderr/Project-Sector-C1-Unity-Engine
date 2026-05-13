using MFPS.Audio;
using MFPS.Core.Motion;
using MFPS.Internal.Structures;
using MFPS.Runtime.Motion;
using MFPSEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
#if ACTK_IS_HERE
using CodeStage.AntiCheat.ObscuredTypes;
#endif

public class bl_Gun : bl_WeaponBase
{
    #region Public members
    public bl_CustomGunBase customWeapon;
    public float CrossHairScale = 8;
    public BulletInstanceMethod bulletInstanceMethod = BulletInstanceMethod.Pooled;
    public ShellShotOptions shellShotOption = ShellShotOptions.SingleShot;
    [FlagEnum] public FireType supportedFireTypes = FireType.Auto;
    public FireType defaultFireType = FireType.Auto;
    public AmmoType ammoType = AmmoType.HeavyAmmo;
    public string BulletName = "bullet";
    public GameObject bulletPrefab;
    public ParticleSystem muzzleFlash = null;     // the muzzle flash for this weapon
    public Transform muzzlePoint = null;    // the muzzle point of this weapon
    public bl_WeaponFXBase weaponFX = null;
    public ParticleSystem shell = null;          // the weapons empty shell particle
    public GameObject impactEffect = null;  // impact effect, used for raycast bullet types
    public Transform centerPivot = null;         // the center pivot of the weapon
    public Vector3 AimPosition; //position of gun when is aimed
    public Vector3 aimRotation = Vector3.zero;
    public bool useSmooth = true;
    public bool allowAim = true;
    public float AimSmooth;
    [ScriptableDrawer] public ShakerPresent shakerPresent;
    [Range(0, 179)]
    public float aimZoom = 50;
    //Shotgun Specific Vars
    public int pelletsPerShot = 10;         // number of pellets per round fired for the shotgun
    public float delayForSecondFireSound = 0.45f;
    //Burst Specific Vars
    public int roundsPerBurst = 3;          // number of rounds per burst fire
    public float lagBetweenBurst = 0.5f;    // time between each shot in a burst
    //Launcher Specific Vars
    public List<GameObject> OnAmmoLauncher = new();
    public bool ThrowByAnimation = false;
    public int impactForce = 50;            // how much force applied to a rigid body
    public float bulletSpeed = 200.0f;      // how fast are your bullets
    public ForceMode projectileForceMode = ForceMode.Impulse;
    public ProjectileDirectionMode projectileDirectionMode = ProjectileDirectionMode.ParentDirection;
    /// <summary>
    /// maximum distance at which a weapon can be expected to be accurate and achieve the intended base damage.
    /// </summary>
    public float effectiveFiringRange = 200;
    public AnimationCurve effectivinessDropoff = AnimationCurve.Linear(0, 1, 1, 0.1f);
    public float bulletDropFactor = 1;
    public float dragCoefficient = 0.1f;
    public bool AutoReload = true;
    public bool m_AllowQuickFire = true;
    public bool canBeThrown = true;
    public ReloadPer reloadPer = ReloadPer.Bullet;
    public int bulletsPerClip = 50;         // number of bullets in each clip
    public int numberOfClips = 5;
    public int maxNumberOfClips = 10;       // maximum number of clips you can hold
    public bool useAmmo = true;
    public bool overrideQuickMeleeAttack = false;
    public float DelayFire = 0.85f;
    public float quickFireDelay = 0.18f;
    public float delayFireOnSprinting = 0.12f; // fire delay in the first shot when the player is sprinting.
    public Vector2 spreadMinMax = new(1, 3);
    public float spreadAimMultiplier = 0.5f;
    public float spreadPerSecond = 0.2f;    // if trigger held down, increase the spread of bullets
    public float spread = 0.0f;             // current spread of the gun
    public float decreaseSpreadPerSec = 0.5f;// amount of accuracy regained per frame when the gun isn't being fired
    public WeaponQuickFireTimeMode quickFireTimeMode = WeaponQuickFireTimeMode.AnimationTime;
    public float quickFireTime = 1;

    public bool isReloading { get; set; } = false;

    // Recoil
    public float RecoilAmount = 5.0f;
    public float RecoilSpeed = 2;
    [ScriptableDrawer] public bl_WeaponRecoilSettings recoilSettings;
    public bool SoundReloadByAnim = false;
    public AudioClip TakeSound;
    public AudioClip FireSound;
    public AudioClip DryFireSound;
    public AudioClip ReloadSound;
    public AudioClip ReloadSound2 = null;
    public AudioClip ReloadSound3 = null;
    public AudioClip boltSound;
    public AudioClip aimSound;
    public AudioClip bashAttackSound;

    public Vector2 fireSoundPitch = new(0.98f, 1.5f);

    //cached player components
    public Renderer[] weaponRenders = null;
    public bl_PlayerSettings playerSettings;
    public PlayerFPState FPState = PlayerFPState.Idle;
    public bool canBeTakenWhenIsEmpty = true;
    #endregion

    #region Public properties
    public int Damage
    {
        get { return Info.Damage + extraDamage; }
        set { extraDamage = value; }//don't modify the base damage, only the extra value
    }

#if !ACTK_IS_HERE
    private int m_remainingClips = 5;
    public int RemainingClips
    {
        get => m_remainingClips;
        set => m_remainingClips = value;
    }
#else
    private ObscuredInt m_remainingClips = 5;
    public ObscuredInt RemainingClips
    {
        get => m_remainingClips;
        set => m_remainingClips = value;
    }
#endif

    public float Zoom
    {
        get { return aimZoom + extraZoom; }
        set { extraZoom = value; }//don't modify the base zoom, only the extra value
    }

    public int BulletsPerMagazine
    {
        get { return bulletsPerClip + extraBullets; }
        set { extraBullets = value; }//don't modify the base bullets, only the extra value
    }

    public float MaxSpread
    {
        get { return extraSpread; }
        set { extraSpread = value; }//don't modify the base bullets, only the extra value
    }

    private float extraWeight = 0;
    public float WeaponWeight
    {
        get => Info.Weight + extraWeight;
        set => extraWeight = value;
    }

    private float extraRecoil = 0;
    public float WeaponRecoil
    {
        get => RecoilAmount + extraRecoil;
        set => extraRecoil = value;
    }

    private float extraAimSpeed = 0;
    public float AimSpeed
    {
        get => AimSmooth + extraAimSpeed;
        set => extraAimSpeed = value;
    }

    private float extraReloadTime = 0;
    public float ReloadTime
    {
        get => Info.ReloadTime + extraReloadTime;
        set => extraReloadTime = value;
    }

    private float extraBulletSpeed = 0;
    public float BulletSpeed
    {
        get => bulletSpeed + extraBulletSpeed;
        set => extraBulletSpeed = value;
    }

    private int extraRange = 0;
    public int Range
    {
        get => Info.Range + extraRange;
        set => extraRange = value;
    }

    public AudioClip FireAudioClip
    {
        get => FireSound;
        set
        {
            if (defaultFireSound == null) defaultFireSound = FireSound;
            FireSound = value;
            if (FireSound == null) FireSound = defaultFireSound;
        }
    }

    public float nextFireTime { get; set; }
    public Camera PlayerCamera { get; private set; }
    public bool BlockAimFoV { get; set; }
    public bool HaveInfinityAmmo { get; private set; }
    public System.Action<bool> onWeaponRendersActive;
    #endregion

    #region Private members
    private bool isEnabled = true;
    private AudioSource Source;
    private Camera WeaponCamera;
    private bool inReloadMode = false;
    [FormerlySerializedAs("AmmoType")]
    private AmmoReserveType ammoReserveType = AmmoReserveType.Bullets;
    private AudioSource FireSource = null;
    private bool isInitialized = false;
    private Transform m_Transform;
    public BulletData BulletSettings = new();
    private Vector2 defaultSpreadRange;
    public int extraDamage, extraBullets = 0;
    private Transform defaultMuzzlePoint = null;
    private bool lastAimState = false;
    private float currentZoom, extraZoom, extraSpread = 0;
    GameObject instancedBullet = null;
    RaycastHit hit;
    private Vector3 DefaultPos;
    private Vector3 CurrentPos;
    private Vector3 currentRotation, defaultRotation;
    private static BulletInstanceData bulletInstanceData;
    private AudioClip defaultFireSound;
    private float lastShotTime = 0;
    private bool pendingFirstShot = false;
    private bool m_canFire = false;
    private bool m_canAim;
    private GunType currentGunType = GunType.None;
    private FireType currentFireMode;
    private List<FireType> fireTypes;
    private AudioSource boldAudioSource;
    private bl_SpringTransform aimSpring;
    private float defaultVolume = 1.0f;
    private float nextBashTime = 0;
    private bool blockNextShot;
    #endregion

    /// <summary>
    /// 
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        Initialized();
    }

    /// <summary>
    /// Initializes the weapon and its associated components, ensuring all dependencies are set up correctly.
    /// </summary>
    /// <remarks>This method should be called to prepare the weapon for use. It initializes various components
    /// such as  the aim spring, audio sources, and player references. If the weapon is already initialized, the method 
    /// ensures the fire audio source volume is synchronized with the main audio source and exits early.</remarks>
    public override void Initialized()
    {
        if (isInitialized)
        {
            FireSource.volume = Source.volume;
            return;
        }

        m_Transform = transform;
        playerSettings = PlayerReferences.playerSettings;
        aimSpring = bl_GlobalReferences.I.aimSpring.InitAndGetInstance(m_Transform);
        if (aimSpring != null) aimSpring.RotationSpring.UseDeltaAngle(true);

        if (FireSource == null) FireSource = PlayerReferences.gunManager.GetFireAudioSource();
        Source = GetComponent<AudioSource>();
        Source.AssignMixerGroup("FP Weapon");
        defaultVolume = Source.volume;
        FireSource.volume = defaultVolume;
        PlayerCamera = PlayerReferences.playerCamera;

        defaultSpreadRange = spreadMinMax;
        ammoReserveType = bl_GameData.CoreSettings.AmmoType;
        if (bl_CrosshairBase.Instance != null) bl_CrosshairBase.Instance.Block = false;
        Setup();
        customWeapon?.Initialitate(this);
        isInitialized = true;
    }

    /// <summary>
    /// 
    /// </summary>
    protected override void OnEnable()
    {
#if MFPSM
        if (bl_UtilityHelper.isMobile)
        {
            bl_TouchHelper.onMobileButton += OnMobileButton;
        }
#endif

        if (!isInitialized) return;

        base.OnEnable();
        PlayClip(TakeSound);
        CanFire = false;
        pendingFirstShot = false;
        UpdateUI();
        if (WeaponAnimation)
        {
            float t = WeaponAnimation.PlayTakeIn();
            Invoke(nameof(DrawComplete), t);
        }
        else
        {
            DrawComplete();
        }
        bl_EventHandler.Gameplay.onAmmoPickUp += this.OnPickUpAmmo;
        bl_EventHandler.onRoundEnd += this.OnRoundEnd;
        if (bl_CrosshairBase.Instance != null)
            bl_CrosshairBase.Instance.SetupCrosshairForWeapon(this);
        if (bl_EquippedWeaponUIBase.Instance != null)
            bl_EquippedWeaponUIBase.Instance.SetFireType(GetFireType());
        playerSettings?.DoSpawnWeaponRenderEffect(weaponRenders);
        OnAmmoLauncher.ForEach(x => { if (x != null) x.SetActive(true); });
#if MFPSTPV
        if (PlayerReferences.TryGetComponent<bl_PlayerCameraSwitcher>(out var tpScript))
        {
            SetWeaponRendersActive(!bl_CameraViewSettings.IsThirdPerson());
        }
#endif
    }

    /// <summary>
    /// 
    /// </summary>
    protected override void OnDisable()
    {
        base.OnDisable();
        bl_EventHandler.Gameplay.onAmmoPickUp -= this.OnPickUpAmmo;
        bl_EventHandler.onRoundEnd -= this.OnRoundEnd;
#if MFPSM
        if (bl_UtilityHelper.isMobile)
        {
            bl_TouchHelper.onMobileButton -= OnMobileButton;
        }
#endif
        if (PlayerCamera == null) { PlayerCamera = PlayerReferences.playerCamera; }
        StopAllCoroutines();
        if (isReloading) { inReloadMode = true; isReloading = false; }
        isAiming = false;
        isFiring = false;
        lastAimState = false;
        ResetDefaultMuzzlePoint();
    }

    /// <summary>
    /// Called by the weapon manager on all the equipped weapons
    /// even when they have not been enabled
    /// </summary>
    public void Setup(bool initial = false)
    {
        // Initial ammo setup
        bulletsLeft = BulletsPerMagazine;
        currentFireMode = defaultFireType;
        if (ammoReserveType == AmmoReserveType.Clips) m_remainingClips = numberOfClips;
        if (!initial)
        {
            if (ammoReserveType == AmmoReserveType.Bullets)
            {
                m_remainingClips = BulletsPerMagazine * numberOfClips;
            }
        }

        DefaultPos = transform.localPosition;
        defaultRotation = transform.localEulerAngles;
        if (PlayerCamera == null) { PlayerCamera = PlayerReferences.playerCamera; }
        WeaponCamera = PlayerReferences.weaponCamera;
        WeaponCamera.fieldOfView = bl_MFPS.Settings != null ? (float)bl_MFPS.Settings.GetSettingOf("Weapon FOV") : 55;
        CanAiming = true;

        fireTypes = new List<FireType>();
        var modes = (FireType[])Enum.GetValues(typeof(FireType));
        for (int i = 0; i < modes.Length; i++)
        {
            if (supportedFireTypes.IsEnumFlagPresent(modes[i]))
            {
                fireTypes.Add(modes[i]);
            }
        }
        if (fireTypes.Count == 0) fireTypes.Add(currentFireMode);
    }

    /// <summary>
    /// check what the player is doing every frame
    /// </summary>
    /// <returns></returns>
    public override void OnUpdate()
    {
        if (!isEnabled)
        {
            Aim();
            return;
        }

        InputUpdate();
        Aim();
        DetermineUpperState();
        BulletSpreadControl();
    }

    /// <summary>
    /// All Input events 
    /// </summary>
    void InputUpdate()
    {
        if (bl_GameData.isChatting || !gunManager.IsGameStarted || !bl_GameInput.IsCursorLocked || bl_GameInput.InputFocus != MFPSInputFocus.Player) return;

        ChangeTypeFire();
#if MFPSM
        if (bl_MFPSMobileControl.HandleWeaponFireInput(this)) return;
#endif

        bool fireDown = bl_GameInput.Fire(GameInputType.Down);
        if (CanFire)
        {
            // if keep pressing and is auto, fire continuously
            if (currentFireMode == FireType.Auto && bl_GameInput.Fire())
            {
                LoopFire();
            }
            // if press once
            if (fireDown) SingleFire();

        }
        else
        {
            if (fireDown && bulletsLeft <= 0 && !isReloading)//if try fire and don't have more bullets
            {
                PlayEmptyFireSound();
            }
        }

        if (fireDown && isReloading)//if try fire while reloading 
        {
            if (WeaponType == GunType.Sniper || WeaponType == GunType.Shotgun)
            {
                if (bulletsLeft > 0)//and has at least one bullet
                {
                    CancelReloading();
                }
            }
        }

        if (!bl_UtilityHelper.isMobile)
        {
            isAiming = bl_GameInput.Aim() && CanAiming;

            //used to decrease weapon accuracy as long as the trigger remains down
            if (WeaponType != GunType.Grenade && WeaponType != GunType.Melee)
            {
                if (WeaponType == GunType.Machinegun)
                {
                    isFiring = (bl_GameInput.Fire() && CanFire); // fire is down, gun is firing
                }
                else
                {
                    if (fireDown && CanFire)
                    {
                        isFiring = true;
                        CancelInvoke(nameof(CancelFiring));
                        Invoke(nameof(CancelFiring), 0.12f);
                    }
                }
            }
        }

        if (bl_GameInput.Reload() && CanReload)
        {
            Reload();
        }
    }

    /// <summary>
    /// Cycles through the available fire modes for the weapon and updates the current fire mode.
    /// </summary>
    /// <remarks>This method switches to the next fire mode in the list of available fire modes.  If the end
    /// of the list is reached, it wraps around to the first fire mode.  The method also updates the UI to reflect the
    /// new fire mode, plays a sound effect,  and dispatches a gameplay event to notify other systems of the
    /// change.</remarks>
    void ChangeTypeFire()
    {
        if (fireTypes == null || fireTypes.Count < 2) return;
        if (!bl_GameInput.SwitchFireMode()) return;

        int current = fireTypes.IndexOf(currentFireMode);
        current = (current + 1) % fireTypes.Count;
        currentFireMode = fireTypes[current];

        if (bl_EquippedWeaponUIBase.Instance != null) bl_EquippedWeaponUIBase.Instance.SetFireType(GetFireType());
        bl_AudioController.PlayClip("switch-fire-type");
        bl_EventHandler.DispatchGameplayPlayerEvent("change fire type");
    }

    /// <summary>
    /// Adjusts the bullet spread based on the player's firing state and movement state.
    /// </summary>
    /// <remarks>This method increases the bullet spread while firing and decreases it when the trigger is
    /// released. The spread adjustment is influenced by the player's movement state, such as crouching, which reduces
    /// the rate of spread increase. The spread value is clamped within a defined range to ensure it remains within
    /// acceptable limits.</remarks>
    void BulletSpreadControl()
    {
        if (isFiring) // if the gun is firing
        {
            spread += (PlayerReferences.firstPersonController.State == PlayerState.Crouching) ? spreadPerSecond * 0.5f : spreadPerSecond; // gun is less accurate with the trigger held down
        }
        else
        {
            spread -= decreaseSpreadPerSec; // gun regains accuracy when trigger is released
        }
        spread = Mathf.Clamp(spread, BaseSpread + extraSpread, spreadMinMax.y + extraSpread);
    }

    /// <summary>
    /// Called by mobile button event
    /// </summary>
    void OnFire()
    {
        if (!gunManager.IsGameStarted) return;

        if (bulletsLeft <= 0 && !isReloading)
        {
            PlayEmptyFireSound();
        }

        if (isReloading)
        {
            if (WeaponType == GunType.Sniper || WeaponType == GunType.Shotgun)
            {
                if (bulletsLeft > 0)
                {
                    CancelReloading();
                }
            }
        }

        if (!CanFire)
            return;

        SingleFire();
    }

    /// <summary>
    /// Fire one time
    /// </summary>
    public void SingleFire()
    {
        if (PlayerReferences.firstPersonController.IsSprinting() && Time.time - lastShotTime > (Info.FireRate * 2) || pendingFirstShot)
        {
            if (pendingFirstShot) return;

            isFiring = true;
            pendingFirstShot = true;

            this.InvokeAfter(delayFireOnSprinting, () =>
            {
                pendingFirstShot = false;

                GunTypeFire();
            });
        }
        else
        {
            GunTypeFire();
        }
        lastShotTime = Time.time;
    }

    /// <summary>
    /// Fire continuously
    /// </summary>
    public void LoopFire()
    {
        if (PlayerReferences.firstPersonController.IsSprinting() && Time.time - lastShotTime > (Info.FireRate * 5) || pendingFirstShot)
        {
            if (pendingFirstShot) return;

            isFiring = true;
            pendingFirstShot = true;

            this.InvokeAfter(delayFireOnSprinting, () =>
            {
                pendingFirstShot = false;
                if (!bl_GameInput.Fire(GameInputType.Down)) return;

                GunTypeFire();
            });
        }
        else
        {
            GunTypeFire();
        }
        lastShotTime = Time.time;
    }

    /// <summary>
    /// Call the fire function depending on the weapon type
    /// </summary>
    /// <param name="auto">automatic fire?</param>
    void GunTypeFire()
    {
        if (customWeapon != null) { customWeapon.OnFireDown(); }
        else
        {
            switch (WeaponType)
            {
                case GunType.Pistol:
                case GunType.Machinegun:
                case GunType.Shotgun:
                case GunType.Sniper:
                    WeaponFire();
                    break;
                case GunType.Grenade:
                    GrenadeFire();
                    break;
                case GunType.Melee:
                    MeleeFire();
                    break;
                case GunType.Launcher:
                    LauncherFire();
                    break;
            }
        }
    }

    #region Fire Handlers
    /// <summary>
    /// Fire the weapon
    /// </summary>
    void WeaponFire()
    {
        if (currentFireMode == FireType.Semi)
        {
            WeaponBurstFire();
            return;
        }

        if (!CheckFireRate()) return;

        FireShell();
        if (blockNextShot)
        {
            blockNextShot = false;
            return;
        }
        OnFireCommons();
        PlayFX();
        PlayFireAnimation();
        CheckAutoReload();
    }

    /// <summary>
    /// Executes a melee fire action, triggering animations, effects, and network replication.
    /// </summary>
    /// <remarks>This method performs a melee fire action by checking the fire rate, playing the fire
    /// animation, triggering visual and audio effects, and replicating the action across the network. It also
    /// dispatches relevant events for crosshair updates and local player actions.</remarks>
    /// <returns>The duration of the fire animation in seconds. Returns 0 if the fire rate check fails.</returns>
    private float MeleeFire()
    {
        if (!CheckFireRate()) return 0;

        // shot a raycast to check if we hit something
        FireOneHitScan();

        float time = WeaponAnimation?.PlayFire() ?? 0.5f;
        OnFireCommons();
        PlayFX();
        PlayerNetwork.ReplicateFire(OriginalWeaponType, Vector3.zero, Vector3.zero);
        if (bl_CrosshairBase.Instance != null) bl_CrosshairBase.Instance.OnFire();
        bl_EventHandler.DispatchLocalPlayerFire(GunID);
        isFiring = false;
        return time;
    }

    /// <summary>
    /// A close range attack with the weapon
    /// </summary>
    /// <returns></returns>
    public bool WeaponBash(out float duration)
    {
        duration = 0;
        if (!overrideQuickMeleeAttack || nextBashTime > Time.time) return false;

        nextBashTime = Time.time + Mathf.Min(Info.FireRate * 5, 1); // maybe a exposed variable for this?
        FireOneHitScan(2);

        duration = WeaponAnimation?.PlayFire(bl_WeaponAnimationBase.AnimationFlags.BashAttack) ?? 0.5f;
        PlayerNetwork.ReplicateFire(OriginalWeaponType, Vector3.zero, Vector3.zero, true, FireReplicationFlag.BashAttack);

        PlayFireAudio(bashAttackSound);
        if (bl_CrosshairBase.Instance != null) bl_CrosshairBase.Instance.OnFire();
        bl_EventHandler.DispatchLocalPlayerFire(GunID);
        isFiring = false;
        return true;
    }

    /// <summary>
    /// burst shooting
    /// </summary>
    /// <returns></returns>
    void WeaponBurstFire()
    {
        float time = Time.time;

        if (nextFireTime > time) return;

        int shots = Mathf.Min(roundsPerBurst, bulletsLeft);
        nextFireTime = time + (Info.FireRate * shots) + lagBetweenBurst;

        StartCoroutine(DoBurst());
        IEnumerator DoBurst()
        {
            int shotCounter = 0;
            while (shotCounter < shots)
            {
                shotCounter++;
                FireShell();
                if (blockNextShot)
                {
                    blockNextShot = false;
                    yield break;
                }
                OnFireCommons();
                PlayFX();
                PlayFireAnimation();

                yield return new WaitForSeconds(Info.FireRate);
                if (bulletsLeft <= 0) { break; }
            }

            CheckAutoReload();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void GrenadeFire(bool fastFire = false)
    {
        if (!CheckFireRate()) return;

        isFiring = true;
        if (ThrowByAnimation)
        {
            // when throw by animation, the grenade fire animation has to invoke the grenade throw function with a animation event
            if (fastFire) WeaponAnimation?.PlayFire(bl_WeaponAnimationBase.AnimationFlags.QuickFire);
            else WeaponAnimation?.PlayFire();
            return;
        }

        StartCoroutine(ThrowGrenade(fastFire));
    }

    /// <summary>
    /// fire your launcher
    /// </summary>
    public IEnumerator ThrowGrenade(bool fastFire = false, bool useDelay = true)
    {
        float fireTime = 0;
        // multiple these values to add compatibility with the default inspector values in older MFPS versions
        float throwForce = BulletSpeed * 22;
        float upwardForce = bulletDropFactor * 33;

        if (useDelay)
        {
            nextFireTime = Time.time + Info.FireRate;

            fireTime = fastFire ? WeaponAnimation.PlayFire(bl_WeaponAnimationBase.AnimationFlags.QuickFire) : WeaponAnimation.PlayFire();

            yield return new WaitForSeconds(fastFire ? quickFireDelay : DelayFire);
        }

        Vector3 direction = (PlayerCamera.transform.forward * throwForce) + (m_Transform.parent.up * upwardForce);
        Vector3 origin = muzzlePoint.position;

#if MFPSTPV
        if (bl_CameraViewSettings.IsThirdPerson())
        {
            var tpWeapon = PlayerReferences.playerNetwork.GetCurrentNetworkWeapon();
            if (tpWeapon)
            {
                origin = tpWeapon.GetFirePoint().position;
            }
        }
#endif

        bl_ProjectileBase projectile = FireOneProjectile(direction, origin);
        string uid = bl_StringUtility.GenerateKey();
        projectile.SetUniqueID(uid);

        bulletsLeft--; // subtract one bullet
        UpdateUI();
        Kick();

        PlayerNetwork.ReplicateGrenadeThrow(spread, origin, transform.parent.rotation, direction, uid);
        PlayFireAudio();
        isFiring = false;

        bl_EventHandler.DispatchGameplayPlayerEvent("fire-grenade");

        //hide the grenade mesh when doesn't have ammo
        if (bulletsLeft <= 0)
        {
            OnAmmoLauncher.ForEach(x =>
           {
               if (x != null)
                   x?.SetActive(false);
           });
        }

        //is Auto reload
        if (!fastFire && AutoReload)
        {
            if (useDelay)
            {
                fireTime -= (fastFire ? quickFireDelay : DelayFire);
                yield return new WaitForSeconds(fireTime);
            }
            if (bulletsLeft <= 0 && m_remainingClips > 0)
            {
                Reload(0);
            }
            else if (bulletsLeft <= 0)
            {
                gunManager?.OnReturnWeapon();
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void QuickGrenadeThrow(Action callBack)
    {
        StartCoroutine(FastGrenadeFire());
        IEnumerator FastGrenadeFire()
        {
            float tt = WeaponAnimation.GetAnimationDuration(bl_WeaponAnimationBase.WeaponAnimationType.AimFire)
                + WeaponAnimation.GetAnimationDuration(bl_WeaponAnimationBase.WeaponAnimationType.TakeIn);

            GrenadeFire(true);
            yield return new WaitForSeconds(quickFireTimeMode == WeaponQuickFireTimeMode.AnimationTime ? tt : quickFireTime);
            if (m_remainingClips > 0)
            {
                bulletsLeft++;
                if (!HaveInfinityAmmo) m_remainingClips--;
                UpdateUI();
            }
            callBack();
            StopAllCoroutines();
            gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void QuickMelee(Action callBack)
    {
        StartCoroutine(QuickMeleeSequence(callBack));
        IEnumerator QuickMeleeSequence(Action callBack)
        {
            float tt = MeleeFire();
            WeaponAnimation?.PlayFire(bl_WeaponAnimationBase.AnimationFlags.QuickFire);
            yield return new WaitForSeconds(quickFireTimeMode == WeaponQuickFireTimeMode.AnimationTime ? tt : quickFireTime);
            callBack();
            gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Fires a single projectile from the launcher, triggering associated animations and common firing behaviors.
    /// </summary>
    /// <remarks>This method enforces the fire rate limit and performs necessary actions such as playing the
    /// firing animation,  handling common firing logic, and checking for automatic reload conditions after
    /// firing.</remarks>
    void LauncherFire()
    {
        if (!CheckFireRate()) return;

        FireOneProjectile(Vector3.zero, muzzlePoint.position, true);
        PlayFireAnimation();
        OnFireCommons();
        CheckAutoReload(1);
    }

    /// <summary>
    /// Fires a shell based on the configured shell shot option.
    /// </summary>
    /// <remarks>This method determines the firing behavior based on the value of the <see
    /// cref="shellShotOption"/> field. If the option is set to <see cref="ShellShotOptions.SingleShot"/>, a single
    /// shell is fired.  If the option is set to <see cref="ShellShotOptions.PelletShot"/>, multiple pellets are fired
    /// in a single round,  with the number of pellets determined by the <see cref="pelletsPerShot"/> field.</remarks>
    void FireShell()
    {
        if (shellShotOption == ShellShotOptions.SingleShot) FireOneShot();
        else if (shellShotOption == ShellShotOptions.PelletShot)
        {
            int pelletCounter = 0;  // counter used for pellets per round

            do
            {
                FireOneShot();
                pelletCounter++; // add another pellet         
            } while (pelletCounter < pelletsPerShot); // if number of pellets fired is less then pellets per round... fire more pellets
        }
    }

    /// <summary>
    /// Fires a single shot from the weapon, instantiating a bullet and handling associated effects.
    /// </summary>
    /// <remarks>This method creates a bullet instance, initializes it with the weapon's settings, and
    /// triggers related events such as crosshair updates and network replication. If the weapon is out of ammunition
    /// after firing, it automatically initiates a reload.</remarks>
    public void FireOneShot()
    {
        // set the gun's info into an array to send to the bullet
        BuildBulletData();
        BulletInstanceData instanceData = GetBulletPosition();
        if (!instanceData.CanShoot)
        {
            blockNextShot = true;
            return;
        }

        if (bulletInstanceMethod == BulletInstanceMethod.Pooled)
        {
            instancedBullet = bl_ObjectPoolingBase.Instance.Instantiate(BulletName, instanceData.Position, instanceData.Rotation); // create a bullet
        }
        else
        {
            instancedBullet = Instantiate(bulletPrefab, instanceData.Position, instanceData.Rotation) as GameObject; // create a bullet
        }

        var projectile = instancedBullet.GetComponent<bl_ProjectileBase>();
        // if the bullet is instantiated from the camera center, don't show the trail
        if (instanceData.Flags.IsEnumFlagPresent(BulletInstanceFlags.FromCamera)) projectile.SetFlag(ProjectileFlags.NoTrail);
        projectile.InitProjectile(BulletSettings);// send the gun's info to the bullet
        if (bl_CrosshairBase.Instance != null) bl_CrosshairBase.Instance.OnFire();
        PlayerNetwork.ReplicateFire(OriginalWeaponType, instanceData.ProjectedHitPoint, BulletSettings.Inaccuracity);
        bl_EventHandler.DispatchLocalPlayerFire(GunID);
        if ((bulletsLeft == 0))
        {
            Reload();
        }
    }

    /// <summary>
    /// Fires a single projectile in the specified direction from the given origin.
    /// </summary>
    /// <remarks>The method handles the instantiation and initialization of the projectile, applying the
    /// appropriate  force and direction based on the current settings. If <paramref name="replicate"/> is set to  <see
    /// langword="true"/>, the firing action is replicated using the network system.</remarks>
    /// <param name="direction">The direction in which the projectile should be fired. This may be adjusted internally based on the  <see
    /// cref="projectileDirectionMode"/> setting.</param>
    /// <param name="origin">The starting position of the projectile.</param>
    /// <param name="replicate">A value indicating whether the firing action should be replicated across the network.  If <see
    /// langword="true"/>, the firing event will be synchronized with other players.</param>
    /// <returns>An instance of <see cref="bl_ProjectileBase"/> representing the fired projectile.</returns>
    public bl_ProjectileBase FireOneProjectile(Vector3 direction, Vector3 origin, bool replicate = false)
    {
        BuildBulletData();

        Quaternion rot = CachedTransform.parent.rotation;
        if (projectileDirectionMode == ProjectileDirectionMode.CameraDirection)
        {
            rot = Quaternion.LookRotation(PlayerCamera.transform.forward);
            direction = PlayerCamera.transform.forward * BulletSpeed;
        }

        GameObject projectileInstance;
        if (bulletInstanceMethod == BulletInstanceMethod.Pooled)
        {
            projectileInstance = bl_ObjectPoolingBase.Instance.Instantiate(BulletName, origin, rot); // create a projectile
        }
        else
        {
            projectileInstance = Instantiate(bulletPrefab, origin, rot) as GameObject; // create a projectile
        }

        if (projectileInstance.TryGetComponent<Rigidbody>(out var proRigid) && projectileDirectionMode != ProjectileDirectionMode.HandleByProjectile)
        {
            proRigid.AddForce(direction, projectileForceMode);
        }

        var projectileScript = projectileInstance.GetComponent<bl_ProjectileBase>();
        projectileScript.InitProjectile(BulletSettings);// send the gun's info to the grenade, bl_Projectile.cs  

        if (replicate) PlayerNetwork.ReplicateFire(OriginalWeaponType, direction, rot.eulerAngles);

        bl_EventHandler.DispatchLocalPlayerFire(GunID);

        return projectileScript;
    }

    /// <summary>
    /// Fires a single hitscan shot, applying damage to any valid target within the specified range.
    /// </summary>
    /// <remarks>This method performs a raycast to detect a target within the specified range. If a target is
    /// hit and implements  <see cref="IMFPSDamageable"/>, damage is applied using the weapon's damage settings.
    /// Additionally, if the target  is tagged as a player or bot, a blood effect is instantiated at the point of
    /// impact.</remarks>
    /// <param name="range">The maximum range of the hitscan in world units. If set to a negative value, the default range of the weapon is
    /// used.</param>
    public void FireOneHitScan(float range = -1)
    {
        if (HitScan(out RaycastHit hit, range))
        {
            if (hit.transform.TryGetComponent<IMFPSDamageable>(out var damageable))
            {
                DamageData damageData = new()
                {
                    Damage = this.Damage,
                    OriginPosition = m_Transform.position,
                    From = LocalPlayer.NickName,
                    Cause = DamageCause.Player
                };
                damageData.SetLocalAsActor();
                damageData.SetGunID(GunID);
                damageable.ReceiveDamage(damageData);

                // if the target is a player or bot
                if (hit.transform.CompareTag(bl_MFPS.HITBOX_TAG))
                {
                    bl_ObjectPoolingBase.Instance.Instantiate("blood", hit.point, Quaternion.FromToRotation(Vector3.up, hit.normal));
                }
            }
        }
    }

    /// <summary>
    /// Fire a raycat from the center of the player camera
    /// </summary>
    public bool HitScan(out RaycastHit hit, float customRange = -1)
    {
        Vector3 position = PlayerCamera.transform.position;
        Vector3 direction = PlayerCamera.transform.TransformDirection(Vector3.forward);
        float range = PlayerReferences.cameraRay == null ? Info.Range : Info.Range + PlayerReferences.cameraRay.ExtraRayDistance;
        if (customRange != -1)
        {
            range = customRange + PlayerReferences.cameraRay.ExtraRayDistance;
        }

        return Physics.SphereCast(position, 0.2f, direction, out hit, range);
    }

    /// <summary>
    /// 
    /// </summary>
    public void OnFireCommons()
    {
        PlayFireAudio();
        if (useAmmo) bulletsLeft--;
        UpdateUI();
        Kick();
        Shake();
        PlayerReferences.onWeaponFire?.Invoke(new MFPSPlayerFireEventData()
        {
            AsLocalPlayer = true,
            GunID = GunID,
        });
    }

    /// <summary>
    /// Play muzzleflash effect.
    /// </summary>
    public void PlayFX()
    {
        if (shell != null) shell.Play();

        if (isAiming && !bl_GameData.CoreSettings.showMuzzleFlashWhileAiming)
        {
            return;
        }

        if (weaponFX == null)
        {
            if (muzzleFlash != null) { muzzleFlash.Play(); }
        }
        else weaponFX.PlayFireFX();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public float PlayFireAnimation()
    {
        if (WeaponAnimation == null) return 0;

        //Play fire animation
        return isAiming ? WeaponAnimation.PlayFire(bl_WeaponAnimationBase.AnimationFlags.Aiming) : WeaponAnimation.PlayFire();
    }

    /// <summary>
    /// 
    /// </summary>
    public void CheckAutoReload(float delay = .4f)
    {
        if (bulletsLeft <= 0 && m_remainingClips > 0 && AutoReload)
        {
            Reload(delay);
        }
    }

    /// <summary>
    /// Calculates and returns the position, rotation, and other metadata for a bullet instance based on the player's
    /// current aiming direction and environmental conditions.
    /// </summary>
    /// <remarks>This method determines the appropriate starting position for a bullet, taking into account
    /// whether the bullet can be instantiated from the weapon's muzzle or needs to originate from the player's camera
    /// due to obstacles. It also calculates the projected hit point of the bullet and adjusts its trajectory to ensure
    /// it aligns with the player's aim.  Special handling is applied for third-person view and aiming scenarios to
    /// ensure realistic bullet behavior. The method also accounts for potential obstacles near the bullet's
    /// instantiation point that could interfere with its trajectory.</remarks>
    /// <returns>A <see cref="BulletInstanceData"/> object containing the calculated position, rotation,  projected hit point,
    /// and flags indicating the origin of the bullet (e.g., from the muzzle or camera).</returns>
    public BulletInstanceData GetBulletPosition()
    {
        bulletInstanceData.Flags = BulletInstanceFlags.FromMuzzle;
        bulletInstanceData.Rotation = m_Transform.parent.rotation;
        Vector3 cp = PlayerCamera.transform.position;
        Vector3 mp = muzzlePoint.position;

        if (bl_MFPS.LocalPlayer.IsThirdPersonView())
        {
            var tpWeapon = PlayerReferences.playerNetwork.GetCurrentNetworkWeapon();
            if (tpWeapon)
            {
                mp = tpWeapon.GetFirePoint().position;
            }
        }

        bulletInstanceData.ProjectedHitPoint = cp + (PlayerCamera.transform.forward * 100);
        bulletInstanceData.CanShoot = true;
        var permissions = bl_GameData.CoreSettings.thirdPersonFirePointPermission;

        // if there's a collider between the player camera and the weapon fire point
        // that means the fire point is going through the collider
        if (Physics.Linecast(cp, mp, bl_GameData.TagsAndLayerSettings.LocalPlayerHitableLayers))
        {
            if (permissions.HasFlag(TPFirePointPermission.HitTargetEvenIfBarrelIsObstructed))
            {
                // since we can't shoot from the fire point (since the bullets will go through the collider)
                // the bullet will be instanced from the center of the player camera
                bulletInstanceData.Position = cp;
            }
            else { bulletInstanceData.CanShoot = false; }
        }
        else
        {
            // we can shoot from the fire point
            bulletInstanceData.Position = mp;
        }

        // Now there is a situation where the bullet wont get to hit the desired destination 
        // this happens when the bullet trajectory is hampered by a collider that is near to the bullet instantiation point.
        // in these case the player may not be aiming to that collider but the bullet will hit that collider instead of where the player is aiming to
        // so to fix this we do the following:

        // detect colliders between the fire point and the direction (5 meters long) where the player is aiming to
        if (permissions.HasFlag(TPFirePointPermission.HitTargetIfBarrelIsCloseToObstacle) && Physics.Raycast(bulletInstanceData.Position, PlayerCamera.transform.forward, out hit, 5, bl_GameData.TagsAndLayerSettings.LocalPlayerHitableLayers, QueryTriggerInteraction.Ignore))
        {
            // if indeed there is a collider hampering the trajectory
            // instance the bullet from the camera to make the bullet avoid the obstacle and hit the desired destination.

            bulletInstanceData.ProjectedHitPoint = hit.point;
            bulletInstanceData.Position = cp;
            bulletInstanceData.Flags &= ~BulletInstanceFlags.FromMuzzle;
            bulletInstanceData.Flags |= BulletInstanceFlags.FromCamera;
        }
        else // All clear, not obstacles detected or not allowed to shoot through obstacles
        {
            if (isAiming)
            {
                if (WeaponType != GunType.Sniper)
                {
                    // this result in a more realist bullet shoot effect when aiming but add a sightly inaccuracy on close targets
                    bulletInstanceData.Rotation = Quaternion.LookRotation(bulletInstanceData.ProjectedHitPoint - mp);
                    if (Physics.Raycast(cp, PlayerCamera.transform.forward, out hit, 500, bl_GameData.TagsAndLayerSettings.LocalPlayerHitableLayers, QueryTriggerInteraction.Ignore))
                    {
                        bulletInstanceData.ProjectedHitPoint = hit.point;
                        bulletInstanceData.Rotation = Quaternion.LookRotation(bulletInstanceData.ProjectedHitPoint - mp);

#if MFPSTPV
                        if (bl_CameraViewSettings.IsThirdPerson())
                        {
                            // In third person view, we have to make sure the projected hit point is not behind the weapon fire point
                            if (bl_MathUtility.Distance(hit.point, cp) <= bl_MathUtility.Distance(mp, cp))
                            {
                                // if the projected hit point is behind the fire point, then the bullet raycast has to be fired from the 
                                // actual fire point
                                bulletInstanceData.ProjectedHitPoint = cp + (PlayerCamera.transform.forward * 100);
                                bulletInstanceData.Rotation = m_Transform.parent.rotation;
                            }
                        }
#endif
                    }

                    // this in the other hand make the bullet travel exactly where the player is aiming but the bullets are instanced from the center of the screen
                    // which if you are using some sort of custom effects in the bullets will be noticed (like shooting bullets from the eyes)
                    // but if this is what you need and you are not using custom bullet trails/effects, comment the above lines and uncomment the following one:

                    // bulletInstanceData.Position = cp;
                }
                else
                {
                    // When using a sniper rifle and aiming, always shoot from the camera center for better accuracy
                    bulletInstanceData.Position = cp;
                    bulletInstanceData.Flags &= ~BulletInstanceFlags.FromMuzzle;
                    bulletInstanceData.Flags |= BulletInstanceFlags.FromCamera;
                }
            }
            else
            {
                // in third person we have to always get the direction from the camera
                if (bl_MFPS.LocalPlayer.IsThirdPersonView())
                {
                    if (Physics.Raycast(cp, PlayerCamera.transform.forward, out hit, 500, bl_GameData.TagsAndLayerSettings.LocalPlayerHitableLayers, QueryTriggerInteraction.Ignore))
                    {

                        // In third person view, we have to make sure the projected hit point is not behind the weapon fire point
                        if (bl_MathUtility.Distance(hit.point, cp) > bl_MathUtility.Distance(mp, cp))
                        {
                            bulletInstanceData.ProjectedHitPoint = hit.point;
                            bulletInstanceData.Rotation = Quaternion.LookRotation(bulletInstanceData.ProjectedHitPoint - mp);
                        }
                        else
                        {
                            // if the projected hit point is behind the fire point, then the bullet raycast has to be fired from the 
                            // actual fire point
                        }
                    }
                }
            }
        }

        return bulletInstanceData;
    }
    #endregion

    /// <summary>
    /// Aiming control
    /// </summary>
    void Aim()
    {
        if (isAiming && !isReloading)
        {
            CurrentPos = AimPosition; //Place in the center ADS
            currentRotation = aimRotation;
            currentZoom = Zoom; //create a zoom camera
            PlayerReferences.weaponSway.UseAimSettings();
            spreadMinMax = defaultSpreadRange * spreadAimMultiplier;
        }
        else // if not aiming
        {
            CurrentPos = DefaultPos; //return to default gun position
            currentRotation = defaultRotation;
            currentZoom = PlayerReferences.DefaultCameraFOV; //return to default fog
            PlayerReferences.weaponSway.ResetSettings();
            spreadMinMax = defaultSpreadRange;
        }
        float delta = Time.deltaTime;

        if (aimSpring != null)
        {
            aimSpring.PositionSpring.Target = CurrentPos;
            aimSpring.RotationSpring.Target = currentRotation;
        }
        else
        {
            m_Transform.SetLocalPositionAndRotation(useSmooth ? Vector3.Lerp(m_Transform.localPosition, CurrentPos, delta * AimSpeed) : //with smooth effect
         Vector3.MoveTowards(m_Transform.localPosition, CurrentPos, delta * AimSpeed), Quaternion.Slerp(m_Transform.localRotation, Quaternion.Euler(currentRotation), delta * AimSpeed));
        }

        if (!BlockAimFoV)
        {
            PlayerReferences.cameraMotion.AddCameraFOV(-1, currentZoom);
        }

        if (lastAimState != isAiming)
        {
            bl_EventHandler.DispatchLocalAimEvent(isAiming);
            if (bl_CrosshairBase.Instance != null) bl_CrosshairBase.Instance.OnAim(isAiming);
            lastAimState = isAiming;
            PlayClip(aimSound, isAiming ? 1 : 0.97f, 0.33f);
            if (isAiming)
            {
                if (aimSpring != null)
                {
                    aimSpring.PositionSpring.SetMultipliers(2, 2);
                    aimSpring.RotationSpring.SetMultipliers(2, 2);
                }
                PlayerReferences.cameraMotion.PlayCurveShake("fast-shake-2", 0.3f, 10, 0.1f);
            }
            else
            {
                if (aimSpring != null)
                {
                    aimSpring.PositionSpring.SetMultipliers(1, 1);
                    aimSpring.RotationSpring.SetMultipliers(1, 1);
                }
            }
        }
        if (aimSpring != null) aimSpring.Update();
    }

    /// <summary>
    /// Fetch the information that will be attached to the next fired bullet
    /// containing all required information from the weapon from which the bullet was shoot
    /// and the information about the player who shoot it
    /// </summary>
    public BulletData BuildBulletData()
    {
        BulletSettings.Damage = Damage;
        BulletSettings.ImpactForce = impactForce;
        if (WeaponType == GunType.Sniper && !isAiming)
        {
            BulletSettings.SetInaccuracity(spread * 3, spreadMinMax.y * 3);
        }
        else
        {
            BulletSettings.SetInaccuracity(spread, spreadMinMax.y);
        }
        BulletSettings.Speed = BulletSpeed;
        BulletSettings.WeaponName = Info.Name;
        BulletSettings.Position = PlayerReferences.transform.localPosition;
        BulletSettings.WeaponID = GunID;
        BulletSettings.isNetwork = false;
        BulletSettings.DropFactor = bulletDropFactor;
        BulletSettings.Range = Range;
        BulletSettings.ActorViewID = bl_MFPS.LocalPlayer.ViewID;
        BulletSettings.MFPSActor = bl_MFPS.LocalPlayer.MFPSActor;
        BulletSettings.IsLocalPlayer = true;
        BulletSettings.EffectivenessDropOff = effectivinessDropoff;
        BulletSettings.EffectiveFiringRange = effectiveFiringRange;
        BulletSettings.DragCoefficient = dragCoefficient;

        return BulletSettings;
    }

    /// <summary>
    /// send kick back to mouse look
    /// when is fire
    /// </summary>
    public void Kick()
    {
        if (recoilSettings == null)
        {
            PlayerReferences.recoil.SetRecoil(new bl_RecoilBase.RecoilData() { Amount = WeaponRecoil, Speed = RecoilSpeed, MaxValue = 5 });
        }
        else
        {
            PlayerReferences.recoil.SetRecoil(new bl_RecoilBase.RecoilData(recoilSettings, extraRecoil));
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void Shake()
    {
        float influence = isAiming ? 0.5f : 1;
        bl_EventHandler.DoPlayerCameraShake(shakerPresent, "fpweapon", influence);
    }

    public void DoMeeleAttack()
    {
        MeleeFire();
    }

    /// <summary>
    /// 
    /// </summary>
    void OnReload()
    {
        if (!CanReload) return;

        Reload();
    }

    /// <summary>
    /// 
    /// </summary>
    public void Reload(float delay = 0.2f)
    {
        if (isReloading || !gameObject.activeInHierarchy)
            return;

        StopCoroutine(nameof(DoReload));
        StartCoroutine(nameof(DoReload), delay);
    }

    /// <summary>
    /// start reload weapon
    /// deduct the remaining bullets in the cartridge of a new clip
    /// as this happens, we disable the options: fire, aim and run
    /// </summary>
    /// <returns></returns>
    IEnumerator DoReload(float waitTime = 0.2f)
    {
        CanFire = false;

        if (isReloading)
            yield break; // if already reloading... exit and wait till reload is finished

        // Delay before start reloading
        yield return new WaitForSeconds(waitTime);

        if (m_remainingClips > 0 || inReloadMode)//if have at least one cartridge
        {
            isReloading = true; // we are now reloading
            bl_EventHandler.DispatchGameplayPlayerEvent("start-reloading");
            bl_EventHandler.Weapon.onLocalReload?.Invoke(ReloadTime);

            if (WeaponAnimation != null)
            {
                if (reloadPer == ReloadPer.Bullet)//insert one bullet at a time
                {
                    int t_repeat = BulletsPerMagazine - bulletsLeft; //get the number of spent bullets
                    int add = (m_remainingClips >= t_repeat) ? t_repeat : (int)m_remainingClips;
                    WeaponAnimation?.PlayReload(ReloadTime, new int[1] { add }, bl_WeaponAnimationBase.AnimationFlags.SplitReload);

                    // Since this reload method const of multiple animation parts, the reload time
                    // will be determined by the animations, the reload will end once the last animation has been played.
                    yield break;
                }
                else
                {
                    bl_WeaponAnimationBase.AnimationFlags flags = bl_WeaponAnimationBase.AnimationFlags.None;
                    if (_bulletLeft <= 0) flags |= bl_WeaponAnimationBase.AnimationFlags.EmptyReload;
                    WeaponAnimation?.PlayReload(ReloadTime, null, flags);
                }
            }

            if (!SoundReloadByAnim)
            {
                StartCoroutine(ReloadSoundIE());
            }

            if (!inReloadMode)
            {
                if (ammoReserveType == AmmoReserveType.Clips)
                {
                    if (!HaveInfinityAmmo) m_remainingClips--;
                }
            }

            if (WeaponType == GunType.Grenade) { OnAmmoLauncher.ForEach(x => { if (x != null) x?.SetActive(true); }); }
            yield return new WaitForSeconds(ReloadTime); // wait for reload time
            FillMagazine();

            bl_EventHandler.DispatchGameplayPlayerEvent("finish-reloading");
        }
        UpdateUI();
        isReloading = inReloadMode = false; // done reloading
        CanAiming = CanFire = true;
    }

    /// <summary>
    /// 
    /// </summary>
    public void FillMagazine()
    {
        if (ammoReserveType == AmmoReserveType.Clips)
        {
            bulletsLeft = BulletsPerMagazine; // fill up the gun
        }
        else
        {
            int need = BulletsPerMagazine - bulletsLeft;
            int add = (m_remainingClips >= need) ? need : (int)m_remainingClips;
            if (!HaveInfinityAmmo) m_remainingClips -= add;
            bulletsLeft += add;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="bullet"></param>
    public void AddBullet(int bullet)
    {
        if (ammoReserveType == AmmoReserveType.Bullets)
        {
            m_remainingClips -= bullet;
        }
        bulletsLeft += bullet;
        UpdateUI();
    }

    /// <summary>
    /// Set unlimited ammo to this weapon
    /// </summary>
    /// <param name="infinity"></param>
    public void SetInifinityAmmo(bool infinity)
    {
        HaveInfinityAmmo = infinity;
        bulletsLeft = BulletsPerMagazine;
        m_remainingClips = ammoReserveType == AmmoReserveType.Bullets ? BulletsPerMagazine * m_remainingClips : (int)m_remainingClips;
        if (gameObject.activeInHierarchy)
            UpdateUI();
    }

    /// <summary>
    /// Sync Weapon state for Upper animations
    /// </summary>
    void DetermineUpperState()
    {
        if (PlayerNetwork == null)
            return;

        if (isFiring && !isReloading)
        {
            FPState = (isAiming) ? PlayerFPState.FireAiming : PlayerFPState.Firing;
        }
        else if (isAiming && !isFiring && !isReloading)
        {
            FPState = PlayerFPState.Aiming;
        }
        else if (isReloading)
        {
            FPState = PlayerFPState.Reloading;
        }
        else if (controller.IsSprinting() && !isReloading && !isFiring && !isAiming)
        {
            FPState = PlayerFPState.Running;
        }
        else
        {
            FPState = PlayerFPState.Idle;
        }
        PlayerNetwork.FPState = FPState;
    }

    /// <summary>
    /// Snap the weapon to the aim position.
    /// </summary>
    public void SetToAim()
    {
        m_Transform.SetLocalPositionAndRotation(AimPosition, Quaternion.Euler(aimRotation));
    }

    #region Audio
    /// <summary>
    /// 
    /// </summary>
    public void PlayFireAudio(AudioClip customFireSound = null)
    {
        var clip = customFireSound != null ? customFireSound : FireAudioClip;
        if (PlayerReferences.fireAudioSource != null)
        {
            PlayerReferences.fireAudioSource.Play(clip, 1, UnityEngine.Random.Range(fireSoundPitch.x, fireSoundPitch.y));

            if (boltSound != null && !SoundReloadByAnim)
            {
                StartCoroutine(DelayFireSound());
            }
            return;
        }

        FireSource.clip = clip;
        FireSource.pitch = UnityEngine.Random.Range(fireSoundPitch.x, fireSoundPitch.y);
        FireSource.Play();

        if (boltSound != null && !SoundReloadByAnim)
        {
            StartCoroutine(DelayFireSound());
        }
    }

    /// <summary>
    /// most shotguns have the sound of shooting and then reloading
    /// </summary>
    /// <returns></returns>
    IEnumerator DelayFireSound()
    {
        yield return new WaitForSeconds(delayForSecondFireSound);
        if (boldAudioSource == null)
        {
            boldAudioSource = gameObject.AddComponent<AudioSource>();
            boldAudioSource.volume = FireSource.volume;
            boldAudioSource.AssignMixerGroup("FP Weapon");
        }

        boldAudioSource.clip = boltSound;
        boldAudioSource.Play();
    }

    /// <summary>
    /// 
    /// </summary>
    public void FinishReload()
    {
        if (ammoReserveType == AmmoReserveType.Clips)
        {
            if (!HaveInfinityAmmo) m_remainingClips--;
        }
        isReloading = false; // done reloading
        CanAiming = true;
        CanFire = true;
        inReloadMode = false;
    }

    /// <summary>
    /// 
    /// </summary>
    public void PlayClip(AudioClip clip, float pitch = 1, float volumeMul = 1)
    {
        if (clip == null) return;

        Source.clip = clip;
        Source.pitch = pitch;
        Source.volume = defaultVolume * volumeMul;
        Source.Play();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="part"></param>
    public void PlayReloadAudio(int part)
    {
        if (SoundReloadByAnim) return;

        if (part == 0) PlayClip(ReloadSound);
        else if (part == 1) PlayClip(ReloadSound2);
        else if (part == 2) PlayClip(ReloadSound3);
    }

    /// <summary>
    /// 
    /// </summary>
    public void PlayEmptyFireSound()
    {
        if (WeaponType != GunType.Melee && DryFireSound != null)
        {
            PlayClip(DryFireSound);
        }
    }

    /// <summary>
    /// use this method to various sounds reload.
    /// if you have only 1 sound, them put only one in inspector
    /// and leave empty other box
    /// </summary>
    /// <returns></returns>
    IEnumerator ReloadSoundIE()
    {
        float t_time = ReloadTime / 3;
        if (ReloadSound != null)
        {
            PlayClip(ReloadSound);
            gunManager?.HeadAnimation(1, t_time);
        }
        if (ReloadSound2 != null)
        {
            if (WeaponType == GunType.Shotgun)
            {
                int t_repeat = BulletsPerMagazine - bulletsLeft;
                for (int i = 0; i < t_repeat; i++)
                {
                    yield return new WaitForSeconds((t_time / t_repeat) + 0.025f);
                    PlayClip(ReloadSound2);
                }
            }
            else
            {
                yield return new WaitForSeconds(t_time);
                PlayClip(ReloadSound2);
            }
        }
        if (ReloadSound3 != null)
        {
            yield return new WaitForSeconds(t_time);
            PlayClip(ReloadSound3);
            gunManager?.HeadAnimation(2, t_time);
        }
        yield return new WaitForSeconds(0.65f);
        gunManager?.HeadAnimation(0, t_time);
    }
    #endregion

    /// <summary>
    /// When we disable the gun ship called the animation
    /// and disable the basic functions
    /// </summary>
    public float DisableWeapon(bool isFastKill = false)
    {
        CanAiming = false;
        if (isReloading) { inReloadMode = true; isReloading = false; }
        CanFire = false;
        gunManager?.HeadAnimation(0, 1);
        if (PlayerCamera == null) { PlayerCamera = PlayerReferences.playerCamera; }
        if (!isFastKill) { StopAllCoroutines(); }

        return WeaponAnimation != null ? WeaponAnimation.PlayTakeOut() : 0;
    }

    /// <summary>
    /// 
    /// </summary>
    void DrawComplete()
    {
        CanFire = bl_GameManager.Instance == null || !bl_GameManager.Instance.GameFinish;
        CanAiming = true;
        if (inReloadMode)
        {
            if (WeaponType == GunType.Grenade)
            {
                FillMagazine();
                inReloadMode = false;
                UpdateUI();
            }
            else
            {
                Reload(0.2f);
            }
        }
    }

    /// <summary>
    /// When round is end we can't fire
    /// </summary>
    void OnRoundEnd()
    {
        isEnabled = false;
        CanFire = false;
        CanAiming = false;
        isAiming = false;
    }

    /// <summary>
    /// 
    /// </summary>
    public bool OnPickUpAmmo(bl_EventHandler.Gameplay.AmmoPickUpData data)
    {
        //if this is not a global ammo but for a specific weapon
        if (data.SupplyMode == WeaponSupplyingMode.ForSpecificWeapon)
        {
            //and this is not the weapon that is for
            if (data.GunID != GunID) return false;
        }
        else if (data.SupplyMode == WeaponSupplyingMode.ForSpecificAmmoType)
        {
            if (data.AmmoType != ammoType) return false;
        }

        if (WeaponType == GunType.Melee) return false;
        if (ammoReserveType == AmmoReserveType.Clips)
        {
            //max number of clips
            if (m_remainingClips >= maxNumberOfClips) return false;

            m_remainingClips += Mathf.FloorToInt(data.Bullets / BulletsPerMagazine);
            if (m_remainingClips > maxNumberOfClips)
            {
                m_remainingClips = maxNumberOfClips;
            }
        }
        else
        {
            if (WeaponType == GunType.Grenade)
            {
                if (bulletsLeft <= 0) bulletsLeft = 1;
                else m_remainingClips += data.Projectiles;

                new MFPSLocalNotification(string.Format("+{0} {1}", data.Projectiles.ToString(), Info.Name));
            }
            else
            {
                if (m_remainingClips >= BulletsPerMagazine * maxNumberOfClips) return false;

                int oldCount = m_remainingClips;
                m_remainingClips += data.Bullets;
                m_remainingClips = Mathf.Clamp(m_remainingClips, 0, BulletsPerMagazine * maxNumberOfClips);
                new MFPSLocalNotification(string.Format("+{0} {1} Bullets", m_remainingClips - oldCount, Info.Name));
            }
        }
        if (isActiveAndEnabled) UpdateUI();
        return true;
    }

#if MFPSM
    private void OnMobileButton(MFPS.Mobile.FPSMobileButton buttonType)
    {
        if (buttonType == MFPS.Mobile.FPSMobileButton.Fire) OnFire();
        else if (buttonType == MFPS.Mobile.FPSMobileButton.Reload) OnReload();
    }
#endif

    /// <summary>
    /// 
    /// </summary>
    /// <param name="newPoint"></param>
    public void OverrideMuzzlePoint(Transform newPoint)
    {
        defaultMuzzlePoint = muzzlePoint;
        muzzlePoint = newPoint;
    }

    /// <summary>
    /// Enable/Disable the weapon renders/meshes
    /// </summary>
    public void SetWeaponRendersActive(bool active)
    {
        weaponRenders ??= m_Transform.GetComponentsInChildren<Renderer>();

        foreach (var item in weaponRenders)
        {
            if (item == null) continue;
            item.gameObject.SetActive(active);
        }
#if CUSTOMIZER
        var customizerWeapon = GetComponent<bl_CustomizerWeapon>();
        if (active && customizerWeapon != null && customizerWeapon.ApplyOnStart)
        {
            customizerWeapon.LoadAttachments();
            customizerWeapon.ApplyAttachments();
        }
#endif
        onWeaponRendersActive?.Invoke(active);
    }

    /// <summary>
    /// Adds a delay to the next allowable firing time.
    /// </summary>
    /// <remarks>This method updates the internal timer to prevent firing until the specified delay has
    /// elapsed.  It is typically used to enforce a cooldown period between actions.</remarks>
    /// <param name="delay">The amount of delay, in seconds, to add to the next firing time. Must be a non-negative value.</param>
    public void AddDelayToNextFire(float delay)
    {
        nextFireTime = Time.time + delay;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private bool CheckFireRate()
    {
        bool passed = nextFireTime <= Time.time;
        if (passed)
        {
            nextFireTime = Time.time + Info.FireRate;
        }
        return passed;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public float GetReloadTime()
    {
        if (reloadPer == ReloadPer.Bullet && WeaponAnimation != null)
        {
            int missingBullets = BulletsPerMagazine - bulletsLeft; //get the number of spent bullets
            int add = (m_remainingClips >= missingBullets) ? missingBullets : (int)m_remainingClips;
            float[] bullets = { add };
            return WeaponAnimation.GetAnimationDuration(bl_WeaponAnimationBase.WeaponAnimationType.Reload, bullets);
        }
        return ReloadTime;
    }

    void CancelFiring() { isFiring = false; }
    void CancelReloading() { WeaponAnimation?.CancelAnimation(bl_WeaponAnimationBase.WeaponAnimationType.Reload); }
    public void ResetDefaultMuzzlePoint() { if (defaultMuzzlePoint != null) muzzlePoint = defaultMuzzlePoint; }
    public void UpdateUI()
    {
        if (bl_EquippedWeaponUIBase.Instance != null)
            bl_EquippedWeaponUIBase.Instance.SetAmmoOf(this);

        bl_EventHandler.Weapon.onLocalAmmoChanged?.Invoke();
    }
    public void SetDefaultWeaponCameraFOV(float fov) => WeaponCamera.fieldOfView = fov;
    public Vector3 GetDefaultPosition() => DefaultPos;
    public bool HasMeeleAttack() => overrideQuickMeleeAttack;

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public override FireType GetFireType() => currentFireMode;

    #region Getters
    /// <summary>
    /// The current <see cref="GunType"/>
    /// This can change if the weapon fire type changes in runtime.
    /// </summary>
    public GunType WeaponType
    {
        get
        {
            if (currentGunType == GunType.None) currentGunType = Info.Type;
            return currentGunType;
        }
        set
        {
            currentGunType = value;
        }
    }

    /// <summary>
    /// The default <see cref="GunType"/> defined in GameData
    /// </summary>
    public GunType OriginalWeaponType
    {
        get => Info.Type;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public override GunType GetWeaponType() => OriginalWeaponType;

    private float BaseSpread
    {
        get { return PlayerReferences.firstPersonController.State == PlayerState.Crouching ? spreadMinMax.x * 0.5f : spreadMinMax.x; }
    }

    public int GetCompactClips { get { return (m_remainingClips / BulletsPerMagazine); } }

    /// <summary>
    /// Determine if the player can shoot this weapon.
    /// </summary>
    public bool CanFire
    {
        get
        {
            return (bulletsLeft > 0 && m_canFire && !isReloading && FireWhileRun);
        }
        set => m_canFire = value;
    }

    public bool FireRatePassed { get { return (Time.time - nextFireTime) > Info.FireRate; } }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public bool AllowQuickFire()
    {
        if (bulletsLeft == 0 && m_remainingClips > 0 && FireRatePassed)
        {
            bulletsLeft++;
            if (!HaveInfinityAmmo) m_remainingClips--;
        }

        return m_AllowQuickFire && bulletsLeft > 0 && FireRatePassed;
    }

    /// <summary>
    ///  Determine if the player can aiming with this weapon in the current state
    /// </summary>
    public bool CanAiming
    {
        get
        {
            return !allowAim
                ? false
                : controller.GetRunToAimBehave() == PlayerRunToAimBehave.BlockAim && controller.IsSprinting() ? false : m_canAim;
        }
        set => m_canAim = value;
    }

    /// <summary>
    /// Determine if the player can reload this weapon
    /// </summary>
    bool CanReload
    {
        get
        {
            bool can = false;
            if (bulletsLeft < BulletsPerMagazine && m_remainingClips > 0 && ReloadWhileRun() && !isReloading)
            {
                can = true;
            }
            if (WeaponType == GunType.Melee && nextFireTime < Time.time)
            {
                can = false;
            }
            return can;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    bool ReloadWhileRun()
    {
        return bl_GameData.CoreSettings.CanReloadWhileRunning || !controller.IsSprinting();
    }

    /// <summary>
    /// 
    /// </summary>
    bool FireWhileRun
    {
        get
        {
            return bl_GameData.CoreSettings.CanFireWhileRunning || !controller.IsSprinting();
        }
    }

    public bool CanBeTaken() { return canBeTakenWhenIsEmpty || bulletsLeft > 0 || m_remainingClips > 0; }
    public bool IsOutOfAmmo() => bulletsLeft <= 0 && m_remainingClips <= 0;

    public bl_FirstPersonControllerBase controller => PlayerReferences.firstPersonController;
    public bl_PlayerNetworkBase PlayerNetwork => PlayerReferences.playerNetwork;
    private bl_GunManager gunManager => PlayerReferences.gunManager;

    private bl_PlayerReferences _playerReferences;
    public bl_PlayerReferences PlayerReferences
    {
        get
        {
            if (_playerReferences == null) _playerReferences = transform.root.GetComponent<bl_PlayerReferences>();
            return _playerReferences;
        }
    }

    private bl_WeaponAnimationBase _anim;
    public bl_WeaponAnimationBase WeaponAnimation
    {
        get
        {
            if (_anim == null) { _anim = GetComponentInChildren<bl_WeaponAnimationBase>(); }
            return _anim;
        }
    }
    #endregion

#if UNITY_EDITOR
    public bool _aimRecord = false;
    public Vector3 _defaultPosition = new(-100, 0, 0);
    public Vector3 _defaultRotation = new(-100, 0, 0);
    public int _editorTabId = 0;
    private void OnDrawGizmos()
    {
        if (muzzlePoint != null)
        {
            Gizmos.color = new Color(0, 1, 0, 0.4f);
            Gizmos.DrawSphere(muzzlePoint.position, 0.022f);
            Gizmos.color = Color.white;
        }
    }
#endif
}