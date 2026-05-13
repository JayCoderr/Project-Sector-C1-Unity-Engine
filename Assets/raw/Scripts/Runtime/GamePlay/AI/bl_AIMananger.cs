using MFPS.Runtime.AI;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class bl_AIMananger : bl_PhotonHelper
{
    #region Public members
    [Header("Settings")]
    public int updateBotsLookAtEach = 50;
    public float distanceCheckInterval = 3;

    /// <summary>
    /// Information and stats of all the bots currently playing
    /// </summary>
    public List<MFPSBotProperties> BotsStatistics
    {
        get; set;
    } = new();
    #endregion

    #region Public properties
    /// <summary>
    /// Is this game using bots?
    /// </summary>
    public bool BotsActive
    {
        get;
        set;
    }

    /// <summary>
    /// Is the bots information already synced by the Mater client?
    /// </summary>
    public bool HasMasterInfo
    {
        get;
        set;
    } = false;
    #endregion

    #region Events
    public delegate void EEvent(List<MFPSBotProperties> stats);
    public static EEvent OnMaterStatsReceived;
    public delegate void StatEvent(MFPSBotProperties stat);
    public static StatEvent OnBotStatUpdate;
    #endregion

    #region Private members
    private bl_GameManager GameManager;
    private List<PlayersSlots> Team1PlayersSlots = new();
    private List<PlayersSlots> Team2PlayersSlots = new();
    private readonly List<string> BotsNames = new();
    private readonly List<bl_AIShooter> SpawningBots = new();
    private readonly List<string> lastLifeBots = new();
    private readonly List<bl_AIShooter> AllBots = new();
    private readonly List<bl_PlayerReferencesCommon> targetsLists = new();
    private readonly List<bl_AITarget> nonTargetedBots = new();
    private int NumberOfBots = 5;
    private bool isMasterAlredyInTeam = false;
    private Dictionary<string, GameObject> modelCache;
    private float timeSinceLastCheck = 0f;
    private readonly Dictionary<(string, string), float> distanceCache = new();
    #endregion

    /// <summary>
    /// 
    /// </summary>
    private void Awake()
    {
        GameManager = bl_GameManager.Instance;
        BotsNames.AddRange(bl_GameTexts.RandomNames);
        bl_PhotonCallbacks.PlayerPropertiesUpdate += OnPhotonPlayerPropertiesChanged;
        bl_PhotonCallbacks.PlayerEnteredRoom += OnPlayerEnter;
        bl_PhotonCallbacks.MasterClientSwitched += OnMasterClientSwitched;
        bl_PhotonCallbacks.PlayerLeftRoom += OnPlayerLeft;

        if (!bl_PhotonNetwork.IsConnected)
            return;

        CheckViewAllocation();
        BotsActive = bl_RoomSettings.GetRoomInfo().withBots;
        NumberOfBots = bl_RoomSettings.GetRoomInfo().maxPlayers;

        bl_EventHandler.onRemoteActorChange += OnRemotePlayerChange;
        bl_EventHandler.onLocalPlayerDeath += OnLocalDeath;
        bl_EventHandler.onLocalPlayerSpawn += OnLocalPlayerSpawn;
    }

    /// <summary>
    /// 
    /// </summary>
    private void Start()
    {
        FirstSpawn();
        if (bl_MFPS.GameData.UsingWaitingRoom() && bl_PhotonNetwork.IsMasterClient)
        {
            // this delay is necessary to wait for the player to join the room when the waiting room is enabled
            // ideally, it should not be a fixed delay, each player should send a ready signal to the master client and then this information should be sent to it.
            this.InvokeAfter(2, SyncBotsDataToAllOthers);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private void OnDisable()
    {
        bl_PhotonCallbacks.PlayerPropertiesUpdate -= OnPhotonPlayerPropertiesChanged;
        bl_PhotonCallbacks.PlayerEnteredRoom -= OnPlayerEnter;
        bl_PhotonCallbacks.MasterClientSwitched -= OnMasterClientSwitched;
        bl_PhotonCallbacks.PlayerLeftRoom -= OnPlayerLeft;
        bl_EventHandler.onRemoteActorChange -= OnRemotePlayerChange;
        bl_EventHandler.onLocalPlayerDeath -= OnLocalDeath;
        bl_EventHandler.onLocalPlayerSpawn -= OnLocalPlayerSpawn;
    }

    /// <summary>
    /// 
    /// </summary>
    void Update()
    {
        timeSinceLastCheck += Time.deltaTime;
        if (timeSinceLastCheck >= distanceCheckInterval)
        {
            CheckProximity();
            timeSinceLastCheck = 0f;
        }
    }

    /// <summary>
    /// Send the bots data to all other clients in the room
    /// This data will automatically send to new players
    /// </summary>
    void SyncBotsDataToAllOthers()
    {
        if (!bl_PhotonNetwork.IsMasterClient) return;

        Player[] players = bl_PhotonNetwork.PlayerList;
        string line = GetCompiledBotsData();
        //and send to the new player so him can have the data and update locally.
        photonView.RPC(nameof(SyncAllBotsStats), RpcTarget.Others, line, 0);

        //also send the slots data so all player have the same list in case the Master Client leave the game
        line = GetCompiledSlotsData();
        //and send to the new player so him can have the data and update locally.
        photonView.RPC(nameof(SyncAllBotsStats), RpcTarget.Others, line, 1);
        bl_EventHandler.Bots.onBotsInitializated?.Invoke();
    }

    /// <summary>
    /// Gets the bots data as a string line
    /// </summary>
    /// <returns></returns>
    public string GetCompiledBotsData()
    {
        //so first we recollect all the stats from the master client and join it in a string line
        string line = string.Empty;
        for (int i = 0; i < BotsStatistics.Count; i++)
        {
            MFPSBotProperties b = BotsStatistics[i];
            line += string.Format("{0},{1},{2},{3},{4},{5},{6}|", b.Name, b.Kills, b.Deaths, b.Assists, b.Score, (int)b.Team, b.ViewID);
        }
        return line;
    }

    /// <summary>
    /// Get the slots list in a string line
    /// </summary>
    /// <returns></returns>
    public string GetCompiledSlotsData()
    {
        string line = string.Empty;
        for (int i = 0; i < Team1PlayersSlots.Count; i++)
        {
            var d = Team1PlayersSlots[i];
            line += string.Format("{0},{1}|", d.Player, d.Bot);
        }
        line += "&";
        if (!isOneTeamMode)
        {
            for (int i = 0; i < Team2PlayersSlots.Count; i++)
            {
                var d = Team2PlayersSlots[i];
                line += string.Format("{0},{1}|", d.Player, d.Bot);
            }
        }
        return line;
    }

    /// <summary>
    /// Instance all bots for the first time
    /// </summary>
    void FirstSpawn()
    {
        if (!bl_PhotonNetwork.IsMasterClient) return;

        SetUpSlots(true);

        if (!BotsActive) return;

        if (isOneTeamMode)
        {
            // allocate all the bots in the same team
            int requiredBots = EmptySlotsCount(Team.All);
            for (int i = 0; i < requiredBots; i++)
            {
                SpawnBot(null, Team.All);
            }
        }
        else
        {
            // split the bots between the teams
            int half = EmptySlotsCount(Team.Team1);
            for (int i = 0; i < half; i++)
            {
                SpawnBot(null, Team.Team1);
            }
            half = EmptySlotsCount(Team.Team2);
            for (int i = 0; i < half; i++)
            {
                SpawnBot(null, Team.Team2);
            }
        }
    }

    /// <summary>
    /// Setup the team slots where players and bots can be assigned.
    /// </summary>
    void SetUpSlots(bool addExistingPlayers)
    {
        Team1PlayersSlots.Clear();
        Team2PlayersSlots.Clear();
        var team1Players = bl_PhotonNetwork.PlayerList.GetPlayersInTeam(isOneTeamMode ? Team.All : Team.Team1).ToList();
        var team2Players = bl_PhotonNetwork.PlayerList.GetPlayersInTeam(Team.Team2).ToList();

        int slots = NumberOfBots;
        if (!isOneTeamMode)
        {
            var gameModeInfo = GetGameMode.GetGameModeInfo();

            if (gameModeInfo.botsTeamSelection == GameModeSettings.PlayerAutoTeamSelection.SplitInTeams)
            {
                if (slots > 1) slots /= 2;
                AssignSlot(slots, addExistingPlayers, team1Players, ref Team1PlayersSlots);
                AssignSlot(slots, addExistingPlayers, team2Players, ref Team2PlayersSlots);
            }
            else
            {
                if (gameModeInfo.botsTeamSelection == GameModeSettings.PlayerAutoTeamSelection.AllInTeam2)
                {
                    AssignSlot(slots, addExistingPlayers, team2Players, ref Team2PlayersSlots);
                }
                else
                {
                    AssignSlot(slots, addExistingPlayers, team1Players, ref Team1PlayersSlots);
                }
            }
        }
        else
        {
            AssignSlot(slots, addExistingPlayers, team1Players, ref Team1PlayersSlots);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private void AssignSlot(int slots, bool addExistingPlayers, List<Player> players, ref List<PlayersSlots> botSlots)
    {
        for (int i = 0; i < slots; i++)
        {
            PlayersSlots s = new() { Bot = string.Empty };

            if (addExistingPlayers && players.Count > 0)
            {
                s.Player = players[0].NickName;
                players.RemoveAt(0);
            }
            else
            {
                s.Player = string.Empty;
            }
            botSlots.Add(s);
        }
    }

    /// <summary>
    /// Spawn the a bot in the selected team
    /// You can set the agent as null to instantiate it for the first time
    /// </summary>
    public bl_AIShooter SpawnBot(bl_AIShooter agent = null, Team _team = Team.None)
    {
        var spawnPoint = bl_SpawnPointManagerBase.Instance.GetSequentialSpawnPoint(_team);
        string AiName = bl_GameData.Instance.BotTeam1.name;

        int rbn = Random.Range(0, BotsNames.Count);
        string AIName = agent == null ? string.Format(bl_GameData.CoreSettings.botsNameFormat, BotsNames[rbn]) : agent.AIName;
        Team AITeam = agent == null ? _team : agent.AITeam;

        if (agent != null)//if this is not a new bot but a bot that was killed
        {
            AiName = (agent.AITeam == Team.Team2) ? bl_GameData.Instance.BotTeam2.name : bl_GameData.Instance.BotTeam1.name;

#if PSELECTOR
            AiName = bl_PlayerSelector.GetBotForTeam(AITeam, AIName).name;
#endif

            if (agent.AITeam == Team.None) { Debug.LogError($"bot {agent.AIName} has not team"); }

            //Check if the bot has been assigned to a team, or if not, check if there's a space for him
            if (VerifyTeamAffiliation(agent, agent.AITeam))
            {
                spawnPoint = bl_SpawnPointManagerBase.Instance.GetSpawnPointForTeam(agent.AITeam);
            }
            else // there's not space in the team for this bot
            {
                //Check if the bot was registered in a team before
                int ind = BotsStatistics.FindIndex(x => x.Name == agent.AIName);
                if (ind != -1 && ind <= BotsStatistics.Count - 1)
                {
                    //delete the bot data since it won't play anymore.
                    BotsStatistics.RemoveAt(ind);
                }
                return null;
            }
        }
        else
        {
            AiName = (_team == Team.Team2) ? bl_GameData.Instance.BotTeam2.name : bl_GameData.Instance.BotTeam1.name;

#if PSELECTOR
            AiName = bl_PlayerSelector.GetBotForTeam(_team, AIName).name;
#endif

            if (!isOneTeamMode)//if team mode, spawn bots in the respective team spawn points.
            {
                spawnPoint = bl_SpawnPointManagerBase.Instance.GetSpawnPointForTeam(_team);
            }
        }

        spawnPoint.GetSpawnPosition(out Vector3 spawnPosition, out Quaternion spawnRot);

        spawnPosition = bl_Hook.ApplyFilters("Filter_BotSpawnPosition", spawnPosition);

        object[] botEssentialData = new object[] { AIName, AITeam };
        botEssentialData = bl_Hook.ApplyFilters("Filter_BotNetInstanceData", botEssentialData);

        var hookData = new object[] { AiName, _team };
        hookData = bl_Hook.ApplyFilters("Filter_BotSpawnPrefab", hookData);

        AiName = (string)hookData[0];

        //use InstantiateSceneObject to make the bots by controlled by Master Client but not destroy them when MC leave the room.
        GameObject bot = PhotonNetwork.InstantiateRoomObject(AiName, spawnPosition, spawnRot, 0, botEssentialData);

        bl_AIShooter newAgent = bot.GetComponent<bl_AIShooter>();
        //if this bot was already in the game
        if (agent != null)
        {
            newAgent.AIName = agent.AIName;
            newAgent.AITeam = agent.AITeam;
            photonView.RPC(nameof(SyncBotStat), RpcTarget.Others, agent.AIName, bot.GetComponent<PhotonView>().ViewID, (byte)3, false);
        }
        else//if this is the first time instancing this bot
        {
            newAgent.AIName = AIName;
            newAgent.AITeam = _team;
            BotsNames.RemoveAt(rbn);
            //insert bot stats
            var bs = new MFPSBotProperties
            {
                Name = newAgent.AIName,
                Team = _team,
                ViewID = bot.GetComponent<PhotonView>().ViewID
            };
            BotsStatistics.Add(bs);
            //reserve a space in the team for this bot
            VerifyTeamAffiliation(newAgent, _team);
        }
        newAgent.Init();
        nonTargetedBots.Add(newAgent.AimTarget);

        //Build Player Data
        MFPSPlayer playerData = new()
        {
            Name = newAgent.AIName,
            Team = newAgent.AITeam,
            Actor = newAgent.transform,
            AimPosition = newAgent.AimTarget.Transform,
            isRealPlayer = false,
            isAlive = true,
        };

        bl_EventHandler.DispatchRemoteActorChange(new bl_EventHandler.PlayerChangeData()
        {
            PlayerName = newAgent.AIName,
            MFPSActor = playerData,
            IsAlive = true,
            NetworkView = newAgent.GetComponent<PhotonView>()
        });

        AllBots.Add(newAgent);

        UpdateTargetList();

        return newAgent;
    }

    /// <summary>
    /// Check if the bot is already assigned in a Team slot
    /// </summary>
    /// <returns></returns>
    private bool VerifyTeamAffiliation(bl_AIShooter agent, Team team)
    {
        var playerSlots = team == Team.Team2 ? Team2PlayersSlots : Team1PlayersSlots;
        //check if the bot is assigned in the team
        if (playerSlots.Exists(x => x.Bot == agent.AIName)) return true;
        else
        {
            //if it's not assigned, check if we can add him
            if (HasSpaceInTeamForBot(team))
            {
                //assign the bot to the team
                int index = playerSlots.FindIndex(x => x.Player == string.Empty && x.Bot == string.Empty);
                playerSlots[index].Bot = agent.AIName;
                return true;
            }
            else { return false; }//bot can't be assigned in team
        }
    }

    /// <summary>
    /// Fetch all the available players (alive) in the map.
    /// </summary>
    private void UpdateTargetList()
    {
        if (!bl_PhotonNetwork.IsMasterClient || bl_GameManager.Instance == null) return;

        targetsLists.Clear();
        var all = bl_GameManager.Instance.OthersActorsInScene;

        for (int i = 0; i < all.Count; i++)
        {
            if (all[i].Actor == null) continue;
            targetsLists.Add(all[i].Actor.GetComponent<bl_PlayerReferencesCommon>());
        }

        if (bl_MFPS.LocalPlayerReferences != null)
        {
            targetsLists.Add(bl_MFPS.LocalPlayerReferences);
        }

        // Update the targets for each bot
        for (int i = 0; i < AllBots.Count; i++)
        {
            if (AllBots[i] == null) continue;

            AllBots[i].UpdateTargetList();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private void CheckProximity()
    {
        int botCount = targetsLists.Count;
        distanceCache.Clear();

        for (int i = 0; i < botCount; i++)
        {
            for (int j = i + 1; j < botCount; j++)
            {
                var bot1 = targetsLists[i];
                var bot2 = targetsLists[j];

                float distance = 0;
                if (bot1 != bot2 && bot1 != null && bot2 != null)
                {
                    distance = Vector3.Distance(bot1.Position, bot2.Position);
                }

                distanceCache[(bot1.PlayerName, bot2.PlayerName)] = distance;
                distanceCache[(bot2.PlayerName, bot1.PlayerName)] = distance; // Cache the distance both ways
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="botIndexA"></param>
    /// <param name="botIndexB"></param>
    /// <returns></returns>
    public static float GetCachedDistance(bl_AITarget botIndexA, bl_AITarget botIndexB)
    {
        if (Instance == null) return -1;
        if (botIndexA == null || botIndexB == null) return 0;
        if (Instance.distanceCache.Count == 0)
        {
            Instance.UpdateTargetList();
            Instance.CheckProximity();
        }

        string bot1Name = botIndexA.Name;
        string bot2Name = botIndexB.Name;

        if (Instance.distanceCache.TryGetValue((bot1Name, bot2Name), out float cachedDistance))
        {
            return cachedDistance;
        }
        else
        {
            // If for some reason the distance is not cached, calculate it (this should not happen if everything is updated correctly)
            float distance = Vector3.Distance(botIndexA.position, botIndexB.position);
            Instance.distanceCache[(bot1Name, bot2Name)] = distance;
            Instance.distanceCache[(bot2Name, bot1Name)] = distance;
            return distance;
        }
    }

    /// <summary>
    /// Get available player targets fot the given bot
    /// </summary>
    /// <param name="bot"></param>
    /// <returns></returns>
    public void GetTargetsFor(bl_AIShooter bot, ref List<bl_AITarget> list)
    {
        list.Clear();

        bl_PlayerReferencesCommon t;
        for (int i = 0; i < targetsLists.Count; i++)
        {
            t = targetsLists[i];
            if (t == null || t.name == bot.AIName) continue;

            if (isOneTeamMode)
            {
                list.Add(t.BotAimTarget);
            }
            else
            {
                if (t.PlayerTeam != Team.None && t.PlayerTeam == bot.AITeam) continue;

                list.Add(t.BotAimTarget);
            }
        }

        var nonPlayers = bl_GameManager.GetNonPlayerTargets();
        if (nonPlayers != null)
        {
            foreach (var item in nonPlayers)
            {
                if (!item.IsAlive()) continue;

                if (item.Team == bot.AITeam && !isOneTeamMode) continue;
                list.Add(item.Target);
            }
        }
    }

    /// <summary>
    /// Mark a player as "Taken" so other bots won't target him
    /// </summary>
    /// <param name="aimTarget"></param>
    public static void ClaimTarget(bl_AITarget aimTarget)
    {
        if (Instance == null) return;

        for (int i = Instance.nonTargetedBots.Count - 1; i >= 0; i--)
        {
            if (Instance.nonTargetedBots[i] == null)
            {
                Instance.nonTargetedBots.RemoveAt(i);
            }
            else if (Instance.nonTargetedBots[i] == aimTarget)
            {
                Instance.nonTargetedBots.RemoveAt(i);
            }
        }
    }

    /// <summary>
    /// Check if the given bot transform has not been marked as "Taken"
    /// </summary>
    /// <param name="target"></param>
    /// <returns></returns>
    public static bool IsTargetAvailable(bl_AITarget target)
    {
        return Instance != null && Instance.nonTargetedBots.Contains(target);
    }

    /// <summary>
    /// Remove the death bot from the available bots and put it on the wait list to respawn
    /// </summary>
    public void OnBotDeath(bl_AIShooter agent, bl_AIShooter killer)
    {
        if (!bl_PhotonNetwork.IsMasterClient)
            return;

        AllBots.Remove(agent);
        for (int i = 0; i < AllBots.Count; i++)
        {
            AllBots[i].CheckTargets();
        }

        AddBotToRespawn(agent);

        UpdateTargetList();
    }

    /// <summary>
    /// 
    /// </summary>
    public void RespawnAllBots(bool forcedRespawn = false)
    {
        if (!bl_PhotonNetwork.IsMasterClient) return;

        for (int i = AllBots.Count - 1; i >= 0; i--)
        {
            if (forcedRespawn)
            {
                // if the bot is still alive
                if (AllBots[i].gameObject != null)
                {
                    AllBots[i].Respawn();
                }
            }

            // if the bot is death
            if (AllBots[i].gameObject == null)
            {
                SpawnBot(AllBots[i]);
                if (AllBots[i] != null)
                {
                    AllBots[i].References.shooterHealth.DestroyEntity();
                }
            }
        }

        for (int i = SpawningBots.Count - 1; i >= 0; i--)
        {
            if (SpawningBots[i] == null || SpawningBots[i].gameObject == null) continue;

            SpawnBot(SpawningBots[i]);
            SpawningBots[i].References.shooterHealth.DestroyEntity();
            SpawningBots.RemoveAt(i);
        }
    }

    /// <summary>
    /// Put a bot to the pending list to respawn after the min respawn time.
    /// </summary>
    public void AddBotToRespawn(bl_AIShooter bot)
    {
        SpawningBots.Add(bot);
        //automatically spawn the bot after the re-spawn time
        if (GetGameMode.GetGameModeInfo().onPlayerDie == GameModeSettings.OnPlayerDie.SpawnAfterDelay)
        {
            Invoke(nameof(SpawnPendingBot), bl_GameData.CoreSettings.PlayerRespawnTime);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private void SpawnPendingBot()
    {
        if (SpawningBots == null || SpawningBots.Count <= 0) return;
        if (SpawningBots[0] != null)
        {
            SpawnBot(SpawningBots[0]);
            //This fix the issue with the duplicate pv id when a master client re-enter in a room.
            bl_PhotonNetwork.Destroy(SpawningBots[0].gameObject);
            SpawningBots.RemoveAt(0);
        }
    }

    /// <summary>
    /// Update the killer bot kills count and sync with everyone
    /// </summary>
    public static void SetBotKill(string botName)
    {
        if (Instance == null) return;

        var stats = Instance.GetBotStatistics(botName);
        if (stats == null) return;

        Instance.SyncBotStat(stats.Name, 0, 0);

        bl_MFPS.RoomGameMode.SetPointToGameMode(1, GameMode.FFA, Team.All);
        bl_MFPS.RoomGameMode.SetPointToGameMode(1, GameMode.TDM, stats.Team);
    }

    /// <summary>
    /// 
    /// </summary>
    public static void SetBotScore(string botName, int score)
    {
        if (Instance == null) return;

        var stat = Instance.GetBotStatistics(botName);
        if (stat == null) return;

        stat.Score += score;
    }

    /// <summary>
    /// Called in all clients when a bot die
    /// Update the killed bot death count and sync with everyone.
    /// </summary>
    /// <param name="botName">bot that die</param>
    public static void SetBotDeath(string botName)
    {
        if (Instance == null) return;

        bl_EventHandler.Bots.onBotDeath?.Invoke(botName);
        //if this bots was already replaced by a real player
        if (Instance.lastLifeBots.Contains(botName))
        {
            //this is his last life, so since he die, remove his data
            //last life due he got replace by a player so this bot wont respawn again.
            int bi = bl_GameManager.Instance.OthersActorsInScene.FindIndex(x => x.Name == botName);
            if (bi != -1)
            {
                bl_EventHandler.Bots.onBotDeath?.Invoke(botName);
                Instance.RemoveBotInfo(botName, true);
                // make sure all the player have the same data
                Instance.SyncBotsDataToAllOthers();
                // don't need to continue since update the stats is not necessary
                return;
            }
        }
        var stat = Instance.GetBotStatistics(botName);
        if (stat == null) return;

        if (bl_PhotonNetwork.IsMasterClient) Instance.SyncBotStat(stat.Name, 0, 1);
    }

    /// <summary>
    /// Update the bot assist count and sync with everyone
    /// </summary>
    /// <param name="botName"></param>
    public static void SetBotAssist(string botName)
    {
        if (Instance == null) return;

        var stat = Instance.GetBotStatistics(botName);
        if (stat == null) return;

        Instance.SyncBotStat(stat.Name, 1, 6);
    }

    /// <summary>
    /// 
    /// </summary>
    public static void UpdateBotView(bl_AIShooter bot, int viewID)
    {
        var stat = Instance.GetBotStatistics(bot.AIName);
        if (stat == null) return;

        stat.ViewID = viewID;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="bot"></param>
    /// <param name="gameState"></param>
    public static void SetBotGameState(bl_AIShooter bot, BotGameState gameState)
    {
        var stat = Instance.GetBotStatistics(bot.AIName);
        if (stat == null) return;

        stat.GameState = gameState;
    }

    #region Photon Events
    /// <summary>
    /// Event called when a new player enter in the match
    /// </summary>
    void OnPlayerEnter(Player player)
    {
        if (player.ActorNumber == bl_PhotonNetwork.LocalPlayer.ActorNumber) return;

        //cause bots statistics are not sync by Hashtables as player data do we need sync it by RPC
        //so for sync it just one time (after will be update by the local client) we send it when a new player enter (only to the new player)
        if (bl_PhotonNetwork.IsMasterClient)
        {
            //so first we recollect all the stats from the master client and join it in a string line
            string line = GetCompiledBotsData();
            //and send to the new player so him can have the data and update locally.
            photonView.RPC(nameof(SyncAllBotsStats), player, line, 0);

            //also send the slots data so all player have the same list in case the Master Client leave the game
            line = GetCompiledSlotsData();
            photonView.RPC(nameof(SyncAllBotsStats), player, line, 1);
        }
    }

    /// <summary>
    /// Event called when a player change some property
    /// This used to listen when a player change of Team
    /// </summary>
    public void OnPhotonPlayerPropertiesChanged(Player player, Hashtable changedProps)
    {
        if (!BotsActive)
            return;
        if (!changedProps.ContainsKey(PropertiesKeys.TeamKey)) return;

        Team team = (Team)changedProps[PropertiesKeys.TeamKey];

        if (team == Team.None) return;

        ReplaceBotWithPlayer(player, team);
    }

    /// <summary>
    /// Replace one of the bots with the given player in the given team
    /// The player will not be immediate replaced but after he dies and won't respawn again.
    /// </summary>
    /// <param name="newPlayer"></param>
    /// <param name="playerTeam"></param>
    public void ReplaceBotWithPlayer(Player newPlayer, Team playerTeam)
    {
        if (!BotsActive) return;

        string remplaceBot = string.Empty;
        var slotList = playerTeam == Team.Team2 ? Team2PlayersSlots : Team1PlayersSlots;

        //check if this player was already assigned (maybe just change of team)
        if (slotList.Exists(x => x.Player == newPlayer.NickName)) return;
        //find a empty slot in the team
        int index = slotList.FindIndex(x => x.Player == string.Empty);
        if (index != -1)
        {
            //replace the bot slot with the new player
            remplaceBot = slotList[index].Bot;
            DeleteBot(remplaceBot);
            slotList[index].Player = newPlayer.NickName;
            slotList[index].Bot = string.Empty;

            //sync the slot change with other players
            if (bl_PhotonNetwork.IsMasterClient)
            {
                int teamCmdID = playerTeam == Team.Team2 ? 2 : 1;
                photonView.RPC(nameof(SyncBotStat), RpcTarget.Others, $"{teamCmdID}|{newPlayer.NickName}", index, (byte)4, false);
            }
            Debug.Log($"<color=yellow><b>{remplaceBot}</b> was replaced by <b>{newPlayer.NickName}</b>, bot will be removed from the game after gets eliminiated.</color>");
        }

        //remove the bot that the master client replace
        if (newPlayer.IsMasterClient && bl_PhotonNetwork.IsMasterClient && !isMasterAlredyInTeam && !string.IsNullOrEmpty(remplaceBot))
        {
            bl_GameManager.Instance.UnregisterMFPSPlayer(remplaceBot);
            bl_AIShooter bot = AllBots.Find(x => x.AIName == remplaceBot);
            if (bot != null)
            {
                //Debug.Log($"<color=blue>Bot {bot.AIName} was replaced by master {player.NickName}</color>");
                PhotonView bv = bot.GetComponent<PhotonView>();
                bot.References.shooterHealth.DestroyEntity();//destroy on remote clients
                AllBots.Remove(bot);
                bl_PhotonNetwork.Destroy(bv.gameObject);
                RemoveBotInfo(remplaceBot, true);
            }
            isMasterAlredyInTeam = true;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="newMasterClient"></param>
    public void OnMasterClientSwitched(Player newMasterClient)
    {
        //if the new master client is the local client
        if (newMasterClient.ActorNumber == bl_PhotonNetwork.LocalPlayer.ActorNumber)
        {
            if (Team1PlayersSlots == null || Team1PlayersSlots.Count <= 0)
                SetUpSlots(false);

            //since bots where not collected on the new master client, lets take them manually
            bl_AIShooter[] allBots = FindObjectsByType<bl_AIShooter>(FindObjectsSortMode.None);
            foreach (var bot in allBots)
            {
                if (bot.IsDeath)//if the bot was death when master client leave the game
                {
                    AddBotToRespawn(bot);
                    continue;
                }
                AllBots.Add(bot);
                bot.Init();
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void OnPlayerLeft(Player player)
    {
        if (!BotsActive || bl_GameManager.Instance.GameFinish) return;

        // do not replace bots if the game mode is Elimination.
        if (bl_MFPS.RoomGameMode.CurrentGameModeID == GameMode.ELIM) return;

        //Check if the player was occupying a slot
        Team team = player.GetPlayerTeam();
        var slotList = team == Team.Team2 ? Team2PlayersSlots : Team1PlayersSlots;
        int index = slotList.FindIndex(x => x.Player == player.NickName);
        //empty the occupied slot
        if (index != -1) { slotList[index].Player = ""; }

        //make the master client instance a new bot to replace the player that just left
        if (bl_PhotonNetwork.IsMasterClient)
        {
            //try to instance the new bot
            var newAgent = SpawnBot(null, team);
            if (newAgent == null) return;

            //find the slot id where the bot was assigned
            int botIndex = slotList.FindIndex(x => x.Bot == newAgent.AIName);
            //sync the new slot with all other players
            photonView.RPC(nameof(SyncBotStat), RpcTarget.Others, $"{(int)team}|{newAgent.AIName}|{newAgent.photonView.ViewID}", botIndex, (byte)5, false);
            string joinMessage = $"{newAgent.AIName} {bl_GameTexts.JoinIn} {team.GetTeamName()}";
            if (isOneTeamMode)
            {
                joinMessage = $"{newAgent.AIName} {bl_GameTexts.JoinedInMatch}";
            }
            //show a notification in all players with the new bot name
            bl_KillFeedBase.Instance.SendMessageEvent(joinMessage);
            Debug.Log($"<color=blue>Bot {newAgent.AIName} has replace the player {player.NickName}.</color>");
        }
    }
    #endregion

    /// <summary>
    /// 
    /// </summary>
    /// <param name="data"></param>
    /// <param name="value"></param>
    /// <param name="cmd"></param>
    [PunRPC]
    public void SyncBotStat(string data, int value, byte cmd, bool calledFromLocal = true)
    {
        if (calledFromLocal)
        {
            photonView.RPC(nameof(SyncBotStat), RpcTarget.All, data, value, cmd, false);
            return;
        }

        MFPSBotProperties bs = BotsStatistics.Find(x => x.Name == data);
        // if the bot stats doesn't exists and the command is not to add it
        if (bs == null && cmd != 5) return;

        if (cmd == 0)//add kill
        {
            bs.Kills++;
            bs.Score += bl_GameData.ScoreSettings.ScorePerKill;
        }
        else if (cmd == 1)//death
        {
            bs.Deaths++;
            bs.GameState = BotGameState.Death;
        }
        else if (cmd == 2)//remove bot
        {
            // mark the bot as replaced but do not remove it from the list yet.
            RemoveBotInfo(data, false);
        }
        else if (cmd == 3)//update view id
        {
            bs.ViewID = value;
            OnBotStatUpdate?.Invoke(bs);
        }
        else if (cmd == 4)//replace bot slot with a player
        {
            string[] dataSplit = data.Split('|');
            var list = int.Parse(dataSplit[0]) == 1 ? Team1PlayersSlots : Team2PlayersSlots;
            if (list.Count <= 0) { Debug.LogWarning("Team slots has not been setup yet."); return; }
            // bl_GameManager.Instance.UnregisterMFPSPlayer(list[value].Bot);
            list[value].Player = dataSplit[1];
            list[value].Bot = "";
        }
        else if (cmd == 5)//add single new bot
        {
            string[] dataSplit = data.Split('|');
            Team team = (Team)int.Parse(dataSplit[0]);
            var list = team == Team.Team2 ? Team2PlayersSlots : Team1PlayersSlots;
            if (list.Count <= 0) { Debug.LogWarning("Team slots has not been setup yet."); return; }
            list[value].Player = "";
            list[value].Bot = dataSplit[1];

            //add the bot statistic
            bs = new MFPSBotProperties
            {
                Name = dataSplit[1],
                Team = team,
                ViewID = int.Parse(dataSplit[2])
            };
            BotsStatistics.Add(bs);
        }
        else if (cmd == 6) // add assist
        {
            bs.Assists++;
            bs.Score += bl_GameData.ScoreSettings.ScorePerKillAssist;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="data"></param>
    /// <param name="cmd"></param>
    [PunRPC]
    void SyncAllBotsStats(string data, int cmd)
    {
        if (cmd == 0)//bots statistics
        {
            BotsStatistics.Clear();
            string[] split = data.Split("|"[0]);
            for (int i = 0; i < split.Length; i++)
            {
                if (string.IsNullOrEmpty(split[i])) continue;
                string[] info = split[i].Split(","[0]);
                MFPSBotProperties bs = new()
                {
                    Name = info[0],
                    Kills = int.Parse(info[1]),
                    Deaths = int.Parse(info[2]),
                    Assists = int.Parse(info[3]),
                    Score = int.Parse(info[4]),
                    Team = (Team)int.Parse(info[5]),
                    ViewID = int.Parse(info[6])
                };
                BotsStatistics.Add(bs);

                if (!bl_GameManager.Instance.OthersActorsInScene.Exists(x => x.Name == bs.Name))
                {
                    bl_GameManager.Instance.OthersActorsInScene.Add(new MFPSPlayer()
                    {
                        Name = bs.Name,
                        isRealPlayer = false,
                        Team = bs.Team,
                    });
                }
            }
            OnMaterStatsReceived?.Invoke(BotsStatistics);
            HasMasterInfo = true;
        }
        else if (cmd == 1)//team slots info
        {
            SetUpSlots(false);
            string[] teams = data.Split('&');
            string[] teamInfo = teams[0].Split('|');//get the first team slots
            for (int i = 0; i < teamInfo.Length; i++)
            {
                if (string.IsNullOrEmpty(teamInfo[i])) continue;

                string[] slot = teamInfo[i].Split(',');
                Team1PlayersSlots[i].Player = slot[0];
                Team1PlayersSlots[i].Bot = slot[1];
            }
            if (!isOneTeamMode)
            {
                teamInfo = teams[1].Split('|');//get the second team slots
                for (int i = 0; i < teamInfo.Length; i++)
                {
                    if (string.IsNullOrEmpty(teamInfo[i])) continue;

                    string[] slot = teamInfo[i].Split(',');
                    Team2PlayersSlots[i].Player = slot[0];
                    Team2PlayersSlots[i].Bot = slot[1];
                }
            }
            bl_EventHandler.Bots.onBotsInitializated?.Invoke();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void RemoveBotInfo(string botName, bool deleteData)
    {
        if (deleteData)
        {
            // remove the bot from the list
            int bi = BotsStatistics.FindIndex(x => x.Name == botName);
            if (bi != -1) BotsStatistics.RemoveAt(bi);

            bi = bl_GameManager.Instance.OthersActorsInScene.FindIndex(x => x.Name == botName);
            if (bi != -1)
            {
                bl_GameManager.Instance.OthersActorsInScene.RemoveAt(bi);
            }
            lastLifeBots.Remove(botName);
        }
        else
        {
            // just mark the bot as replaced so the data is delete when the bot die
            lastLifeBots.Add(botName);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="Name"></param>
    void DeleteBot(string Name)
    {
        if (BotsStatistics.Exists(x => x.Name == Name))
        {
            SyncBotStat(Name, 0, 2);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="modelName"></param>
    /// <param name="modelInstance"></param>
    /// <returns></returns>
    public static bool TryGetModel(string modelName, out GameObject modelInstance)
    {
        if (Instance == null)
        {
            modelInstance = null;
            return false;
        }

        if (Instance.modelCache == null || !Instance.modelCache.ContainsKey(modelName))
        {
            modelInstance = null;
            return false;
        }

        modelInstance = Instance.modelCache[modelName];
        return true;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="modelName"></param>
    /// <param name="Model"></param>
    public static void CacheModel(string modelName, GameObject Model, bool asChild = true)
    {
        if (Instance == null)
        {
            return;
        }

        if (Instance.modelCache == null)
        {
            Instance.modelCache = new Dictionary<string, GameObject>();
        }

        if (Instance.modelCache.ContainsKey(modelName))
        {
            Instance.modelCache[modelName] = Model;
        }
        else
        {
            Instance.modelCache.Add(modelName, Model);
            if (asChild) Model.transform.SetParent(Instance.transform);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private void OnRemotePlayerChange(bl_EventHandler.PlayerChangeData changeData)
    {
        UpdateTargetList();
    }

    /// <summary>
    /// 
    /// </summary>
    private void OnLocalDeath()
    {
        UpdateTargetList();
    }

    /// <summary>
    /// 
    /// </summary>
    private void OnLocalPlayerSpawn()
    {
        UpdateTargetList();
        nonTargetedBots.Add(bl_MFPS.LocalPlayerReferences.BotAimTarget);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="team"></param>
    /// <returns></returns>
    public List<MFPSPlayer> GetAllBotsInTeam(Team team)
    {
        List<MFPSPlayer> list = new();
        for (int i = 0; i < bl_GameManager.Instance.OthersActorsInScene.Count; i++)
        {
            if (bl_GameManager.Instance.OthersActorsInScene[i].isRealPlayer) continue;

            if (bl_GameManager.Instance.OthersActorsInScene[i].Team == team)
            {
                list.Add(bl_GameManager.Instance.OthersActorsInScene[i]);
            }
        }
        return list;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="botName"></param>
    /// <returns></returns>
    public MFPSBotProperties GetBotStatistics(string botName)
    {
        return BotsStatistics.FirstOrDefault(x => x.Name == botName);
    }

    /// <summary>
    /// Count the empty slots (not occupied by a real player)
    /// </summary>
    /// <returns></returns>
    public int EmptySlotsCount(Team team)
    {
        int count = 0;
        var list = team == Team.Team2 ? Team2PlayersSlots : Team1PlayersSlots;
        for (int i = 0; i < list.Count; i++)
        {
            if (string.IsNullOrEmpty(list[i].Player)) count++;
        }
        return count;
    }

    /// <summary>
    /// 
    /// </summary>
    private bool HasSpaceInTeam(Team team)
    {
        return team == Team.Team2
            ? Team2PlayersSlots.Exists(x => x.Player == string.Empty)
            : Team1PlayersSlots.Exists(x => x.Player == string.Empty);
    }

    /// <summary>
    /// 
    /// </summary>
    private bool HasSpaceInTeamForBot(Team team)
    {
        return team == Team.Team2
            ? Team2PlayersSlots.Exists(x => x.Player == string.Empty && x.Bot == string.Empty)
            : Team1PlayersSlots.Exists(x => x.Player == string.Empty && x.Bot == string.Empty);
    }

    /// <summary>
    /// 
    /// </summary>
    public static bl_AIShooter GetBot(int viewID)
    {
        foreach (var agent in Instance.AllBots)
        {
            if (agent != null && agent.photonView.ViewID == viewID)
            {
                return agent;
            }
        }
        return null;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public static List<bl_AIShooter> GetAllBotsInstanced()
    {
        if (Instance == null) return new List<bl_AIShooter>();
        return Instance.AllBots;
    }

    #region SubClasses
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public MFPSBotProperties GetBotWithMoreKills()
    {
        if (BotsStatistics == null || BotsStatistics.Count <= 0)
        {
            MFPSBotProperties bs = new()
            {
                Name = "None",
                Kills = 0,
                Team = Team.None,
                Score = 0,
            };
            return bs;
        }

        return BotsStatistics.Aggregate((i1, i2) => i1.Kills > i2.Kills ? i1 : i2);
    }

    [System.Serializable]
    public class PlayersSlots
    {
        public string Player;
        public string Bot;
    }
    #endregion

    private static bl_AIMananger _instance;
    public static bl_AIMananger Instance
    {
        get
        {
            if (_instance == null) { _instance = FindAnyObjectByType<bl_AIMananger>(); }
            return _instance;
        }
    }
}