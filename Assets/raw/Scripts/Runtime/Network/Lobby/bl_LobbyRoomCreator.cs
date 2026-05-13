using MFPS.Internal.Structures;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;

public class bl_LobbyRoomCreator : MonoBehaviour, IConnectionCallbacks, ILobbyCallbacks
{
    public string defaultRoomPrefix = "Room";

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public MFPSRoomInfo BuildRoomInfo()
    {
        var mode = GameModeInfo;
        var room = new MFPSRoomInfo();

        room.roomName = RoomName;
        room.sceneId = bl_LobbyRoomCreatorUI.Instance.Map;
        room.maxPlayers = MaxPlayers;
        room.maxPing = MaxPing;
        room.goal = Goal;
        room.gameMode = mode.gameMode;
        room.time = TimeLimit;
        room.password = Password;
        room.friendlyFire = FriendlyFire;
        room.autoTeamSelection = AutoTeamSelection;
        room.withBots = BotsActive;
        room.roundStyle = PerRoundGame;
        room.WeaponOption = (byte)WeaponOption;
        return room;
    }

    #region Photon Callbacks

    public void OnConnected()
    {
    }

    public void OnConnectedToMaster()
    {
    }

    public void OnDisconnected(DisconnectCause cause)
    {
    }

    public void OnRegionListReceived(RegionHandler regionHandler)
    {
    }

    public void OnCustomAuthenticationResponse(Dictionary<string, object> data)
    {
    }

    public void OnCustomAuthenticationFailed(string debugMessage)
    {
    }

    public void OnJoinedLobby()
    {
    }

    public void OnLeftLobby()
    {
    }

    public void OnRoomListUpdate(List<RoomInfo> roomList)
    {
    }

    public void OnLobbyStatisticsUpdate(List<TypedLobbyInfo> lobbyStatistics)
    {
    }
    #endregion

    #region Getters
    public MapInfo Map => MapList[bl_LobbyRoomCreatorUI.Instance.Map];
    public int MaxPlayers => GameModeInfo.maxPlayers[bl_LobbyRoomCreatorUI.Instance.MaxPlayer];
    public int MaxPing => bl_Lobby.Instance.MaxPing[bl_LobbyRoomCreatorUI.Instance.MaxPing];
    public int Goal => GameModeInfo.GetGoalValue(bl_LobbyRoomCreatorUI.Instance.Goal);
    public int TimeLimit => GameModeInfo.timeLimits[bl_LobbyRoomCreatorUI.Instance.TimeLimit];
    public GameModeSettings GameModeInfo => bl_LobbyRoomCreatorUI.Instance.CurrentGameMode;
    public bool IsPrivate => bl_LobbyRoomCreatorUI.Instance.IsPrivate;
    public bool FriendlyFire => bl_LobbyRoomCreatorUI.Instance.FriendlyFire;
    public bool BotsActive => GameModeInfo.supportBots ? bl_LobbyRoomCreatorUI.Instance.BotsActive : false;
    public bool AutoTeamSelection => GameModeInfo.AutoTeamSelection ? true : bl_LobbyRoomCreatorUI.Instance.AutoTeamSelection;
    public RoundStyle PerRoundGame => GameModeInfo.HasForcedRoundMode() ? GameModeInfo.GetAllowedRoundMode() : bl_LobbyRoomCreatorUI.Instance.GamePerRound;
    public string Password => bl_LobbyRoomCreatorUI.Instance.RoomPassword;
    public string RoomName => bl_LobbyRoomCreatorUI.Instance.RoomName;
    public int WeaponOption => bl_LobbyRoomCreatorUI.Instance.WeaponOption;

    private List<MapInfo> m_mapList;
    public List<MapInfo> MapList
    {
        get
        {
            if (m_mapList == null) m_mapList = bl_GameData.Instance.AllScenes;
            return m_mapList;
        }
    }
    #endregion

    private static bl_LobbyRoomCreator _instance;
    public static bl_LobbyRoomCreator Instance
    {
        get
        {
            if (_instance == null) _instance = FindAnyObjectByType<bl_LobbyRoomCreator>();
            return _instance;
        }
    }
}