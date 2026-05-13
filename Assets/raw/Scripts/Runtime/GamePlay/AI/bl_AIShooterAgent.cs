using MFPS.Runtime.AI;
using MFPSEditor;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.AI;
using static bl_AIShooterAttackBase;
using NetHashTable = ExitGames.Client.Photon.Hashtable;

#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(NavMeshAgent))]
public class bl_AIShooterAgent : bl_AIShooter
{
    #region Public Members
    [Space(5)]
    [ScriptableDrawer] public bl_AIBehaviorSettings behaviorSettings;
    [ScriptableDrawer] public bl_AISoldierSettings soldierSettings;

    [Header("Others")]
    public LayerMask ObstaclesLayer;
    public bool DebugStates = false;
    public bool showGizmos = false;

    [Header("References")]
    public bl_Footstep footstep;

    public List<NetworkMessagesCount> networkMessagesCounts = new();
    #endregion

    #region Public Properties
    public override bl_AITarget AimTarget
    {
        get => References.BotAimTarget;
    }

    /// <summary>
    /// Is the target in the bot vision range (field of view and distance)
    /// </summary>
    public bool IsTargetInSight { get; set; }

    /// <summary>
    /// Is the target hampered by something from the bot position to the target position.
    /// </summary>
    public bool IsTargetHampered { get; set; }

    /// <summary>
    /// Is the target in the bot vision range (field of view only)
    /// This can be used to check if the bot is currently looking at the target.
    /// </summary>
    public bool IsTargetInFieldOfView { get; set; }

    public bl_AIShooterAttackBase AIWeapon { get; set; }
    public bl_PlayerHealthManagerBase AIHealth { get; set; }
    public override float CachedTargetDistance { get; set; } = 0;
    public StringBuilder DebugLineBuilder { get; set; }
    = new StringBuilder(64); //last ID 45
    public bool IsCrouch { get; set; } = false;

    #endregion

    #region Private members
    public const string RPC_NAME = "RPCShooterBot";
    private Animator Anim;
    private Vector3 finalPosition;
    private float lastPathTime = 0;
    private bl_AICoverPoint CoverPoint = null;
    private bool ForceCoverFire = false;
    private float CoverTime = 0;
    private Vector3 LastHitDirection;
    private int SwitchCoverTimes = 0;
    private float lookTime;
    private float time, delta = 0;
    private float nextEnemysCheck = 0;
    private float lastDestinationTime;
    private float destinationLockTime;
    private float holdStateTimer;
    private float lastRandomPointT;
    private float patrolLockTime;
    private readonly float lastHoldPositionTime;
    private float lastHotAreaDecitionT = 0;
    private float velocityMagnitud = 0;
    private bool forceUpdateRotation;
    private Vector3 localRotation = Vector3.zero;
    private readonly int[] animationHash = new int[] { 0 };
    private bool wasTargetHampered, isHoldingState;
    private int precisionRate = 8;
    private List<bl_AITarget> availableTargets = new();
    private Transform headBoneReference;
    private bool isMaster, isGameStarted, randomOnStartTake;
    private float lastCrouchTime, lastSpeedChange, timeSinceHampered, timeSinceVisual = 0;
    private float[] randomValues;
    private Vector3 thisFrameLocalPosition, lastFrameLocalPosition, lastMoveDir;
    private float instanceTime = 0;
    private float rotationVelocity;
    private int invalidDestinationCount = 0;
    private int currentFrame = 0;
    private AIStateExecutionRecord executionRecord;
    private readonly Vector3[] corners = new Vector3[2];

    // Turn in place settings
    private const float TURN_IN_PLACE_THRESHOLD = 45f; // Angle threshold in degrees
    private const float MOVEMENT_THRESHOLD = 0.1f; // Minimum velocity to consider the bot as moving

    // AI Improvement Variables
    private Vector3 lastKnownTargetPosition;
    private Vector3 targetVelocity;
    private Vector3 predictedTargetPosition;
    private float lastTargetPositionUpdateTime;
    private float strafeTimer;
    private int strafeDirection = 1;
    private float lastStrafeChangeTime;
    private bool shouldFlank;
    private float lastFlankDecisionTime;
    private float lastAmmoCheckTime;
    private int consecutiveHits;
    private int consecutiveMisses;
    private float adaptiveDifficultyModifier = 1f;

    // Performance optimization cache
    private float cachedDistanceSqr;
    private float lastDistanceUpdateTime;
    private const float DISTANCE_UPDATE_INTERVAL = 0.2f;

    // Tactical decision making
    private readonly float lastTacticalDecisionTime;
    private Vector3 lastSeenTargetPosition;
    private readonly float targetLostTime;
    private bool isInAdvantageousPosition;
    private int shotsFiredSinceLastHit;
    private float lastSuccessfulHitTime;

    // Human-like behavior
    private float reactionDelay;
    private float aimDriftTimer;
    private Vector3 aimDriftOffset;
    private readonly bool isAimingDownSight;
    private float concentrationLevel = 1f;
    #endregion

    #region Sub Classes
    [System.Serializable]
    public class NetworkMessagesCount
    {
        public AIRemoteCallType CallType;
        public int Count;
    }
    #endregion

