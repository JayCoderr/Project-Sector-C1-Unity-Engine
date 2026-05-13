using MFPS.Internal.Structures;
using Photon.Realtime;
using HashTable = ExitGames.Client.Photon.Hashtable;

public class bl_KillFeed : bl_KillFeedBase
{
    private bool showKillFeed = true;

    /// <summary>
    /// 
    /// </summary>
    void Awake()
    {
        if (bl_PhotonNetwork.InRoom)
        {
            bl_UIReferences.SafeUIInvoke(() => { OnJoined(); });
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private void Start()
    {
        showKillFeed = bl_RoomSettings.GetRoomInfo().gameMode.GetModeInfo().ShowKillFeed;
    }

    /// <summary>
    /// 
    /// </summary>
    void OnEnable()
    {
        bl_PhotonCallbacks.PlayerLeftRoom += OnPhotonPlayerDisconnected;
    }

    /// <summary>
    /// 
    /// </summary>
    void OnDisable()
    {
        bl_PhotonCallbacks.PlayerLeftRoom -= OnPhotonPlayerDisconnected;
    }

    /// <summary>
    /// Player Joined? sync
    /// </summary>
    void OnJoined()
    {
#if LOCALIZATION
        string joinCmd = bl_Localization.AsCommand("joinmatch");
        SendMessageEvent(string.Format("{0} {1}", bl_PhotonNetwork.LocalPlayer.NickName, joinCmd));
#else
        SendMessageEvent(string.Format("{0} {1}", bl_PhotonNetwork.LocalPlayer.NickName, bl_GameTexts.JoinedInMatch));
#endif
    }

    /// <summary>
    /// 
    /// </summary>
    public override void SendKillMessageEvent(FeedData feedData)
    {
        if (!showKillFeed) return;

        var data = new HashTable
        {
            { "killer", feedData.LeftText },
            { "killed", feedData.RightText },
            { "gunid", (int)feedData.Data["gunid"] },
            { "team", feedData.Team },
            { "headshot", (bool)feedData.Data["headshot"] },
            { "mt", KillFeedMessageType.WeaponKillEvent }
        };
        SendMessageOverNetwork(data);
    }

    /// <summary>
    /// 
    /// </summary>
    public override void SendMessageEvent(string message, bool localOnly = false)
    {
        var data = new HashTable
        {
            { "message", message },
            { "mt", KillFeedMessageType.Message }
        };
        if (localOnly) { ReceiveMessage(data); return; }

        SendMessageOverNetwork(data);
    }

    /// <summary>
    /// 
    /// </summary>
    public override void SendTeamHighlightMessage(string teamHighlightMessage, string normalMessage, Team playerTeam)
    {
        var data = new HashTable
        {
            { "killer", teamHighlightMessage },
            { "message", normalMessage },
            { "team", playerTeam },
            { "mt", KillFeedMessageType.TeamHighlightMessage }
        };
        SendMessageOverNetwork(data);
    }

    /// <summary>
    /// 
    /// </summary>
    void SendMessageOverNetwork(HashTable data)
    {
        bl_PhotonNetwork.Instance.SendDataOverNetwork(PropertiesKeys.KillFeedEvent, data);
    }

    /// <summary>
    /// 
    /// </summary>
    public override void OnMessageReceive(HashTable data)
    {
        KillFeedMessageType mtype = (KillFeedMessageType)data["mt"];
        switch (mtype)
        {
            case KillFeedMessageType.WeaponKillEvent:
                ReceiveWeaponKillEvent(data);
                break;
            case KillFeedMessageType.Message:
                ReceiveMessage(data);
                break;
            case KillFeedMessageType.TeamHighlightMessage:
                ReceiveOnePlayerMessage(data);
                break;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void ReceiveWeaponKillEvent(HashTable data)
    {
        var kf = new KillFeed
        {
            Killer = (string)data["killer"],
            Killed = (string)data["killed"],
            GunID = (int)data["gunid"],
            HeadShot = (bool)data["headshot"],
            KillerTeam = (Team)data["team"],
            messageType = KillFeedMessageType.WeaponKillEvent
        };

        bl_KillFeedUIBase.Instance.SetKillFeed(kf);
    }

    /// <summary>
    /// 
    /// </summary>
    void ReceiveMessage(HashTable data)
    {
        var kf = new KillFeed
        {
            Message = (string)data["message"],
            messageType = KillFeedMessageType.Message
        };

#if LOCALIZATION
        bl_Localization.Instance.ParseCommad(ref kf.Message);
#endif

        bl_KillFeedUIBase.Instance.SetKillFeed(kf);
    }

    /// <summary>
    /// 
    /// </summary>
    void ReceiveOnePlayerMessage(HashTable data)
    {
        var kf = new KillFeed
        {
            Killer = (string)data["killer"],
            Message = (string)data["message"],
            KillerTeam = (Team)data["team"],
            messageType = KillFeedMessageType.TeamHighlightMessage
        };

        bl_KillFeedUIBase.Instance.SetKillFeed(kf);
    }

    public void OnPhotonPlayerDisconnected(Player otherPlayer)
    {
#if LOCALIZATION
        SendMessageEvent(string.Format("{0} {1}", otherPlayer.NickName, bl_Localization.Instance.GetText(18)), true);
#else
        SendMessageEvent(string.Format("{0} {1}", otherPlayer.NickName, bl_GameTexts.LeftOfMatch), true);
#endif
    }
}