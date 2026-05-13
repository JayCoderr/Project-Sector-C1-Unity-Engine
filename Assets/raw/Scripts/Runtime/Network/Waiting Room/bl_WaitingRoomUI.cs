using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class bl_WaitingRoomUI : bl_WaitingRoomUIBase
{
    [Header("References")]
    public GameObject Content;
    public GameObject LoadingMapUI;
    public GameObject StartScreen;
    public GameObject waitingRequiredPlayersUI;
    public GameObject spectatorButton = null;
    public RectTransform PlayerListPanel;
    public TextMeshProUGUI RoomNameText;
    public TextMeshProUGUI MapNameText;
    public TextMeshProUGUI GameModeText;
    public TextMeshProUGUI TimeText;
    public TextMeshProUGUI GoalText;
    public TextMeshProUGUI BotsText;
    public TextMeshProUGUI FriendlyFireText;
    public TextMeshProUGUI PlayerCountText;
    public Image MapPreview;
    public Button[] readyButtons;
    public bl_WaitingPlayerListBase playerList;

    /// <summary>
    /// 
    /// </summary>
    private void OnEnable()
    {
        Content.SetActive(false);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="show"></param>
    public override void ShowLoadingScreen(bool show)
    {
        LoadingMapUI.SetActive(show);
    }

    /// <summary>
    /// 
    /// </summary>
    public override void SetActive(bool active)
    {
        if (active)
        {
            UpdateRoomInfoUI();
            playerList.InstancePlayerList();
            UpdatePlayerCount();
            Content.SetActive(true);
            StartScreen.SetActive(true);
        }
        else
        {
            StartScreen.SetActive(false);
            Content.SetActive(false);
            bl_LobbyUI.Instance.blackScreenFader.FadeOut(0.5f);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public override void UpdateRoomInfoUI()
    {
        GameMode mode = GetGameModeUpdated;
        var room = bl_PhotonNetwork.CurrentRoom;
        RoomNameText.text = room.Name.ToUpper();
        int mapId = (int)room.CustomProperties[PropertiesKeys.RoomSceneID];
        var si = bl_GameData.Instance.AllScenes[mapId];
        MapPreview.sprite = si.Preview;
        MapNameText.text = si.ShowName.ToUpper();
        GameModeText.text = mode.GetName().ToUpper();
        int t = (int)room.CustomProperties[PropertiesKeys.TimeRoomKey];
        TimeText.text = (t / 60).ToString().ToUpper() + ":00";
        BotsText.text = string.Format("BOTS: {0}", (bool)room.CustomProperties[PropertiesKeys.WithBotsKey] ? "ON" : "OFF");
        FriendlyFireText.text = string.Format("FRIENDLY FIRE: {0}", (bool)room.CustomProperties[PropertiesKeys.RoomFriendlyFire] ? "ON" : "OFF");
        UpdatePlayerCount();
        readyButtons[0].gameObject.SetActive(bl_PhotonNetwork.IsMasterClient);
        readyButtons[1].gameObject.SetActive(!bl_PhotonNetwork.IsMasterClient);
        readyButtons[1].GetComponentInChildren<TextMeshProUGUI>().text = bl_WaitingRoomBase.Instance.IsLocalReady() ? "CANCEL".Localized(67).ToUpper() : "READY".Localized(184).ToUpper();

        string goal = room.CustomProperties[PropertiesKeys.RoomGoal].ToString();
        if (goal == "0" || string.IsNullOrEmpty(goal))
        {
            GoalText.text = GetGameModeUpdated.GetModeInfo().GoalName.ToUpper();
        }
        else
        {
            GoalText.text = $"{goal} {GetGameModeUpdated.GetModeInfo().GoalName.ToUpper()}";
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public override void UpdatePlayerCount()
    {
        int required = GetGameModeUpdated.GetGameModeInfo().RequiredPlayersToStart;
        if (required > 1)
        {
            bool allRequired = (bl_PhotonNetwork.PlayerList.Length >= required);
            readyButtons[0].interactable = (bl_PhotonNetwork.IsMasterClient && bl_PhotonNetwork.PlayerList.Length >= required);
            PlayerCountText.text = string.Format("{0} OF {2} PLAYERS ({1} MAX)", bl_PhotonNetwork.PlayerList.Length, bl_PhotonNetwork.CurrentRoom.MaxPlayers, required);
            waitingRequiredPlayersUI?.SetActive(!allRequired);
        }
        else
        {
            readyButtons[0].interactable = true;
            waitingRequiredPlayersUI?.SetActive(false);
            PlayerCountText.text = string.Format("{0} PLAYERS ({1} MAX)", bl_PhotonNetwork.PlayerList.Length, bl_PhotonNetwork.CurrentRoom.MaxPlayers);
        }

        int spectatorsCount = GetSpectatorsCount();
        if (spectatorsCount > 0)
        {
            PlayerCountText.text += $" SPECTATORS {spectatorsCount}";
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public override void UpdatePlayers()
    {
        playerList.UpdatePlayerProperties();
    }

    /// <summary>
    /// 
    /// </summary>
    public override void UpdatePlayerList()
    {
        playerList.InstancePlayerList();
        if (spectatorButton != null)
        {
            spectatorButton.SetActive(bl_PhotonNetwork.LocalPlayer.GetPlayerTeam() != Team.None);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void SetLocalReady()
    {
        bl_WaitingRoomBase.Instance.SetLocalPlayerReady();
        readyButtons[1].GetComponentInChildren<TextMeshProUGUI>().text = bl_WaitingRoomBase.Instance.IsLocalReady() ? "CANCEL".Localized(67).ToUpper() : "READY".Localized(184).ToUpper();
    }

    /// <summary>
    /// 
    /// </summary>
    public void MasterStartTheGame()
    {
        bl_WaitingRoomBase.Instance.StartGame();
    }

    /// <summary>
    /// 
    /// </summary>
    public void EnterAsSpectator()
    {
        bl_LobbyUI.ShowConfirmationWindow(bl_GameTexts.EnterAsSpectator.Localized(211), () =>
        {
            bl_WaitingRoomBase.Instance.JoinToTeam(Team.None);
            bl_SpectatorModeBase.EnterAsSpectator = true;
            if (spectatorButton != null) spectatorButton.SetActive(false);
        });
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private int GetSpectatorsCount()
    {
        int count = 0;
        var players = bl_PhotonNetwork.PlayerList;
        foreach (var player in players)
        {
            if (player.GetPlayerTeam() == Team.None)
            {
                count++;
            }
        }
        return count;
    }

    /// <summary>
    /// 
    /// </summary>
    public void LeaveRoom(bool comfirmed)
    {
        if (comfirmed)
        {
            bl_LobbyUI.Instance.blackScreenFader.FadeIn(0.5f);
            bl_PhotonNetwork.LeaveRoom();
        }
        else
        {
            bl_LobbyUI.ShowConfirmationWindow(bl_GameTexts.LeaveRoomConfirmation.Localized(209), () =>
            {
                LeaveRoom(true);
            });
        }
    }

}