using MFPSEditor;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bl_PlayerSettings : bl_PhotonHelper
{
    #region Public members
    [ReadOnly] public Team PlayerTeam = Team.None;
    public List<MonoBehaviour> RemoteOnlyScripts = new();
    public List<MonoBehaviour> LocalOnlyScripts = new();
    [Space(5)]
    public GameObject LocalObjects;
    public GameObject RemoteObjects;
    [Header("Player References")]
    public Transform FlagPosition;
    public Transform carrierPoint;
    public Mesh directionMesh;

    [Header("Hands Textures")]
    [ScriptableDrawer] public bl_FPArmsMaterial armsMaterial;
    private readonly List<bl_FPArmsMaterial.MaterialColor> currentWeaponMaterials = new();
    #endregion

    /// <summary>
    /// Is this player a network player or an local/offline player?
    /// DO NOT confuse this with the <see cref="PhotonView.IsMine"/> property.
    /// </summary>
    public bool IsNetworkPlayer
    {
        get;
        private set;
    } = true;

    /// <summary>
    /// 
    /// </summary>
    void Awake()
    {
        if ((!bl_PhotonNetwork.IsConnected || !bl_PhotonNetwork.InRoom) && bl_MFPS.IsMultiplayerScene) return;

        // if this player was not network instantiated
        if (!bl_MFPS.IsMultiplayerScene && gameObject.scene.name != null)
        {
            PlayerTeam = Team.All;
            bl_UtilityHelper.LockCursor(true);
            IsNetworkPlayer = false;
        }
        else
        {
            PlayerTeam = (Team)photonView.InstantiationData[0];
            IsNetworkPlayer = true;
        }

        if (IsMine)
        {
            OnLocalPlayer();
        }
        else
        {
            OnRemotePlayer();
        }
    }

    /// <summary>
    /// We call this function only if this is a Remote player
    /// </summary>
    public void OnRemotePlayer()
    {
        for (int i = 0; i < LocalOnlyScripts.Count; i++)
        {
            if (LocalOnlyScripts[i] != null)
            {
                LocalOnlyScripts[i].enabled = false;
            }
        }
        LocalObjects.SetActive(false);
        gameObject.tag = bl_MFPS.REMOTE_PLAYER_TAG;
        gameObject.layer = (int)Mathf.Log(bl_GameData.TagsAndLayerSettings.RemotePlayerRootLayer.value, 2);

        //Build Player Data
        MFPSPlayer playerData = new()
        {
            Name = photonView.Owner.NickName,
            Team = PlayerTeam,
            Actor = transform,
            isRealPlayer = true,
            isAlive = true,
            ActorView = photonView,
            AimPosition = carrierPoint,
        };

        bl_EventHandler.DispatchRemoteActorChange(new bl_EventHandler.PlayerChangeData()
        {
            PlayerName = photonView.Owner.NickName,
            MFPSActor = playerData,
            IsAlive = true,
            NetworkView = photonView
        });
    }

    /// <summary>
    /// We call this function only if we are Local player
    /// </summary>
    public void OnLocalPlayer()
    {
        if (!string.IsNullOrEmpty(bl_PhotonNetwork.NickName)) gameObject.name = bl_PhotonNetwork.NickName;
        for (int i = 0; i < RemoteOnlyScripts.Count; i++)
        {
            if (RemoteOnlyScripts[i] != null)
            {
                RemoteOnlyScripts[i].enabled = false;
            }
        }
        RemoteObjects.SetActive(false);
        gameObject.tag = bl_MFPS.LOCAL_PLAYER_TAG;
        gameObject.layer = bl_GameData.TagsAndLayerSettings.GetLocalPlayerLayerIndex();

        armsMaterial?.SelectTeamMaterial(PlayerTeam);
        if (bl_GameData.CoreSettings.doSpawnHandMeshEffect)
        {
            StartCoroutine(DoSpawnLoop());
        }
        PlayerReferences.DefaultCameraFOV = (float)bl_MFPS.Settings.GetSettingOf("FOV");
        bl_EventHandler.onGameSettingsChange += OnGameSettingsChanged;
    }

    /// <summary>
    /// 
    /// </summary>
    private void OnDisable()
    {
        SetDeafultWeaponRender();
        if (IsMine)
        {
            bl_EventHandler.onGameSettingsChange -= OnGameSettingsChanged;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private void OnDestroy()
    {
        if (!IsMine)
        {
            bl_EventHandler.DispatchRemoteActorChange(new bl_EventHandler.PlayerChangeData()
            {
                PlayerName = gameObject.name,
                MFPSActor = null,
                IsAlive = false,
            });
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void OnGameSettingsChanged()
    {
        PlayerReferences.DefaultCameraFOV = (float)bl_MFPS.Settings.GetSettingOf("FOV");
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (carrierPoint == null)
        {
            var pa = GetComponent<bl_PlayerReferences>().playerAnimations;
            if (pa != null)
            {
                Animator animator = pa.Animator;
                if (animator != null) { carrierPoint = animator.GetBoneTransform(HumanBodyBones.UpperChest); }
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (directionMesh == null) return;

        Gizmos.color = new Color(0.7187521f, 1f, 0.2688679f, 1.00f);
        Gizmos.DrawMesh(directionMesh, transform.position, transform.rotation, Vector3.one * 0.5f);
    }
#endif

    /// <summary>
    /// 
    /// </summary>
    public void DoSpawnWeaponRenderEffect(Renderer[] renderers)
    {
        if (!bl_GameData.CoreSettings.doSpawnHandMeshEffect) return;
        if (currentWeaponMaterials.Count > 0)
        {
            //set default color
            foreach (var item in currentWeaponMaterials)
            {
                if (item.Material == null) continue;
                item.Material.color = item.Color;
            }
        }
        currentWeaponMaterials.Clear();
        foreach (var item in renderers)
        {
            if (item == null) continue;
            Material[] mats = item.sharedMaterials;
            for (int i = 0; i < mats.Length; i++)
            {
                if (mats[i] == null || !mats[i].HasProperty("_Color")) continue;
                bl_FPArmsMaterial.MaterialColor mc = new(mats[i]);
                currentWeaponMaterials.Add(mc);
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    IEnumerator DoSpawnLoop()
    {
        float d = 0;
        float st = bl_GameData.CoreSettings.SpawnProtectedTime;
        float value;
        Color teamColor = PlayerTeam.GetTeamColor();
        while (d < 1)
        {
            d += Time.deltaTime / st;
            value = Mathf.PingPong(Time.time, 0.25f) * 4;
            if (currentWeaponMaterials.Count > 0)
            {
                for (int i = 0; i < currentWeaponMaterials.Count; i++)
                {
                    if (currentWeaponMaterials[i].Material == null) continue;
                    currentWeaponMaterials[i].Material.color = Color.Lerp(currentWeaponMaterials[i].Color, teamColor, value);
                }
            }
            yield return null;
        }
        SetDeafultWeaponRender();
    }

    /// <summary>
    /// 
    /// </summary>
    void SetDeafultWeaponRender()
    {
        if (currentWeaponMaterials.Count > 0)
        {
            foreach (var item in currentWeaponMaterials)
            {
                item.Material.color = item.Color;
            }
        }
    }

    public PhotonView View { get { return photonView; } }
    public bool isTeamMate { get { return (PlayerTeam == bl_PhotonNetwork.LocalPlayer.GetPlayerTeam() && !isOneTeamMode); } }

    private bl_PlayerReferences _playerReferences;
    public bl_PlayerReferences PlayerReferences
    {
        get
        {
            if (_playerReferences == null) _playerReferences = GetComponentInParent<bl_PlayerReferences>();
            return _playerReferences;
        }
    }

}