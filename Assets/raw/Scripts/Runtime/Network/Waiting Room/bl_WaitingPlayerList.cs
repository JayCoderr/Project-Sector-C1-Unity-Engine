using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;

public class bl_WaitingPlayerList : bl_WaitingPlayerListBase
{
    public GameObject WaitingPlayerPrefab;
    public RectTransform PlayerListPanel;
    public List<RectTransform> PlayerListHeaders = new List<RectTransform>();

    private List<bl_WaitingPlayerUIBase> playerListCache = new List<bl_WaitingPlayerUIBase>();

    /// <summary>
    /// 
    /// </summary>
    public override void InstancePlayerList()
    {
        playerListCache.ForEach(x => { if (x != null) { Destroy(x.gameObject); } });
        playerListCache.Clear();

        Player[] list = bl_PhotonNetwork.PlayerList;
        List<Player> secondTeam = new List<Player>();
        bool otm = isOneTeamModeUpdate;
        PlayerListHeaders.ForEach(x => x.gameObject.SetActive(!otm));
        for (int i = 0; i < list.Length; i++)
        {
            if (otm)
            {
                if (bl_PhotonNetwork.IsMasterClient)
                {
                    if (list[i].GetPlayerTeam() != Team.All)
                    {
                        list[i].SetPlayerTeam(Team.All);
                    }
                }
                SetPlayerToList(list[i], Team.All);
            }
            else
            {
                if (list[i].GetPlayerTeam() == Team.Team1)
                {
                    SetPlayerToList(list[i], Team.Team1);
                }
                else if (list[i].GetPlayerTeam() == Team.Team2)
                {
                    secondTeam.Add(list[i]);
                }
            }
        }
        if (!otm) { PlayerListHeaders[1].SetAsLastSibling(); }
        if (secondTeam.Count > 0)
        {
            for (int i = 0; i < secondTeam.Count; i++)
            {
                SetPlayerToList(secondTeam[i], Team.Team2);
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public override void UpdatePlayerProperties()
    {
        playerListCache.ForEach(x => { if (x != null) x.UpdateState(); });
    }

    /// <summary>
    /// 
    /// </summary>
    public override void SetPlayerToList(Player player, Team team)
    {
        GameObject g = Instantiate(WaitingPlayerPrefab) as GameObject;
        var wp = g.GetComponent<bl_WaitingPlayerUIBase>();
        wp.SetInfo(player);
        g.transform.SetParent(PlayerListPanel, false);
        playerListCache.Add(wp);
    }
}