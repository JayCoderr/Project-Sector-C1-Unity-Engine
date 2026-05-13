using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class bl_PhotonHelper : MonoBehaviourPun
{

    protected GameMode mGameMode = GameMode.FFA;

    /// <summary>
    ///  get a photonView by the viewID
    /// </summary>
    /// <param name="view"></param>
    /// <returns></returns>
    public PhotonView FindPlayerView(int view)
    {
        PhotonView m_view = PhotonView.Find(view);
        return m_view != null ? m_view : null;
    }

    /// <summary>
    /// Find a player gameobject by the viewID 
    /// </summary>
    /// <returns></returns>
    public GameObject FindPlayerRoot(int viewId)
    {
        var view = FindPlayerView(viewId);
        return view == null ? null : view.gameObject;
    }

    /// <summary>
    /// 
    /// </summary>
    public void CheckViewAllocation()
    {
        if (bl_PhotonNetwork.IsMasterClient && photonView.ViewID <= 0)
        {
            PhotonNetwork.AllocateRoomViewID(photonView);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="go"></param>
    /// <returns></returns>
    public PhotonView GetPhotonView(GameObject go)
    {
        if (!go.TryGetComponent<PhotonView>(out var view))
        {
            view = go.GetComponentInChildren<PhotonView>();
        }
        return view;
    }

    /// <summary>
    /// 
    /// </summary>
    public Transform Root
    {
        get
        {
            return transform.root;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public Transform Parent
    {
        get
        {
            return transform.parent;
        }
    }

    /// <summary>
    /// True if the PhotonView is "mine" and can be controlled by this client.
    /// </summary>
    /// <remarks>
    /// PUN has an ownership concept that defines who can control and destroy each PhotonView.
    /// True in case the owner matches the local PhotonPlayer.
    /// True if this is a scene photon view on the Master client.
    /// </remarks>
    public bool IsMine
    {
        get
        {
            return photonView.IsMine;
        }
    }

    /// <summary>
    /// Get Photon.connect
    /// </summary>
    public bool IsConnected
    {
        get
        {
            return bl_PhotonNetwork.IsConnected;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public GameObject FindPhotonPlayer(Player p)
    {
        GameObject player = GameObject.Find(p.NickName);
        return player == null ? null : player;
    }

    /// <summary>
    /// Get the team of players
    /// </summary>
    /// <param name="p"></param>
    /// <returns></returns>
    public Team GetTeam(Player p)
    {
        if (p == null || !IsConnected)
            return Team.All;

        return (Team)p.CustomProperties[PropertiesKeys.TeamKey];
    }

    /// <summary>
    /// Get current room gamemode
    /// </summary>
    public GameMode GetGameMode
    {
        get
        {
            if (!IsConnected || !bl_PhotonNetwork.InRoom)
                return GameMode.FFA;

            return (GameMode)bl_PhotonNetwork.CurrentRoom.CustomProperties[PropertiesKeys.GameModeKey];
        }
    }

    /// <summary>
    /// Get current gamemode (OBSOLETE)
    /// </summary>
    public GameMode GetGameModeUpdated => GetGameMode;

    /// <summary>
    /// 
    /// </summary>
    public string LocalName
    {
        get
        {
            return bl_PhotonNetwork.LocalPlayer != null && IsConnected ? bl_PhotonNetwork.LocalPlayer.NickName : "None";
        }
    }

    /// <summary>
    /// is the a one team game mode?
    /// </summary>
    public bool isOneTeamMode => GetGameMode.IsOneTeamMode();

    /// <summary>
    /// is the a one team game mode?
    /// </summary>
    public bool isOneTeamModeUpdate => GetGameModeUpdated.IsOneTeamMode();

    public Player LocalPlayer => bl_PhotonNetwork.LocalPlayer;
}