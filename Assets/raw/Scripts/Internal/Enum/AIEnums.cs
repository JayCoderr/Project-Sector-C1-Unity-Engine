namespace MFPS.Runtime.AI
{
    /// <summary>
    /// The default bots states
    /// </summary>
    public enum AIAgentState
    {
        Idle = 0,
        Patroling = 1,
        Following = 2,
        Covering = 3,
        Looking = 4,
        Searching = 5,
        HoldingPosition = 6,
        HoldingState = 7
    }

    /// <summary>
    /// Bots behave mode
    /// </summary>
    public enum AIAgentBehave
    {
        Aggressive = 0,
        Pasive = 1,
        Protective = 2,
    }

    /// <summary>
    /// Internal use only
    /// </summary>
    public enum AIAgentCoverArea
    {
        ToPoint = 0,
        ToTarget = 1,
        ToRandomPoint = 2,
        AwayFromTarget = 3,
    }

    /// <summary>
    /// Internal use only
    /// </summary>
    public enum AITargetOutRangeBehave
    {
        SearchNewNearestTarget,
        KeepFollowingBasedOnState,
    }

    /// <summary>
    /// Options for when the bot target is out of the navmesh
    /// </summary>
    public enum AITargetOutNavmeshBehave
    {
        SearchNewNearestTarget,
        RandomPatrol
    }

    /// <summary>
    /// 
    /// </summary>
    public enum AIWeaponAccuracy
    {
        Pro,
        Casual,
        Novice,
    }

    /// <summary>
    /// 
    /// </summary>
    public enum AILookAt
    {
        Target,
        Path,
        HitDirection,
        PathToTarget,
        LastMoveInverseDirection,
    }

    /// <summary>
    /// 
    /// </summary>
    public enum AIRemoteCallType : byte
    {
        DestroyBot = 0,
        SyncTarget = 1,
        CrouchState = 2,
        Respawn = 3,
    }

    /// <summary>
    /// 
    /// </summary>
    public enum BotKillConsideration
    {
        /// <summary>
        /// Bot kills will count in the match scoreboard and kill/score will be granted to the killer.
        /// </summary>
        SameAsRealPlayers,

        /// <summary>
        /// The bot kill will count for the player in the match scoreboard but won't increase the player account stats (score and kills count)
        /// </summary>
        OnlyAffectMatchScoreboard,

        /// <summary>
        /// Bot kills doesn't count even for the match scoreboard
        /// </summary>
        DoNotCountAtAll,
    }
}