using System;
using UnityEngine;

[Serializable]
public class GameModeSettings
{
    [Serializable]
    public enum OnRoundStartedSpawn
    {
        SpawnAfterSelectTeam,
        WaitUntilRoundFinish,
    }

    public enum OnPlayerDie
    {
        SpawnAfterDelay,
        SpawnAfterRoundFinish,
    }

    public enum RoundModeAllowedOptions
    {
        AllowBoth,
        RoundModeOnly,
        SingleRoundOnly,
        OneRoundOfMultipleRoundsOnly,
    }

    public enum AfterFinishMatchLogic
    {
        LeaveRoom,
        ReturnToWaitingRoom,
    }

    public enum PlayerAutoTeamSelection
    {
        SplitInTeams,
        AllInTeam1,
        AllInTeam2,
        /// <summary>
        /// Defined the logic in a custom script
        /// </summary>
        Custom,
    }

    /// <summary>
    /// Game mode name
    /// This is the name that will show to the player
    /// </summary>
    public string ModeName;

    /// <summary>
    /// Game Mode internal identifier
    /// The unique enum identifier for this mode
    /// </summary>
    public GameMode gameMode;

    /// <summary>
    /// Is this mode enable in your game?
    /// </summary>
    [LovattoToogle] public bool isEnabled = true;

    [Tooltip("Is this game mode a team based mode or solo/free for all mode?")]
    [LovattoToogle] public bool isTeamMode = true;

    /// <summary>
    /// Allow play with bots in this mode?
    /// </summary>
    [Header("Settings")]
    [LovattoToogle] public bool supportBots = false;

    /// <summary>
    /// Force automatically assign the teams for this mode
    /// </summary>
    [LovattoToogle] public bool AutoTeamSelection = false;

    /// <summary>
    /// The minimum number of players that have to be joined in the match
    /// before the game can start
    /// </summary>
    [Tooltip("The minimum number of players that have to be joined in the match before the game can start")]
    [Range(1, 32)] public int RequiredPlayersToStart = 1;

    /// <summary>
    /// What happen when a player joins in the game when the match has started
    /// </summary>
    [Tooltip("What happen when a player joins in the game when the match has started")]
    public OnRoundStartedSpawn onRoundStartedSpawn = OnRoundStartedSpawn.SpawnAfterSelectTeam;

    /// <summary>
    /// Override the spawn point selection mode for this game mode
    /// If set to None, the spawn point selection mode will be the default one of the map scene.
    /// </summary>
    [Tooltip("Override the spawn point selection mode for this game mode. If set to None, the spawn point selection mode will be the default one of the map scene.")]
    public SpawnPointSelectionMode overrideSpawnSelectionMode = SpawnPointSelectionMode.None;

    /// <summary>
    /// What happen when a player die
    /// </summary>
    [Tooltip("What happen when a player die")]
    public OnPlayerDie onPlayerDie = OnPlayerDie.SpawnAfterDelay;

    /// <summary>
    /// Allow players to select the game mode round mode
    /// or force a round mode
    /// </summary>
    [Tooltip("Allow players to select the game mode round mode or force a round mode")]
    public RoundModeAllowedOptions RoundModeAllowed = RoundModeAllowedOptions.AllowBoth;

    /// <summary>
    /// When the match finish in this mode, the player should leave the room and back to the lobby
    /// or return to the waiting room with the same team and players?
    /// </summary>
    [Tooltip("When the match finish in this mode, the player should leave the room and back to the lobby or return to the waiting room with the same team and players?")]
    public AfterFinishMatchLogic AfterFinishMatch = AfterFinishMatchLogic.LeaveRoom;

    /// <summary>
    /// Override the global kill cam mode (defined in GameData) for this game mode
    /// </summary>
    public KillCameraType OverrideKillCamMode = KillCameraType.None;

    [Tooltip("Define how the real players will be placed in the teams when AutoTeam selection is on.\n\nSplit In Teams = Half of the players in each team.\nAll In One Team = All the players in an specific team.")]
    public PlayerAutoTeamSelection playersTeamSelection = PlayerAutoTeamSelection.SplitInTeams;

