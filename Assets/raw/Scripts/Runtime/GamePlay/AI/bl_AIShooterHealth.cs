using MFPS.Audio;
using MFPS.Runtime.AI;
using Photon.Pun;
using UnityEngine;

/// <summary>
/// Handle all relating to the bot health
/// This script has not direct references, you can replace with your own script
/// Simply make sure to inherit your script from <see cref="bl_PlayerHealthManagerBase"/>
/// </summary>
public class bl_AIShooterHealth : bl_PlayerHealthManagerBase
{

    [Range(10, 500)] public int Health = 100;

    #region Private members
    private bl_AIShooter m_AIShooter;
    private int LastActorEnemy = -1;
    private RepetingDamageInfo repetingDamageInfo;
    private bl_AIShooterReferences references;
    private bl_AIShooter shooterAgent;
    #endregion

    /// <summary>
    /// 
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        TryGetComponent(out references);
        m_AIShooter = references.aiShooter;
        shooterAgent = references.aiShooter;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="damageData"></param>
    public override void DoDamage(DamageData damageData)
    {
        if (m_AIShooter.IsDeath)
            return;

        //if was not killed by a listed weapon
        if (damageData.CauseBySpecialWeapon(out int specialId))
        {
            //try to find the custom weapon name
            var iconData = bl_KillFeedBase.Instance.GetCustomIconByIndex(specialId);
            if (iconData != null)
            {
                damageData.SpecialWeaponName = $"cmd:{iconData.Name}";
            }
        }

        if (damageData.MFPSActor == null)
        {
            Debug.LogWarning($"Bot receive damage but actor was not provided from ViewID: {damageData.ActorViewID}");
        }

        if (!isOneTeamMode)
        {
            if (!bl_RoomSettings.Instance.CurrentRoomInfo.friendlyFire && damageData.MFPSActor.Team == m_AIShooter.AITeam) return;
        }

        photonView.RPC(nameof(RpcDoDamage), RpcTarget.All, damageData);
    }

