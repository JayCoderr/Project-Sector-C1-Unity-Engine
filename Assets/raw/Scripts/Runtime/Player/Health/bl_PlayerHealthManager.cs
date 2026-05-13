using MFPS.Audio;
using MFPS.Core.Motion;
using MFPSEditor;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using UnityEngine;
#if ACTK_IS_HERE
using CodeStage.AntiCheat.ObscuredTypes;
#endif

public class bl_PlayerHealthManager : bl_PlayerHealthManagerBase
{
    #region Public members
    [Header("Settings")]
    [Range(0, 100)] public int health = 100;
    [Range(1, 100)] public int maxHealth = 100;
    [Range(1, 10)] public float StartRegenerateIn = 4f;
    [Range(1, 5)] public float RegenerationSpeed = 3f;
    [Range(10, 100)] public int RegenerateUpTo = 100;

    [Header("Shake")]
    [ScriptableDrawer] public ShakerPresent damageShakerPresent;

    [Header("Effects")]
    public AudioClip[] HitsSound;
    [SerializeField] private AudioClip[] InjuredSounds = null;

    private bool m_HealthRegeneration = false;
    public bool HealthRegeneration { get => m_HealthRegeneration; set => m_HealthRegeneration = value; }
    #endregion

    #region Private members
    private bool isDead = false;
    private string lastDamageGiverActor;
    private float TimeToRegenerate = 4;
    private bool isSuscribed = false;
    private int protecTime = 0;
    private RepetingDamageInfo repetingDamageInfo;
    private bool showIndicator = false;
    private float nextHealthSend = 0;
    private bl_PlayerReferences playerReferences;
#if !ACTK_IS_HERE
    private int currentHealth = 0;
#else
    private ObscuredInt currentHealth = 0;
#endif
    #endregion

    #region Unity Callbacks
    /// <summary>
    /// 
    /// </summary>
    protected override void Awake()
    {
        if (!bl_PhotonNetwork.IsConnected) return;

        base.Awake();
        TryGetComponent(out playerReferences);
        m_HealthRegeneration = bl_GameData.CoreSettings.HealthRegeneration;
        protecTime = bl_GameData.CoreSettings.SpawnProtectedTime;
        showIndicator = bl_GameData.CoreSettings.showDamageIndicator;
    }

    /// <summary>
    /// 
    /// </summary>
    void Start()
    {
        if (!IsConnected)
            return;

        SetCurrentHealth(health);
        if (IsMine)
        {
            bl_MFPS.LocalPlayer.IsAlive = true;
            gameObject.name = bl_PhotonNetwork.NickName;
        }

        protecTime = bl_Hook.ApplyFilters("SpawnProtectedTime", protecTime);
        if (protecTime > 0) { InvokeRepeating(nameof(OnProtectCount), 1, 1); }
    }

