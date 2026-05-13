using Photon.Realtime;
using System;

public abstract class bl_GameModeBase : bl_PhotonHelper
{
    #region Structures
    /// <summary>
    /// The information collected from the game mode logic when a match ends
    /// </summary>
    public struct MatchOverInformation
    {
        /// <summary>
        /// Did the player win or is part of the winner team?
        /// </summary>
        public bool IsLocalWinner;
        /// <summary>
        /// Is the match completely over or is just a round over?
        /// </summary>
        public bool IsCompletelyOver;
        /// <summary>
        /// In case you want to show a custom result message when the match is over instead of the default
        /// DEFEAT or VICTORY message.
        /// </summary>
        public string LocalResultTitle;
        /// <summary>
        /// The reason of the match ends, e.g: Time Limit Reached, Score Limit Reach, No Enough player, etc...
        /// </summary>
        public string FinishReason;
        /// <summary>
        /// (Optional) Show the scores in the Match Over UI?
        /// </summary>
        public bool DisplayScores;
        /// <summary>
        /// (Optional) For SOLO game modes where at the end the round the local player score should be displayed.
        /// </summary>
        public int LocalPlayerScore;
        /// <summary>
        /// (Optional) In case you want to show the winner name in the match over UI.
        /// </summary>
        public string WinnerName;
    }

    /// <summary>
    /// 
    /// </summary>
    public enum MatchResult
    {
        Draw,
        Victory,
        Defeat
    }

    /// <summary>
    /// 
    /// </summary>
    public enum FinishRoundCause
    {
        TimeReached,
        ScoreReached,
        ForceFinish,
        NoEnoughPlayers,
        Other,
    }

    /// <summary>
    /// 
    /// </summary>
    public enum FinishMatchMode
    {
        LeaveMap,
        StartNewRound,
    }
    #endregion

    #region Parameters
    public Action<FinishRoundCause> finishRoundCallback = null;

    private string m_finishRoundReason = "";
    private bool isSubscribeToEvents = false;
    #endregion

    #region Internal
    /// <summary>
    /// 
    /// </summary>
    public virtual void Awake()
    {
        if (!bl_PhotonNetwork.IsConnected)
            return;

        Initialize();
    }

    /// <summary>
    /// This called by Game Manager when this mode is the current room mode
    /// Do not call this manually
    /// </summary>
    public void ActiveThisMode()
    {
        isSubscribeToEvents = true;
        bl_PhotonCallbacks.PlayerPropertiesUpdate += OnPlayerPropertiesUpdate;
        bl_PhotonCallbacks.PlayerEnteredRoom += OnOtherPlayerEnter;
        bl_PhotonCallbacks.RoomPropertiesUpdate += OnRoomPropertiesUpdate;
        bl_PhotonCallbacks.PlayerLeftRoom += OnOtherPlayerLeave;
        bl_EventHandler.onLocalPlayerDeath += OnLocalPlayerDeath;
        bl_EventHandler.onLocalKill += OnLocalPlayerKill;
        bl_EventHandler.onLocalPlayerSpawn += OnLocalPlayerSpawn;
    }

    /// <summary>
    /// 
    /// </summary>
    public virtual void OnDisable()
    {
        if (isSubscribeToEvents)
        {
            bl_PhotonCallbacks.PlayerEnteredRoom -= OnOtherPlayerEnter;
            bl_PhotonCallbacks.RoomPropertiesUpdate -= OnRoomPropertiesUpdate;
            bl_PhotonCallbacks.PlayerLeftRoom -= OnOtherPlayerLeave;
            bl_EventHandler.onLocalPlayerDeath -= OnLocalPlayerDeath;
            bl_PhotonCallbacks.PlayerPropertiesUpdate -= OnPlayerPropertiesUpdate;
            bl_EventHandler.onLocalKill -= OnLocalPlayerKill;
            bl_EventHandler.onLocalPlayerSpawn -= OnLocalPlayerSpawn;

            isSubscribeToEvents = false;
        }
    }
    #endregion

    /// <summary>
    /// initialize the game mode
    /// </summary>
    public abstract void Initialize();

    /// <summary>
    /// use to determinate if the local player win in this game mode
    /// </summary>
    public abstract bool IsLocalPlayerWinner();

    /// <summary>
    /// This SHOULD be called from the inherited script when the match is over
    /// when is over? that depends of the game mode logic or when the time runs out.
    /// </summary>
    public virtual void FinishMatch(FinishMatchMode matchMode = FinishMatchMode.StartNewRound)
    {
        MatchOverInformation info = GetMatchOverInformation();
        bl_RoundFinishScreenBase.Instance.Show(info);
    }

    /// <summary>
    /// This SHOULD be called from the inherited script when a round is over
    /// when is over? that depends of the game mode logic or when the time runs out.
    /// </summary>
    public virtual void FinishRound(FinishRoundCause finishCause)
    {
        // if the room time if finished but the countdown is still running, do not finish the match.
        if (bl_MatchTimeManagerBase.Instance.TimeState == RoomTimeState.Countdown) return;

        if (finishRoundCallback != null && finishCause != FinishRoundCause.ForceFinish)
        {
            // handle the finish round logic in the custom callback
            finishRoundCallback?.Invoke(finishCause);
            return;
        }

        bl_EventHandler.DispatchRoundEndEvent();
        bool isGameOver = bl_RoomSettings.Instance.CurrentRoomInfo.roundStyle == RoundStyle.OneMatch || finishCause == FinishRoundCause.ForceFinish;
        bl_GameManager.Instance.OnGameTimeFinish(isGameOver);

        bl_MatchFinishResumeBase.Instance.CollectMatchData();
        bl_CrosshairBase.Instance.Show(false);
        bl_MatchTimeManagerBase.Instance.StartNewRoundCountdown();

        if (finishCause == FinishRoundCause.TimeReached)
        {
            m_finishRoundReason = bl_GameTexts.TimeLimitReached;
        }
        else if (finishCause == FinishRoundCause.ScoreReached)
        {
            m_finishRoundReason = bl_GameTexts.ScoreLimitReached;
        }
        else if (finishCause == FinishRoundCause.NoEnoughPlayers)
        {
            m_finishRoundReason = bl_GameTexts.NoEnoughPlayers;
        }
        else
        {
            // You should set the reason from the inherited script
        }

        FinishMatch();
    }