    #region Unity Methods
    /// <summary>
    /// 
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        bl_PhotonCallbacks.PlayerEnteredRoom += OnPhotonPlayerConnected;
        Agent = References.Agent;
        AIHealth = References.shooterHealth;
        AIWeapon = References.shooterWeapon;
        IsTargetHampered = false;
        Agent.updateRotation = false;
        thisFrameLocalPosition = CachedTransform.localPosition;
        lastFrameLocalPosition = thisFrameLocalPosition;
        randomValues = new float[]
        {
            Random.Range(4,8), Random.Range(1,5)
        };
        animationHash[0] = Animator.StringToHash("Crouch");
        bl_AIMananger.SetBotGameState(this, BotGameState.Playing);
        instanceTime = Time.time;
        executionRecord = new AIStateExecutionRecord();
        if (footstep != null) footstep.SetTimeOffset();
    }

    /// <summary>
    /// 
    /// </summary>
    protected override void OnEnable()
    {
        base.OnEnable();
        bl_EventHandler.onLocalPlayerSpawn += OnLocalSpawn;
        bl_EventHandler.onMatchStart += OnMatchStart;
        bl_EventHandler.onMatchStateChanged += OnMatchStateChange;
    }

    /// <summary>
    /// 
    /// </summary>
    protected override void OnDisable()
    {
        base.OnDisable();
        bl_EventHandler.onLocalPlayerSpawn -= OnLocalSpawn;
        bl_PhotonCallbacks.PlayerEnteredRoom -= OnPhotonPlayerConnected;
        bl_EventHandler.onMatchStart -= OnMatchStart;
        bl_EventHandler.onMatchStateChanged -= OnMatchStateChange;
    }
    #endregion

    /// <summary>
    /// By default, this is invoked when the bot prefab model has been initialized.
    /// </summary>
    public void OnModelInit()
    {
        Anim = References.PlayerAnimator;
        headBoneReference = References.aiAnimation.GetHumanBone(HumanBodyBones.Head);
    }

    /// <summary>
    /// 
    /// </summary>
    public override void Init()
    {
        isMaster = bl_PhotonNetwork.IsMasterClient;
        isGameStarted = bl_MatchTimeManagerBase.Instance.TimeState == RoomTimeState.Started;
        References.shooterNetwork.CheckNamePlate();
        References.Agent.enabled = isMaster;
    }

    /// <summary>
    /// 
    /// </summary>
    public override void OnUpdate()
    {
        time = Time.time;
        delta = Time.deltaTime;
        currentFrame = Time.frameCount;
        thisFrameLocalPosition = CachedTransform.localPosition;

        if (IsDeath) return;

        isNewDebug = true;

        if (!isGameStarted) return;

        if (Target != null && isMaster)
        {
            // Update predictive targeting (optimized to run less frequently)
            if (currentFrame % 3 == 0)
            {
                UpdateTargetPrediction();
            }

            if (AgentState != AIAgentState.Covering)
            {
                if (AgentState != AIAgentState.HoldingPosition)
                {
                    TargetControl();
                }
                else
                {
                    OnHoldingPosition();
                }
            }
            else
            {
                OnCovering();
            }

            // Update human-like aim drift
            UpdateAimDrift();
        }

        CheckEnemiesOnSight();
        CheckEnemyHotAreas();
        LookAtControl();
        HoldingStateTimeControl();

        // Smart strafing during combat (optimized execution rate)
        if (isMaster && HasATarget && !IsTargetHampered && CachedTargetDistance < soldierSettings.mediumRange)
        {
            if (currentFrame % 10 == 0)
            {
                UpdateCombatStrafing();
            }
        }
    }

    /// <summary>
    /// this is called each second instead of each frame
    /// </summary>
    public override void OnSlowUpdate()
    {
        if (IsDeath) return;
        if (bl_MatchTimeManagerBase.Instance.IsTimeUp() || !isGameStarted)
        {
            return;
        }

        velocityMagnitud = Agent.velocity.magnitude;
        if (velocityMagnitud > 0.1f)
        {
            Vector3 movement = thisFrameLocalPosition - lastFrameLocalPosition;
            if (movement.sqrMagnitude > 0.0001f) // Avoid normalizing near-zero vectors
            {
                lastMoveDir = movement.normalized;
            }
        }

        if (isMaster)
        {
            if (!HasATarget)
            {
                SetDebugState(-1, true);
                //Get the player nearest player
                SearchPlayers();
                //if target null yet, then patrol         
                RandomPatrol(!isOneTeamMode);
            }
            else
            {
                CheckEnemiesDistances();
                CheckVision();
            }
        }

        FootStep();
        lastFrameLocalPosition = thisFrameLocalPosition;
    }

    /// <summary>
    /// Called every frame when the <see cref="AIAgentState"/> is NOT <see cref="AIAgentState.Covering"/>"/> and is not holding position.
    /// </summary>
    private void TargetControl()
    {
        if (isHoldingState) return;

        // limit the number of times the code below is executed as we don't need to call it every frame
        if ((currentFrame + 1) % precisionRate != 0) return;

        CachedTargetDistance = bl_AIMananger.GetCachedDistance(Target, AimTarget);
        if (CachedTargetDistance >= soldierSettings.limitRange)
        {
            WhenTargetOutOfRange();
        }
        else if (CachedTargetDistance > soldierSettings.closeRange && CachedTargetDistance < soldierSettings.mediumRange)
        {
            WhenTargetOnMediumRange();
        }
        else if (CachedTargetDistance <= soldierSettings.closeRange)
        {
            WhenTargetOnCloseRange();
        }
        else if (CachedTargetDistance < soldierSettings.limitRange)
        {
            WhenTargetOnLimitRange();
        }
        else
        {
            Debug.Log("Unknown state: " + CachedTargetDistance);
            SetDebugState(101, true);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void WhenTargetOutOfRange()
    {
        if (behaviorSettings.targetOutRangeBehave == AITargetOutRangeBehave.SearchNewNearestTarget && time - instanceTime > 30)
        {
            var newTarget = GetNearestPlayer(true);
            if (newTarget != Target)
            {
                SetTarget(newTarget);
                return;
            }
        }

        precisionRate = 8;
        if (AgentState == AIAgentState.Following || FocusOnSingleTarget)
        {
            if (!isOneTeamMode)
            {
                //update the target position each 300 frames
                if (!Agent.hasPath || (currentFrame % 300) == 0)
                {
                    if (bl_UtilityHelper.SquaredDistance(TargetPosition, Agent.destination) > 1)//update the path only if the target has moved substantially
                    {
                        SetDestination(TargetPosition, 3);
                    }
                }

                if (time - lastDestinationTime > 8)
                {
                    if (HasATarget) SetDestination(TargetPosition, 3);
                    else RandomPatrol(false);
                }
            }
            else
            {
                //in one team mode, when the target is in the limit range
                //the bot will start to random patrol instead of following the player.
                RandomPatrol(true);
            }
            CheckLongDistanceShot();
            SetDebugState(0, true);
        }
        else if (AgentState == AIAgentState.Searching)
        {
            SetDebugState(1, true);
            RandomPatrol(true);
        }
        else
        {
            SetDebugState(2, true);
            SetTarget(null);
            RandomPatrol(false);
            SetState(AIAgentState.Patroling);
        }
        SetSpeed(GetBehave() == AIAgentBehave.Aggressive ? soldierSettings.runSpeed : soldierSettings.walkSpeed);
    }

    /// <summary>
    /// 
    /// </summary>
    void WhenTargetOnMediumRange()
    {
        SetDebugState(3, true);
        precisionRate = 4;
        OnTargeContest(false);
    }

    /// <summary>
    /// 
    /// </summary>
    void WhenTargetOnCloseRange()
    {
        SetDebugState(4, true);
        precisionRate = 2;
        Follow();
    }

    /// <summary>
    /// 
    /// </summary>
    void WhenTargetOnLimitRange()
    {
        SetDebugState(5, true);
        OnTargeContest(true);
        CheckLongDistanceShot();
        precisionRate = 1;
    }

    /// <summary>
    /// Called every frame when the bot <see cref="AIAgentState"/> is <see cref="AIAgentState.Covering"/>
    /// </summary>
    void OnCovering()
    {
        // limit the number of times this method is called as we don't need to call it every frame
        if (currentFrame % precisionRate != 0 || isHoldingState) return;

        if (Target != null)
        {
            CachedTargetDistance = TargetDistance;
            if (CachedTargetDistance <= soldierSettings.mediumRange)//if in look range and in front, start follow him and shot
            {
                if (GetBehave() == AIAgentBehave.Aggressive)
                {
                    if (!Agent.hasPath)
                    {
                        // if the bot was covering but the target is near and in sight, then try to following them
                        SetDebugState(38, true);
                        SetState(AIAgentState.Following);
                        SetDestination(TargetPosition, 3);
                    }
                    else
                    {
                        SetDebugState(6, true);

                        if (IsTargetInSight)
                        {
                            TriggerFire(IsTargetHampered ? FireReason.OnMove : FireReason.Forced);
                        }

                        if (CachedTargetDistance < soldierSettings.mediumRange)
                        {
                            SetCrouch(false, 2);
                        }

                        // if the bot health is low and the target is too close
                        if (AIHealth.GetHealth() <= soldierSettings.criticalHealth && CachedTargetDistance < soldierSettings.closeRange)
                        {
                            SetState(AIAgentState.Following);
                            SetDestination(GetPositionAround(TargetPosition, soldierSettings.mediumRange), 5, true, 4); // (4 is arbitrary)
                        }

                        // if this part of the code have been executed consecutively for 20 frames, then the bot is stuck in a loop
                        if (executionRecord.AddCall("6", currentFrame, precisionRate, 300) == 2)
                        {
                            if (Probability(0.5f)) { RandomPatrol(false, 4); } else { SwitchCover(); }
                        }
                    }
                }
                else // if the bot beave is not aggressive, then will keep covering
                {
                    if (GetBehave() == AIAgentBehave.Protective && CachedTargetDistance < soldierSettings.closeRange)
                    {
                        SetDebugState(47, true);
                        Cover(true, AIAgentCoverArea.AwayFromTarget, true);
                        SetHoldState(5); // hold the state for 5 seconds (5 is arbitrary)
                    }
                    else
                    {
                        SetDebugState(7, true);
                        SetState(AIAgentState.Covering); // why is this needed?
                        if (!HasAPath)
                        {
                            // try to find a cover point
                            Cover(false);
                        }
                        else
                        {
                            // if the bot is already covering (moving to a cover point)                        
                        }
                    }
                }

                DetermineLookAtBasedOnTarget();
                TriggerFire(FireReason.OnMove);
            }
            else if (CachedTargetDistance > soldierSettings.limitRange && CanCover(7))// if out of line of sight, start searching him
            {
                SetDebugState(8, true);
                SetState(AIAgentState.Searching);
                SetCrouch(false);
                TriggerFire(FireReason.OnMove);
            }
            else if (ForceCoverFire && !IsTargetHampered)//if bot is cover and still get damage, start shoot at the target (panic)
            {
                SetDebugState(9, true);
                SetLookAtState(AILookAt.Target);
                TriggerFire(FireReason.Forced);

                if (GetBehave() == AIAgentBehave.Protective || CachedTargetDistance > soldierSettings.mediumRange)
                {
                    if (CanCover(behaviorSettings.maxCoverTime)) { SwitchCover(); }
                }
            }
            else if (CanCover(behaviorSettings.maxCoverTime) && CachedTargetDistance >= soldierSettings.closeRange)//if has been a time since cover and nothing happen, try a new spot.
            {
                SetDebugState(10, true);
                SwitchCover();
                TriggerFire(FireReason.OnMove);
            }
            else
            {
                // if the bot is covering

                if (IsTargetInSight)
                {
                    // if the target is in sight while covering, force fire at them
                    Speed = soldierSettings.walkSpeed;
                    TriggerFire(FireReason.Forced);
                    SetDebugState(11, true);
                }
                else
                {
                    if (HasAPath)
                    {
                        // if the bot is moving to a cover point
                        SetDebugState(12, true);
                        Speed = soldierSettings.runSpeed;
                        TriggerFire(FireReason.OnMove);
                        SetCrouch(false);
                    }
                    else
                    {
                        // if the bot has reached the cover point and is not in sight of the target
                        // at this point the bot is standing still in the cover point
                        SetDebugState(45, true);
                        TriggerFire(CachedTargetDistance <= soldierSettings.closeRange ? FireReason.Forced : FireReason.LongRange);

                        // if the target is too close, then follow them
                        if (CachedTargetDistance <= soldierSettings.closeRange)
                        {
                            SetState(AIAgentState.Following);
                        }
                        else if (CachedTargetDistance > soldierSettings.mediumRange)
                        {
                            // if the target is too far, then search for them
                            // SetState(AIAgentState.Searching);
                            SetCrouch(Probability(0.5f), 3);
                        }
                        else // if the target is in medium range
                        {

                        }
                    }

                    SetLookAtState(AILookAt.Target);
                    CheckConfrontation();
                }
            }
        }

        if (Agent.pathStatus == NavMeshPathStatus.PathComplete)//once the bot reach the target cover point
        {
            if (CoverPoint != null && CoverPoint.Crouch) { SetCrouch(true); }//and the point required crouch -> do crouch
        }
    }

    /// <summary>
    /// Try to find a cover point near the bot to take cover.
    /// </summary>
    /// <param name="overridePoint">Whether to override the current cover point.</param>
    /// <param name="coverArea">The area to find a cover point near.</param>
    /// <param name="force">Whether to force the cover action.</param>
    /// <returns>True if the AI agent successfully takes cover, false otherwise.</returns>
    private bool Cover(bool overridePoint, AIAgentCoverArea coverArea = AIAgentCoverArea.ToPoint, bool force = false)
    {
        // if the target if too far, there's not point in covering right now (if the bot is aggressive)
        if (GetBehave() == AIAgentBehave.Aggressive && CachedTargetDistance > 20 && !force)
        {
            SetState(AIAgentState.Following);
            return false;
        }

        Transform nearPointReference = transform; // hold a reference transform in the map to find a cover point near it
        switch (coverArea)
        {
            case AIAgentCoverArea.ToTarget:
                nearPointReference = Target.Transform;//find a point near the target
                break;
        }

        if (overridePoint)//override the current cover point (if have any)
        {
            //if the agent has complete his current destination
            if (Agent.pathStatus == NavMeshPathStatus.PathComplete)
            {
                //get a random point in 30 metters (30 is arbitrary)
                if (coverArea == AIAgentCoverArea.ToRandomPoint)
                {
                    // Try tactical cover first
                    CoverPoint = GetTacticalCoverPoint(thisFrameLocalPosition, soldierSettings.mediumRange);
                    if (CoverPoint == null)
                    {
                        //look for another random cover point 
                        CoverPoint = bl_AICovertPointManager.Instance.GetCoverOnRadius(nearPointReference, soldierSettings.mediumRange);
                    }
                }
                else if (coverArea == AIAgentCoverArea.AwayFromTarget)
                {
                    CoverPoint = bl_AICovertPointManager.Instance.GetCoverOutsideOfRadius(TargetPosition, soldierSettings.mediumRange);
                }
                else
                {
                    // Try tactical cover selection first
                    CoverPoint = GetTacticalCoverPoint(nearPointReference.position, soldierSettings.mediumRange);
                    if (CoverPoint == null)
                    {
                        //Get the nearest cover point
                        CoverPoint = bl_AICovertPointManager.Instance.GetCloseCover(nearPointReference, CoverPoint);
                    }
                }
            }
            SetDebugState(13);
        }
        else
        {
            SetDebugState(14);
            // Try tactical cover first
            CoverPoint = GetTacticalCoverPoint(nearPointReference.position, soldierSettings.mediumRange);
            if (CoverPoint == null)
            {
                //look for a near cover point
                CoverPoint = bl_AICovertPointManager.Instance.GetCloseCover(nearPointReference);
            }
        }

        if ((time - CoverTime) < behaviorSettings.coverColdDown && !force)
        {
            SetDebugState(40);
            // if the bot cooldown time for cover has not passed
            DetermineLookAtBasedOnTarget();
            return true;
        }

        CoverTime = time; // block future cover points search request for a time

        if (CoverPoint != null)//if a point was found
        {
            SetDebugState(15);
            // if the cover point is relatively far, run to it, otherwise walk
            Speed = CoverPoint.DistanceTo(CachedTransform) < soldierSettings.closeRange ? soldierSettings.walkSpeed : soldierSettings.runSpeed;
            SetDestination(CoverPoint.Position, 0.1f, false, RandomRange(3, 5)); // hold the found cover point for x seconds
            SetState(AIAgentState.Covering);
            TriggerFire(GetBehave() == AIAgentBehave.Aggressive ? FireReason.Normal : FireReason.OnMove);
            DetermineLookAtBasedOnTarget();
            return true;
        }
        else
        {
            // if there are not nears cover points
            if (HasATarget)
            {
                SetDebugState(16);
                //follow the target and hold the state for x seconds
                SetDestination(TargetPosition, 3, false, RandomRange(3, 5));
                DetermineSpeedBaseOnRange();
                if (GetBehave() == AIAgentBehave.Aggressive) FocusOnSingleTarget = true; //and follow not matter the distance
                SetState(AIAgentState.Searching);
            }
            else
            {
                SetDebugState(17);
                // Try to get the nearest cover point not matter the distance
                CoverPoint = bl_AICovertPointManager.Instance.GetNearestCoverForced(CachedTransform);
                if (CoverPoint != null)
                {
                    SetDestination(CoverPoint.Position, 0.1f);
                    Speed = Probability(0.5f) ? soldierSettings.walkSpeed : soldierSettings.runSpeed;
                }
                else
                {
                    // if there are no cover points, then patrol
                    RandomPatrol(false, RandomRange(3, 6)); // hold the patrol for x seconds
                }
            }
            return false;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void OnHoldingPosition()
    {
        //if (Time.frameCount % 10 != 0) return;

        SetDebugState(37, true);
        if (time > lastHoldPositionTime)
        {
            SetState(Target != null ? AIAgentState.Following : AIAgentState.Searching);
            return;
        }
    }

    /// <summary>
    /// When the target is at look range
    /// This is only called when there is a target
    /// </summary>
    void OnTargeContest(bool overrideCover)
    {
        if (AgentState == AIAgentState.Following || ForceFollowAtHalfHealth)
        {
            if (!HasAPath)
            {
                SetDebugState(35);
                SetDestination(TargetPosition, 2, true, Random.value * 3);
                SetCrouch(false);
            }
            else
            {
                SetDebugState(18);
                DetermineSpeedBaseOnRange();

                // if the bot is too close to the target, then get a random point around to avoid walking over the target
                if (CachedTargetDistance < soldierSettings.closeRange && !IsTargetHampered && time - lastRandomPointT > 3)
                {
                    Vector3 randomDes = GetRandomPointAtDirection(3);
                    SetDestination(randomDes, 0, false, 3);
                }

                if (executionRecord.AddCall("18", currentFrame, precisionRate, 200) == 2)
                {
                    if (!Cover(overrideCover, AIAgentCoverArea.ToPoint, true))
                    {
                        // if there are no cover points, then get a random point around the map
                        SetRandomPointInMapAsDestination(50, 5, true); // 50 and 5 are arbitrary
                    }
                }
            }

            // check if the bot should fire to the target
            float minDis = GetBehave() == AIAgentBehave.Aggressive ? soldierSettings.farRange : soldierSettings.mediumRange;
            if (CachedTargetDistance < minDis)
            {
                var fr = CachedTargetDistance < soldierSettings.closeRange ? FireReason.Forced : FireReason.OnMove;
                // we just trigger the fire here, further checks like if the target is in sight are done in the TriggerFire method
                TriggerFire(fr);
                SetDebugState(46);
            }
        }
        else if (AgentState == AIAgentState.Covering)
        {
            if (CanCover(5) && CachedTargetDistance >= soldierSettings.closeRange)
            {
                SetDebugState(21);
                Cover(true);
            }
            else
            {
                SetDebugState(22);
            }
        }
        else
        {
            OnDirectConfrontation();
        }

        DetermineLookAtBasedOnTarget();
    }

    /// <summary>
    // When the target is near
    // Determine if can attack him
    /// </summary>
    void OnDirectConfrontation()
    {
        precisionRate = 1;
        // if the target is near
        if (CachedTargetDistance <= soldierSettings.closeRange)
        {
            SetCrouch(false);
            // if the bot is low on health, will try to get a cover point instead of direct attack
            if (AIHealth.GetHealth() <= soldierSettings.criticalHealth)
            {
                SetDebugState(36);
                SetState(AIAgentState.Covering);
                Cover(false, AIAgentCoverArea.ToRandomPoint);
            }
            else
            {
                SetDebugState(23);
                if (time > lastHoldPositionTime)
                {
                    HoldPosition();
                }
                DetermineSpeedBaseOnRange();
            }
        }
        else
        {
            // if the target is not that close
            SetDebugState(44);
            DetermineSpeedBaseOnRange();

            if (velocityMagnitud < 1)
            {
                if (executionRecord.AddCall("44", currentFrame, 1, 200) == 2)
                {
                    Follow();
                }
            }
        }

        CheckConfrontation();
    }

    /// <summary>
    /// Search for a target to follow
    /// Called every second when the bot has no target
    /// </summary>
    private void SearchPlayers()
    {
        if (isHoldingState) return;
        if (availableTargets.Count <= 1)
        {
            UpdateTargetList();
        }

        SetDebugState(-2);
        CachedTargetDistance = float.MaxValue;
        // Check if there is a target within the bot range.
        // if the bot is in aggresive mode, will search for the nearest enemy not matter the distance.
        bl_AITarget target = GetBehave() == AIAgentBehave.Aggressive ? GetNearestPlayer(true) : GetTargetOnRange(GetBehave() == AIAgentBehave.Protective ? soldierSettings.farRange : soldierSettings.mediumRange, true);
        if (target != null)
        {
            SetTarget(target);
        }

        // Take a random target at the beginning
        if (bl_PhotonNetwork.IsMasterClient && !randomOnStartTake && availableTargets.Count > 0)
        {
            if (behaviorSettings.GetRandomTargetOnStart)
            {
                bl_AITarget randomTarget = null;
                // first, try to get a target that is not claimed by other bot
                foreach (var item in availableTargets)
                {
                    if (bl_AIMananger.IsTargetAvailable(item))
                    {
                        randomTarget = item;
                        bl_AIMananger.ClaimTarget(item);
                        break;
                    }
                }
                // if there is no target free available, take a random one
                if (randomTarget == null) { randomTarget = availableTargets[Random.Range(0, availableTargets.Count)]; }

                SetTarget(randomTarget);
                randomOnStartTake = true;
            }
        }

        if (!HasATarget)
        {
            if (AgentState == AIAgentState.Following || AgentState == AIAgentState.Looking) { SetState(AIAgentState.Searching); }
        }
    }

    /// <summary>
    /// If a enemy is not in range, then make the AI randomly patrol in the map
    /// </summary>
    /// <param name="precision">Patrol closed to the enemy's area</param>
    void RandomPatrol(bool precision, float lockTime = -1)
    {
        if (IsDeath || !bl_PhotonNetwork.IsMasterClient || isHoldingState)
            return;
        if (patrolLockTime - time > 0) return;

        // if the call require locke the patrol result for a time
        // this basically means that the bot will not be able to request another patrol until the lock time is over
        if (lockTime > 0) { patrolLockTime = time + lockTime; }

        float precisionArea = soldierSettings.farRange;
        if (precision)
        {
            if (TargetDistance < soldierSettings.mediumRange)
            {
                SetDebugState(24);
                if (!HasATarget)
                {
                    SetTarget(GetNearestPlayer(false));
                }
                SetState(GetBehave() == AIAgentBehave.Protective ? AIAgentState.Covering : AIAgentState.Looking);
                precisionArea = 5;
            }
            else
            {
                SetDebugState(25);
                SetState(GetBehave() == AIAgentBehave.Aggressive ? AIAgentState.Following : AIAgentState.Searching);
                precisionArea = 8;
            }
        }
        else
        {
            SetDebugState(26);
            SetState(AIAgentState.Patroling);
            if (GetBehave() == AIAgentBehave.Aggressive && !HasATarget)
            {
                SetTarget(GetNearestPlayer(false));
                SetCrouch(false);
            }
            ForceCoverFire = false;
        }

        SetLookAtState(AILookAt.Path);
        AIWeapon.IsFiring = false;

        if (!Agent.hasPath || (time - lastPathTime) > randomValues[0])
        {
            SetDebugState(27);
            bool toAnCover = (Random.value <= behaviorSettings.randomCoverProbability);//probability of get a cover point as random destination
            Vector3 randomDirection = TargetPosition + (Random.insideUnitSphere * precisionArea);
            if (toAnCover)
            {
                bl_AICoverPoint coverPoint = bl_AICovertPointManager.Instance.GetCoverOnRadius(CachedTransform, 20);
                if (coverPoint != null) randomDirection = coverPoint.Position;
            }
            if (!HasATarget && isOneTeamMode)
            {
                randomDirection += thisFrameLocalPosition;
            }
            if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, precisionArea, 1))
            {
                finalPosition = hit.position;
            }
            lastPathTime = time + Random.Range(1, randomValues[0]);
            DetermineSpeedBaseOnRange();
            SetCrouch(false);
        }
        else
        {
            if (Agent.hasPath)
            {
                SetDebugState(28);
            }
            else
            {
                SetDebugState(32);
            }
        }
        SetDestination(finalPosition, 1, true);
    }

    /// <summary>
    /// Control the direction of the bot's body rotation
    /// </summary>
    private void LookAtControl()
    {
        if (!isMaster) return;

        AILookAt cla = LookingAt;
        bool force = false;

        if (cla == AILookAt.Target)
        {
            // if the bot is supposed to look at the target but he don't have one
            if (!HasATarget)
            {
                LookingAt = HasAPath ? AILookAt.Path : AILookAt.LastMoveInverseDirection;

                force = true;
            }
            else
            {
                // if has a target but is hampered
                // avoid look at the target if it's hampered, allow 3 seconds delay to look to the path in case just get hampered
                if (IsTargetHampered && timeSinceHampered != -1 && (time - timeSinceHampered) > soldierSettings.targetLostFocusTime)
                {
                    if (HasAPath)
                    {
                        LookingAt = AILookAt.PathToTarget;
                        force = true;
                    }
                    else
                    {
                        // at this point it means the bot is looking at the target but the vision it's hampered
                        // so we need to change the look target if the target distance is not close, because if it's close, the bot can look
                        // at the target even if it is hampered as we can argue that he can hear the target, also, if in any moment the target gets in sight it will be attacked.
                        if (CachedTargetDistance > soldierSettings.closeRange)
                        {
                            LookingAt = AILookAt.LastMoveInverseDirection;
                            force = true;
                        }
                    }
                }
            }
        }
        else if (cla == AILookAt.Path || cla == AILookAt.PathToTarget)
        {
            // if the bot is supposed to look at the path but he don't have one
            if (!HasAPath)
            {
                // if the bot has a target and the target is in sight, then look at the target
                if (HasATarget && IsTargetInSight && !IsTargetHampered)
                {
                    LookingAt = AILookAt.Target;
                }
                else
                {
                    // look behind
                    LookingAt = AILookAt.LastMoveInverseDirection;
                }

                force = true;
            }
        }
        else if (cla == AILookAt.LastMoveInverseDirection)
        {
            if (HasAPath)
            {
                LookingAt = AILookAt.Path;
                force = true;
            }
        }

        if ((currentFrame % bl_AIMananger.Instance.updateBotsLookAtEach) == 0 || forceUpdateRotation || force)
        {
            switch (LookingAt)
            {
                case AILookAt.Path:
                case AILookAt.PathToTarget:
                    int cID = Agent.path.GetCornersNonAlloc(corners) > 1 ? 1 : 0;
                    var v = corners[cID];
                    v.y = thisFrameLocalPosition.y + 0.2f;
                    LookAtPosition = v;

                    v = LookAtPosition - thisFrameLocalPosition;
                    LookAtDirection = v;
                    break;
                case AILookAt.Target:
                    v = TargetPosition;
                    v.y += 0.22f;
                    LookAtPosition = v;
                    LookAtDirection = LookAtPosition - thisFrameLocalPosition;
                    break;
                case AILookAt.HitDirection:
                    LookAtPosition = LastHitDirection;
                    LookAtDirection = LookAtPosition - thisFrameLocalPosition;
                    if (LookAtDirection == Vector3.zero) { LookAtDirection = thisFrameLocalPosition + (CachedTransform.forward * 10); }
                    break;
                case AILookAt.LastMoveInverseDirection:
                    LookAtPosition = thisFrameLocalPosition + (-lastMoveDir * 2);
                    LookAtDirection = -lastMoveDir;
                    break;
            }

            // if the target is too close (3 meters or less)
            if (bl_UtilityHelper.IsWithinDistance(thisFrameLocalPosition, LookAtPosition, 3))
            {
                // force to look at the the bot head position
                Vector3 lap = LookAtPosition;
                lap.y = headBoneReference.position.y;
                LookAtPosition = lap;
                LookAtDirection = LookAtPosition - thisFrameLocalPosition;
            }

            Vector3 headLookDirection = LookAtPosition - thisFrameLocalPosition;
            Quaternion headRotation = Quaternion.LookRotation(headLookDirection);
            Quaternion desiredBodyRotation = Quaternion.LookRotation(LookAtDirection);

            float angleDifference = Mathf.DeltaAngle(headRotation.eulerAngles.y, desiredBodyRotation.eulerAngles.y);

            float yAngle;
            if (Mathf.Abs(angleDifference) > soldierSettings.maxBodyHeadAngleDifference)
            {
                float limitedBodyYAngle = headRotation.eulerAngles.y +
                    (Mathf.Sign(angleDifference) * soldierSettings.maxBodyHeadAngleDifference);

                Quaternion limitedBodyRotation = Quaternion.Euler(
                    0f,
                    limitedBodyYAngle,
                    0f
                );

                // Apply the constrained body rotation
                yAngle = limitedBodyRotation.eulerAngles.y;
            }
            else
            {
                // Apply the desired body rotation
                yAngle = desiredBodyRotation.eulerAngles.y;
            }

            localRotation = CachedTransform.localEulerAngles;
            localRotation.y = yAngle;

            forceUpdateRotation = false;
        }

        // Turn in place logic: only rotate when moving or when angle difference exceeds threshold
        bool isMoving = velocityMagnitud > MOVEMENT_THRESHOLD;
        float currentAngleDifference = Mathf.DeltaAngle(CachedTransform.localEulerAngles.y, localRotation.y);

        // Rotate if the bot is moving OR the angle difference is significant
        if (isMoving || Mathf.Abs(currentAngleDifference) > TURN_IN_PLACE_THRESHOLD)
        {
            float smoothYAngle = Mathf.SmoothDampAngle(CachedTransform.localEulerAngles.y, localRotation.y, ref rotationVelocity, soldierSettings.rotationSmoothing, Mathf.Infinity, delta);
            CachedTransform.localRotation = Quaternion.Euler(0, smoothYAngle, 0);
        }
        // If not moving and angle is within threshold, keep current rotation (turn in place behavior)
    }

    /// <summary>
    /// Called every frame
    /// </summary>
    private void Follow()
    {
        if (AgentState == AIAgentState.Covering && Random.value > 0.5f) return;

        SetLookAtState(IsTargetHampered ? AILookAt.Path : AILookAt.Target);

        SetCrouch(false);

        // Check if bot should attempt flanking maneuver
        if (ShouldAttemptFlank() && time - lastFlankDecisionTime > 8f)
        {
            Vector3 flankPos = GetFlankingPosition();
            if (Vector3.Distance(flankPos, thisFrameLocalPosition) > 5f)
            {
                SetDestination(flankPos, 2f, true, 5f);
                SetDebugState(48);
                TriggerFire(FireReason.OnMove);
                return;
            }
        }

        SetDestination(TargetPosition, 3, true);

        // if the target is too close (3 meters or less)
        if (CachedTargetDistance <= 3)
        {
            CoverTime = 0;
            Speed = soldierSettings.walkSpeed;

            // first, try to get a tactical cover point near the target
            if (Probability(behaviorSettings.toSelectACoverNearTarget))
            {
                var tacticalCover = GetTacticalCoverPoint(TargetPosition, soldierSettings.mediumRange);
                if (tacticalCover != null)
                {
                    CoverPoint = tacticalCover;
                    SetDestination(CoverPoint.Position, 0.1f, false, RandomRange(3, 5));
                    SetState(AIAgentState.Covering);
                    SetDebugState(29);
                    CheckConfrontation();
                    SetCrouch(false);
                    TriggerFire(FireReason.Forced);
                    return;
                }
            }

            // Fallback to old behavior
            if (Cover(true, AIAgentCoverArea.ToTarget))
            {
                SetDebugState(29);
            }
            else if (Probability(0.85f) && Cover(true, AIAgentCoverArea.ToRandomPoint))
            {
                SetDebugState(30);
            }
            else
            {
                SetDebugState(34);
                // if there is no cover point, move back
                SetDestination(thisFrameLocalPosition - (CachedTransform.forward * 3), 0.1f, false, 2);
            }
            SetState(AIAgentState.Covering);
            CheckConfrontation();
            SetCrouch(false);
            TriggerFire(FireReason.Forced);
        }
        else
        {
            DetermineSpeedBaseOnRange();
            DetermineLookAtBasedOnTarget();
            SetDebugState(33);
            TriggerFire(CachedTargetDistance < soldierSettings.closeRange ? FireReason.Forced : FireReason.Normal);

            // if the bot is too close to the target and have visual, then get a random point around to avoid walking over the target
            if (CachedTargetDistance < soldierSettings.closeRange && !IsTargetHampered && time - lastRandomPointT > 5)
            {
                Vector3 randomDes = GetRandomPointAtDirection(RandomRange(2, 7));
                SetDestination(randomDes, 0, false, RandomRange(2, 5));
                lastRandomPointT = time;
            }
        }
    }

    /// <summary>
    /// Trigger fire with improved decision making
    /// </summary>
    public void TriggerFire(FireReason reason = FireReason.Normal)
    {
        if (time - instanceTime < 1) { return; }
        if (time - timeSinceVisual < soldierSettings.reactionTime) return;

        // Add human-like reaction delay
        if (reactionDelay > 0)
        {
            reactionDelay -= delta;
            return;
        }

        if (LookingAt == AILookAt.Path || LookingAt == AILookAt.LastMoveInverseDirection)
        {
            SetLookAtState(AILookAt.Target);
        }

        // Track shots fired for adaptive difficulty
        shotsFiredSinceLastHit++;

        // Update concentration based on combat situation
        if (reason == FireReason.Forced || IsTargetInSight)
        {
            concentrationLevel = Mathf.Min(1f, concentrationLevel + 0.1f);
        }

        AIWeapon.Fire(reason);
    }

    /// <summary>
    /// Determine the bot speed based on the target distance
    /// If the target is too far, the bot will run, if not, will walk
    /// </summary>
    private void DetermineSpeedBaseOnRange()
    {
        if (time - lastSpeedChange < randomValues[1])
        {
            return;
        }

        lastSpeedChange = time;
        Speed = (Target != null && CachedTargetDistance > soldierSettings.mediumRange) ? soldierSettings.runSpeed : soldierSettings.walkSpeed;
    }

    /// <summary>
    /// Determine the look at state based on the target
    /// If has a target and it is onsight, then look at the target, if not, look at the path
    /// </summary>
    private void DetermineLookAtBasedOnTarget()
    {
        if (HasATarget)
        {
            if (!IsTargetHampered)
            {
                if (time - timeSinceVisual > soldierSettings.reactionTime)
                {
                    SetLookAtState(AILookAt.Target);
                }
            }
            else
            {
                if (time - timeSinceHampered > soldierSettings.targetLostFocusTime)
                {
                    if (HasAPath)
                    {
                        SetLookAtState(AILookAt.PathToTarget);
                    }
                    else
                    {
                        SetLookAtState(AILookAt.LastMoveInverseDirection);
                    }
                }
            }
        }
        else
        {
            SetLookAtState(AILookAt.Path);
        }
    }

    /// <summary>
    /// Hold the current bot state for the given time
    /// The bot won't take any other action until the hold time is over
    /// </summary>
    /// <param name="holdTime"></param>
    public void SetHoldState(float holdTime)
    {
        holdStateTimer = time + holdTime;
        isHoldingState = true;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="newSpeed"></param>
    private void SetSpeed(float newSpeed)
    {
        if (newSpeed == Speed) return;

        if (time - lastSpeedChange < randomValues[1])
        {
            return;
        }

        lastSpeedChange = time;

        Speed = newSpeed;
    }

    /// <summary>
    /// 
    /// </summary>
    public void SetState(AIAgentState newState)
    {
        if (newState == AgentState) return;

        OnAgentStateChanged(AgentState, newState);
        AgentState = newState;
    }

    /// <summary>
    /// Change the bot target and sync it with other clients
    /// </summary>
    public void SetTarget(bl_AITarget newTarget, bool force = false)
    {
        if (Target == newTarget && !force)
        {
            return;
        }

        // when get a new target, reset the vision to wait until the next refresh
        if (Target == null && newTarget != null)
        {
            IsTargetHampered = true;
            IsTargetInSight = false;
            IsTargetInFieldOfView = false;
            timeSinceVisual = -1;
            timeSinceHampered = time;
        }

        if (newTarget != null && newTarget.GetTeam() == AITeam)
        {
            Debug.Log("This target is from the same team");
        }

        if (newTarget != null)
        {
            CachedTargetDistance = bl_UtilityHelper.Distance(thisFrameLocalPosition, newTarget.position);
            CheckVision();
        }

        OnTargetChanged(Target, newTarget);
        Target = newTarget;

        if (!bl_PhotonNetwork.IsMasterClient) return;

        // sync the bot target
        var data = bl_UtilityHelper.CreatePhotonHashTable();
        data.Add("type", AIRemoteCallType.SyncTarget);

        if (Target == null)
        {
            data.Add("viewID", -1);
            photonView.RPC(RPC_NAME, RpcTarget.Others, data);
        }
        else
        {
            var view = Target.GetNetView();
            view ??= GetPhotonView(Target.Transform.root.gameObject);

            if (view != null)
            {
                data.Add("viewID", view.ViewID);
                photonView.RPC(RPC_NAME, RpcTarget.Others, data);
            }
            else
            {
                Debug.Log("This Target " + Target.name + " do not have a network view, this target can't be sync.");
            }
        }
    }

    /// <summary>
    /// Determine the look at state based on the target
    /// If has a target and it is onsight, then look at the target, if not, look at the path
    /// </summary>
    private void DetermineLookAtState(AILookAt newLookAt)
    {
        if (LookingAt == newLookAt) return;
        // if is requested to look at the target, but the target is hampered, then don't look at it, keep looking at the path
        if (LookingAt == AILookAt.PathToTarget && newLookAt == AILookAt.Target)
        {
            if (IsTargetHampered) return;
            else if (time - timeSinceVisual < soldierSettings.reactionTime) return; // reaction time to look at the target
        }

        LookingAt = newLookAt;
        forceUpdateRotation = true;
    }

    /// <summary>
    /// Sets the destination for the AI agent to move towards.
    /// </summary>
    /// <param name="position">The position to move towards.</param>
    /// <param name="stopedDistance">The distance at which the agent should stop.</param>
    /// <param name="checkRate">Whether to check the rate of setting the destination.</param>
    /// <param name="lockTime">The time to lock the destination for.</param>
    public void SetDestination(Vector3 position, float stopedDistance, bool checkRate = false, float lockTime = -1)
    {
        if (destinationLockTime - time > 0) return;
        if (checkRate && (time - lastDestinationTime) < (2 * Random.value))
        {
            if (Agent.hasPath) return;
        }

        if (bl_UtilityHelper.IsWithinDistance(position, thisFrameLocalPosition, stopedDistance) || !Agent.isOnNavMesh)
        {
            return;
        }

        Agent.stoppingDistance = stopedDistance;

        // set the AI destination to the new position
        if (!Agent.SetDestination(position))
        {
            // if the destination is invalid (out of the navmesh), increase the invalid destination count
            invalidDestinationCount++;

            // if we have tried to set the destination to an invalid position for more than 10 times
            if (invalidDestinationCount > 10)
            {
                destinationLockTime = 0;
                if (behaviorSettings.targetOutNavmeshBehave == AITargetOutNavmeshBehave.RandomPatrol)
                {
                    // try to random patrol around the target area, and lock the destination for 5 seconds
                    // 5 seconds is arbitrary, but it's a good time to prevent the bot from trying to set the destination to the same invalid position
                    RandomPatrol(false, 5);
                    invalidDestinationCount = 0;
                }
                else SetTarget(null); // TODO: make sure not to assign the same target again
            }
        }
        else
        {
            invalidDestinationCount = 0;
            // Debug.Log($"{AIName}:{AITeam} Set Destination, Has Target: {HasATarget} State: {AgentState}");
        }
        lastDestinationTime = time;

        if (lockTime > 0)
        {
            destinationLockTime = time + lockTime;
        }
    }

    /// <summary>
    /// Set a random point around the map as destination
    /// </summary>
    /// <param name="force"></param>
    public void SetRandomPointInMapAsDestination(int radius = 100, float holdStateTime = 0, bool force = false)
    {
        if (AgentState == AIAgentState.HoldingPosition && !force) { return; }

        Vector3 randomDirection = thisFrameLocalPosition + (Random.insideUnitSphere * radius);
        int temp = 0;
        while (NavMesh.SamplePosition(randomDirection, out _, radius, 1) == false)
        {
            randomDirection = thisFrameLocalPosition + (Random.insideUnitSphere * radius);
            temp++;
            if (temp > 10) { break; }
        }
        SetDebugState(42, true);
        if (holdStateTime > 0) SetHoldState(holdStateTime);
    }

    /// <summary>
    /// 
    /// </summary>
    void SetCrouch(bool crouch, float holdStateFor = 0)
    {
        if (IsCrouch == crouch || time - lastCrouchTime < 0.5f) return;
        lastCrouchTime = time + holdStateFor;
        if (crouch && (AgentState == AIAgentState.Following || AgentState == AIAgentState.Looking))
        {
            crouch = false;
        }

        Anim.SetBool(animationHash[0], crouch);
        Speed = crouch ? soldierSettings.crounchSpeed : soldierSettings.walkSpeed;
        if (IsCrouch != crouch)
        {
            var data = bl_UtilityHelper.CreatePhotonHashTable();
            data.Add("type", AIRemoteCallType.CrouchState);
            data.Add("state", crouch);

            photonView.RPC(RPC_NAME, RpcTarget.Others, data);
            IsCrouch = crouch;
        }
    }

    /// <summary>
    /// This is called when the bot have a Target
    /// this check if other enemy is nearest and change of target if it's require
    /// </summary>
    void CheckEnemiesDistances()
    {
        if (!behaviorSettings.checkEnemysWhenHaveATarget || availableTargets.Count <= 0) return;
        if (time < nextEnemysCheck || time - instanceTime < 30 || isHoldingState) return;

        CachedTargetDistance = bl_AIMananger.GetCachedDistance(AimTarget, Target);
        for (int i = 0; i < availableTargets.Count; i++)
        {
            //if the enemy transform is not null or the same target that have currently have or death.
            if (availableTargets[i] == null || availableTargets[i] == Target) continue;
            //calculate the distance from this other enemy
            float otherDistance = bl_AIMananger.GetCachedDistance(AimTarget, availableTargets[i]);
            if (otherDistance > soldierSettings.limitRange) continue;//if this enemy is too far away...
            //and check if it's nearest than the current target (5 meters close at least)
            if (otherDistance < CachedTargetDistance && (CachedTargetDistance - otherDistance) > 5)
            {
                //calculate the angle between this bot and the other enemy to check if it's in a "View Angle"
                if (IsInSight(availableTargets[i].position))
                {
                    //so then get it as new dangerous target
                    SetTarget(availableTargets[i]);
                    //prevent to change target in at least the next 3 seconds
                    nextEnemysCheck = time + 3;
                }
            }
        }
    }

    /// <summary>
    /// Called every frame when the bot has a target and is not covering or holding position
    /// Check wheter the bot has a enemy in sight that is not the current target
    /// If it's true, then change the target to this new enemy that opose a bigger threat.
    /// </summary>
    private void CheckEnemiesOnSight()
    {
        if (!behaviorSettings.checkEnemysWhenHaveATarget || availableTargets.Count <= 0) return;
        if (currentFrame % behaviorSettings.nearThreatCheckRate != 0) return;

        for (int i = 0; i < availableTargets.Count; i++)
        {
            var t = availableTargets[i];

            //if the enemy transform is not null or the same target that have currently have or death.
            if (t == null || t == Target || t.IsDeath) continue;

            // only consider change to this target if is in vision range, is not hampered, and represent a near threat
            // Check cheaper conditions first
            if (!IsInSight(t.position)) continue;
            float distance = bl_AIMananger.GetCachedDistance(AimTarget, t);
            if (!HasClearShotTo(t.position))
            {
                // if doesn't have a clear shot and is not a near threat, then continue
                if (!IsANearThreat(t)) continue;
            }

            // but if the current target is in sight, only change the target if the new target is closer.
            if (!IsTargetHampered && distance >= CachedTargetDistance)
            {
                continue;
            }

            // finally, do a probability check to change the target
            if (!Probability(soldierSettings.enemyAwareness)) continue;

            SetTarget(t);

            break;
        }
    }

    /// <summary>
    /// Check if the bot has been just looking at the target for too long without any action
    /// If it's true, then change the state to follow the target or try to fire.
    /// </summary>
    private void CheckConfrontation()
    {
        if (AgentState != AIAgentState.Covering)
        {
            if (lookTime >= randomValues[0])
            {
                SetState(AIAgentState.Following);
                lookTime = 0;
                return;
            }
            lookTime += delta;
            SetState(AIAgentState.Looking);
        }

        FireReason fr = FireReason.Normal;
        if (CachedTargetDistance < soldierSettings.closeRange) { fr = FireReason.Forced; }
        else if (CachedTargetDistance < soldierSettings.mediumRange) { fr = FireReason.OnMove; }

        TriggerFire(fr);
        SetCrouch(IsTargetInSight && !IsTargetHampered);
    }

    /// <summary>
    /// This is called once each second to cache the current vision state of bot
    /// If it has a target, then check if it's in sight and if it's hampered by an obstacle.
    /// </summary>
    private void CheckVision()
    {
        if (!HasATarget || !bl_PhotonNetwork.IsMasterClient)
        {
            IsTargetHampered = false;
            return;
        }

        // Check if the target is in sight (vision angle and distance)
        IsTargetInSight = IsInSight(TargetPosition);
        // Check if the target is hampered by an obstacle
        IsTargetHampered = !HasClearShotTo(TargetPosition);
        // Check if the target is in the field of view
        IsTargetInFieldOfView = IsInFieldOfView(TargetPosition);

        // check if the in sight state has changed
        if (wasTargetHampered != IsTargetHampered)
        {
            OnTargetHamperedChanged(wasTargetHampered, IsTargetHampered);
            wasTargetHampered = IsTargetHampered;
        }
    }

    /// <summary>
    /// Check if the bot has a target and if it's dead, then set the target to null
    /// This fix an issue in old versions of MFPS and not sure if this is still needed.
    /// </summary>
    public override void CheckTargets()
    {
        if (Target != null && Target.IsDeath)
        {
            SetTarget(null);
        }
    }

    /// <summary>
    /// Check if the bot has the oportunity to make long distance shots
    /// </summary>
    private void CheckLongDistanceShot()
    {
        if (currentFrame % 10 != 0) return;
        if (behaviorSettings.longShotsProbility <= 0 || !HasATarget || IsTargetHampered || isHoldingState) return;
        if (CachedTargetDistance < soldierSettings.mediumRange) { return; }
        if (Time.time - instanceTime < 5) { return; } // wait 5 seconds before start making long distance shots

        float longShotsProbility = behaviorSettings.longShotsProbility;
        if (AIWeapon.GetAttackRange() == AttackerRange.Far)
        {
            longShotsProbility = 1; // sniper bots always make long distance shots
        }
        else if (AIWeapon.GetAttackRange() == AttackerRange.Close)
        {
            longShotsProbility -= longShotsProbility * 0.7f; // close range bots have less probability to make long distance shots
        }
        longShotsProbility = Mathf.Clamp(longShotsProbility, 0, 1);

        if (!Probability(longShotsProbility)) return;

        TriggerFire(FireReason.Forced);
        HoldPosition(5, true);
        if (Probability(0.85f)) SetCrouch(true, 4);
    }

    /// <summary>
    /// Check if the bot destination is in an enemy hot area
    /// </summary>
    private void CheckEnemyHotAreas()
    {
        if (time - lastHotAreaDecitionT < 5 || IsDeath) return;
        if (currentFrame % 312 != 0) return; // 312 is arbitrary
        if (behaviorSettings.engagementBias >= 1) return;

        if (!bl_AIAreas.IsInEnemyHotArea(this, TargetPosition, out Vector3 hotAreaCenter))
            return;

        // if the destination position is inside a hot enemy area

        float engagementBias = behaviorSettings.engagementBias;
        if (AIWeapon.GetAttackRange() == AttackerRange.Far)
        {
            // if the bot is an sniper, then won't take the risk to engage the target in a hot area
            engagementBias = 0;
        }
        // take a probability based decision to engage the target not matter if is located in a hot area.
        if (Probability(engagementBias))
        {
            // the bot decided to engage the target taking the risk.
            // hold this decision for 5 seconds
            lastHotAreaDecitionT = time;
            return;
        }

        // check if the bot is near the hot area
        if (bl_UtilityHelper.IsWithinDistance(thisFrameLocalPosition, hotAreaCenter, soldierSettings.mediumRange))
        {
            // take an equal chance to hold position or find a random point near the hot area
            if (Probability(0.5f))
            {
                // Hold position
                HoldPosition(5, true);
                if (Probability(0.3f)) SetCrouch(true, 4);
            }
            else
            {
                // Find a random point near the hot area
                Vector3 randomPointNearHotArea = GetPositionAround(hotAreaCenter, 20); // 20 is arbitrary
                SetDestination(randomPointNearHotArea, 0, false, 5);
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void SwitchCover()
    {
        if (Agent.pathStatus != NavMeshPathStatus.PathComplete)
            return;

        if (SwitchCoverTimes <= 3)
        {
            Cover(true, AIAgentCoverArea.ToTarget);
            SwitchCoverTimes++;
        }
        else
        {
            SetState(AIAgentState.Following);
            SetDestination(TargetPosition, 3);
            SwitchCoverTimes = 0;
        }
    }

    /// <summary>
    /// This fix an issue in old versions of MFPS and not sure if this is still needed.
    /// </summary>
    public void KillTheTarget()
    {
        if (!HasATarget) return;

        SetTarget(null);
        var data = bl_UtilityHelper.CreatePhotonHashTable();
        data.Add("type", AIRemoteCallType.SyncTarget);
        data.Add("viewID", -1);

        photonView.RPC(RPC_NAME, RpcTarget.Others, data);
    }

    /// <summary>
    /// Fetch and cache the available targets for this bot
    /// Available bots means all the alive bots that are not in the same team as this bot.
    /// </summary>
    public override void UpdateTargetList()
    {
        if (Target != null) return;

        bl_AIMananger.Instance.GetTargetsFor(this, ref availableTargets);
        AimTarget.name = AIName;
    }

    /// <summary>
    /// This is called when a forced bot respawn is called and this bot is still alive
    /// </summary>
    public override void Respawn()
    {
        var data = bl_UtilityHelper.CreatePhotonHashTable();
        data.Add("type", AIRemoteCallType.Respawn);
        photonView.RPC(RPC_NAME, RpcTarget.All, data);
    }

    /// <summary>
    /// Make the bot stay where he is for a given time
    /// </summary>
    /// <param name="forTime"></param>
    public void HoldPosition(float forTime = 10, bool pauseMovement = false)
    {
        SetState(AIAgentState.HoldingPosition);
        lastDestinationTime = Time.time + forTime;
        if (pauseMovement) PauseMovement(forTime);
    }

    /// <summary>
    /// Stop the bot movement for a given time
    /// </summary>
    public void PauseMovement(float pauseTime = -1)
    {
        if (!Agent.isOnNavMesh || Agent.isStopped) return;

        Agent.isStopped = true;
        if (pauseTime > 0)
        {
            this.InvokeAfter(pauseTime, () =>
            {
                if (IsDeath) return;

                Agent.isStopped = false;
                SetCrouch(false);
            });
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private void FootStep()
    {
        if (footstep != null && velocityMagnitud > 0.2f)
            footstep.UpdateStep(velocityMagnitud);
    }

    /// <summary>
    /// 
    /// </summary>
    private void HoldingStateTimeControl()
    {
        if (!isHoldingState) return;

        if (time > holdStateTimer)
        {
            isHoldingState = false;
            holdStateTimer = 0;
        }
    }

    [PunRPC]
    public void RPCShooterBot(NetHashTable data, PhotonMessageInfo info)
    {
        var callType = (AIRemoteCallType)data["type"];

        var cid = networkMessagesCounts.FindIndex(x => x.CallType == callType);
        if (cid == -1)
        {
            networkMessagesCounts.Add(new NetworkMessagesCount()
            {
                CallType = callType,
                Count = 1
            });
        }
        else
        {
            networkMessagesCounts[cid].Count++;
        }

        switch (callType)
        {
            case AIRemoteCallType.DestroyBot:
                DestroyBot(data, info);
                break;
            case AIRemoteCallType.SyncTarget:
                SyncTargetAI(data);
                break;
            case AIRemoteCallType.CrouchState:
                Anim.SetBool(animationHash[0], (bool)data["state"]);
                break;
            case AIRemoteCallType.Respawn:
                DoAliveRespawn();
                break;
        }
    }

    /// <summary>
    /// Do a respawn without destroying the bot instance
    /// </summary>
    private void DoAliveRespawn()
    {
        Agent.isStopped = false;
        AIHealth.SetHealth(100, true);
        isGameStarted = false;
        Target = null;
        this.InvokeAfter(2, () => { isGameStarted = bl_MatchTimeManagerBase.Instance.TimeState == RoomTimeState.Started; });
        if (bl_PhotonNetwork.IsMasterClient)
        {
            var spawn = bl_SpawnPointManagerBase.Instance.GetSequentialSpawnPoint(AITeam).transform;
            Agent.Warp(spawn.position);
            CachedTransform.SetPositionAndRotation(spawn.position, spawn.rotation);
        }
    }

    /// <summary>
    /// Called from Master Client on all clients when a bot die
    /// </summary>
    public void DestroyBot(NetHashTable data, PhotonMessageInfo info)
    {
        if (data.ContainsKey("instant"))
        {
            if (bl_PhotonNetwork.IsMasterClient) bl_PhotonNetwork.Destroy(gameObject);
            return;
        }

        var damageData = (DamageData)data["damage"];
        References.aiAnimation.Ragdolled(damageData);

        if ((bl_PhotonNetwork.Time - info.SentServerTime) > bl_GameData.CoreSettings.PlayerRespawnTime + 1)
        {
            Debug.LogWarning("The death call get with too much delay.");
        }
        this.InvokeAfter(bl_GameData.CoreSettings.PlayerRespawnTime + 2, () =>
        {
            if (GetGameMode.GetGameModeInfo().onRoundStartedSpawn != GameModeSettings.OnRoundStartedSpawn.WaitUntilRoundFinish)
                Debug.LogWarning("For some reason this bot has not been destroyed yet.");
        });
    }

    /// <summary>
    /// 
    /// </summary>
    void SyncTargetAI(NetHashTable data)
    {
        var view = (int)data["viewID"];
        if (view == -1)
        {
            SetTarget(null);
            return;
        }

        GameObject pr = FindPlayerRoot(view);
        if (pr == null)
        {
            Debug.Log($"Couldn't find the network view: {view}");
            return;
        }

        if (pr.TryGetComponent(out bl_PlayerReferencesCommon prc))
        {
            SetTarget(prc.BotAimTarget);
        }
    }

    /// <summary>
    /// Optimized distance calculation with caching
    /// </summary>
    public void RefreshDistance()
    {
        if (!HasATarget) return;

        // Use cached distance if updated recently
        if (time - lastDistanceUpdateTime < DISTANCE_UPDATE_INTERVAL)
        {
            CachedTargetDistance = Mathf.Sqrt(cachedDistanceSqr);
            return;
        }

        cachedDistanceSqr = (thisFrameLocalPosition - TargetPosition).sqrMagnitude;
        CachedTargetDistance = Mathf.Sqrt(cachedDistanceSqr);
        lastDistanceUpdateTime = time;
    }

    /// <summary>
    /// 
    /// </summary>
    public void SetLookAtState(AILookAt newLookAt)
    {
        if (LookingAt == newLookAt) return;
        // if is requested to look at the target, but the target is hampered, then don't look at it, keep looking at the path
        if (LookingAt == AILookAt.PathToTarget && newLookAt == AILookAt.Target)
        {
            if (IsTargetHampered) return;
            else if (time - timeSinceVisual < soldierSettings.reactionTime) return; // reaction time to look at the target
        }

        LookingAt = newLookAt;
        forceUpdateRotation = true;
    }

    #region AI Improvements

    /// <summary>
    /// Update target prediction based on target movement
    /// Optimized version with caching
    /// </summary>
    private void UpdateTargetPrediction()
    {
        if (!HasATarget) return;

        Vector3 currentTargetPos = TargetPosition;

        // Calculate target velocity with smoothing
        if (time - lastTargetPositionUpdateTime > 0.15f)
        {
            Vector3 newVelocity = (currentTargetPos - lastKnownTargetPosition) / (time - lastTargetPositionUpdateTime);

            // Smooth velocity to avoid jitter
            targetVelocity = Vector3.Lerp(targetVelocity, newVelocity, 0.5f);
            lastKnownTargetPosition = currentTargetPos;
            lastTargetPositionUpdateTime = time;

            // Predict future position with weapon-specific adjustments
            float bulletSpeed = AIWeapon.GetAttackRange() == AttackerRange.Far ? 200f : 100f;
            float timeToReach = CachedTargetDistance / bulletSpeed;

            // Add some randomness to make it more human-like
            float predictionAccuracy = concentrationLevel * adaptiveDifficultyModifier;
            Vector3 prediction = currentTargetPos + (targetVelocity * timeToReach * predictionAccuracy);

            // Add slight inaccuracy based on distance
            float inaccuracyFactor = Mathf.Clamp01(CachedTargetDistance / soldierSettings.farRange);
            prediction += Random.insideUnitSphere * inaccuracyFactor * (1f - predictionAccuracy);

            predictedTargetPosition = prediction;
        }
    }

    /// <summary>
    /// Determine if the bot should attempt to flank the target
    /// Improved with better decision making
    /// </summary>
    private bool ShouldAttemptFlank()
    {
        if (!HasATarget) return false;

        // Cache decision for performance
        if (time - lastFlankDecisionTime < 8f) return shouldFlank;

        lastFlankDecisionTime = time;

        // Don't flank if conditions aren't right
        if (CachedTargetDistance < soldierSettings.closeRange || CachedTargetDistance > soldierSettings.farRange)
        {
            shouldFlank = false;
            return false;
        }

        if (AIHealth.GetHealth() < soldierSettings.criticalHealth)
        {
            shouldFlank = false;
            return false;
        }

        // Don't flank if already in advantageous position
        if (isInAdvantageousPosition)
        {
            shouldFlank = false;
            return false;
        }

        // Aggressive bots are more likely to flank
        float flankProbability = GetBehave() == AIAgentBehave.Aggressive ? 0.35f : 0.15f;

        // Increase probability if target is stationary or predictable
        if (targetVelocity.sqrMagnitude < 0.5f)
        {
            flankProbability += 0.15f;
        }

        // Increase if bot is struggling to hit the target
        if (shotsFiredSinceLastHit > 10)
        {
            flankProbability += 0.2f;
        }

        // Decrease if recently got hit
        if (time - lastSuccessfulHitTime < 2f)
        {
            flankProbability *= 0.5f;
        }

        shouldFlank = Probability(flankProbability);
        return shouldFlank;
    }

    /// <summary>
    /// Get a flanking position around the target
    /// Improved with NavMesh validation and tactical positioning
    /// </summary>
    private Vector3 GetFlankingPosition()
    {
        if (!HasATarget) return thisFrameLocalPosition;

        Vector3 toTarget = TargetPosition - thisFrameLocalPosition;
        toTarget.y = 0;

        // Choose side based on cover availability and less exposure
        bool checkLeft = Random.value > 0.5f;
        Vector3 perpendicular = Vector3.Cross(toTarget.normalized, Vector3.up) * (checkLeft ? 1 : -1);

        // Try multiple distances to find best position
        float[] distances = { soldierSettings.mediumRange * 0.8f, soldierSettings.mediumRange * 0.6f, soldierSettings.closeRange * 1.5f };

        foreach (float dist in distances)
        {
            Vector3 flankPosition = TargetPosition + (perpendicular * dist);

            // Add some forward bias to get closer angle
            flankPosition += toTarget.normalized * (dist * 0.3f);

            if (NavMesh.SamplePosition(flankPosition, out NavMeshHit hit, dist * 0.5f, 1))
            {
                // Check if position has line of sight to target
                if (!Physics.Linecast(hit.position + Vector3.up, TargetPosition + Vector3.up, ObstaclesLayer, QueryTriggerInteraction.Ignore))
                {
                    return hit.position;
                }
            }
        }

        // Try opposite side if first side failed
        if (checkLeft)
        {
            perpendicular = Vector3.Cross(toTarget.normalized, Vector3.up) * -1;
            Vector3 flankPosition = TargetPosition + (perpendicular * soldierSettings.mediumRange * 0.7f);

            if (NavMesh.SamplePosition(flankPosition, out NavMeshHit hit, soldierSettings.mediumRange * 0.4f, 1))
            {
                return hit.position;
            }
        }

        return thisFrameLocalPosition;
    }

    /// <summary>
    /// Smart strafing behavior during combat
    /// Optimized and more tactical
    /// </summary>
    private void UpdateCombatStrafing()
    {
        if (AgentState != AIAgentState.Following && AgentState != AIAgentState.Looking) return;
        if (time - lastStrafeChangeTime < 1.2f) return;

        // Don't strafe if moving to destination or too close
        if ((HasAPath && Agent.remainingDistance > 2f) || CachedTargetDistance < 3f) return;

        strafeTimer += delta * 10f; // Accelerated for frame rate independence

        // Change strafe direction based on getting hit or random timer
        bool shouldChangeDirection = strafeTimer > Random.Range(15f, 30f);

        if (shouldChangeDirection)
        {
            strafeDirection *= -1;
            strafeTimer = 0;
            lastStrafeChangeTime = time;

            // Calculate strafe position with obstacles check
            Vector3 strafeOffset = CachedTransform.right * strafeDirection * Random.Range(2f, 5f);
            Vector3 strafePos = thisFrameLocalPosition + strafeOffset;

            // Check for obstacles
            if (!Physics.Raycast(thisFrameLocalPosition, strafeOffset.normalized, strafeOffset.magnitude, ObstaclesLayer))
            {
                if (NavMesh.SamplePosition(strafePos, out NavMeshHit hit, 5f, 1))
                {
                    SetDestination(hit.position, 0.3f, false, 1.5f);
                }
            }
        }
    }

    /// <summary>
    /// Update human-like aim drift for more realistic behavior
    /// </summary>
    private void UpdateAimDrift()
    {
        if (!HasATarget || !IsTargetInSight) return;

        aimDriftTimer += delta;

        // Update concentration level based on combat state
        if (IsTargetHampered || velocityMagnitud > 0.1f)
        {
            concentrationLevel = Mathf.MoveTowards(concentrationLevel, 0.7f, delta * 0.5f);
        }
        else
        {
            concentrationLevel = Mathf.MoveTowards(concentrationLevel, 1f, delta * 0.3f);
        }

        // Add slight aim drift when moving or at long range
        if (aimDriftTimer > 0.1f)
        {
            float driftAmount = (1f - concentrationLevel) * 0.5f;

            if (CachedTargetDistance > soldierSettings.mediumRange)
            {
                driftAmount *= 1.5f;
            }

            aimDriftOffset = Vector3.Lerp(aimDriftOffset, Random.insideUnitSphere * driftAmount, delta * 2f);
            aimDriftTimer = 0;
        }
    }

    /// <summary>
    /// Check if bot should reload based on ammo and tactical situation
    /// </summary>
    private bool ShouldReloadNow()
    {
        if (time - lastAmmoCheckTime < 0.5f) return false;
        lastAmmoCheckTime = time;

        // Don't check if weapon doesn't exist
        if (AIWeapon == null) return false;

        // Get current ammo (you'll need to add this method to your weapon script)
        // This is a placeholder - adjust based on your actual weapon implementation
        // float ammoPercentage = AIWeapon.GetAmmoPercentage();

        // Reload if ammo is low and not in immediate danger
        // if (ammoPercentage < 0.3f && (!HasATarget || CachedTargetDistance > soldierSettings.closeRange))
        // {
        //     return true;
        // }

        // Reload if ammo is critically low
        // if (ammoPercentage < 0.1f)
        // {
        //     return true;
        // }

        return false;
    }

    /// <summary>
    /// Adjust bot difficulty based on performance
    /// Enhanced with more factors
    /// </summary>
    private void UpdateAdaptiveDifficulty()
    {
        int totalShots = consecutiveHits + consecutiveMisses;
        if (totalShots == 0) return;

        float hitRatio = (float)consecutiveHits / totalShots;

        // Adjust modifier based on hit ratio with smoother transitions
        if (hitRatio > 0.75f)
        {
            adaptiveDifficultyModifier = Mathf.Max(0.6f, adaptiveDifficultyModifier - 0.03f);
        }
        else if (hitRatio > 0.6f)
        {
            adaptiveDifficultyModifier = Mathf.MoveTowards(adaptiveDifficultyModifier, 0.85f, 0.02f);
        }
        else if (hitRatio < 0.25f)
        {
            adaptiveDifficultyModifier = Mathf.Min(1.4f, adaptiveDifficultyModifier + 0.04f);
        }
        else if (hitRatio < 0.4f)
        {
            adaptiveDifficultyModifier = Mathf.MoveTowards(adaptiveDifficultyModifier, 1.1f, 0.02f);
        }

        // Reset counters periodically
        if (totalShots > 25)
        {
            consecutiveHits = (int)(consecutiveHits * 0.5f);
            consecutiveMisses = (int)(consecutiveMisses * 0.5f);
        }
    }

    /// <summary>
    /// Get the predicted target position for more accurate aiming
    /// Optimized version
    /// </summary>
    public Vector3 GetPredictedTargetPosition()
    {
        if (!HasATarget) return TargetPosition;

        // Use predicted position if target is moving and bot can see them
        if (targetVelocity.sqrMagnitude > 0.3f && IsTargetInSight)
        {
            // Apply aim drift for human-like behavior
            return predictedTargetPosition + aimDriftOffset;
        }

        return TargetPosition;
    }

    /// <summary>
    /// Enhanced cover selection that considers tactical advantages
    /// Optimized with early exits and caching
    /// </summary>
    private bl_AICoverPoint GetTacticalCoverPoint(Vector3 nearPosition, float radius)
    {
        if (!HasATarget) return bl_AICovertPointManager.Instance.GetCoverOnRadius(CachedTransform, radius);

        var coverPoints = bl_AICovertPointManager.Instance.GetAllCoverPointsInRadius(nearPosition, radius);
        if (coverPoints == null || coverPoints.Count == 0) return null;

        bl_AICoverPoint bestCover = null;
        float bestScore = float.MinValue;

        // Limit evaluation to performance (max 10 covers)
        int maxEvaluations = Mathf.Min(coverPoints.Count, 10);

        for (int i = 0; i < maxEvaluations; i++)
        {
            var cover = coverPoints[i];
            if (cover == null) continue;

            float score = EvaluateCoverPoint(cover);

            if (score > bestScore)
            {
                bestScore = score;
                bestCover = cover;
            }
        }

        // Mark if we found an advantageous position
        isInAdvantageousPosition = bestScore > 25f;

        return bestCover;
    }

    /// <summary>
    /// Evaluate a cover point for tactical advantage
    /// Separated for better performance and readability
    /// </summary>
    private float EvaluateCoverPoint(bl_AICoverPoint cover)
    {
        float score = 0;

        // Distance to cover (prefer not too close, not too far)
        float distanceToCover = Vector3.Distance(thisFrameLocalPosition, cover.Position);
        float optimalDistance = 15f;
        float distanceScore = Mathf.Max(0, 10f - Mathf.Abs(distanceToCover - optimalDistance));
        score += distanceScore;

        // Distance from cover to target (prefer medium range)
        float coverToTarget = Vector3.Distance(cover.Position, TargetPosition);
        if (coverToTarget > soldierSettings.closeRange && coverToTarget < soldierSettings.mediumRange)
        {
            score += 20;
        }
        else if (coverToTarget > soldierSettings.mediumRange && coverToTarget < soldierSettings.farRange)
        {
            score += 10;
        }

        // Line of sight to target from cover
        Vector3 coverCheckPos = cover.Position + Vector3.up * 1.5f;
        Vector3 targetCheckPos = TargetPosition + Vector3.up;

        if (!Physics.Linecast(coverCheckPos, targetCheckPos, ObstaclesLayer, QueryTriggerInteraction.Ignore))
        {
            score += 15;
        }
        else
        {
            // Partial sight is still valuable
            score += 5;
        }

        // Height advantage
        float heightDifference = cover.Position.y - TargetPosition.y;
        if (heightDifference > 1f)
        {
            score += 12;
        }
        else if (heightDifference < -2f)
        {
            score -= 5; // Penalty for low ground
        }

        // Avoid same cover
        if (CoverPoint != null && cover != CoverPoint)
        {
            score += 8;
        }

        // Prefer covers that provide escape routes (has neighbors)
        if (cover.HasNeighbords())
        {
            score += 6;
        }

        return score;
    }

    #endregion

    #region Event Callbacks
    /// <summary>
    /// Called when the bot not direct vision to the target -> have direct vision to the target and vice versa
    /// </summary>
    /// <param name="from">was hampered?</param>
    /// <param name="to">is hampered?</param>
    private void OnTargetHamperedChanged(bool from, bool to)
    {
        if (isHoldingState) return;
        if (from == false)//the player lost the line of vision with the target
        {
            timeSinceHampered = time;
            timeSinceVisual = -1;
        }
        else
        {
            timeSinceHampered = -1;
            timeSinceVisual = time;
        }
    }

    /// <summary>
    /// Called when the bot is unable to fight, for example when is reloading.
    /// </summary>
    public void OnUnableToFight()
    {
        // if there are no enemies nearby, then there is no problem.
        if (!HasEnemiesNearby()) return;

        if (AgentState != AIAgentState.Covering)
        {
            // try to cover
            Cover(true, AIAgentCoverArea.ToPoint, true);
        }
        else
        {
            SetCrouch(true);
        }
    }

    /// <summary>
    /// Called when the bot gets hit by a bullet/explosion
    /// </summary>
    public override void OnGetHit(Vector3 pos)
    {
        LastHitDirection = pos;

        //if the AI is not covering, will look for a cover point
        if (AgentState != AIAgentState.Covering)
        {
            if ((AgentState == AIAgentState.HoldingPosition || isHoldingState) && behaviorSettings.breakHoldPositionOnDamage)
            {
                isHoldingState = false;
                holdStateTimer = 0;
            }

            //if the AI is following and attacking the target he will not look for cover point
            if (AgentState == AIAgentState.Following && TargetDistance <= soldierSettings.mediumRange && !IsTargetHampered)
            {
                SetLookAtState(AILookAt.Target);
                return;
            }
            Cover(false);
        }
        else
        {
            //if already in a cover and still get shoots from far away will force the AI to fire.
            if (!IsTargetInSight)
            {
                SetLookAtState(AILookAt.Target);
                Cover(true);
            }
            else
            {
                ForceCoverFire = true;
                SetLookAtState(AILookAt.HitDirection);
            }
            //if the AI is cover but still get hit, he will search other cover point 
            if (AIHealth.GetHealth() <= 50 && Agent.pathStatus == NavMeshPathStatus.PathComplete)
            {
                Cover(true);
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private void OnAgentStateChanged(AIAgentState from, AIAgentState to)
    {
    }

    /// <summary>
    /// 
    /// </summary>
    private void OnTargetChanged(bl_AITarget from, bl_AITarget to)
    {
    }

    /// <summary>
    /// 
    /// </summary>
    void OnLocalSpawn()
    {
        References.shooterNetwork.CheckNamePlate();
    }

    /// <summary>
    /// When a new player joins in the room
    /// </summary>
    /// <param name="newPlayer"></param>
    public void OnPhotonPlayerConnected(Player newPlayer)
    {
        if (bl_PhotonNetwork.IsMasterClient && newPlayer.ActorNumber != bl_PhotonNetwork.LocalPlayer.ActorNumber)
        {
            // received in bl_AIShooterHealth.cs
            photonView.RPC("RpcSyncHealth", newPlayer, AIHealth.GetHealth());
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void OnMatchStateChange(MatchState state)
    {
        isGameStarted = state == MatchState.Waiting || state == MatchState.Starting
            ? false
            : bl_MatchTimeManagerBase.Instance.TimeState == RoomTimeState.Started;

        if (state == MatchState.Finishing)
        {
            AIShooterReferences.shooterWeapon.CanFire = false;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void OnMatchStart()
    {
        isGameStarted = bl_GameManager.Instance.GameMatchState != MatchState.Waiting && bl_MatchTimeManagerBase.Instance.TimeState == RoomTimeState.Started;
        UpdateTargetList();
    }

    public override void OnDeath() { CancelInvoke(); }
    #endregion

    /// <summary>
    /// Is the given position in sight of the bot vision range?
    /// </summary>
    /// <returns></returns>
    public bool IsInSight(Vector3 targetPosition)
    {
        Vector3 directionToEnemy = targetPosition - thisFrameLocalPosition;

        // Check if enemy is within range and within field of view.
        if (directionToEnemy.sqrMagnitude <= soldierSettings.visionRange * soldierSettings.visionRange)
        {
            if (Vector3.Dot(CachedTransform.forward, directionToEnemy.normalized) > Mathf.Cos(soldierSettings.fieldOfView * 0.5f * Mathf.Deg2Rad))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Does the bot have a clear shot (free of obstacles) to the given position?
    /// </summary>
    /// <param name="targetPosition"></param>
    /// <returns></returns>
    public bool HasClearShotTo(Vector3 targetPosition)
    {
        return !Physics.Linecast(AIWeapon.GetFirePosition(), targetPosition, ObstaclesLayer, QueryTriggerInteraction.Ignore);
    }

    /// <summary>
    /// Check if a given position is within the bot field of view not considering the distance
    /// </summary>
    /// <param name="targetPosition"></param>
    /// <returns></returns>
    public bool IsInFieldOfView(Vector3 targetPosition)
    {
        Vector3 directionToTarget = targetPosition - thisFrameLocalPosition;

        // Check if the target is within the field of view.
        if (Vector3.Dot(CachedTransform.forward, directionToTarget.normalized) > Mathf.Cos(soldierSettings.fieldOfView * 0.5f * Mathf.Deg2Rad))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Check whether the given target represents a threat to the bot
    /// </summary>
    /// <param name="target"></param>
    /// <returns></returns>
    public bool IsANearThreat(bl_AITarget target)
    {
        if (target == null || !behaviorSettings.detectNearbyEnemies) return false;
        // if not that close, then it's not a threat
        if (bl_AIMananger.GetCachedDistance(AimTarget, target) > soldierSettings.closeRange) return false;

        // check if is above this bot (for multi floors levels)
        if (target.position.y > thisFrameLocalPosition.y + 2) return false;

        /// check if the target is in sight
        if (!HasClearShotTo(target.position)) return false;

        return true;
    }

    /// <summary>
    /// Check if there are enemies nearby
    /// </summary>
    /// <returns></returns>
    public bool HasEnemiesNearby()
    {
        if (availableTargets.Count <= 0) return false;

        foreach (var item in availableTargets)
        {
            if (item == null) continue;

            if (bl_AIMananger.GetCachedDistance(item, AimTarget) < soldierSettings.closeRange)
            {
                return true;
            }
        }

        return true;
    }

    #region Getters
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public bl_AITarget GetNearestPlayer(bool preferNonTargeted)
    {
        if (availableTargets.Count <= 0) return null;

        bl_AITarget closestTarget = null;
        bl_AITarget closestNonTargeted = null;
        bl_AITarget closestOnSight = null;
        float d = 1000;
        for (int i = 0; i < availableTargets.Count; i++)
        {
            if (availableTargets[i] == null || availableTargets[i].IsDeath) continue;
            float dis = bl_AIMananger.GetCachedDistance(AimTarget, availableTargets[i]);
            if (dis < d)
            {
                d = dis;
                closestTarget = availableTargets[i];
                if (IsInSight(closestTarget.position))
                {
                    closestOnSight = closestTarget;
                    // don't need to continue if we found a target in sight
                    // since it means the target is close and on the line of sight, that is a good target.
                    break;
                }
                if (preferNonTargeted)
                {
                    if (bl_AIMananger.IsTargetAvailable(closestTarget))
                    {
                        closestNonTargeted = closestTarget;
                    }
                }
            }
        }

        // prioritize on sight target
        if (closestOnSight != null) return closestOnSight;

        if (closestNonTargeted != null)
        {
            bl_AIMananger.ClaimTarget(closestNonTargeted);
        }

        // as second option, prioretize the non targeted target, if there is no one, then return the closest target.
        return closestNonTargeted != null ? closestNonTargeted : closestTarget;

    }

    /// <summary>
    /// Get a target that is on the given range
    /// </summary>
    /// <param name="range">The range to filter the targets</param>
    /// <param name="returnFirstInSight">If true, return the first target on range that is on sight, if false, return a random target in range.</param>
    /// <returns></returns>
    public bl_AITarget GetTargetOnRange(float range = -1, bool returnFirstInSight = false)
    {
        bl_AITarget t = null;
        if (availableTargets.Count <= 0) return t;

        if (range == -1) range = soldierSettings.mediumRange;

        List<bl_AITarget> inRange = new();
        float distance;
        for (int i = 0; i < availableTargets.Count; i++)
        {
            t = availableTargets[i];
            if (t == null || t.IsDeath) continue;

            distance = bl_AIMananger.GetCachedDistance(AimTarget, t);
            if (distance < range)
            {
                if (returnFirstInSight && (IsInSight(t.position) || HasClearShotTo(t.position)))
                {
                    return t;
                }
                inRange.Add(t);
            }
        }

        t = inRange.Count > 0 ? inRange[Random.Range(0, inRange.Count)] : null;

        return t;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public AIAgentBehave GetBehave()
    {
        var attackRange = AIWeapon.GetAttackRange();
        if (attackRange == AttackerRange.Medium) return behaviorSettings.agentBehave;
        return attackRange == AttackerRange.Close ? AIAgentBehave.Aggressive : AIAgentBehave.Protective;
    }

    private float Speed
    {
        get => Agent.speed;
        set
        {
            bool cr = Anim.GetBool(animationHash[0]);
            Agent.speed = cr ? 2 : value;
        }
    }

    public override Vector3 TargetPosition
    {
        get
        {
            if (Target != null) { return Target.position; }
            if (!isOneTeamMode && availableTargets.Count > 0)
            {
                bl_AITarget t = GetNearestPlayer(true);
                return t != null ? t.position : CachedTransform.position + (CachedTransform.forward * 3);
            }
            return Vector3.zero;
        }
    }

    private MFPSPlayer m_MFPSActor;
    public MFPSPlayer BotMFPSActor
    {
        get
        {
            m_MFPSActor ??= bl_GameManager.Instance.GetMFPSPlayer(AIName);
            return m_MFPSActor;
        }
    }

    private bl_AIShooterReferences _AIShooterReferences = null;
    public bl_AIShooterReferences AIShooterReferences
    {
        get
        {
            if (_AIShooterReferences == null) { _AIShooterReferences = GetComponent<bl_AIShooterReferences>(); }
            return _AIShooterReferences;
        }
    }

    bool isNewDebug = false;
    /// <summary>
    /// Last 47
    /// </summary>
    public void SetDebugState(int stateID, bool initial = false)
    {
        if (!DebugStates) return;

        if (initial && isNewDebug)
        {
            DebugLineBuilder.Clear();
            DebugLineBuilder.Append(stateID);
            isNewDebug = false; // This might need correction based on your logic. It seems odd to set it true after checking it.
            return;
        }

        DebugLineBuilder.Append(" > ").Append(stateID).Append(' ');
    }

    public bool Probability(float probability) { return Random.value <= probability; }
    public bool ForceFollowAtHalfHealth => AIHealth.GetHealth() < 50 && behaviorSettings.forceFollowAtHalfHealth;
    public float VelocityMagnitud => velocityMagnitud;
    public float TargetDistance
    {
        get
        {
            return Target != null
                ? bl_AIMananger.GetCachedDistance(AimTarget, Target)
                : bl_UtilityHelper.Distance(CachedTransform.localPosition, TargetPosition);
        }
    }
    public bool HasATarget { get => Target != null; }
    public bool HasAPath => Agent.hasPath;
    private bool CanCover(float inTimePassed) { return ((time - CoverTime) >= inTimePassed); }
    #endregion

#if UNITY_EDITOR

    private void OnDrawGizmos()
    {
        if (IsDeath || !showGizmos) return;

        if (Agent != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(Agent.destination, 0.3f);
            Gizmos.DrawLine(CachedTransform.localPosition + Vector3.down, Agent.destination);

            if (Agent.path != null && Agent.path.corners != null)
            {
                Gizmos.color = Color.yellow;
                for (int i = 0; i < Agent.path.corners.Length; i++)
                {
                    if (i == 0)
                    {
                        // Gizmos.DrawSphere(Agent.path.corners[i], 0.05f);
                        continue;
                    }
                    else if (i == 1)
                    {
                        Gizmos.DrawSphere(Agent.path.corners[i], 0.2f);
                    }

                    Gizmos.DrawLine(Agent.path.corners[i - 1], Agent.path.corners[i]);
                }
            }
        }

        if (Target != null)
        {
            Color c = Color.red;
            if (!IsTargetHampered)
            {
                c = new Color(0.6988705f, 0.9056604f, 0.5852616f, 1.00f);
                if (IsTargetInSight) { c = Color.green; }
            }
            Gizmos.color = c;
            Gizmos.DrawLine(AimTarget.position, TargetPosition);
        }
        if (CachedTransform == null || !Application.isPlaying) return;

        Gizmos.color = Color.magenta;
        Handles.color = Color.magenta;
        Handles.DrawDottedLine(CachedTransform.localPosition + (Vector3.up * 0.5f), LookAtPosition, 1);
        Gizmos.DrawRay(CachedTransform.localPosition, CachedTransform.forward * 2);
        Gizmos.DrawWireCube(LookAtPosition, Vector3.one * 0.3f);

        if (soldierSettings != null)
        {
            var hc = new Color(1, 1, 1, 0.1f);
            if (Application.isPlaying)
            {

                if (IsTargetInSight) hc = new Color(0, 1, 0, 0.1f);
                else if (IsTargetInFieldOfView)
                    hc = new(0.96f, 0.81f, 0, 0.1f);
            }
            Handles.color = hc;
            Handles.DrawSolidArc(CachedTransform.position + new Vector3(0, 0.3f, 0), Vector3.up,
            Quaternion.Euler(0, -soldierSettings.fieldOfView * 0.5f, 0) * CachedTransform.forward,
            soldierSettings.fieldOfView, soldierSettings.visionRange);
        }

        Gizmos.color = Color.grey;
        Gizmos.DrawRay(CachedTransform.localPosition + Vector3.down, -lastMoveDir);
    }

    private void OnDrawGizmosSelected()
    {
        if (!DebugStates || soldierSettings == null) return;
        Gizmos.color = Color.yellow;
        bl_GizmosUtility.DrawWireArc(CachedTransform.position, soldierSettings.limitRange, 360, 12, Quaternion.identity);
        Gizmos.color = Color.white;
        bl_GizmosUtility.DrawWireArc(CachedTransform.position, soldierSettings.farRange, 360, 12, Quaternion.identity);
        Gizmos.color = Color.yellow;
        bl_GizmosUtility.DrawWireArc(CachedTransform.position, soldierSettings.mediumRange, 360, 12, Quaternion.identity);
        Gizmos.color = Color.white;
        bl_GizmosUtility.DrawWireArc(CachedTransform.position, soldierSettings.closeRange, 360, 12, Quaternion.identity);

        /* if (soldierSettings != null)
         {
             Handles.color = new Color(0, 1, 0, 0.1f);
             Handles.DrawSolidArc(CachedTransform.position + new Vector3(0, 0.3f, 0), Vector3.up,
             Quaternion.Euler(0, -soldierSettings.fieldOfView * 0.5f, 0) * CachedTransform.forward,
             soldierSettings.fieldOfView, soldierSettings.visionRange);
         }*/
    }
#endif

    /// <summary>
    /// Enhanced reaction to being attacked
    /// </summary>
    /// <param name="attacker"></param>
    public override void OnAttacked(bl_PlayerReferencesCommon attacker)
    {
        if (attacker == null) return;
        if (!isOneTeamMode && attacker.PlayerTeam == AITeam) return;

        // Track successful hit against us
        if (attacker == Target)
        {
            consecutiveMisses++;
            lastSuccessfulHitTime = time;
        }
        else
        {
            consecutiveHits++;
        }

        UpdateAdaptiveDifficulty();

        // Add reaction delay based on difficulty
        reactionDelay = Random.Range(0.1f, 0.3f) * (2f - adaptiveDifficultyModifier);

        // Reduce concentration when hit
        concentrationLevel = Mathf.Max(0.5f, concentrationLevel - 0.2f);

        if (!HasATarget)
        {
            SetTarget(attacker.BotAimTarget);
            return;
        }

        float attackerDistance = bl_AIMananger.GetCachedDistance(AimTarget, attacker.BotAimTarget);

        // Switch to closer, more dangerous threat
        if (attackerDistance < CachedTargetDistance - 3f)
        {
            SetTarget(attacker.BotAimTarget);
            return;
        }

        // If current target is not visible but attacker is closer
        if (IsTargetHampered && attackerDistance < soldierSettings.mediumRange)
        {
            SetTarget(attacker.BotAimTarget);
            return;
        }

        // Tactical retreat when health is critical
        if (AgentState != AIAgentState.Covering && AIHealth.GetHealth() < soldierSettings.criticalHealth && !FocusOnSingleTarget)
        {
            isHoldingState = false;
            holdStateTimer = 0;

            // Find cover away from attackers
            Cover(false, AIAgentCoverArea.ToTarget, true);
            SetHoldState(RandomRange(2, 4));
        }
    }
}
