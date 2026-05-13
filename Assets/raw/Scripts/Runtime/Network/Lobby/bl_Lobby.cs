using MFPS.Audio;
using MFPS.Internal.Structures;
using MFPS.Runtime.UI;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

[DefaultExecutionOrder(-998)]
public class bl_Lobby : bl_PhotonHelper, IConnectionCallbacks, ILobbyCallbacks, IMatchmakingCallbacks
{
    #region Public members
    [Header("Photon")]
    public SeverRegionCode DefaultServer = SeverRegionCode.usw;
    [LovattoToogle] public bool ShowPhotonStatistics;

    [Header("Room Options")]
    //Room Max Ping
    public int[] MaxPing = new int[] { 100, 200, 500, 1000 };
    #endregion

    #region Public properties
    public LobbyConnectionState connectionState { get; private set; } = LobbyConnectionState.Disconnected;
    public int CurrentMaxPing { get; set; }
    public int ServerRegionID { get; set; } = -1;
    public bool rememberMe { get; set; }
    public string justCreatedRoomName { get; set; }
    public bool JoinedWithPassword { get; set; } = false;
    public string CachePlayerName { get; set; }
    public Action onShowMenu;
    public static Action<string> onMenuWindowChange;
    public event Action<ExitGames.Client.Photon.Hashtable> onBerforeRoomHashtable;
    #endregion

    #region Private members
    private bool quittingApp = false;
    private bool isSeekingMatch = false;
    bool alreadyLoadHome = false;
    private bool autoRejoinLobby = false;
    private GameModeSettings[] _GameModes;
    #endregion

    /// <summary>
    /// 
    /// </summary>
    void Awake()
    {
#if ULSP
        if (!bl_PhotonNetwork.InRoom)
        {
            if (bl_ULoginUtility.RedirectToLoginIfNeeded()) return;
        }
#endif
        SetupNetworkSettings();
        Screen.sleepTimeout = SleepTimeout.SystemSetting;
        bl_UtilityHelper.BlockCursorForUser = false;
        bl_UtilityHelper.LockCursor(false);

        // Loading process
        StartCoroutine(InitialLoadProcess());
        bl_LobbyUI.Instance.InitialSetup();
    }