    /// <summary>
    /// Return the current match result for the local player
    /// This should return the current state, doesn't matter if the game is still going on or if is over.
    /// </summary>
    /// <returns></returns>
    public virtual MatchResult GetMatchResult()
    {
        var winningTeam = GetWinningTeam();
        if (winningTeam == Team.None) return MatchResult.Draw;

        return winningTeam == PlayerTeam ? MatchResult.Victory : MatchResult.Defeat;
    }

    /// <summary>
    /// Build and return the the information about the match when this is over
    /// This information is going to be used to display it to the player in the match over UI.
    /// </summary>
    /// <returns></returns>
    public virtual MatchOverInformation GetMatchOverInformation()
    {
        Team winningTeam = GetWinningTeam();
        string winner = winningTeam.GetTeamName();
        if (winningTeam == Team.None || winningTeam == Team.All)
        {
            winner = string.Empty;
        }

        var moi = new MatchOverInformation()
        {
            IsLocalWinner = IsLocalPlayerWinner(),
            DisplayScores = true,
            LocalResultTitle = GetMatchTextResult(),
            FinishReason = m_finishRoundReason,
            WinnerName = winner
        };
        return moi;
    }

    #region Local Player Events
    /// <summary>
    /// Called when the local player spawn in the map. Only called when this mode is active.
    /// </summary>
    public virtual void OnLocalPlayerSpawn() { }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="info"></param>
    public virtual void OnLocalPlayerKill(KillInfo info)
    {
        OnLocalPlayerKill();
    }

    /// <summary>
    /// Local player kill someone (only local receive it)
    /// </summary>
    public virtual void OnLocalPlayerKill() { }

    /// <summary>
    /// Local player death (only local receive it)
    /// </summary>
    public virtual void OnLocalPlayerDeath() { }

    /// <summary>
    /// Call when LOCAL player score a point in this game mode
    /// </summary>
    /// <param name="points"></param>
    public virtual void OnLocalPoint(int points, Team teamToAddPoint) { }
    #endregion

    #region Room Events
    /// <summary>
    /// when a new player enter in room
    /// </summary>
    public virtual void OnOtherPlayerEnter(Player newPlayer) { }

    /// <summary>
    /// when a player leave the room
    /// </summary>
    public virtual void OnOtherPlayerLeave(Player otherPlayer) { }

    /// <summary>
    /// Room Properties Update
    /// </summary>
    public virtual void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged) { }

    /// <summary>
    /// Players property update
    /// </summary>
    /// <param name="target"></param>
    /// <param name="changedProps"></param>
    public virtual void OnPlayerPropertiesUpdate(Player target, ExitGames.Client.Photon.Hashtable changedProps) { }
    #endregion

    #region Utils

    /// <summary>
    /// Get the local player team
    /// </summary>
    public Team PlayerTeam => bl_MFPS.LocalPlayer.Team;

    /// <summary>
    /// Get the local player enemy team
    /// </summary>
    public Team EnemyTeam => bl_MFPS.LocalPlayer.Team.OppsositeTeam();

    /// <summary>
    /// Is the given game mode the room active game mode?
    /// </summary>
    /// <param name="mode"></param>
    /// <returns></returns>
    public bool IsThisModeActive(GameMode mode)
    {
        return bl_GameManager.Instance.IsGameMode(mode, this);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public string GetMatchTextResult()
    {
        MatchResult state = GetMatchResult();
        if (state == MatchResult.Victory) return bl_GameTexts.Victory;
        else return state == MatchResult.Defeat ? bl_GameTexts.Defeat : bl_GameTexts.Draw;
    }

    /// <summary>
    /// Get the current winning team based on the team score values.
    /// </summary>
    /// <returns></returns>
    public Team GetWinningTeam()
    {
        int team1 = bl_PhotonNetwork.CurrentRoom.GetRoomScore(Team.Team1);
        int team2 = bl_PhotonNetwork.CurrentRoom.GetRoomScore(Team.Team2);

        Team winner = team1 > team2 ? Team.Team1 : team1 < team2 ? Team.Team2 : Team.None;
        return winner;
    }

    /// <summary>
    /// Check if any of the team has reached the match goal based on the team scores
    /// </summary>
    /// <returns>True if any of the team has reached the goal, use <paramref name="winnerTeam"/>to determine which team reached the score.</para></returns>
    public bool CheckIfTeamReachedGoal(out Team winnerTeam)
    {
        if (!bl_RoomSettings.Instance.RoomInfoFetched)
        {
            winnerTeam = Team.None;
            return false;
        }

        int team1 = bl_PhotonNetwork.CurrentRoom.GetRoomScore(Team.Team1);
        int team2 = bl_PhotonNetwork.CurrentRoom.GetRoomScore(Team.Team2);
        int goal = bl_RoomSettings.Instance.GameGoal;

        if (team1 >= goal)
        {
            winnerTeam = Team.Team1;
            return true;
        }

        if (team2 >= goal)
        {
            winnerTeam = Team.Team2;
            return true;
        }

        winnerTeam = Team.None;
        return false;
    }
    #endregion

}