    [Tooltip("Define how the bots will be placed in the teams when AutoTeam selection is on.\n\nSplit In Teams = Half of the bots in each team.\nAll In One Team = All the bots in an specific team.")]
    public PlayerAutoTeamSelection botsTeamSelection = PlayerAutoTeamSelection.SplitInTeams;

    /// <summary>
    /// The score/goal name of this game mode
    /// </summary>
    public string GoalName = "Kills";

    /// <summary>
    /// Does this game mode has a time limit?
    /// If it doesn't have a time limit, you have to set the logic in your game mode script to finish the game.
    /// </summary>
    [LovattoToogle] public bool UnlimitedTime = false;

    /// <summary>
    /// Can players pick up weapons in this game mode?
    /// </summary>
    [LovattoToogle] public bool allowedPickupWeapons = true;

    /// <summary>
    /// Count kill assistances in this game mode?
    /// </summary>
    [LovattoToogle] public bool AllowKillAssist = true;

    /// <summary>
    /// Allow players to spectate others (other than the killer) in this game mode?
    /// </summary>
    [LovattoToogle] public bool AllowSpectateEnemies = true;

    /// <summary>
    /// Allow create matches with a single weapon type?
    /// e.g: Only sniper, only shotgun, etc...
    /// </summary>
    [LovattoToogle] public bool AllowSingleWeaponTypeOnly = true;

    /// <summary>
    /// Allow players to switch teams mid game in this game mode?
    /// (If the option is enabled in GameData)
    /// </summary>
    [LovattoToogle] public bool AllowSwitchTeam = true;

    /// <summary>
    /// Show the global kill feed in this game mode?
    /// </summary>
    [LovattoToogle] public bool ShowKillFeed = true;

    /// <summary>
    /// Unlist the room from the server list after the match has started?
    /// So new players can't join in the middle of the match
    /// </summary>
    [LovattoToogle] public bool UnlistGameAfterStarted = false;

    [Tooltip("Show the player scoreboard in this game mode?")]
    [LovattoToogle] public bool ShowPlayerScoreboard = true;

    [Tooltip("If true, an icon will appear on the position where teammates gets eliminated.")]
    [LovattoToogle] public bool showDeathIconsOnTeammates = true;

    /// <summary>
    /// Max players options allowed for this game mode
    /// These are the options that appear in the room creator window
    /// Max players are the total player allowed, in two team game modes the max players
    /// per team is equal to 1/2 of the max player
    /// </summary>
    [Header("Options")]
    public int[] maxPlayers = new int[] { 6, 2, 4, 8 };

    /// <summary>
    /// The max score, kills or wherever the goal is in this mode options
    /// These are the options that appear in the room creator window
    /// </summary>
    public int[] GameGoalsOptions = new int[] { 50, 100, 150, 200 };

    /// <summary>
    /// The round time limits options for this game mode
    /// These are the options that appear in the room creator window
    /// The time is in seconds 60 = 1 minute
    /// </summary>
    public int[] timeLimits = new int[] { 900, 600, 1200, 300 };

    public string GetGoalFullName(int goalID) { return string.Format("{0} {1}", GameGoalsOptions[goalID], GoalName); }

    /// <summary>
    /// Get the game mode goal name
    /// e.g: kills, captures, score, points, etc...
    /// </summary>
    public string GetGoalName(int goalID)
    {
        if (GameGoalsOptions.Length <= 0) return GoalName;
        return $"{GameGoalsOptions[goalID]} {GoalName}";
    }

    /// <summary>
    /// Get the goal value by the index in the GameGoalsOptions list
    /// </summary>
    /// <returns></returns>
    public int GetGoalValue(int goalID)
    {
        if (GameGoalsOptions.Length <= 0) return 0;
        if (goalID >= GameGoalsOptions.Length) return GameGoalsOptions[GameGoalsOptions.Length - 1];

        return GameGoalsOptions[goalID];
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public bool HasForcedRoundMode() => RoundModeAllowed != RoundModeAllowedOptions.AllowBoth;

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public RoundStyle GetAllowedRoundMode()
    {
        if (RoundModeAllowed == RoundModeAllowedOptions.RoundModeOnly) return RoundStyle.Rounds;
        return RoundStyle.OneMatch;
    }
}