    /// <summary>
    /// 
    /// </summary>
    protected override void OnEnable()
    {
        base.OnEnable();
        if (IsMine)
        {
            bl_MFPS.LocalPlayer.ViewID = photonView.ViewID;
            bl_EventHandler.Gameplay.onPickUpHealth += OnPickUp;
            bl_EventHandler.onRoundEnd += OnRoundEnd;
            bl_PhotonCallbacks.PlayerEnteredRoom += OnPhotonPlayerConnected;
            isSuscribed = true;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    protected override void OnDisable()
    {
        base.OnDisable();
        if (isSuscribed)
        {
            bl_EventHandler.Gameplay.onPickUpHealth -= OnPickUp;
            bl_EventHandler.onRoundEnd -= OnRoundEnd;
            bl_PhotonCallbacks.PlayerEnteredRoom -= OnPhotonPlayerConnected;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public override void OnUpdate()
    {
        if (IsMine)
        {
            RegenerateHealth();
        }
    }
    #endregion

    /// <summary>
    /// Applies damage to the player based on the provided damage data.
    /// </summary>
    /// <remarks>This method handles various conditions under which damage can or cannot be applied, such as
    /// friendly fire settings,  protection states, and the current match state. If damage is successfully applied, it
    /// is synchronized across the network.</remarks>
    /// <param name="damageData">The data representing the damage to be applied, including the source and amount of damage.</param>
    public override void DoDamage(DamageData damageData)
    {
        bool canDamage = true;
        if (!DamageEnabled && !bl_GameManager.IsDamageDisable)
        {
            //Fix: bots can't damage Master Client teammates.
            if (damageData.MFPSActor != null && (damageData.MFPSActor.Team != playerReferences.PlayerTeam || bl_RoomSettings.Instance.CurrentRoomInfo.friendlyFire))
            {
                canDamage = true;
            }
            else canDamage = false;
        }

        if (!canDamage || IsProtectionEnable || bl_GameManager.IsDamageDisable || (bl_GameManager.Instance != null && bl_GameManager.Instance.GameMatchState == MatchState.Waiting))
        {
            if (!IsMine)
            {
                playerReferences.playerAnimations.OnGetHit();
            }
            return;
        }

        if (bl_PhotonNetwork.IsConnectedInRoom) photonView.RPC(nameof(SyncDamage), RpcTarget.AllBuffered, damageData);
        else SyncDamage(damageData, new PhotonMessageInfo(PhotonNetwork.LocalPlayer, PhotonNetwork.ServerTimestamp, null));
    }

    /// <summary>
    /// Synchronizes damage received by the player across the network and processes the effects of the damage.
    /// </summary>
    /// <remarks>This method is invoked via a network Remote Procedure Call (RPC) to ensure that damage is
    /// synchronized  across all clients in the game. It handles various scenarios such as local player damage effects, 
    /// lethal damage, and damage caused by bots or other players.   The method also triggers appropriate events, such
    /// as camera shake, damage indicators, and sound effects,  depending on the context of the damage. If the player's
    /// health reaches zero, the player is marked as dead  and the death sequence is initiated.</remarks>
    /// <param name="damageData">The data representing the damage, including the amount, source, and cause.</param>
    /// <param name="messageInfo">Information about the network message, including the sender of the damage.</param>
    [PunRPC]
    void SyncDamage(DamageData damageData, PhotonMessageInfo messageInfo)
    {
        if (isDead || IsProtectionEnable)
            return;

        Player sender = messageInfo.Sender;

        if (DamageEnabled)
        {
            if (IsMine)
            {
                if (bl_GameData.CoreSettings.cameraShakeOnDamage) bl_EventHandler.DoPlayerCameraShake(damageShakerPresent, "damage");
                if (showIndicator) bl_DamageIndicatorBase.Instance?.SetHit(new bl_DamageIndicatorBase.HitInfo()
                {
                    Direction = damageData.OriginPosition,
                    Actor = damageData.From
                });
                TimeToRegenerate = bl_Hook.ApplyFilters("TimeToRegenerateHealth", StartRegenerateIn);
                bl_EventHandler.Player.onlocalPlayerReceiveDamage?.Invoke(damageData.Damage);
            }
            else
            {
                bool isLethal = (currentHealth - damageData.Damage) < 1;
                var hitData = new MFPSHitData()
                {
                    HitTransform = transform,
                    HitPosition = transform.position,
                    Damage = damageData.Damage,
                    HitName = gameObject.name,
                    PlayerAutorName = damageData.MFPSActor != null ? damageData.MFPSActor.Name : string.Empty,
                    WasLethal = isLethal,
                    OriginPosition = damageData.OriginPosition,
                };

                // When the damage is given by the local player to another player
                if (sender != null && sender.ActorNumber == bl_PhotonNetwork.LocalPlayer.ActorNumber && damageData.Cause != DamageCause.Bot)
                {
                    if (bl_CrosshairBase.Instance != null) bl_CrosshairBase.Instance.OnHit(isLethal);
                    bl_AudioController.PlayClip(isLethal ? "head-hit" : "body-hit");
                    bl_EventHandler.DispatchLocalPlayerHitEnemy(hitData);
                }
                // if the hit was caused by a bot
                else if (!damageData.IsActorARealPlayer())
                {
                    bl_EventHandler.Bots.onBotHitPlayer(hitData);
                }
            }

            var hitSounds = InjuredSounds;
            if (damageData.Cause.HasFlag(DamageCause.Player | DamageCause.Bot)) hitSounds = HitsSound;
            bl_AudioController.GetAudioInstance().SetRandomClip(hitSounds).SetSpatialBlend(0).SetRange(5, 10).Play();

            lastDamageGiverActor = damageData.From;
        }

        if (currentHealth > 0)
        {
            SetCurrentHealth(currentHealth - damageData.Damage);
            if (!IsMine)
            {
                playerReferences.playerAnimations.OnGetHit();
            }
        }

        if (currentHealth < 1)
        {
            SetCurrentHealth(0);

            PlayerDie(true, lastDamageGiverActor, damageData.IsHeadShot, damageData.Cause, damageData.GunID, damageData.OriginPosition, sender);
        }
    }

    /// <summary>
    /// Handles the player's death, synchronizing the event across the network if required.
    /// </summary>
    /// <remarks>If <paramref name="syncToAll"/> is <see langword="true"/> and the player is in a networked
    /// room, the death event is synchronized to all players using network RPC. For the local player, this method updates
    /// the player's alive status and dispatches a local death event.</remarks>
    /// <param name="syncToAll">Indicates whether the death event should be synchronized to all players in the room.</param>
    /// <param name="killer">The name of the player or entity responsible for the death.</param>
    /// <param name="headShot">A value indicating whether the death was caused by a headshot. <see langword="true"/> if it was a headshot;
    /// otherwise, <see langword="false"/>.</param>
    /// <param name="cause">The cause of the player's death, represented as a <see cref="DamageCause"/>.</param>
    /// <param name="gunID">The identifier of the weapon used to cause the death.</param>
    /// <param name="hitPos">The position in world space where the player was hit.</param>
    /// <param name="sender">The <see cref="Player"/> object representing the player who caused the death.</param>
    [PunRPC]
    void PlayerDie(bool syncToAll, string killer, bool headShot, DamageCause cause, int gunID, Vector3 hitPos, Player sender)
    {
        isDead = true;
        if (syncToAll && bl_PhotonNetwork.IsConnectedInRoom)
        {
            if (bl_PhotonNetwork.IsMasterClient) photonView.RPC(nameof(PlayerDie), RpcTarget.All, false, lastDamageGiverActor, headShot, cause, gunID, hitPos, sender);
            return;
        }

        if (IsMine)
        {
            bl_MFPS.LocalPlayer.IsAlive = false;
            bl_EventHandler.DispatchPlayerLocalDeathEvent();
        }

        Die(killer, headShot, cause, gunID, hitPos, sender);
    }

    /// <summary>
    /// Handles the player's death, including updating game state, triggering events, and managing visual effects.
    /// </summary>
    /// <remarks>This method performs several actions upon the player's death: <list type="bullet">
    /// <item>Updates the player's state to indicate they are no longer alive.</item> <item>Triggers death-related
    /// events, such as notifying the game manager and invoking callbacks.</item> <item>Handles visual effects,
    /// including enabling ragdoll physics and disabling player-related objects.</item> <item>Updates game statistics,
    /// such as kill counts and death counts, depending on the context.</item> <item>Manages kill feed notifications and
    /// kill camera setup for the local player.</item> </list> This method is called both for local and remote players,
    /// with specific logic depending on whether the player is controlled locally or remotely.</remarks>
    /// <param name="killer">The name of the player or entity responsible for the kill.</param>
    /// <param name="isHeadshot">A value indicating whether the death was caused by a headshot. <see langword="true"/> if it was a headshot;
    /// otherwise, <see langword="false"/>.</param>
    /// <param name="cause">The cause of the player's death, such as a weapon, explosion, or environmental factor.</param>
    /// <param name="gunID">The identifier of the weapon used to kill the player. This is used to retrieve weapon-specific information.</param>
    /// <param name="hitPos">The position where the player was hit, typically used for visual effects like ragdoll physics.</param>
    /// <param name="sender">The player object representing the entity that caused the death. This may include additional metadata about the
    /// killer.</param>
	void Die(string killer, bool isHeadshot, DamageCause cause, int gunID, Vector3 hitPos, Player sender)
    {
        isDead = true;
        transform.parent = null;
        playerReferences.characterController.enabled = false;
        if (bl_GameManager.Instance != null) bl_GameManager.Instance.GetMFPSPlayer(gameObject.name).isAlive = false;
        var gameData = bl_GameData.Instance;
        bl_GunInfo gunInfo = gameData.GetWeapon(gunID);
        bool isExplosion = gunInfo.Type == GunType.Grenade || gunInfo.Type == GunType.Launcher;
        playerReferences.onDie?.Invoke();

        if (!IsMine)
        {
            // convert into ragdoll the remote player
            playerReferences.playerRagdoll.Ragdolled(new bl_PlayerRagdollBase.RagdollInfo()
            {
                ForcePosition = hitPos,
                IsFromExplosion = isExplosion,
                AutoDestroy = true
            });
        }
        else
        {
            Transform ngr = (gameData.coreSettings.DropWeaponOnDeath == false) ? null : playerReferences.RemoteWeapons != null ? playerReferences.RemoteWeapons.transform : null;
            if (ngr != null) { ngr.gameObject.SetActive(false); }

            playerReferences.playerRagdoll.SetLocalRagdoll(new bl_PlayerRagdollBase.RagdollInfo()
            {
                ForcePosition = hitPos,
                Velocity = playerReferences.characterController.velocity,
                IsFromExplosion = isExplosion,
                AutoDestroy = true,
                RightHandChild = ngr
            });
        }

        //disable all other player prefabs child's
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(false);
        }

        string weapon = cause.ToString();
        if (cause == DamageCause.Player || cause == DamageCause.Bot || cause == DamageCause.Explosion)
        {
            weapon = gunInfo.Name;
        }

        var mplayer = new MFPSPlayer(photonView, true, false);
        var eDeathData = new bl_EventHandler.PlayerDeathData()
        {
            Player = mplayer,
            KillerName = killer
        };
        bl_EventHandler.onPlayerDeath?.Invoke(eDeathData);

        if (!IsMine)// when player is not ours
        {
            //if the local player was the one who kill this player
            if (lastDamageGiverActor == LocalName)
            {
                AddKill(isHeadshot, weapon, gunID, cause, hitPos);
            }

            // Show an icon on the map where the player died (only if is a teammate)
            if (playerReferences.IsTeamMateOfLocalPlayer() && GetGameMode.GetGameModeInfo().showDeathIconsOnTeammates)
            {
                bl_ObjectPoolingBase.Instance.Instantiate("deathicon", transform.position, transform.rotation);
            }

            bl_EventHandler.DispatchRemotePlayerDeath(eDeathData);
        }
        else //when is the local player who have been eliminated
        {

            //Set to respawn again
            if (GetGameMode.GetGameModeInfo().onPlayerDie == GameModeSettings.OnPlayerDie.SpawnAfterDelay)
            {
                bl_Hook.DoFirstAction("Action_RespawnLocalPlayer");
            }

            if (cause == DamageCause.Bot)
            {
                // increase the deaths count for the local player
                if (gameData.coreSettings.howConsiderBotsEliminations == MFPS.Runtime.AI.BotKillConsideration.SameAsRealPlayers)
                    bl_PhotonNetwork.LocalPlayer.PostDeaths(1);
            }
            else
            {
                // increase the deaths count for the local player
                bl_PhotonNetwork.LocalPlayer.PostDeaths(1);
            }

            var killCamInfo = new bl_KillCamBase.KillCamInfo()
            {
                TargetName = killer,
                GunID = gunID,
                Target = transform,
                FallbackTarget = playerReferences.playerRagdoll.transform
            };
            if (sender.TryGetProp("cardID", out object value)) killCamInfo.SetProperty("cardID", value);
            killCamInfo = bl_Hook.ApplyFilters("Filter_KillCamInfo", killCamInfo);

            //Show the kill camera
            if (bl_KillCamBase.Instance != null) bl_KillCamBase.Instance.SetTarget(killCamInfo).SetActive(true);

            // if the local player kill himself
            if (killer == LocalName)
            {
                string reasonStr = cause == DamageCause.FallDamage ? bl_GameTexts.DeathByFall.Localized(20) : bl_GameTexts.CommittedSuicide.Localized(19);
                bl_KillFeedBase.Instance?.SendTeamHighlightMessage(LocalName, reasonStr, bl_MFPS.LocalPlayer.Team);
            }

            // if who killed this player was a bot
            if (cause == DamageCause.Bot)
            {
                Team botTeam = Team.All;
                if (!isOneTeamMode) botTeam = bl_MFPS.LocalPlayer.Team.OppsositeTeam();
                var feed = new bl_KillFeedBase.FeedData()
                {
                    LeftText = killer,
                    RightText = gameObject.name,
                    Team = botTeam
                };
                feed.AddData("gunid", gunID);
                feed.AddData("headshot", isHeadshot);

                bl_KillFeedBase.Instance?.SendKillMessageEvent(feed);
                //the local player will update the bot stats instead of the Master in this case.
                bl_AIMananger.SetBotKill(killer);
            }

            StartCoroutine(DestroyThis());
        }
    }

    /// <summary>
    /// Records a kill event, updates the player's score, and dispatches relevant notifications and events.
    /// </summary>
    /// <remarks>This method handles kill feed updates, score calculations, and local notifications for the
    /// kill event.  It also ensures that friendly fire kills do not contribute to the player's score.</remarks>
    /// <param name="isHeadshot">Indicates whether the kill was a headshot. <see langword="true"/> if it was a headshot; otherwise, <see
    /// langword="false"/>.</param>
    /// <param name="m_weapon">The name of the weapon used to perform the kill.</param>
    /// <param name="gunID">The unique identifier of the weapon used. Special weapons have negative IDs.</param>
    /// <param name="cause">The cause of the damage that resulted in the kill.</param>
    /// <param name="direction">The direction from which the damage originated, represented as a <see cref="Vector3"/>.</param>
    public void AddKill(bool isHeadshot, string m_weapon, int gunID, DamageCause cause, Vector3 direction)
    {
        // if is an special weapon
        if (gunID >= 300)
        {
            gunID -= 299;
            gunID = -gunID;
        }

        //send kill feed kill message
        var feed = new bl_KillFeedBase.FeedData()
        {
            LeftText = LocalName,
            RightText = gameObject.name,
            Team = bl_PhotonNetwork.LocalPlayer.GetPlayerTeam()
        };
        feed.AddData("gunid", gunID);
        feed.AddData("headshot", isHeadshot);

        bl_KillFeedBase.Instance.SendKillMessageEvent(feed);

        // if this was a friendly fire kill, don't add the score
        if (bl_MFPS.LocalPlayer.Team == playerReferences.PlayerTeam) return;

        //Add a new kill and update the player information
        bl_PhotonNetwork.LocalPlayer.PostKill(1);

        int headShotScore = bl_Hook.ApplyFilters("Filter_HeadShotScore", bl_GameData.ScoreSettings.ScorePerHeadShot);
        // calculate the score gained for this kill
        int score = bl_Hook.ApplyFilters("Filter_KillScore", bl_GameData.ScoreSettings.ScorePerKill);
        if (isHeadshot)
        {
            bl_GameManager.Headshots++;
            score += headShotScore;
        }

        //show an local notification for the kill
        var localKillInfo = new KillInfo
        {
            Killer = lastDamageGiverActor,
            Killed = gameObject.name,
            byHeadShot = isHeadshot,
            KillMethod = m_weapon,
            GunID = gunID,
            Cause = cause,
            FromPosition = direction,
            ToPosition = transform.position
        };
        bl_EventHandler.DispatchLocalKillEvent(localKillInfo);

        var elimatedTeam = photonView.Owner.GetPlayerTeam();
        if (isOneTeamMode)
        {
            // add the score to the player total gained score in this match
            bl_PhotonNetwork.LocalPlayer.PostScore(score);
        }
        else if (elimatedTeam != Team.All && elimatedTeam != bl_PhotonNetwork.LocalPlayer.GetPlayerTeam())
        {
            // add the score to the player total gained score in this match
            bl_PhotonNetwork.LocalPlayer.PostScore(score);
        }
    }

    /// <summary>
    /// Do constant damage to the player in a loop until cancel.
    /// </summary>
    public override void DoRepetingDamage(RepetingDamageInfo info)
    {
        repetingDamageInfo = info;
        InvokeRepeating(nameof(MakeDamageRepeting), 0, info.Rate);
    }

    /// <summary>
    /// Applies repeated damage based on the current repeating damage configuration.
    /// </summary>
    /// <remarks>If the repeating damage configuration is null, the operation cancels any ongoing repeating
    /// damage. Otherwise, it applies damage using the specified damage data, defaulting to a new instance with default
    /// values if the damage data is not provided.</remarks>
    void MakeDamageRepeting()
    {
        if (repetingDamageInfo == null)
        {
            CancelRepetingDamage();
            return;
        }

        var damageinfo = repetingDamageInfo.DamageData;
        damageinfo ??= new DamageData
        {
            OriginPosition = Vector3.zero,
            Cause = DamageCause.Map
        };
        damageinfo.Damage = repetingDamageInfo.Damage;

        DoDamage(damageinfo);
    }

    /// <summary>
    /// 
    /// </summary>
    public override void CancelRepetingDamage()
    {
        CancelInvoke(nameof(MakeDamageRepeting));
    }

    /// <summary>
    /// This function is called every second to regenerate the health of the player
    /// </summary>
    void RegenerateHealth()
    {
        if (!m_HealthRegeneration || currentHealth < 1) return;
        if (currentHealth >= RegenerateUpTo) return;

        int projectedHealth = currentHealth;
        if (TimeToRegenerate <= 0)
        {
            projectedHealth = currentHealth;
            projectedHealth += 1;
        }
        else
        {
            TimeToRegenerate -= Time.deltaTime * 1.15f;
        }

        if (Time.time - nextHealthSend >= (1 / RegenerationSpeed))
        {
            nextHealthSend = Time.time;
            if (bl_PhotonNetwork.IsConnectedInRoom) photonView.RPC(nameof(PickUpHealth), RpcTarget.All, projectedHealth);
            else PickUpHealth(projectedHealth);
        }
    }

    /// <summary>n
    /// Make the local player kill himself
    /// </summary>
    public override bool Suicide()
    {
        if (!IsMine || !bl_MFPS.LocalPlayer.IsAlive) return false;
        if (IsProtectionEnable) return false;

        DamageData e = new()
        {
            Damage = 500,
            From = base.LocalName,
            OriginPosition = transform.position,
            IsHeadShot = false
        };
        DoDamage(e);
        return true;
    }

    /// <summary>
    /// This method handles the protection countdown for the player.
    /// </summary>
    void OnProtectCount()
    {
        protecTime--;
        if (IsMine)
        {
            if (bl_UIReferences.Instance != null) bl_UIReferences.Instance.OnSpawnCount(protecTime);
        }
        if (protecTime <= 0)
        {
            CancelInvoke(nameof(OnProtectCount));
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    IEnumerator DestroyThis()
    {
        yield return new WaitForSeconds(0.3f);
        DestroyEntity();
    }

    /// <summary>
    /// Destroy this entity
    /// </summary>
    public override void DestroyEntity()
    {
        PhotonNetwork.Destroy(this.gameObject);
    }

    /// <summary>
    /// This event is called when player pick up a med kit
    /// </summary>
    /// <param name="amount"> amount for sum at current health</param>
    void OnPickUp(int amount)
    {
        SetHealth(amount);
    }

    /// <summary>
    /// Updates the current health of the entity by adding or replacing the specified amount.
    /// </summary>
    /// <remarks>This method has no effect if the entity's current health is less than 1 or if the entity is
    /// marked as dead. If the health exceeds the maximum allowed health, it is capped at the maximum value. The method
    /// synchronizes the health update across the network for other players.</remarks>
    /// <param name="amount">The amount of health to add or set. If <paramref name="replace"/> is <see langword="true"/>, this value replaces
    /// the current health.</param>
    /// <param name="replace">A value indicating whether the specified <paramref name="amount"/> should replace the current health. If <see
    /// langword="false"/>, the amount is added to the current health.</param>
    public override void SetHealth(int amount, bool replace = false)
    {
        if (currentHealth < 1 || isDead) return;

        if (photonView.IsMine)
        {
            amount = bl_Hook.ApplyFilters("Filter_HealthPickUpAmount", amount);
            int newHealth = currentHealth + amount;
            if (replace) newHealth = amount;

            SetCurrentHealth(newHealth);
            if (currentHealth > maxHealth)
            {
                SetCurrentHealth(maxHealth);
            }
            photonView.RPC(nameof(PickUpHealth), RpcTarget.OthersBuffered, newHealth);
        }
    }

    [PunRPC]
    void RpcSyncHealth(int newHealth, PhotonMessageInfo info)
    {
        if (info.photonView.ViewID == photonView.ViewID)
        {
            SetCurrentHealth((int)newHealth);
        }
    }

    /// <summary>
    /// Sync Health when pick up a med kit.
    /// </summary>
    [PunRPC]
    void PickUpHealth(int t_amount)
    {
        if (currentHealth < 1 || isDead) return;

        SetCurrentHealth((int)t_amount);
        if (currentHealth > maxHealth)
        {
            SetCurrentHealth(maxHealth);
        }
    }

    /// <summary>
    /// Set the current health of this player localy.
    /// Calling this function directly will not sync the health with other players.
    /// </summary>
    /// <param name="newHealth"></param>
    private void SetCurrentHealth(int newHealth)
    {
        if (currentHealth != newHealth)
        {
            currentHealth = newHealth;
            if (IsMine) bl_EventHandler.Player.onLocalHealthChanged?.Invoke(currentHealth, maxHealth);
        }
    }

    private bool IsProtectionEnable { get { return (protecTime > 0); } }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public override int GetHealth() => currentHealth;

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public override int GetMaxHealth() => maxHealth;

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public override bool IsDeath()
    {
        return isDead;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="newPlayer"></param>
    public void OnPhotonPlayerConnected(Player newPlayer)
    {
        if (photonView.IsMine)
        {
            photonView.RPC(nameof(RpcSyncHealth), newPlayer, (int)currentHealth);
        }
    }

    /// <summary>
    /// When round is end 
    /// desactive some functions
    /// </summary>
    void OnRoundEnd()
    {
        DamageEnabled = false;
    }
}