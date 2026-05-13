using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class bl_PlayerNetwork : bl_PlayerNetworkBase, IPunObservable
{
    #region Public members
    /// <summary>
    /// the player's team is not ours
    /// </summary>
    public Team RemoteTeam { get; set; }
    /// <summary>
    /// the object to which the player looked
    /// </summary>
    [FormerlySerializedAs("HeatTarget")]
    public Transform HeadTarget;
    /// <summary>
    /// smooth interpolation amount
    /// </summary>
    public float SmoothingDelay = 8f;
    /// <summary>
    /// list all remote weapons
    /// </summary>
    public List<bl_NetworkGun> NetworkGuns = new();
    [SerializeField]
    PhotonTransformViewPositionModel m_PositionModel = new();
    [SerializeField]
    PhotonTransformViewRotationModel m_RotationModel = new();
    public Material InvicibleMat;
    #endregion

    #region Private members
    private bl_NamePlateBase DrawName;
    private bool SendInfo = false;
    private bool isWeaponBlocked = false;
    private Transform m_Transform;
    private Vector3 networkHeadLookAt = Vector3.zero;// Head Look to
    private bool networkIsGrounded;
    private string RemotePlayerName = string.Empty;
    private Vector3 networkVelocity;
    PhotonTransformViewPositionControl m_PositionControl;
    PhotonTransformViewRotationControl m_RotationControl;
    bool m_ReceivedNetworkUpdate = false;
    private float modelYaw;
    private int currentGunID = -1;
    private bool turnInPlace = false;
    private bl_PlayerReferences m_PlayerReferences;
#if UMM
    private bl_MiniMapEntityBase MiniMapItem = null;
#endif
    #endregion

    #region Public Properties
    /// <summary>
    /// Is this player teammate of the local player?
    /// </summary>
    public bool IsFriend
    {
        get;
        set;
    }

    /// <summary>
    /// The current TPWeapon in the hands of this player
    /// </summary>
    public bl_NetworkGun CurrenGun
    {
        get;
        set;
    }
    #endregion

    #region Unity Callbacks
    protected override void Awake()
    {
        base.Awake();
        m_PlayerReferences = PlayerReferences;
        bl_PhotonCallbacks.PlayerEnteredRoom += OnPhotonPlayerConnected;
        bl_EventHandler.Match.onSpectatorModeChanged += OnSpectatorModeChanged;
        if ((!bl_PhotonNetwork.IsConnected || !bl_PhotonNetwork.InRoom) && bl_MFPS.IsMultiplayerScene) return;

        m_Transform = transform;
        m_PositionControl = new PhotonTransformViewPositionControl(m_PositionModel);
        m_RotationControl = new PhotonTransformViewRotationControl(m_RotationModel);
        modelYaw = m_PlayerReferences.playerAnimations.transform.eulerAngles.y;
        m_PlayerReferences.playerAnimations.Initialize();
        DrawName = GetComponent<bl_NamePlateBase>();
        foreach (var x in NetworkGuns)
        {
            if (x != null) x.gameObject.SetActive(false);
        }
#if UMM
        MiniMapItem = this.GetComponent<bl_MiniMapEntityBase>();
        if (IsMine && MiniMapItem != null)
        {
            MiniMapItem.enabled = false;
        }
#endif

        if (!photonView.ObservedComponents.Contains(this))
        {
            photonView.ObservedComponents.Add(this);
        }

        SlowLoop();
        DrawName.SetActive(IsFriend);
        turnInPlace = bl_GameData.CoreSettings.useTurnInPlace;
    }

    /// <summary>
    /// 
    /// </summary>
    private void Start()
    {
        InvokeRepeating(nameof(SlowLoop), 0, 1);
    }

    /// <summary>
    /// 
    /// </summary>
    protected override void OnDisable()
    {
        base.OnDisable();
        bl_PhotonCallbacks.PlayerEnteredRoom -= OnPhotonPlayerConnected;
        bl_EventHandler.Match.onSpectatorModeChanged -= OnSpectatorModeChanged;
    }

    /// <summary>
    /// 
    /// </summary>
    public override void OnUpdate()
    {
        if ((!IsConnected || !bl_PhotonNetwork.InRoom) && bl_MFPS.IsMultiplayerScene) return;

        if (photonView != null && !IsMine)
        {
            OnRemotePlayer();
        }
        else
        {
            OnLocalPlayer();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public override void OnLateUpdate()
    {
        if (IsMine)
        {
            // Since turn in place code in the player animation script (disable on first person)
            // we need to update the code from here.
            if (!bl_MFPS.LocalPlayer.IsThirdPersonView())
            {
                m_PlayerReferences.playerAnimations.UpdateTIPFromFPV();
            }
        }
    }
    #endregion

    /// <summary>
    /// serialization method of photon
    /// </summary>
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        m_PositionControl.OnPhotonSerializeView(m_Transform.localPosition, stream, info);
        m_RotationControl.OnPhotonSerializeView(m_Transform.localRotation, stream, info);

#if UNITY_EDITOR
        if (IsMine == false)
        {
            DoDrawEstimatedPositionError();
        }
#endif

        if (stream.IsWriting)
        {
            //We own this player: send the others our data
            stream.SendNext(GetHeadLookPosition());
            stream.SendNext(FpControler.State);
            stream.SendNext(FpControler.isGrounded);
            stream.SendNext((byte)m_PlayerReferences.gunManager.GetCurrentGunID);//send as byte, max value is 255.
            stream.SendNext(FPState);
            stream.SendNext(FpControler.Velocity);
            if (turnInPlace)
            {
                stream.SendNext(m_PlayerReferences.playerAnimations.CachedTransform.eulerAngles.y);
                stream.SendNext(m_PlayerReferences.playerAnimations.GetTurnStepIndex());
            }
        }
        else
        {
            //Network player, receive data
            networkHeadLookAt = (Vector3)stream.ReceiveNext();
            NetworkBodyState = (PlayerState)stream.ReceiveNext();
            networkIsGrounded = (bool)stream.ReceiveNext();
            NetworkGunID = (int)(byte)stream.ReceiveNext();
            FPState = (PlayerFPState)stream.ReceiveNext();
            networkVelocity = (Vector3)stream.ReceiveNext();
            if (turnInPlace)
            {
                modelYaw = (float)stream.ReceiveNext();
                byte turnIdx = (byte)stream.ReceiveNext();
                m_PlayerReferences.playerAnimations.HandleRemoteTurnIndex(turnIdx);
            }

            m_ReceivedNetworkUpdate = true;
        }
    }

    /// <summary>
    /// Function called each frame when the player is local
    /// </summary>
    void OnLocalPlayer()
    {
        //send the state of player local for remote animation
        var anim = m_PlayerReferences.playerAnimations;
        anim.BodyState = FpControler.State;
        anim.IsGrounded = FpControler.isGrounded;
        anim.Velocity = FpControler.Velocity;
        anim.FPState = FPState;
    }

    /// <summary>
    /// Function called each frame when the player is remote
    /// </summary>
    void OnRemotePlayer()
    {
        if (m_Transform.parent == null)
        {
            UpdatePosition();
            UpdateRotation();
        }

        HeadTarget.LookAt(networkHeadLookAt);

        var anim = m_PlayerReferences.playerAnimations;
        anim.BodyState = NetworkBodyState;//send the state of player local for remote animation
        anim.IsGrounded = networkIsGrounded;
        anim.Velocity = networkVelocity;
        anim.FPState = FPState;
        if (turnInPlace) anim.ApplyModelYaw(modelYaw);

        if (!isOneTeamMode)
        {
            //Determine if remote player is teamMate or enemy
            if (IsFriend)
                OnTeammatePlayer();
            else
                OnEnemyPlayer();
        }
        else
        {
            OnEnemyPlayer();
        }

        if (!isWeaponBlocked && currentGunID != NetworkGunID)
        {
            CurrentTPVGun();
            currentGunID = NetworkGunID;
        }
    }

    /// <summary>
    /// Function called each frame when this Remote player is an enemy
    /// </summary>
    void OnEnemyPlayer()
    {
        PlayerHealthManager.DamageEnabled = true;
#if UMM
        if (bl_MiniMapData.Instance.showEnemysWhenFire)
        {
#if KSA
            if (bl_KillStreakHandler.Instance != null && bl_KillStreakHandler.Instance.ActiveAUVs > 0) return;
#endif
            if (FPState == PlayerFPState.Firing || FPState == PlayerFPState.FireAiming)
            {
                MiniMapItem?.SetActiveIcon(true);
            }
            else
            {
                MiniMapItem?.SetActiveIcon(false);
            }
        }
#endif
    }

    /// <summary>
    /// Function called each frame when this Remote player is a teammate.
    /// </summary>
    void OnTeammatePlayer()
    {
        PlayerHealthManager.DamageEnabled = bl_RoomSettings.Instance.CurrentRoomInfo.friendlyFire;
        DrawName.SetActive(true);
        m_PlayerReferences.characterController.enabled = false;

        if (!SendInfo)
        {
            SendInfo = true;
            m_PlayerReferences.playerRagdoll.IgnorePlayerCollider();
        }

#if UMM
        MiniMapItem?.SetActiveIcon(true);
#endif
    }

    /// <summary>
    /// This function control which TP Weapon should be showing on remote players.
    /// </summary>
    /// <param name="local">Should take the current gun from the local player or from the network data?</param>
    /// <param name="force">Double-check even if the equipped weapon has not changed.</param>
    public override void CurrentTPVGun(bool local = false, bool force = false)
    {
        if (m_PlayerReferences.gunManager == null)
            return;
        if (NetworkGunID == currentGunID && !force) return;

        bool found = false;

        //Get the current gun ID local and sync with remote
        int count = NetworkGuns.Count;
        for (int i = 0; i < count; i++)
        {
            var tpWeapon = NetworkGuns[i];
            if (tpWeapon == null) continue;

            int currentID = (local) ? m_PlayerReferences.gunManager.GetCurrentWeapon().GunID : NetworkGunID;
            if (tpWeapon.GetWeaponID == currentID)
            {
                tpWeapon.gameObject.SetActive(true);
                if (!local)
                {
                    CurrenGun = tpWeapon;
                    CurrenGun.ActiveThisWeapon();
                }
                found = true;
            }
            else
            {
                if (tpWeapon != null)
                    tpWeapon.gameObject.SetActive(false);
            }
        }

        // if the tp weapon is not found in this player instance, then try to get it from the weapon container
        if (!found && m_PlayerReferences.RemoteWeapons != null)
        {
            var tpWeapon = m_PlayerReferences.RemoteWeapons.GetWeapon(NetworkGunID, m_PlayerReferences.PlayerAnimator.avatar);
            if (tpWeapon != null)
            {
                tpWeapon.gameObject.SetActive(true);
                CurrenGun = tpWeapon;
                CurrenGun.ActiveThisWeapon();
                NetworkGuns.Add(tpWeapon);
            }
        }
    }

    /// <summary>
    /// Show a TPWeapon temporary without actually equip it.
    /// </summary>
    /// <param name="toGunId">the GunID of the weapon to switch</param>
    /// <param name="timeToShow">delay time in milliseconds to switch back to the equipped weapon.</param>
    public override async void TemporarySwitchTPWeapon(int toGunId, int timeToShow)
    {
        bl_NetworkGun toTPWeapon = GetTPWeapon(toGunId);
        bl_NetworkGun equippedWeapon = CurrenGun;
        if (toTPWeapon != null) toTPWeapon.gameObject.SetActive(true);
        if (equippedWeapon != null) equippedWeapon.gameObject.SetActive(false);
        await Task.Delay(timeToShow);
        if (toTPWeapon != null) toTPWeapon.gameObject.SetActive(false);
        if (equippedWeapon != null) equippedWeapon.gameObject.SetActive(true);
    }

    /// <summary>
    /// Called each second
    /// </summary>
    void SlowLoop()
    {
        if (photonView == null || photonView.Owner == null) return;

        RemotePlayerName = photonView.Owner.NickName;
        RemoteTeam = photonView.Owner.GetPlayerTeam();
        if (gameObject.name != RemotePlayerName) gameObject.name = RemotePlayerName;
        if (DrawName != null) { DrawName.SetName(RemotePlayerName); }
        IsFriend = RemoteTeam == bl_PhotonNetwork.LocalPlayer.GetPlayerTeam();
    }

    /// <summary>
    /// Change the current TPWeapon on this remote player.
    /// </summary>
    public override void SetNetworkWeapon(GunType weaponType, bl_NetworkGun networkGun)
    {
        m_PlayerReferences.playerAnimations?.SetNetworkGun(weaponType, networkGun);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="gunID"></param>
    /// <returns></returns>
    public bl_NetworkGun GetTPWeapon(int gunID)
    {
        int count = NetworkGuns.Count;
        for (int i = 0; i < count; i++)
        {
            if (NetworkGuns[i] != null && NetworkGuns[i].GetWeaponID == gunID)
            {
                return NetworkGuns[i];
            }
        }

        if (m_PlayerReferences.RemoteWeapons != null)
        {
            var tpWeapon = m_PlayerReferences.RemoteWeapons.GetWeapon(gunID);
            if (tpWeapon != null)
            {
                NetworkGuns.Add(tpWeapon);
            }
            return tpWeapon;
        }

        return null;
    }

    /// <summary>
    /// Replicate a player command to all other clients
    /// </summary>
    /// <param name="commandId">Command Identifier</param>
    /// <param name="arg">Concatenate arguments</param>
    /// <param name="calledFromLocal"></param>
    [PunRPC]
    public override void ReplicatePlayerCommand(int commandId, string arg, bool calledFromLocal = true)
    {
        if (calledFromLocal)
        {
            photonView.RPC(nameof(ReplicatePlayerCommand), RpcTarget.All, commandId, arg, false);
            return;
        }

        onPlayerCommand?.Invoke(new PlayerCommandData()
        {
            CommandID = commandId,
            Arg = arg,
        });
    }

    /// <summary>
    /// Send a call to all other clients to sync a bullet
    /// </summary>
    [PunRPC]
    public override void ReplicateFire(GunType weaponType, Vector3 hitPosition, Vector3 inacuracity, bool calledFromLocal = true, FireReplicationFlag flag = FireReplicationFlag.None)
    {
        if (calledFromLocal && bl_MFPS.IsMultiplayerScene)
        {
            photonView.RPC(nameof(ReplicateFire), RpcTarget.Others, weaponType, hitPosition, inacuracity, false, flag);
#if MFPSTPV
            if (bl_CameraViewSettings.IsThirdPerson())
            {
                m_PlayerReferences.playerAnimations?.PlayFireAnimation(weaponType, flag);
                if (CurrenGun != null)
                {
                    CurrenGun.FireVisualOnly(flag);
                }
            }
#endif
            return;
        }

        if (CurrenGun == null) return;

        CurrenGun.OnNetworkFire(weaponType, new bl_NetworkGun.NetworkFireData()
        {
            HitPosition = hitPosition,
            Inaccuracy = inacuracity
        }, flag);
    }

    /// <summary>
    /// public method to send the RPC shot synchronization
    /// </summary>
    [PunRPC]
    public override void ReplicateGrenadeThrow(float t_spread, Vector3 pos, Quaternion rot, Vector3 direction, string id, bool calledFromLocal = true)
    {
        if (calledFromLocal && bl_MFPS.IsMultiplayerScene)
        {
            photonView.RPC(nameof(ReplicateGrenadeThrow), RpcTarget.Others, new object[] { t_spread, pos, rot, direction, id, false });
            return;
        }

        if (CurrenGun == null) return;

        CurrenGun.GrenadeFire(t_spread, pos, rot, direction, id);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="projectileData"></param>
    /// <param name="calledFromLocal"></param>    
    public override void ReplicateCustomProjectile(CustomProjectileData projectileData, bool calledFromLocal = true)
    {
        var netData = bl_UtilityHelper.CreatePhotonHashTable();
        netData.Add("position", projectileData.Origin);
        netData.Add("rotation", projectileData.Rotation);

        photonView.RPC(nameof(PhotonCustomProjectile), RpcTarget.Others, netData);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="data"></param>
    [PunRPC]
    private void PhotonCustomProjectile(Hashtable data)
    {
        if (CurrenGun == null) return;

        CurrenGun?.FireCustomLogic(data);
        m_PlayerReferences.playerAnimations.PlayFireAnimation(CurrenGun.Info.Type);
    }

    /// <summary>
    /// Sync a player animation command
    /// </summary>
    /// <param name="command"></param>
    [PunRPC]
    public override void ReplicatePlayerAnimationCommand(PlayerAnimationCommands command, string arg = "", bool calledFromLocal = true)
    {
        if (!calledFromLocal)
        {
            m_PlayerReferences.playerAnimations?.CustomCommand(command, arg, false);
        }
        else
        {
            photonView.RPC(nameof(ReplicatePlayerAnimationCommand), RpcTarget.All, command, arg, false);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="blockState"></param>
    [PunRPC]
    public override void SetWeaponBlocked(int blockState, bool calledFromLocal = true)
    {
        isWeaponBlocked = blockState == 1;
        if (calledFromLocal && !bl_PhotonNetwork.OfflineMode)
        {
            photonView.RPC(nameof(SetWeaponBlocked), RpcTarget.Others, blockState, false);
            return;
        }

        if (isWeaponBlocked)
        {
            int count = NetworkGuns.Count;
            for (int i = 0; i < count; i++)
            {
                NetworkGuns[i].gameObject.SetActive(false);
            }
        }
        else
        {
            CurrentTPVGun(false, true);
        }

        m_PlayerReferences.playerAnimations.BlockWeapons(blockState);
        currentGunID = -1;
    }

    [PunRPC]
    void SyncCustomizer(int weaponID, string line, PhotonMessageInfo info)
    {
        if (photonView.ViewID != info.photonView.ViewID) return;
#if CUSTOMIZER
        bl_NetworkGun ng = NetworkGuns.Find(x => x.GetGunID() == weaponID);
        if (ng != null)
        {
            if (ng.TryGetComponent<bl_CustomizerWeapon>(out var customizerWeapon))
            {
                customizerWeapon.ApplyAttachments(line);
            }
            else
            {
                Debug.LogWarning("You have not setup the attachments in the TPWeapon: " + weaponID);
            }
        }
#endif
    }

    /// <summary>
    /// 
    /// </summary>
    void UpdatePosition()
    {
        if (m_PositionModel.SynchronizeEnabled == false || m_ReceivedNetworkUpdate == false)
        {
            return;
        }

        m_Transform.localPosition = m_PositionControl.UpdatePosition(m_Transform.localPosition);
    }
    /// <summary>
    /// 
    /// </summary>
    void UpdateRotation()
    {
        if (m_RotationModel.SynchronizeEnabled == false || m_ReceivedNetworkUpdate == false)
        {
            return;
        }

        m_Transform.localRotation = m_RotationControl.GetRotation(m_Transform.localRotation);
    }

    /// <summary>
    /// 
    /// </summary>
    void DoDrawEstimatedPositionError()
    {
        if (NetworkBodyState == PlayerState.InVehicle) return;

        Vector3 targetPosition = m_PositionControl.GetNetworkPosition();

        Debug.DrawLine(targetPosition, m_Transform.position, Color.red, 2f);
        Debug.DrawLine(m_Transform.position, m_Transform.position + Vector3.up, Color.green, 2f);
        Debug.DrawLine(targetPosition, targetPosition + Vector3.up, Color.red, 2f);
    }

    /// <summary>
    /// These values are synchronized to the remote objects if the interpolation mode
    /// </summary>
    /// <param name="speed">The current movement vector of the object in units/second.</param>
    /// <param name="turnSpeed">The current turn speed of the object in angles/second.</param>
    public void SetSynchronizedValues(Vector3 speed, float turnSpeed) => m_PositionControl.SetSynchronizedValues(speed, turnSpeed);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="newPlayer"></param>
    public void OnPhotonPlayerConnected(Player newPlayer)
    {
        if (photonView.IsMine)
        {
            photonView.RPC(nameof(SetWeaponBlocked), RpcTarget.Others, isWeaponBlocked ? 1 : 0, false);
        }
    }

    /// <summary>
    /// Get the current equipped network weapon, which is the world view weapon model.
    /// </summary>
    /// <returns></returns>
    public override bl_NetworkGun GetCurrentNetworkWeapon() => CurrenGun;

    /// <summary>
    /// 
    /// </summary>
    private void OnSpectatorModeChanged(bool enterIn)
    {
        if (bl_SpectatorModeBase.IsSpectator && bl_GameData.CoreSettings.showNameplatesOnSpectator)
        {
            DrawName.SetActive(true);
            return;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private Vector3 GetHeadLookPosition()
    {
        return m_PlayerReferences == null || m_PlayerReferences.playerIK == null
            ? HeadTarget.position + HeadTarget.forward
            : m_PlayerReferences.playerIK.HeadLookTarget.position;
    }

    private bl_FirstPersonControllerBase FpControler => m_PlayerReferences.firstPersonController;
    private bl_PlayerHealthManagerBase PlayerHealthManager => m_PlayerReferences.playerHealthManager;
}