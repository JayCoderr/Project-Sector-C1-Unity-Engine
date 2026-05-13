using MFPS.Internal.Structures;
using Photon.Realtime;

/// <summary>
/// use to get all room properties easily
/// usage:  RoomProperties props = PhotonNetwork.CurrentRoom.GetRoomInfo();
/// </summary>
public class MFPSRoomInfo
{
    public string roomName { get; set; }
    public int sceneId { get; set; }
    public string password { get; set; }
    public GameMode gameMode { get; set; }
    public int goal { get; set; }
    public int time { get; set; }
    public int maxPing { get; set; }
    public Room room { get; set; }
    public int maxPlayers { get; set; }
    public bool friendlyFire { get; set; }
    public bool withBots { get; set; }
    public bool autoTeamSelection { get; set; }
    public RoundStyle roundStyle { get; set; }
    public byte WeaponOption { get; set; }

    public bool isPrivate { get { return !string.IsNullOrEmpty(password); } }

    public MFPSRoomInfo() { }

    public MFPSRoomInfo(Room roomTarget)
    {
        room = roomTarget;
        roomName = room.Name;
        maxPlayers = room.MaxPlayers;
        GetProps(room.CustomProperties);
    }

    public void GetProps(ExitGames.Client.Photon.Hashtable data)
    {
        sceneId = (int)data[PropertiesKeys.RoomSceneID];
        password = (string)data[PropertiesKeys.RoomPassword];
        gameMode = (GameMode)data[PropertiesKeys.GameModeKey];
        time = (int)data[PropertiesKeys.TimeRoomKey];
        goal = (int)data[PropertiesKeys.RoomGoal];
        maxPing = (int)data[PropertiesKeys.MaxPing];
        friendlyFire = (bool)data[PropertiesKeys.RoomFriendlyFire];
        withBots = (bool)data[PropertiesKeys.WithBotsKey];
        autoTeamSelection = (bool)data[PropertiesKeys.TeamSelectionKey];
        roundStyle = (RoundStyle)data[PropertiesKeys.RoomRoundKey];
        WeaponOption = (byte)data[PropertiesKeys.RoomWeaponOption];
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public MapInfo GetMapInfo()
    {
        return bl_GameData.Instance.AllScenes[sceneId];
    }

    /// <summary>
    /// Are all weapons allowed in this room?
    /// </summary>
    /// <returns></returns>
    public bool AllWeaponsAllowed()
    {
        return WeaponOption == 0;
    }

    /// <summary>
    /// Get the weapon type allowed in this room
    /// </summary>
    /// <returns>If return <see cref="GunType.None"/> that means all the weapons are allowed</returns>
    public GunType WeaponTypeAllowed()
    {
        return WeaponOption == 0 ? GunType.None : bl_GameData.Instance.allowedWeaponOnlyOptions[WeaponOption - 1];
    }
}