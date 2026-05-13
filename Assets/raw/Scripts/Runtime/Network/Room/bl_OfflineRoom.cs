using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;

[DefaultExecutionOrder(-1000)]
public class bl_OfflineRoom : MonoBehaviourPunCallbacks
{
    [Header("Offline Room")]
    public GameMode gameMode = GameMode.FFA;
    [LovattoToogle] public bool forceOffline = false;
    [LovattoToogle] public bool withBots = false;
    [LovattoToogle] public bool autoTeamSelection = true;
    [LovattoToogle] public bool friendlyFire = false;
    [Range(1, 64)] public int maxPlayers = 1;
    public int MatchTime = 9989;
    public int gameModeGoal = 100;
    public int onlyWeaponOption = 0;
    public RoundStyle roundStyle = RoundStyle.OneMatch;

    /// <summary>
    /// 
    /// </summary>
    private void Awake()
    {
        PhotonNetwork.AddCallbackTarget(this);
        if (!bl_PhotonNetwork.IsConnectedInRoom)
        {
            if (bl_GameData.CoreSettings.offlineMode || forceOffline)
            {
#if CLASS_CUSTOMIZER
                bl_ClassManager.Instance.Init();
#endif
                bl_Input.Initialize();
                bl_PhotonNetwork.OfflineMode = true;
                bl_PhotonNetwork.NickName = "Offline Player";
            }
            else
            {
                bl_PhotonNetwork.OfflineMode = false;
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private void OnDestroy()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    /// <summary>
    /// 
    /// </summary>
    public override void OnConnectedToMaster()
    {
        Debug.Log("Offline Connected to Master");
        var roomOption = GetRoomProperties();

        roomOption = bl_Hook.ApplyFilters("Filter_OfflineRoomProps", roomOption);

        PhotonNetwork.CreateRoom("Offline Room", new RoomOptions()
        {
            MaxPlayers = (byte)maxPlayers,
            IsVisible = true,
            IsOpen = true,
            CustomRoomProperties = roomOption,
            CleanupCacheOnLeave = true,
            PublishUserId = true,
            EmptyRoomTtl = 0,
        }, null);
    }

    public Hashtable GetRoomProperties()
    {
        return new()
        {
            [PropertiesKeys.TimeRoomKey] = MatchTime,
            [PropertiesKeys.GameModeKey] = gameMode,
            [PropertiesKeys.RoomSceneID] = 0,
            [PropertiesKeys.RoomRoundKey] = roundStyle,
            [PropertiesKeys.TeamSelectionKey] = autoTeamSelection,
            [PropertiesKeys.RoomGoal] = gameModeGoal,
            [PropertiesKeys.RoomFriendlyFire] = friendlyFire,
            [PropertiesKeys.MaxPing] = 1000,
            [PropertiesKeys.RoomPassword] = "",
            [PropertiesKeys.WithBotsKey] = withBots,
            [PropertiesKeys.RoomWeaponOption] = (byte)onlyWeaponOption
        };
    }

    private static bl_OfflineRoom _instance;
    public static bl_OfflineRoom Instance
    {
        get
        {
            if (_instance == null) { _instance = FindAnyObjectByType<bl_OfflineRoom>(); }
            return _instance;
        }
    }
}