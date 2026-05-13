using MFPS.Audio;
using MFPS.Runtime.AI;
using MFPSEditor;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using UnityEngine;

public class bl_AIShooterAttack : bl_AIShooterAttackBase
{
    #region Public members
    [Header("Settings")]
    [LovattoToogle] public bool ForceFireWhenTargetClose = true;
    [Range(0.1f, 5)] public float maxCooldownFireTime = 4;
    [Range(0, 5)] public int Grenades = 3;
    [SerializeField, Range(10, 100)] private float GrenadeSpeed = 50;
    [Tooltip("The minimum/maximum distance to throw a grenade, if the target is closer or farther than this range, the bot will not throw a grenade.")]
    [SerializeField] private Vector2 grenadeThrowRange = new(20, 40);
    [Range(0, 1), Tooltip("The probability of throwing a grenade instead of fire the equiped weapon, 1 = 100% = one grenade each 5 shots")]
    public float grenadeProbability = 0.05f;
    [Tooltip("The minimum number of shots to throw a grenade, this is to avoid the bot to throw grenades too often.")]
    public int minFollowingShotsForGrenade = 7;

    [Header("Weapons")]
    [ScriptableDrawer] public bl_BotWeaponContainer weaponsContainer;

    [Header("References")]
    [SerializeField] private GameObject[] throwables = null;
    [SerializeField] private Transform GrenadeFirePoint = null;
    [SerializeField] private AudioSource FireSource = null;
    #endregion

    public override bool IsFiring
    {
        get;
        set;
    }

    #region Private members
    private int bullets;
    private bool isReloading = false;
    private float nextAttackTime;
    private int FollowingShoots = 0;
    private bl_AIShooterReferences AiReferences;
    private GameObject bullet;
    private int WeaponID = -1;
    private readonly BulletData m_BulletData = new();
    private bl_AIShooterAgent AI;
    private bl_NetworkGun currentWeapon;
#if UMM
    private bl_MiniMapEntity miniMapItem;
#endif 
    #endregion

    /// <summary>
    /// 
    /// </summary>
    private void Awake()
    {
        TryGetComponent(out AiReferences);
        AI = GetComponent<bl_AIShooterAgent>();
        bl_PhotonCallbacks.PlayerEnteredRoom += OnPhotonPlayerConnected;
        bl_PhotonCallbacks.LeftRoom += OnLocalLeftRoom;
#if UMM
        miniMapItem = GetComponent<bl_MiniMapEntity>();
#endif
    }