    /// <summary>
    /// 
    /// </summary>
    [PunRPC]
    void RpcDoDamage(DamageData damageData)
    {
        if (m_AIShooter.IsDeath) return;

        string weaponName;
        if (damageData.CauseBySpecialWeapon(out int _))
        {
            weaponName = "cmd:" + damageData.SpecialWeaponName;
        }
        else
        {
            weaponName = bl_GameData.Instance.GetWeapon(damageData.GunID).Name;
        }

        // fix an issue were bots hit themself with an explosion and start shooting themself after.
        bool selfDamage = !damageData.IsActorARealPlayer() && damageData.ActorViewID == photonView.ViewID;
        if (selfDamage && !weaponName.Contains("Grenade")) return;

        Health -= damageData.Damage;
        if (LastActorEnemy != damageData.ActorViewID && !selfDamage)
        {
            if (shooterAgent != null)
                shooterAgent.FocusOnSingleTarget = false;
        }
        if (!selfDamage) LastActorEnemy = damageData.ActorViewID;

        if (bl_PhotonNetwork.IsMasterClient && !selfDamage)
        {
            shooterAgent?.OnGetHit(damageData.OriginPosition);
        }

        bool isLethal = Health < 1;
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

        // if was the local player who cause the damage
        if (damageData.ActorViewID == bl_MFPS.LocalPlayer.ViewID)
        {
            bl_CrosshairBase.Instance.OnHit(isLethal);
            bl_AudioController.PlayClip(isLethal ? "head-hit" : "body-hit");
            bl_EventHandler.DispatchLocalPlayerHitEnemy(hitData);
        }
        // if was a bot who cause the damage
        else if (!damageData.IsActorARealPlayer())
        {
            bl_EventHandler.Bots.onBotHitPlayer?.Invoke(hitData);
        }

        if (Health > 0)
        {
            if (!selfDamage)
            {
                Transform t = bl_GameManager.Instance.FindActor(damageData.ActorViewID);
                if (t != null && !t.name.Contains("(die)"))
                {
                    if (m_AIShooter.Target == null)
                    {
                        if (shooterAgent != null)
                            shooterAgent.FocusOnSingleTarget = true;
                        if (t.TryGetComponent(out bl_PlayerReferencesCommon prc))
                        {
                            m_AIShooter.Target = prc.BotAimTarget;
                        }
                    }
                    else
                    {
                        if (t != m_AIShooter.Target)
                        {
                            // if the player who hit this bot is closer than the current target, then change the target
                            float cd = bl_UtilityHelper.Distance(CachedTransform.localPosition, m_AIShooter.Target.position);
                            float od = bl_UtilityHelper.Distance(CachedTransform.localPosition, t.position);
                            if (od < cd && (cd - od) > 7) // seven is a magic number.
                            {
                                if (shooterAgent != null)
                                    shooterAgent.FocusOnSingleTarget = true;
                                if (t.TryGetComponent(out bl_PlayerReferencesCommon prc))
                                {
                                    m_AIShooter.Target = prc.BotAimTarget;
                                }
                            }
                        }
                    }
                }
            }
            references.aiAnimation.OnGetHit();
        }
        else
        {
            Die(damageData.ActorViewID, !damageData.IsActorARealPlayer(), weaponName, damageData);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void Die(int viewID, bool fromBot, string weaponName, DamageData damageData)
    {
        bool selfDamage = fromBot && viewID == photonView.ViewID;
        //Debug.Log($"{gameObject.name} die with {weaponName} from viewID {viewID} Bot?= {fromBot}");
        m_AIShooter.IsDeath = true;
        m_AIShooter.OnDeath();
        gameObject.name += " (die)";
        m_AIShooter.AimTarget.name += " (die)";
        references.shooterWeapon.OnDeath();
        m_AIShooter.enabled = false;
        references.onDie?.Invoke();

        if (bl_PhotonNetwork.IsMasterClient && references.Agent.enabled) references.Agent.isStopped = true;
        GetComponent<bl_NamePlateBase>().SetActive(false);
        //update the MFPSPlayer data
        MFPSPlayer player = bl_GameManager.Instance.GetMFPSPlayer(m_AIShooter.AIName);
        if (player != null) player.isAlive = false;

        bl_AIShooter killerBot = null;
        //find the killer view in the scene
        var killerView = PhotonView.Find(viewID);

        //if was local player who terminated this bot
        if (viewID == bl_MFPS.LocalPlayer.ViewID && !fromBot)
        {
            Team team = bl_PhotonNetwork.LocalPlayer.GetPlayerTeam();
            //send kill feed message
            bl_GameData.GetWeaponDataByName(weaponName, out int gunID, out bl_GunInfo info);
            if (weaponName.Contains("cmd:"))
            {
                weaponName = weaponName.Replace("cmd:", "");
                gunID = -(bl_KillFeedBase.Instance.GetCustomIconIndex(weaponName) + 1);
            }

            // if is a grenade or launcher, make sure do not detect headshot
            if (info != null && (info.Type == GunType.Grenade || info.Type == GunType.Launcher))
            {
                damageData.IsHeadShot = false;
            }

            var feed = new bl_KillFeedBase.FeedData()
            {
                LeftText = LocalName,
                RightText = m_AIShooter.AIName,
                Team = team
            };
            feed.AddData("gunid", gunID);
            feed.AddData("headshot", damageData.IsHeadShot);

            bl_KillFeedBase.Instance.SendKillMessageEvent(feed);

            if (references.PlayerTeam != bl_MFPS.LocalPlayer.Team || isOneTeamMode)
            {
                var killConsideration = bl_GameData.CoreSettings.howConsiderBotsEliminations;
                if (killConsideration != BotKillConsideration.DoNotCountAtAll && !selfDamage)
                {
                    if (isOneTeamMode)
                    {
                        //Add a new kill and update information
                        bl_PhotonNetwork.LocalPlayer.PostKill(1);//Send a new kill
                        bl_RoomSettings.IncreaseMatchPersistData("bot-kills", 1);
                    }
                    // do not increase the player kill count if kill a teammate bot
                    else if (shooterAgent.AITeam != Team.All && shooterAgent.AITeam != bl_MFPS.LocalPlayer.Team)
                    {
                        //Add a new kill and update information
                        bl_PhotonNetwork.LocalPlayer.PostKill(1);//Send a new kill
                        bl_RoomSettings.IncreaseMatchPersistData("bot-kills", 1);
                    }
                }

                // only grant score to the kill player if bot eliminations counts as real players.
                if (killConsideration == BotKillConsideration.SameAsRealPlayers && !selfDamage)
                {
                    int score = bl_Hook.ApplyFilters("Filter_KillScore", bl_GameData.ScoreSettings.ScorePerKill);
                    //If headshot will give you double experience
                    if (damageData.IsHeadShot)
                    {
                        bl_GameManager.Headshots++;
                        score += bl_Hook.ApplyFilters("Filter_HeadShotScore", bl_GameData.ScoreSettings.ScorePerHeadShot);
                    }

                    if (isOneTeamMode) bl_PhotonNetwork.LocalPlayer.PostScore(score);
                    else if (shooterAgent.AITeam != Team.All && shooterAgent.AITeam != bl_MFPS.LocalPlayer.Team)
                    {
                        //Send to update score to player
                        bl_PhotonNetwork.LocalPlayer.PostScore(score);
                    }
                }

                //show an local notification for the kill
                var localKillInfo = new KillInfo
                {
                    Killer = bl_PhotonNetwork.LocalPlayer.NickName,
                    Killed = string.IsNullOrEmpty(m_AIShooter.AIName) ? gameObject.name.Replace("(die)", "") : m_AIShooter.AIName,
                    byHeadShot = damageData.IsHeadShot,
                    KillMethod = weaponName,
                    GunID = gunID,
                    Cause = fromBot ? DamageCause.Bot : DamageCause.Player,
                    FromPosition = damageData.OriginPosition,
                    ToPosition = transform.position,
                };
                bl_EventHandler.DispatchLocalKillEvent(localKillInfo);
            }
        }
        //if was killed by another bot
        else if (fromBot)
        {
            if (bl_PhotonNetwork.IsMasterClient)
            {
                bl_AIShooter bot = null;
                string killer = "Unknown";
                if (killerView != null && !selfDamage)
                {
                    bot = killerView.GetComponent<bl_AIShooter>();//killer bot
                    killer = bot.AIName;
                    if (string.IsNullOrEmpty(killer)) { killer = killerView.gameObject.name.Replace(" (die)", ""); }
                    //update bot stats
                    bl_AIMananger.SetBotKill(killer);
                }

                if (bot == null) bot = bl_AIMananger.GetBot(viewID);

                if (bot == null)
                {
                    Debug.LogWarning($"Bot with viewId {viewID} kill another bot but he was not found in the scene.");
                }

                //send kill feed message
                int gunID = bl_GameData.Instance.GetWeaponID(weaponName);

                var feed = new bl_KillFeedBase.FeedData()
                {
                    LeftText = killer,
                    RightText = m_AIShooter.AIName,
                    Team = bot == null ? Team.None : bot.AITeam
                };
                feed.AddData("gunid", gunID);
                feed.AddData("headshot", damageData.IsHeadShot);

                bl_KillFeedBase.Instance.SendKillMessageEvent(feed);

                if (bot != null)
                {
                    killerBot = bot;
                }
                else
                {
                    Debug.Log("Bot who kill this bot can't be found");
                }
            }
        }//else, (if other player kill this bot) -> do nothing.

        if (GetGameMode.GetGameModeInfo().showDeathIconsOnTeammates)
        {
            if (m_AIShooter.AITeam == bl_PhotonNetwork.LocalPlayer.GetPlayerTeam())
            {
                bl_ObjectPoolingBase.Instance.Instantiate("deathicon", transform.position, transform.rotation);
            }
        }

        string killerName = killerView != null ? killerView.gameObject.name : "Unknown";
        if (fromBot && killerBot != null) { killerName = killerBot.AIName; }

        var mplayer = new MFPSPlayer(photonView, false, false);
        var eDeathData = new bl_EventHandler.PlayerDeathData()
        {
            Player = mplayer,
            KillerName = killerName,
        };
        bl_EventHandler.DispatchRemotePlayerDeath(eDeathData);
        bl_EventHandler.onPlayerDeath?.Invoke(eDeathData);

        //update the bot deaths count.
        bl_AIMananger.SetBotDeath(m_AIShooter.AIName);

        if (bl_PhotonNetwork.IsMasterClient)
        {
            //respawn management here
            //only master client called it since the spawn will be sync by PhotonNetwork.Instantiate()
            bl_AIMananger.Instance.OnBotDeath(m_AIShooter, killerBot);

            //Only Master client should send the RPC
            var deathData = bl_UtilityHelper.CreatePhotonHashTable();
            deathData.Add("type", AIRemoteCallType.DestroyBot);
            deathData.Add("damage", damageData);

            //Should buffer this RPC?
            this.photonView.RPC(bl_AIShooterAgent.RPC_NAME, RpcTarget.All, deathData);//callback is in bl_AIShooterAgent.cs -> DestroyBot(...)
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public override void DestroyEntity()
    {
        var deathData = bl_UtilityHelper.CreatePhotonHashTable();
        deathData.Add("type", AIRemoteCallType.DestroyBot);
        deathData.Add("instant", true);
        this.photonView.RPC(bl_AIShooterAgent.RPC_NAME, RpcTarget.AllBuffered, deathData);//callback is in bl_AIShooterAgent.cs
    }

    /// <summary>
    /// 
    /// </summary>
    public override void DoRepetingDamage(RepetingDamageInfo info)
    {
        repetingDamageInfo = info;
        InvokeRepeating(nameof(MakeDamageRepeting), 0, info.Rate);
    }

    /// <summary>
    /// 
    /// </summary>
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

        /*  string weaponName = (damageinfo.Cause == DamageCause.Map || repetingDamageInfo.DamageData.GunID == 0) ?
              "[Burn]" :
              bl_MFPS.Weapon.GetWeaponInfo(repetingDamageInfo.DamageData.GunID).Name;*/

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
    /// 
    /// </summary>
    /// <param name="healthToAdd"></param>
    public override void SetHealth(int healthToAdd, bool overrideHealth)
    {
        if (overrideHealth) Health = healthToAdd;
        else Health += healthToAdd;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public override bool Suicide()
    {
        return false;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public override int GetHealth() => Health;

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public override int GetMaxHealth() => 100;

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public override bool IsDeath()
    {
        return m_AIShooter.IsDeath;
    }

    [PunRPC]
    void RpcSyncHealth(int _health)
    {
        Health = _health;
    }
}