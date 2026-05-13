using MFPS.Runtime.AI;
using Photon.Pun;
using Photon.Realtime;
using System;
using UnityEngine;

[Serializable]
public class MFPSPlayer
{
    public string Name;
    public Transform Actor;
    public bool isRealPlayer = true;
    public bool isAlive = true;
    public Team Team = Team.None;
    public Transform AimPosition;

    public PhotonView m_actorView;

    /// <summary>
    /// Photon View of this actor
    /// </summary>
    public PhotonView ActorView
    {
        get => m_actorView;
        set
        {
            m_actorView = value;
            if (m_actorView != null) ActorViewID = m_actorView.ViewID;
        }
    }

    private int m_viewID;
    /// <summary>
    /// Photon View ID of this actor
    /// </summary>
    public int ActorViewID
    {
        get => m_viewID;
        set => m_viewID = value;
    }

    /// <summary>
    /// Is this actor is a local player
    /// </summary>
    public bool IsLocal => (ActorView != null && ActorView.IsMine) && isRealPlayer;

    /// <summary>
    /// Photon Player Actor Number of this actor
    /// </summary>
    public int ActorNumber
    {
        get
        {
            return ActorView == null || ActorView.Owner == null ? -1 : ActorView.Owner.ActorNumber;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public MFPSBotProperties GetBotStats()
    {
        MFPSBotProperties botProps = bl_AIMananger.Instance.GetBotStatistics(Name);
        if (botProps == null)
        {
            Debug.LogWarning($"Bot {Name} not found in AI Manager, this could be a de-sync issue.");
            //return a empty container to prevent any error
            botProps = new MFPSBotProperties
            {
                Name = Name
            };
        }
        return botProps;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public Player GetNetworkPlayer()
    {
        if (ActorView != null) return ActorView.Owner;
        else
        {
            var playerList = bl_PhotonNetwork.PlayerList;
            for (int i = 0; i < playerList.Length; i++)
            {
                if (playerList[i].NickName == Name) return playerList[i];
            }
            return null;
        }
    }

    /// <summary>
    /// Get a real player or bot property value
    /// </summary>
    /// <param name="key"></param>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    public object GetPlayerPropertie(string key, object defaultValue = null)
    {
        switch (key)
        {
            case PropertiesKeys.KillsKey:
                return isRealPlayer ? GetNetworkPlayer().GetKills() : GetBotStats().Kills;
            case PropertiesKeys.DeathsKey:
                return isRealPlayer ? GetNetworkPlayer().GetDeaths() : GetBotStats().Deaths;
            case PropertiesKeys.ScoreKey:
                return isRealPlayer ? GetNetworkPlayer().GetPlayerScore() : GetBotStats().Score;
            default:
                Debug.LogWarning($"Property {key} has not been setup yet.");
                return defaultValue;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="player"></param>
    /// <returns></returns>
    public bool IsTeamMate(MFPSPlayer player)
    {
        return player.Team == Team;
    }

    /// <summary>
    /// Is this player is a team mate of the local player
    /// NOTE: This only check if the local player is in the same team as this player, in solo game modes even if they are in the same team, they are not team mates.
    /// </summary>
    /// <returns></returns>
    public bool IsTeamMateOfLocalPlayer()
    {
        return Team == bl_MFPS.LocalPlayer.Team;
    }

    /// <summary>
    /// Is this player is death or null (actor transform instance is null)
    /// </summary>
    /// <returns></returns>
    public bool IsDeathOrNull()
    {
        return !isAlive || Actor == null;
    }

    public static MFPSPlayer FindActorByNetView(int netViewID)
    {
        if (bl_GameManager.Instance == null)
        {
            return null;
        }

        return bl_GameManager.Instance.GetMFPSActor(netViewID);
    }

    #region Constructors    
    public MFPSPlayer() { }

    public MFPSPlayer(PhotonView view, bool realPlayer = true, bool alive = true)
    {
        BuildFromView(view, realPlayer, alive);
    }

    public MFPSPlayer BuildFromView(PhotonView view, bool realPlayer = true, bool alive = true)
    {
        isRealPlayer = realPlayer;
        isAlive = alive;
        if (view == null) return this;

        Actor = view.transform;
        ActorView = view;
        AimPosition = Actor;
        if (view.InstantiationData != null)
        {
            if (!isRealPlayer)
            {
                Name = (string)view.InstantiationData[0];
                Team = (Team)view.InstantiationData[1];
            }
            else
            {
                Name = view.Owner.NickName;
                Team = (Team)view.InstantiationData[0];
            }
        }
        return this;
    }
    #endregion
}

public class NonPlayerTarget
{
    public bl_AITarget Target;
    public int OwnerActorNumber;
    public Team Team;

    public bool IsAlive()
    {
        return Target != null && !Target.IsDeath;
    }

    public bool IsFriendly()
    {
        return Team == bl_MFPS.LocalPlayer.Team;
    }

    public bool IsLocalOwner()
    {
        return OwnerActorNumber == bl_MFPS.LocalPlayer.ActorNumber;
    }
}