    /// <summary>
    /// 
    /// </summary>
    void Start()
    {
        nextAttackTime = Time.time;

        SelectWeapon();
        if (FireSource != null && bl_AudioController.Instance != null)
        {
            FireSource.maxDistance = bl_AudioController.Instance.maxWeaponDistance;
            FireSource.spatialBlend = 1;
            FireSource.rolloffMode = bl_AudioController.Instance.audioRolloffMode;
            FireSource.minDistance = bl_AudioController.Instance.maxWeaponDistance * 0.09f;
            FireSource.spatialize = true;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private void OnDisable()
    {
        bl_PhotonCallbacks.PlayerEnteredRoom -= OnPhotonPlayerConnected;
        bl_PhotonCallbacks.LeftRoom -= OnLocalLeftRoom;
    }

    /// <summary>
    /// 
    /// </summary>
    void OnLocalLeftRoom()
    {
        Destroy(gameObject);
    }

    /// <summary>
    /// 
    /// </summary>
    private void SelectWeapon()
    {
        if (!bl_PhotonNetwork.IsMasterClient) return;

        WeaponID = Random.Range(0, weaponsContainer.weapons.Count);

        // if the game mode only allow a specific weapon type
        if (bl_RoomSettings.GetRoomInfo().WeaponOption != 0)
        {
            GunType onlyType = bl_GameData.Instance.allowedWeaponOnlyOptions[bl_RoomSettings.GetRoomInfo().WeaponOption - 1];
            int indexOf = weaponsContainer.weapons.FindIndex(x =>
            {
                return bl_GameData.Instance.GetWeapon(x.GunID).Type == onlyType;
            });
            if (indexOf != -1)
            {
                WeaponID = indexOf;
            }
            else
            {
                Debug.LogWarning($"The selected weapon type ({onlyType}) is not available or not supported for the bot, the bot will use a random weapon.");
            }
        }

        photonView.RPC(nameof(SyncAIWeapon), RpcTarget.All, WeaponID);
    }

    /// <summary>
    /// 
    /// </summary>
    public override void Fire(FireReason fireReason = FireReason.Normal)
    {
        if (!CanFire || AI.IsTargetHampered || isReloading || bl_MatchTimeManagerBase.Instance.IsTimeUp())
            return;

        // check if the target is close and if the bot is forced to confront
        if (ForceFireWhenTargetClose && AI.CachedTargetDistance < AI.soldierSettings.closeRange) { fireReason = FireReason.Forced; }

        // if the bot is not forced to fire
        if (fireReason == FireReason.Normal)
        {
            // and the target is not in front of the bot
            if (!AI.IsTargetInSight)
                return;
        }

        if (currentWeapon == null || nextAttackTime > Time.time) return;

        if (AI.HasATarget)
        {
            if (AI.Target.IsDeath)
            {
                AI.KillTheTarget();
                return;
            }
            else
            {
                AI.Target.OnAttacked(AI.References);
            }

            // make sure the bot is facing the target
            if (!AI.IsTargetInFieldOfView)
            {
                // don't fire if the target is not in sight
                return;
            }
        }

        if (Grenades > 0 && grenadeThrowRange.IsWithinRange(AI.TargetDistance) && FollowingShoots > minFollowingShotsForGrenade)
        {
            if (CheckProbability(grenadeProbability))
            {
                int throwableId = Random.Range(0, throwables.Length);
                ThrowBotGrenade(true, Vector3.zero, Vector3.zero, throwableId);
                nextAttackTime = Time.time + 3.3f;
                return;
            }
        }

        AiReferences.PlayerAnimator.Play($"Fire{GetCurrentGunInfo().Type}", 1, 0);
        if (GetAttackRange() == AttackerRange.Far) nextAttackTime = Time.time + (GetCurrentGunInfo().FireRate * Random.Range(2, 5));
        else if (fireReason == FireReason.OnMove) nextAttackTime = Time.time + (GetCurrentGunInfo().FireRate * Random.Range(2, 4));
        else if (fireReason == FireReason.LongRange) nextAttackTime = Time.time + (GetCurrentGunInfo().FireRate * Random.Range(3, 5));
        else nextAttackTime = Time.time + GetCurrentGunInfo().FireRate;

        switch (GetCurrentGunInfo().Type)
        {
            case GunType.Shotgun:
                int bulletCounter = 0;
                do
                {
                    FireSingleProjectile(FirePoint.position, AI.TargetPosition, false);
                    bulletCounter++;
                } while (bulletCounter < currentWeapon.LocalGun.roundsPerBurst);
                break;
            default:
                FireSingleProjectile(FirePoint.position, AI.TargetPosition, false);
                break;
        }
        PlayFireEffects();
        bullets--;
        FollowingShoots++;
        photonView.RPC(nameof(RpcAIFire), RpcTarget.Others, FirePoint.position, AI.TargetPosition);

        if (bullets <= 0)
        {
            BotReload(true);
        }
        else
        {
            if (FollowingShoots > GetMaxFollowingShots())
            {
                if (!CheckProbability(AI.soldierSettings.longBurtsProbability))
                {
                    float cooldownTime = Random.Range(0.01f, maxCooldownFireTime);
                    if (fireReason == FireReason.Forced)
                    {
                        // if the bot is forced to fire, the cooldown time is reduced
                        cooldownTime = Random.Range(0.01f, maxCooldownFireTime * 0.3f);
                    }

                    nextAttackTime += cooldownTime;
                    FollowingShoots = 0;
                }
            }
        }
        IsFiring = true;
    }

    /// <summary>
    /// 
    /// </summary>
    private void FireSingleProjectile(Vector3 origin, Vector3 direction, bool remote)
    {
        if (currentWeapon == null) return;

        bullet = bl_ObjectPoolingBase.Instance.Instantiate(currentWeapon.LocalGun.BulletName, origin, transform.root.rotation);
        bullet.transform.LookAt(direction);
        //build bullet data
        m_BulletData.Damage = GetCurrentGunInfo().Damage;
        m_BulletData.isNetwork = remote;
        m_BulletData.Position = transform.position;
        m_BulletData.WeaponID = currentWeapon.GetWeaponID;

        // if the bot is a sniper
        if (GetAttackRange() == AttackerRange.Far)
        {
            float movingMul = AI.VelocityMagnitud > 0.1f ? 2f : 1f;
            m_BulletData.SetInaccuracity(0.5f * movingMul, 2 * movingMul);
        }
        else if (AI.behaviorSettings.weaponAccuracy == AIWeaponAccuracy.Pro)
        {
            m_BulletData.SetInaccuracity(2, 3);
        }
        else if (AI.behaviorSettings.weaponAccuracy == AIWeaponAccuracy.Casual)
        {
            m_BulletData.SetInaccuracity(3, 6);
        }
        else m_BulletData.SetInaccuracity(5, 10);

        m_BulletData.Speed = 300;
        m_BulletData.Range = GetCurrentGunInfo().Range;
        m_BulletData.WeaponName = GetCurrentGunInfo().Name;
        m_BulletData.ActorViewID = photonView.ViewID;
        m_BulletData.MFPSActor = AI.BotMFPSActor;
        bullet.GetComponent<bl_ProjectileBase>().InitProjectile(m_BulletData);

        AI.AIShooterReferences.onWeaponFire?.Invoke(new MFPSPlayerFireEventData()
        {
            AsLocalPlayer = false,
            GunID = currentWeapon.GetWeaponID
        });
    }

    /// <summary>
    /// 
    /// </summary>
    private void PlayFireEffects()
    {
        currentWeapon.PlayMuzzleflash();
        currentWeapon.PlayLocalFireAudio();
#if UMM
        ShowMiniMapItem();
#endif
    }

    /// <summary>
    /// 
    /// </summary>
    public override void OnDeath()
    {
        StopAllCoroutines();

#if UMM
        if (miniMapItem != null)
        {
            miniMapItem.DestroyIcon(false);
        }
#endif
    }

    /// <summary>
    /// 
    /// </summary>
    [PunRPC]
    public void ThrowBotGrenade(bool calledFromLocal, Vector3 velocity, Vector3 forward, int throwableId)
    {
        if (calledFromLocal)
        {
            velocity = GetVelocity(AI.TargetPosition);
            forward = AI.TargetPosition;

            Vector3 backoffPos = AI.GetRandomPointBehind(5);
            if (backoffPos != Vector3.zero)
            {
                AI.SetDestination(backoffPos, 0, false, 4);
            }

            photonView.RPC(nameof(ThrowBotGrenade), RpcTarget.All, false, velocity, forward, throwableId);
            return;
        }

        StartCoroutine(DoThrow());
        IEnumerator DoThrow()
        {
            AiReferences.PlayerAnimator.Play("FireGrenade", 1, 0);
            nextAttackTime = Time.time + GetCurrentGunInfo().FireRate;
            yield return new WaitForSeconds(0.4f);
            GameObject bullet = Instantiate(throwables[throwableId], GrenadeFirePoint.position, transform.root.rotation) as GameObject;

            m_BulletData.Damage = 100;
            m_BulletData.isNetwork = !bl_PhotonNetwork.IsMasterClient;
            m_BulletData.Position = transform.position;
            m_BulletData.WeaponID = 3;
            m_BulletData.SetInaccuracity(2, 3);
            m_BulletData.Speed = GrenadeSpeed;
            m_BulletData.Range = 10;
            m_BulletData.EffectiveFiringRange = 3;
            m_BulletData.WeaponName = throwables[throwableId].name;
            m_BulletData.ActorViewID = photonView.ViewID;
            m_BulletData.MFPSActor = new MFPSPlayer(photonView, false, true);
            bullet.GetComponent<bl_ProjectileBase>().InitProjectile(m_BulletData);//bl_Projectile.cs

            Rigidbody r = bullet.GetComponent<Rigidbody>();
            r.linearVelocity = velocity;
            r.AddRelativeTorque(Vector3.right * -5500.0f);
            forward -= r.transform.position;
            r.transform.forward = forward;
            Grenades--;
#if UMM
            ShowMiniMapItem();
#endif
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    [PunRPC]
    void BotReload(bool calledFromLocal)
    {
        if (calledFromLocal)
        {
            if (bullets == currentWeapon.LocalGun.bulletsPerClip || isReloading) return;

            CanFire = false;
            AI.OnUnableToFight();
            photonView.RPC(nameof(BotReload), RpcTarget.All, false);
            return;
        }

        if (isReloading) return;

        StartCoroutine(DoReload());
        IEnumerator DoReload()
        {
            isReloading = true;
            CanFire = false;
            var Anim = AiReferences.PlayerAnimator;
            float reloadTime = GetCurrentGunInfo().ReloadTime;
            // just a small delay to let the fire animation finish
            yield return new WaitForSeconds(0.15f);
            Anim.SetInteger("UpperState", 2);
            yield return new WaitForEndOfFrame();
            // get the current animation state
            AnimatorStateInfo info = Anim.GetNextAnimatorStateInfo(1);
            if (info.IsName("Reload"))
            {
                Anim.SetFloat("ReloadSpeed", info.length / reloadTime);
            }
            yield return new WaitForSeconds(reloadTime);
            Anim.SetInteger("UpperState", 0);
            Anim.SetFloat("ReloadSpeed", 1);
            bullets = currentWeapon.LocalGun.bulletsPerClip;
            CanFire = true;
            isReloading = false;
        }
    }

    [PunRPC]
    void RpcAIFire(Vector3 pos, Vector3 look)
    {
        if (currentWeapon == null) return;

        AiReferences.PlayerAnimator.SetInteger("UpperState", 0);
        AiReferences.PlayerAnimator.Play($"Fire{currentWeapon.Info.Type}", 1, 0);
        switch (currentWeapon.Info.Type)
        {
            case GunType.Shotgun:
                int bulletCounter = 0;
                do
                {
                    FireSingleProjectile(pos, look, true);
                    bulletCounter++;
                } while (bulletCounter < currentWeapon.LocalGun.roundsPerBurst);
                break;
            default:
                FireSingleProjectile(pos, look, true);
                break;
        }
        PlayFireEffects();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="ID"></param>
    [PunRPC]
    void SyncAIWeapon(int ID)
    {
        WeaponID = ID;
        int selectedGunID = weaponsContainer.weapons[ID].GunID;
        currentWeapon = AiReferences.remoteWeapons.GetWeapon(selectedGunID, AiReferences.PlayerAnimator.avatar);
        if (currentWeapon == null)
        {
            Debug.LogWarning($"Weapon with ID {selectedGunID} not found in the weapon container, most likely it has not been integrated yet.");
            return;
        }

        currentWeapon.SetActive(true);
        bullets = currentWeapon.LocalGun.bulletsPerClip;

        if (AiReferences != null) AiReferences.PlayerAnimator.SetInteger("GunType", (int)currentWeapon.Info.Type);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="target"></param>
    /// <returns></returns>
    private Vector3 GetVelocity(Vector3 target)
    {
        Vector3 velocity;
        Vector3 toTarget = target - transform.position;
        float speed = 15;
        // Set up the terms we need to solve the quadratic equations.
        float gSquared = Physics.gravity.sqrMagnitude;
        float b = (speed * speed) + Vector3.Dot(toTarget, Physics.gravity);
        float discriminant = (b * b) - (gSquared * toTarget.sqrMagnitude);

        // Check whether the target is reachable at max speed or less.
        if (discriminant < 0)
        {
            velocity = toTarget;
            velocity.y = 0;
            velocity.Normalize();
            velocity.y = 0.7f;

            Debug.DrawRay(transform.position, velocity * 3.0f, Color.blue);

            velocity *= speed;
            return velocity;
        }

        float discRoot = Mathf.Sqrt(discriminant);

        // Highest shot with the given max speed:
        float T_max = Mathf.Sqrt((b + discRoot) * 2f / gSquared);

        // Convert from time-to-hit to a launch velocity:
        velocity = (toTarget / T_max) - (Physics.gravity * T_max / 2f);

        return velocity;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public override AttackerRange GetAttackRange()
    {
        if (currentWeapon == null) return AttackerRange.Medium;

        if (currentWeapon.Info.Type == GunType.Sniper)
        {
            return AttackerRange.Far;
        }
        else if (currentWeapon.Info.Type == GunType.Shotgun)
        {
            return AttackerRange.Close;
        }
        else
        {
            return AttackerRange.Medium;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    IEnumerator PlayReloadSound()
    {
        var sounds = GetCurrentWeaponData().ReloadSounds;
        for (int i = 0; i < sounds.Length; i++)
        {
            FireSource.clip = sounds[i];
            FireSource.Play();
            yield return new WaitForSeconds(0.5f);
        }
    }

#if UMM
    void ShowMiniMapItem()
    {
        if (AI.IsTeamMate) return;
#if KSA
        if (bl_KillStreakHandler.Instance != null && bl_KillStreakHandler.Instance.ActiveAUVs > 0) return;
#endif
        if (miniMapItem != null && !miniMapItem.isTeamMateBot())
        {
            CancelInvoke(nameof(HideMiniMapItem));
            miniMapItem.SetActiveIcon(true);
            Invoke(nameof(HideMiniMapItem), 0.25f);
        }
    }

    void HideMiniMapItem()
    {
        if (AI.IsTeamMate) return;
        if (miniMapItem != null)
        {
            miniMapItem.SetActiveIcon(false);
        }
    }
#endif

    public void OnPhotonPlayerConnected(Player newPlayer)
    {
        if (bl_PhotonNetwork.IsMasterClient && WeaponID != -1)
        {
            photonView.RPC(nameof(SyncAIWeapon), newPlayer, WeaponID);
        }
    }

    private int GetMaxFollowingShots()
    {
        return GetCurrentWeaponData().MaxFollowingShots;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public override bl_NetworkGun GetEquippedWeapon()
    {
        return currentWeapon;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="probability"></param>
    /// <returns></returns>
    private bool CheckProbability(float probability)
    {
        return Random.value <= probability;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public override Vector3 GetFirePosition()
    {
        return FirePoint.position;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public bl_BotWeaponContainer.BotWeaponData GetCurrentWeaponData()
    {
        return weaponsContainer.weapons[WeaponID];
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public bl_GunInfo GetCurrentGunInfo()
    {
        return currentWeapon.Info;
    }

    /// <summary>
    /// 
    /// </summary>
    public Transform FirePoint
    {
        get
        {
            return currentWeapon != null ? currentWeapon.GetFirePoint() : transform;
        }
    }
}