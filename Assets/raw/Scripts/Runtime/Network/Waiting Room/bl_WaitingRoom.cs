using ExitGames.Client.Photon;
using MFPS.Internal.Structures;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using HashTable = ExitGames.Client.Photon.Hashtable;

public class bl_WaitingRoom : bl_WaitingRoomBase, IMatchmakingCallbacks, IInRoomCallbacks
{
    public List<int> readyPlayers = new List<int>();
    static readonly RaiseEventOptions EventsAll = new RaiseEventOptions();

    private bool isLocalReady = false;

    /// <summary>
    /// 
    /// </summary>
    void Awake()
    {
        readyPlayers.Clear();
        PhotonNetwork.AddCallbackTarget(this);
        EventsAll.Receivers = ReceiverGroup.All;
        PhotonNetwork.NetworkingClient.EventReceived += OnEventCustom;
    }

    /// <summary>
    /// 
    /// </summary>
    void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
        PhotonNetwork.NetworkingClient.EventReceived -= OnEventCustom;
    }

    /// <summary>
    /// 
    /// </summary>
    public void AutoJoinToTeam(bool byId = false)
    {
        if (isOneTeamModeUpdate)
        {
            JoinToTeam(Team.All);
        }
        else
        {
            Team team = Team.Team1;
            if (!byId)
            {
                int team1Count = PhotonNetwork.PlayerList.GetPlayersInTeam(Team.Team1).Length;
                int team2Count = PhotonNetwork.PlayerList.GetPlayersInTeam(Team.Team2).Length;
                if (team1Count > team2Count) { team = Team.Team2; }
                else if (team1Count < team2Count) { team = Team.Team1; }
                else { team = Team.Team1; }
            }
            else
            {
                // Set the player to a team depending on the player ID to balance the teams
                int playerID = bl_PhotonNetwork.LocalPlayer.ActorNumber;
                if (playerID % 2 == 0) { team = Team.Team2; }
                else { team = Team.Team1; }
            }

            team = bl_Hook.ApplyFilters("Filter_AutoSelectedTeam", team);
            JoinToTeam(team);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public override void JoinToTeam(Team team)
    {
        bl_PhotonNetwork.LocalPlayer.SetPlayerTeam(team);
        bl_SpectatorModeBase.EnterAsSpectator = false;
    }

    /// <summary>
    /// 
    /// </summary>
    void OnLocalEnterInWaitingRoom()
    {
        var table = bl_UtilityHelper.CreatePhotonHashTable();
        table.Add(PropertiesKeys.UserRole, bl_GameData.Instance.RolePrefix);
        table.Add(PropertiesKeys.PlayerTotalScore, bl_MFPS.LocalPlayer.Stats.GetAllTimeScore());

        table = bl_Hook.ApplyFilters("Filter_RoomPropertiesReset", table);

        bl_PhotonNetwork.LocalPlayer.SetCustomProperties(table);
    }

    /// <summary>
    /// Local
    /// </summary>
    public override void SetLocalPlayerReady()
    {
        isLocalReady = !isLocalReady;
        var table = new HashTable
        {
            { PropertiesKeys.WaitingState, isLocalReady ? (int)PlayerState.Ready : (int)PlayerState.NoReady },
            { PropertiesKeys.PlayerID, bl_PhotonNetwork.LocalPlayer.ActorNumber }
        };
        PhotonNetwork.RaiseEvent(PropertiesKeys.WaitingPlayerReadyEvent, table, EventsAll, SendOptions.SendReliable);
    }

    /// <summary>
    /// From Server
    /// </summary>
    void OnReceivePlayerState(HashTable data)
    {
        if (!data.ContainsKey(PropertiesKeys.WaitingState)) return;

        PlayerState state = (PlayerState)(int)data[PropertiesKeys.WaitingState];
        int playerID = (int)data[PropertiesKeys.PlayerID];
        if (state == PlayerState.Ready)
        {
            if (!readyPlayers.Contains(playerID)) { readyPlayers.Add(playerID); }
        }
        else
        {
            if (readyPlayers.Contains(playerID)) { readyPlayers.Remove(playerID); }
        }
        OnUpdateStates();
    }

    /// <summary>
    /// Called by Master Client after click on the start button
    /// </summary>
    public override void StartGame()
    {
        if (!bl_PhotonNetwork.IsMasterClient) return;

        SetWaitingState(WaitingState.Started);
    }

    /// <summary>
    /// 
    /// </summary>
    public static void SetWaitingState(WaitingState state)
    {
        HashTable table = new HashTable
        {
            { PropertiesKeys.WaitingState, state }
        };

        HashTable expectTable = new HashTable(1);
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(PropertiesKeys.WaitingState, out object temp))
        {
            expectTable[PropertiesKeys.WaitingState] = temp;
        }

        PhotonNetwork.CurrentRoom.SetCustomProperties(table, expectTable);
        //We don't need use a RPC cuz all player will receive a callback on OnRoomPropertiesUpdate and we can work with that
    }

    /// <summary>
    /// 
    /// </summary>
    void OnUpdateStates()
    {
        bl_WaitingRoomUIBase.Instance.UpdatePlayers();
        if (bl_PhotonNetwork.IsMasterClient)
        {

        }
    }

    /// <summary>
    /// 
    /// </summary>
    void OnReceiveInitialInfo(HashTable data)
    {
        string source = (string)data[PropertiesKeys.PlayerID];
        int[] split = source.Split(","[0]).Select(x => int.Parse(x)).ToArray();
        readyPlayers.AddRange(split);
        OnUpdateStates();
    }

    /// <summary>
    /// RaiseEvent = RPC, I just used this cuz I like it more :)
    /// </summary>
    public void OnEventCustom(EventData data)
    {
        if (data.CustomData == null) return;
        if (data.CustomData is not HashTable) return;

        var t = (HashTable)data.CustomData;
        switch (data.Code)
        {
            case PropertiesKeys.WaitingPlayerReadyEvent:
                OnReceivePlayerState(t);
                break;
            case PropertiesKeys.WaitingInitialSyncEvent:
                OnReceiveInitialInfo(t);
                break;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void LoadMap()
    {
        MapInfo map = bl_GameData.Instance.AllScenes[bl_PhotonNetwork.CurrentRoom.GetRoomInfo().sceneId];
        bl_GlobalReferences.I.sceneLoader.LoadMapScene(map.RealSceneName);
    }

    /// <summary>
    /// 
    /// </summary>
    public override void ManuallyJoin()
    {
        if (bl_MFPS.GameData.UsingWaitingRoom())
        {
            StartCoroutine(WaitUntilFullyJoin(false));
        }
    }

    /// <summary>
    /// 
    /// </summary>
    IEnumerator WaitUntilFullyJoin(bool autoTeam = true)
    {
        //CHECK IF THE ROOM IS ALREADY STARTED
        while (!bl_PhotonNetwork.InRoom)
        {
            yield return null;
        }

        // check if the room has a password and if the player has entered it
        if ((string)bl_PhotonNetwork.CurrentRoom.CustomProperties[PropertiesKeys.RoomPassword] != string.Empty && !bl_Lobby.Instance.JoinedWithPassword)
        {
            // if not, that means the player enter to a room with password using the matchmaking,
            // the matchmaking is controller by Photon so it doesn't check if a room has a password before select it
            // so we can only determine this once joined to the room
            bl_PhotonNetwork.LeaveRoom();
            Debug.Log("Leaving room because this is a private room.");
            yield break;
        }

        //if this is the master client is means that the room is not initialized
        if (bl_PhotonNetwork.IsMasterClient)
        {
            //so let set the state as waiting
            SetWaitingState(WaitingState.Waiting);
        }
        else//if is a normal client
        {
            //check the state the Waiting room
            if ((WaitingState)PhotonNetwork.CurrentRoom.CustomProperties[PropertiesKeys.WaitingState] == WaitingState.Started)//if is started
            {
                //go directly to the game map and skip the waiting room menu
                PhotonNetwork.IsMessageQueueRunning = false;
                LoadMap();
                yield break;
            }
        }

        //if is not started yet -> show the UI and hide the Lobby menu
        bl_LobbyUI.Instance.HideAll();
        OnLocalEnterInWaitingRoom();
        if (autoTeam) AutoJoinToTeam();
        yield return new WaitForSeconds(0.5f);
        bl_WaitingRoomUIBase.Instance.SetActive(true);
        bl_LobbyUI.Instance.blackScreenFader.FadeOut(0.4f);
    }

    /// <summary>
    /// 
    /// </summary>
    IEnumerator MoveToMap()
    {
        bl_WaitingRoomUIBase.Instance.ShowLoadingScreen(true);
        yield return new WaitForSeconds(2);
        yield return bl_LobbyUI.Instance.blackScreenFader.FadeIn(0.3f);
        LoadMap();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="player"></param>
    /// <returns></returns>
    public override bool IsPlayerReady(Player player)
    {
        return readyPlayers.Contains(player.ActorNumber);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public override bool IsLocalReady()
    {
        return isLocalReady;
    }

    #region Photon Callbacks
    public void OnCreatedRoom()
    {
    }

    public void OnCreateRoomFailed(short returnCode, string message)
    {
    }

    public void OnFriendListUpdate(List<FriendInfo> friendList)
    {
    }

    public void OnJoinedRoom()
    {
        if (bl_MFPS.GameData.UsingWaitingRoom())
        {
            Debug.Log("Connected to room, switching to waiting room.");
            StartCoroutine(WaitUntilFullyJoin());
        }
    }

    public void OnJoinRandomFailed(short returnCode, string message)
    {
    }

    public void OnJoinRoomFailed(short returnCode, string message)
    {
    }

    public void OnLeftRoom()
    {
        bl_WaitingRoomUIBase.Instance.SetActive(false);
        if (bl_PhotonNetwork.IsConnected)
        {
            if (!bl_PhotonNetwork.InLobby) { PhotonNetwork.JoinLobby(); }
            else { Debug.Log("Leave Pre Match but already connected to lobby."); bl_LobbyUI.Instance.blackScreenFader.FadeOut(0.5f); }
        }
        else
        {
            Debug.Log("Disconnected from Pre-Match");
        }
    }

    public void OnPlayerEnteredRoom(Player newPlayer)
    {
        bl_WaitingRoomUIBase.Instance.UpdatePlayerCount();
        if (bl_PhotonNetwork.IsMasterClient)
        {
            HashTable table = new HashTable();
            string rp = string.Join(",", readyPlayers.Select(x => x.ToString()).ToArray());
            table.Add(PropertiesKeys.PlayerID, rp);

            PhotonNetwork.RaiseEvent(PropertiesKeys.WaitingPlayerReadyEvent, table, new RaiseEventOptions() { TargetActors = new int[] { newPlayer.ActorNumber } }, SendOptions.SendReliable);
        }
    }

    public void OnPlayerLeftRoom(Player otherPlayer)
    {
        bl_WaitingRoomUIBase.Instance.UpdatePlayerList();
        if (readyPlayers.Contains(otherPlayer.ActorNumber)) { readyPlayers.Remove(otherPlayer.ActorNumber); }
    }

    public void OnRoomPropertiesUpdate(HashTable propertiesThatChanged)
    {
        //if the wait state has change
        if (propertiesThatChanged.ContainsKey(PropertiesKeys.WaitingState))
        {
            WaitingState state = (WaitingState)propertiesThatChanged[PropertiesKeys.WaitingState];
            if (state == WaitingState.Started)//if the master client start the game
            {
                PhotonNetwork.IsMessageQueueRunning = false;
                //load the map scene
                StartCoroutine(MoveToMap());
            }
        }
        else
        {
            bl_WaitingRoomUIBase.Instance.UpdateRoomInfoUI();
            if (propertiesThatChanged.ContainsKey(PropertiesKeys.GameModeKey) || propertiesThatChanged.ContainsKey(PropertiesKeys.TeamKey))
            {
                if (!isOneTeamModeUpdate && bl_PhotonNetwork.LocalPlayer.GetPlayerTeam() == Team.All)
                {
                    AutoJoinToTeam(true);
                }

                bl_WaitingRoomUIBase.Instance.UpdatePlayerList();
            }

            if (propertiesThatChanged.ContainsKey(PropertiesKeys.PlayerTotalScore) || propertiesThatChanged.ContainsKey(PropertiesKeys.UserRole))
            {
                bl_WaitingRoomUIBase.Instance.UpdatePlayerList();
            }
        }
    }

    public void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        bl_WaitingRoomUIBase.Instance.UpdatePlayerCount();
        bl_WaitingRoomUIBase.Instance.UpdatePlayerList();
    }

    public void OnMasterClientSwitched(Player newMasterClient)
    {
        bl_WaitingRoomUIBase.Instance.UpdateRoomInfoUI();
    }
    #endregion

    [System.Serializable]
    public enum PlayerState
    {
        NoReady = 0,
        Ready = 1,
    }

    [System.Serializable]
    public enum WaitingState
    {
        Waiting = 0,
        Started = 1,
    }
}