    /// <summary>
    /// Setup Photon
    /// </summary>
    void SetupNetworkSettings()
    {
        bl_PhotonNetwork.AddCallbackTarget(this);
        bl_PhotonNetwork.OfflineMode = false;
        bl_PhotonNetwork.IsMessageQueueRunning = true;
        PhotonNetwork.UseRpcMonoBehaviourCache = true;
        PhotonNetwork.EnableCloseConnection = true;
        PhotonNetwork.PhotonServerSettings.AppSettings.FixedRegion = DefaultServer.ToString();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    IEnumerator InitialLoadProcess()
    {
#if LOCALIZATION
        bl_Localization.Instance.LoadStoreLanguage(false);
#endif
        bl_LobbyLoadingScreenBase.Instance
     .SetActive(true)
     .SetText(bl_GameTexts.LoadingLocalContent.Localized(39));

        yield return bl_LobbyUI.Instance.blackScreenFader.FadeOut(0.5f);

        if (!bl_GameData.isDataCached)
        {
            // Load GameData asynchronously
            yield return StartCoroutine(bl_GameData.AsyncLoadData());
            yield return null;
        }

        bl_LobbyUI.Instance.Setup();

        // if the player is still in the room (e.g. after a match)
        if (bl_PhotonNetwork.InRoom && bl_MFPS.GameData.UsingWaitingRoom())
        {
            // go directly to the waiting room
            bl_WaitingRoomBase.Instance.ManuallyJoin();
            bl_LobbyLoadingScreenBase.Instance.HideIn(0.2f, true);
            CachePlayerName = NickName;
            OnLobbyReady();
        }
        else
        {
            GetPlayerName();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void OnDisable()
    {
        bl_PhotonNetwork.RemoveCallbackTarget(this);
    }

    /// <summary>
    /// 
    /// </summary>
    public void ConnectToServer()
    {
        if (IsConnectedToServer) return;

        connectionState = LobbyConnectionState.Connecting;
        PhotonNetwork.GameVersion = bl_GameData.CoreSettings.GameVersion;
        AuthenticationValues authValues = new(NickName);

#if ULSP
        if (bl_DataBase.IsUserLogged && bl_LoginProDataBase.Instance.usePhotonAuthentication)
        {
            authValues.AuthType = CustomAuthenticationType.Custom;
            authValues.AddAuthParameter("type", "0");
            authValues.AddAuthParameter("photonId", NickName);
            authValues.AddAuthParameter("userId", bl_DataBase.LocalLoggedUser.ID.ToString());
            authValues.AddAuthParameter("ip", bl_DataBase.LocalLoggedUser.IP);
            if (bl_LoginProDataBase.UseDeviceId())
            {
                authValues.AddAuthParameter("deviceId", SystemInfo.deviceUniqueIdentifier);
            }
        }
#endif

        PhotonNetwork.AuthValues = authValues;
        ServerRegionID = bl_PhotonNetwork.GetPreferedRegion();

        //if we don't have a custom region to connect or we are using a self hosted server
        if (ServerRegionID == -1 || !PhotonNetwork.PhotonServerSettings.AppSettings.UseNameServer)
        {
            //connect using the default PhotonServerSettings
            PhotonNetwork.ConnectUsingSettings();
        }
        else
        {
            //Change the cloud server region
            ConnectToServerRegion(ServerRegionID, true);
        }

        bl_LobbyLoadingScreenBase.Instance
            .SetActive(true)
            .SetText(bl_GameTexts.ConnectingToGameServer.Localized(40));
    }

    /// <summary>
    /// Change the server region (Photon PUN Only)
    /// </summary>
    public void ConnectToServerRegion(int id, bool initialConnection = false)
    {
        if (IsConnectedToServer && !initialConnection)
        {
            // When the player manually change of region
            connectionState = LobbyConnectionState.ChangingRegion;
            bl_LobbyLoadingScreenBase.Instance.SetActive(true)
           .SetText(bl_GameTexts.ConnectingToGameServer.Localized(40));

            ServerRegionID = id;
            Invoke(nameof(DelayDisconnect), 0.2f);
            return;
        }

        if (!string.IsNullOrEmpty(PhotonNetwork.PhotonServerSettings.AppSettings.AppIdRealtime))
        {
            connectionState = LobbyConnectionState.Connecting;
            SeverRegionCode code = bl_ServerRegionDropdown.GetRegionIdentifier(id);
            if (code == SeverRegionCode.none) code = DefaultServer;

            PhotonNetwork.NetworkingClient.AppId = PhotonNetwork.PhotonServerSettings.AppSettings.AppIdRealtime;
            PhotonNetwork.ConnectToRegion(code.ToString());
            PlayerPrefs.SetString(PropertiesKeys.GetUniqueKey("preferredregion2"), code.ToString());
        }
        else
        {
            Debug.LogWarning("Need your AppId for change server, please add it in PhotonServerSettings");
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void Disconnect()
    {
        SetLobbyChat(false);
        if (IsConnectedToServer)
        {
            bl_PhotonNetwork.Disconnect();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void DelayDisconnect() => bl_PhotonNetwork.Disconnect();

    /// <summary>
    /// This is called from the server every time the room list is updated.
    /// </summary>
    public void ServerList(List<RoomInfo> roomList)
    {
        if (bl_LobbyRoomListBase.Instance == null) return;

        roomList = bl_Hook.ApplyFilters("Filter_RoomList", roomList);
        bl_LobbyRoomListBase.Instance.SetRoomList(roomList);
    }

    /// <summary>
    /// 
    /// </summary>
    public void SetNickName(string nick)
    {
        nick = nick.Replace("\n", "").Trim();

        if (bl_GameData.CoreSettings.verifySingleSession)
        {
            if (string.IsNullOrEmpty(NickName))
            {
                // Session verification is required, which will be done after connect to the server.
                CachePlayerName = nick;
            }
            else NickName = nick; // in case a nick name change happens, make sure to update it.
        }
        else NickName = nick;

        if (rememberMe)
        {
            PlayerPrefs.SetString(PropertiesKeys.GetUniqueKey("remembernick"), nick);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void GetPlayerName()
    {
        if (string.IsNullOrEmpty(NickName))
        {
            // if the user has log in with an account
            if (bl_MFPSDatabase.IsUserLogged && !string.IsNullOrEmpty(bl_MFPSDatabase.User.NickName))
            {
                SetNickName(bl_MFPSDatabase.User.NickName);
                bl_EventHandler.DispatchCoinUpdate(null);
                GoToMainMenu();
            }
            else
            {
                GeneratePlayerName();
            }
        }
        else
        {
            bl_EventHandler.DispatchCoinUpdate(null);
            GoToMainMenu();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void GeneratePlayerName()
    {
        connectionState = LobbyConnectionState.WaitingForUserName;
        if (!rememberMe)
        {
            string storedNick = PropertiesKeys.GetUniqueKey("playername");
            if (!PlayerPrefs.HasKey(storedNick) || !bl_GameData.CoreSettings.RememberPlayerName)
            {
                CachePlayerName = string.Format(bl_GameData.CoreSettings.guestNameFormat, Random.Range(1, 9999));
            }
            else if (bl_GameData.CoreSettings.RememberPlayerName)
            {
                CachePlayerName = PlayerPrefs.GetString(storedNick, string.Format(bl_GameData.CoreSettings.guestNameFormat, Random.Range(1, 9999)));
            }
            bl_LobbyUI.Instance.PlayerNameField.text = CachePlayerName;
            SetNickName(CachePlayerName);
            bl_LobbyUI.Instance.ChangeWindow("player name");
            bl_LobbyLoadingScreenBase.Instance.HideIn(0.2f, true);
        }
        else
        {
            // Assign the saved nick name automatically
            CachePlayerName = PlayerPrefs.GetString(PropertiesKeys.GetUniqueKey("remembernick"));
            if (string.IsNullOrEmpty(CachePlayerName))
            {
                rememberMe = false;
                GeneratePlayerName();
                return;
            }
            SetNickName(CachePlayerName);
            GoToMainMenu();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void GoToMainMenu()
    {
        if (!IsConnectedToServer)
        {
            ConnectToServer();
        }
        else
        {
            if (!bl_PhotonNetwork.InLobby)
            {
                if (PhotonNetwork.NetworkClientState != ClientState.JoiningLobby && PhotonNetwork.IsConnectedAndReady)
                {
                    PhotonNetwork.JoinLobby();
                }
            }
            else
            {

                if (string.IsNullOrEmpty(NickName))
                {
                    Debug.LogError("Player nick name has not been authenticated!");
                }

                connectionState = LobbyConnectionState.Connected;
                if (!alreadyLoadHome) { bl_LobbyUI.Instance.Home(); alreadyLoadHome = true; }
                SetLobbyChat(true);
                ResetValues();
                bl_LobbyLoadingScreenBase.Instance.HideIn(0.2f, true);

                // Fix: room list doesn't update when the player is already in the lobby
                autoRejoinLobby = true;
                PhotonNetwork.LeaveLobby();
            }
        }
    }

    /// <summary>
    ///
    /// </summary>
    public void SetPlayerName(string InputName)
    {
        if (bl_GameData.CoreSettings.filterProfanityWords)
        {
            if (bl_StringUtility.ContainsProfanity(InputName, out _))
            {
                bl_LobbyUI.ShowOverAllMessage(bl_GameTexts.NoPermittedNickName);
                return;
            }
        }

        CachePlayerName = InputName;
        PlayerPrefs.SetString(PropertiesKeys.GetUniqueKey("playername"), CachePlayerName);
        SetNickName(CachePlayerName);
        ConnectToServer();
    }

    /// <summary>
    /// 
    /// </summary>
    public void AutoMatch()
    {
        if (isSeekingMatch)
            return;

        isSeekingMatch = true;
        StartCoroutine(DoSearch());
        IEnumerator DoSearch()
        {
            //active the search match UI
            bl_LobbyUI.Instance.SeekingMatchUI.SetActive(true);
            // This is not actually needed :)
            yield return new WaitForSeconds(3);
            PhotonNetwork.JoinRandomRoom();
            isSeekingMatch = false;
            bl_LobbyUI.Instance.SeekingMatchUI.SetActive(false);
        }
    }

    /// <summary>
    /// This function is called when hit Play button but there is not rooms to join in.
    /// </summary>
    public void OnNoRoomsToJoin(short returnCode, string message)
    {
        Debug.Log("No games to join found on matchmaking, creating one.");
        //create random room properties
        int scid = Random.Range(0, bl_GameData.Instance.AllScenes.Count);
        var mapInfo = bl_GameData.Instance.AllScenes[scid];

        var allModes = mapInfo.GetAllowedGameModes(GetGameModes());
        int modeRandom = Random.Range(0, allModes.Length);
        var gameMode = allModes[modeRandom];

        int maxPlayersRandom = Random.Range(0, gameMode.maxPlayers.Length);
        int timeRandom = Random.Range(0, gameMode.timeLimits.Length);
        int randomGoal = Random.Range(0, gameMode.GameGoalsOptions.Length);

        var roomInfo = new MFPSRoomInfo
        {
            roomName = string.Format("[PUBLIC] {0}{1}", NickName[..2], Random.Range(0, 9999)),
            gameMode = gameMode.gameMode,
            time = gameMode.timeLimits[timeRandom],
            sceneId = scid,
            roundStyle = gameMode.GetAllowedRoundMode(),
            autoTeamSelection = gameMode.AutoTeamSelection,
            goal = gameMode.GetGoalValue(randomGoal),
            friendlyFire = false,
            maxPing = MaxPing[CurrentMaxPing],
            password = string.Empty,
            withBots = gameMode.supportBots,
            maxPlayers = gameMode.maxPlayers[maxPlayersRandom],
            WeaponOption = (byte)0
        };

        roomInfo = bl_Hook.ApplyFilters("Filter_AutoCreatedRoomProps", roomInfo);

        CreateRoom(roomInfo);
    }

    /// <summary>
    /// Create a room with the settings specified in the Room Creator menu.
    /// </summary>
    public void CreateRoom()
    {
        var roomInfo = bl_LobbyRoomCreator.Instance.BuildRoomInfo();
        CreateRoom(roomInfo);
    }

    /// <summary>
    /// Create and Join in a room with the given info.
    /// </summary>
    /// <param name="roomInfo"></param>
    public void CreateRoom(MFPSRoomInfo roomInfo)
    {
        SetLobbyChat(false);
        justCreatedRoomName = roomInfo.roomName;
        JoinedWithPassword = true;
        bl_EventHandler.Lobby.onBeforeJoiningRoom?.Invoke();

        //Save Room properties for load in room
        ExitGames.Client.Photon.Hashtable roomOption = new()
        {
            [PropertiesKeys.TimeRoomKey] = roomInfo.time,
            [PropertiesKeys.GameModeKey] = roomInfo.gameMode,
            [PropertiesKeys.RoomSceneID] = roomInfo.sceneId,
            [PropertiesKeys.RoomRoundKey] = roomInfo.roundStyle,
            [PropertiesKeys.TeamSelectionKey] = roomInfo.autoTeamSelection,
            [PropertiesKeys.RoomGoal] = roomInfo.goal,
            [PropertiesKeys.RoomFriendlyFire] = roomInfo.friendlyFire,
            [PropertiesKeys.MaxPing] = roomInfo.maxPing,
            [PropertiesKeys.RoomPassword] = roomInfo.password,
            [PropertiesKeys.WithBotsKey] = roomInfo.withBots,
            [PropertiesKeys.RoomWeaponOption] = roomInfo.WeaponOption
        };

        onBerforeRoomHashtable?.Invoke(roomOption);

        PhotonNetwork.CreateRoom(roomInfo.roomName, new RoomOptions()
        {
            MaxPlayers = (byte)roomInfo.maxPlayers,
            IsVisible = true,
            IsOpen = true,
            CustomRoomProperties = roomOption,
            CleanupCacheOnLeave = true,
            CustomRoomPropertiesForLobby = roomOption.Keys.Select(x => x.ToString()).ToArray(),
            PublishUserId = true,
            EmptyRoomTtl = 0,
            BroadcastPropsChangeToAll = true,
        }, null);

        bl_LobbyUI.Instance.blackScreenFader.FadeIn(0.3f);
        if (bl_AudioController.Instance != null) { bl_AudioController.Instance.StopBackground(); }
    }

    /// <summary>
    /// 
    /// </summary>
    public void SignOut()
    {
        PlayerPrefs.SetString(PropertiesKeys.GetUniqueKey("remembernick"), string.Empty);
        Disconnect();
        bl_LobbyUI.Instance.ChangeWindow("player name");
        bl_MFPSDatabase.User.SignOut();
    }

    #region UGUI
    public void SetRememberMe(bool value)
    {
        rememberMe = value;
    }

    /// <summary>
    /// 
    /// </summary>
    public void LoadLocalLevel(string level)
    {
        if (!bl_UtilityHelper.SceneExistsInBuildSettings(level))
        {
            Debug.LogWarning($"Scene: '{level}' not exists or is disable in Build Settings.");
            return;
        }

        bl_LobbyUI.Instance.blackScreenFader.FadeIn(0.75f, () =>
         {
             bl_UtilityHelper.LoadLevel(level);
         });
    }
    #endregion

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private IEnumerator MoveToGameScene()
    {
        //Wait for check
        SetLobbyChat(false);
        if (bl_AudioController.Instance != null) { bl_AudioController.Instance.StopBackground(); }
        while (!bl_PhotonNetwork.InRoom)
        {
            yield return null;
        }
        // once we are in the room, we load the game scene, but we pause the network message queue until the scenes are loaded
        bl_PhotonNetwork.IsMessageQueueRunning = false;

        MapInfo map = bl_PhotonNetwork.CurrentRoom.GetRoomInfo().GetMapInfo();
        bl_GlobalReferences.I.sceneLoader.LoadMapScene(map.RealSceneName);
    }

    /// <summary>
    /// 
    /// </summary>
    private void OnApplicationQuit()
    {
        quittingApp = true;
        bl_GameData.isDataCached = false;
        Disconnect();
    }

    /// <summary>
    /// 
    /// </summary>
    void SetLobbyChat(bool connect)
    {
        if (bl_LobbyChat.Instance == null) return;
        if (!bl_GameData.CoreSettings.UseLobbyChat) return;

        if (connect) { bl_LobbyChat.Instance.Connect(bl_GameData.CoreSettings.GameVersion); }
        else { bl_LobbyChat.Instance.Disconnect(); }
    }

    /// <summary>
    /// Called when the lobby is ready to use
    /// Connected to server and loaded all data
    /// </summary>
    void OnLobbyReady()
    {
        bl_EventHandler.Lobby.onLobbyReady?.Invoke();
    }

    /// <summary>
    /// Local player name
    /// </summary>
    public string NickName
    {
        get => bl_PhotonNetwork.NickName;
        set => bl_PhotonNetwork.NickName = value;
    }

    /// <summary>
    /// 
    /// </summary>
    public bool IsConnectedToServer => bl_PhotonNetwork.IsConnected;

    public GameModeSettings[] GetGameModes()
    {
        if (_GameModes == null)
        {
            var gameModes = new List<GameModeSettings>();
            for (int i = 0; i < bl_GameData.Instance.gameModes.Count; i++)
            {
                if (bl_GameData.Instance.gameModes[i].isEnabled)
                {
                    gameModes.Add(bl_GameData.Instance.gameModes[i]);
                }
            }
            _GameModes = gameModes.ToArray();
        }
        return _GameModes;
    }

    #region Photon Callbacks

    /// <summary>
    /// Called once the game successfully establish connection with the game server
    /// </summary>
    public void OnConnected()
    {
        if (PhotonNetwork.PhotonServerSettings.AppSettings.UseNameServer)
        {
            Debug.Log("Server connection established to: " + PhotonNetwork.CloudRegion);
        }
        else
        {
            Debug.Log($"Server connection established to: {PhotonNetwork.ServerAddress} ({PhotonNetwork.Server})");
        }
    }

    /// <summary>
    /// Called once the game successfully establish connection with the master server
    /// </summary>
    public void OnConnectedToMaster()
    {
        // MFPS intermediately connect to a 'common' lobby.
        if (bl_PhotonNetwork.IsConnectedAndReady) PhotonNetwork.JoinLobby();
        Debug.Log("Connected to Master.");
    }

    /// <summary>
    /// Called when the player successfully connect to the common lobby
    /// </summary>
    public void OnJoinedLobby()
    {
        if (autoRejoinLobby)
        {
            autoRejoinLobby = false;
            return;
        }

        bl_LobbyUI.Instance.Home();

        // Require session verification
        if (string.IsNullOrEmpty(NickName))
        {
            connectionState = LobbyConnectionState.Authenticating;
            Debug.Log($"Player has joined to the lobby, now verifying session for {CachePlayerName}...");
            RequestSessionVerification();
            bl_LobbyLoadingScreenBase.Instance.SetText(bl_GameTexts.VerifyingSession.Localized(199));
        }
        else
        {
            connectionState = LobbyConnectionState.Connected;
            Debug.Log($"Player <b>{NickName}</b> joined to the lobby, UserId: {bl_PhotonNetwork.LocalPlayer.UserId}");
            bl_LobbyLoadingScreenBase.Instance.SetActive(true).HideIn(2, true);
            OnLobbyReady();
        }

        SetLobbyChat(true);
        ResetValues();
    }

    /// <summary>
    /// 
    /// </summary>
    public void OnLeftLobby()
    {
        if (autoRejoinLobby)
        {
            PhotonNetwork.JoinLobby();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private void RequestSessionVerification()
    {
        var user = new string[] { CachePlayerName };
        PhotonNetwork.FindFriends(user);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="cause"></param>
    public void OnDisconnected(DisconnectCause cause)
    {
        if (cause == DisconnectCause.DisconnectByClientLogic)
        {
            if (quittingApp)
            {
                Debug.Log("Disconnect from Server!");
                return;
            }
            if (ServerRegionID == -1 && connectionState != LobbyConnectionState.Authenticating && PhotonNetwork.PhotonServerSettings.AppSettings.UseNameServer)
            {
                Debug.Log("Disconnect from cloud!");
            }
            else if (connectionState == LobbyConnectionState.ChangingRegion)
            {
                Debug.Log($"Changing server! region to: {ServerRegionID}");
                ConnectToServerRegion(ServerRegionID);
                bl_LobbyRoomListBase.Instance.ClearList();
                return;
            }
            else if (connectionState == LobbyConnectionState.Authenticating)
            {
                Debug.Log($"Authentication complete, finishing connection...");
                bl_LobbyLoadingScreenBase.Instance.SetText(bl_GameTexts.FinishingUp.Localized(198));
                if (PhotonNetwork.PhotonServerSettings.AppSettings.UseNameServer)
                {
                    PhotonNetwork.AuthValues = new AuthenticationValues(NickName);
                    if (ServerRegionID == -1)
                    {
                        ConnectToServer();
                    }
                    else
                    {
                        ConnectToServerRegion(ServerRegionID);
                    }
                }
                else // if photon server is used
                {
                    ConnectToServer();
                }
                return;
            }
            else
            {
                Debug.Log("Disconnect from Server.");
            }
        }
        else
        {
            bl_LobbyUI.Instance.blackScreenFader.FadeOut(1);
            bl_LobbyUI.Instance.connectionPopupMessage.SetConfirmationText(bl_GameTexts.TryAgain.ToUpper());

            // CustomAuthenticationFailed is handled in OnCustomAuthenticationFailed(...)
            if (cause != DisconnectCause.CustomAuthenticationFailed)
            {
                bl_LobbyUI.Instance.connectionPopupMessage.AskConfirmation(string.Format(bl_GameTexts.DisconnectCause.Localized(41), cause.ToString()), () =>
                {
                    ConnectToServer();
                    Debug.LogWarning("Failed to connect to server, cause: " + cause);
                });
            }
        }
        connectionState = LobbyConnectionState.Disconnected;
    }

    public void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        Debug.Log("Room list updated, total rooms: " + roomList.Count);
        ServerList(roomList);
    }

    public void OnRegionListReceived(RegionHandler regionHandler)
    {
    }

    public void ResetValues()
    {
        if (bl_LobbyRoomCreatorUI.Instance == null) { bl_LobbyRoomCreatorUI.Instance = transform.GetComponentInChildren<bl_LobbyRoomCreatorUI>(true); }
        //Create a random name for a future room that player create
        bl_LobbyRoomCreatorUI.Instance.SetupSelectors();
        bl_PhotonNetwork.IsMessageQueueRunning = true;
    }

    public void OnJoinedRoom()
    {
        if (!bl_MFPS.GameData.UsingWaitingRoom())
        {
            Debug.Log($"Local client joined to the room '{bl_PhotonNetwork.CurrentRoom.Name}'");
            StartCoroutine(MoveToGameScene());
        }
        else
        {
            SetLobbyChat(false);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="data"></param>
    public void OnCustomAuthenticationResponse(Dictionary<string, object> data)
    {
        Debug.LogWarning($"Custom authentication response");
        foreach (var item in data)
        {
            Debug.Log($"CA Pair: {item.Key}: {item.Value}");
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="debugMessage"></param>
    public void OnCustomAuthenticationFailed(string debugMessage)
    {
        bl_LobbyUI.Instance.connectionPopupMessage.AskConfirmation($"Custom Authentication Failed: {debugMessage}", () =>
        {
            ConnectToServer();
        });

        Debug.LogWarning($"Custom Authentication Failed: {debugMessage}");
    }

    public void OnLobbyStatisticsUpdate(List<TypedLobbyInfo> lobbyStatistics)
    {
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="friendList"></param>
    public void OnFriendListUpdate(List<FriendInfo> friendList)
    {
        if (string.IsNullOrEmpty(NickName))
        {
            foreach (var player in friendList)
            {
                if (player.UserId == CachePlayerName && player.IsOnline)
                {
                    // Another session is open
                    Debug.Log("This account has a session open.");
                    bl_LobbyUI.Instance.connectionPopupMessage.SetConfirmationText(bl_GameTexts.Ok.ToUpper());
                    bl_LobbyUI.Instance.connectionPopupMessage.AskConfirmation(bl_GameTexts.AnotherSessionOpen.Localized(197), () =>
                    {
                        connectionState = LobbyConnectionState.Disconnected;
                        Disconnect();
                        bl_LobbyLoadingScreenBase.Instance.HideIn(0.2f, true);
                        SignOut();
                    });
                    return;
                }
            }

            // No session open for this account
            connectionState = LobbyConnectionState.Authenticating;
            NickName = CachePlayerName;
            bl_EventHandler.DispatchCoinUpdate(null);
            Disconnect();
        }
    }

    public void OnCreatedRoom()
    {
    }

    public void OnCreateRoomFailed(short returnCode, string message)
    {
    }

    public void OnJoinRoomFailed(short returnCode, string message)
    {
    }

    public void OnJoinRandomFailed(short returnCode, string message)
    {
        OnNoRoomsToJoin(returnCode, message);
    }

    public void OnLeftRoom()
    {
        JoinedWithPassword = false;
    }
    #endregion

    public AddonsButtonsHelper AddonsButtons = new();
    public class AddonsButtonsHelper
    {
        public GameObject this[int index]
        {
            get
            {
                return bl_LobbyUI.Instance.AddonsButtons[index];
            }
        }
    }

    private static bl_Lobby _instance;
    public static bl_Lobby Instance
    {
        get
        {
            _instance = _instance != null ? _instance : FindAnyObjectByType<bl_Lobby>();
            return _instance;
        }
    }
}