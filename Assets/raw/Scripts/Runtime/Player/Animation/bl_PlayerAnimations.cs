using MFPS.Internal;
using MFPSEditor;
using System.Collections.Generic;
using UnityEngine;

public class bl_PlayerAnimations : bl_PlayerAnimationsBase
{
    #region Public members
    [Tooltip("Override animation clips in the Animator controller when a specific trigger and value are detected, this allow us to easily change the animations for certain states without need to create new layers or state machines in the animator controller.")]
    [ScriptableDrawer] public bl_PlayerAnimationSettings settings;
    #endregion

    #region Public properties
    public bl_NetworkGun CurrentNetworkGun
    {
        get;
        set;
    }
    public bool useFootSteps { get; set; } = true;
    public float VelocityMagnitude
    {
        get;
        set;
    }
    #endregion

    #region Private members
    private float reloadSpeed = 1;
    private PlayerState lastBodyState = PlayerState.Idle;
    private bl_Footstep footstep;
    private float deltaTime = 0.02f;
    private Transform m_Transform;
    private bool HitType = false;
    private GunType cacheWeaponType = GunType.Machinegun;
    private float vertical, horizontal;
    private Transform PlayerRoot;
    private float movementSpeed;
    private float lastYaw;
    private float accumYaw;          // signed degrees since last consumed step
    private float refractoryUntil;
    private float lastRootYaw;
    private float modelYawOffset;       // applied to model.localRotation.y in degrees (counter-rotation)
    private bool releasing;
    private float releaseStart;
    private float releaseTarget;
    private float releaseT;
    private float smoothedAngSpeed;
    bool wasIdle;
    float armAt;
    private Quaternion initialLocalRot;
    bool wasUnderSpeed, firedFromPending;
    float settleStartTime, pendingYaw, yawAtLastFire, lastTIPTrigger;
    private static readonly int HashTurnLeft = Animator.StringToHash("TurnLeft");
    private static readonly int HashTurnRight = Animator.StringToHash("TurnRight");
    private AnimatorOverrideController overrideController;
    private List<KeyValuePair<AnimationClip, AnimationClip>> overrideClips;
    private bl_PlayerAnimationSettings.OverrideAnimationClips weaponTypeOverride;
    private Dictionary<AnimationClip, AnimationClip> defaultWeaponTyperOverrides;
    const int YAW_STEPS = 8; // 360 / stepAngle (45� -> 8)
    private byte turnStepIdx; // 0..YAW_STEPS-1
    private byte _prevTurnStepIdx;
    private bool useTurnInPlace;
    private bool pauseTIP = false;
    private static readonly Dictionary<RuntimeAnimatorController, RuntimeAnimatorController> cachedRACs = new();

    private int _bodyStateHash;
    private int _verticalHash;
    private int _horizontalHash;
    private int _speedHash;
    private int _isGroundHash;
    private int _upperStateHash;
    private int _moveHash;
    private int _gunTypeHash;
    private int _doubleJumpHash;
    private int _hitTypeHash;
    private int _hitHash;
    private int _flashedHash;
    private int _reloadSpeedHash;

    private static readonly int _slideStateHash = Animator.StringToHash("Slide");
    private static readonly int _emptyUpperStateHash = Animator.StringToHash("EmptyUpper");
    private static readonly int _glidingStateHash = Animator.StringToHash("gliding-1");
    private static readonly int _equipStateHash = Animator.StringToHash("Equip");
    private static readonly int _quickFireKnifeStateHash = Animator.StringToHash("QuickFireKnife");
    private static readonly int _quickFireGrenadeStateHash = Animator.StringToHash("QuickFireGrenade");
    private static readonly int _fireKnifeHash = Animator.StringToHash("FireKnife");
    private static readonly int _rifleMeleeAttackHash = Animator.StringToHash("RifleMeleeAttack");
    private static readonly int _rifleFireHash = Animator.StringToHash("RifleFire");
    private static readonly int _pistolMeleeAttackHash = Animator.StringToHash("PistolMeleeAttack");
    private static readonly int _pistolFireHash = Animator.StringToHash("PistolFire");
    private static readonly int _launcherFireHash = Animator.StringToHash("LauncherFire");
    private static readonly int _shotgunMeleeAttackHash = Animator.StringToHash("ShotgunMeleeAttack");
    private static readonly int _shotgunFireHash = Animator.StringToHash("ShotgunFire");
    private static readonly int _rightHitHash = Animator.StringToHash("Right Hit");
    private static readonly int _leftHitHash = Animator.StringToHash("Left Hit");

    private int lastFPState = -1;

    // Ground state detection for animation
    private bool lastIsGrounded = true;
    private bool animatorIsGrounded = true;
    private float groundStateChangeTime = 0;
    private bool groundChangeFromPlayerAction = false;
    private const float GROUND_STATE_ANIMATION_DELAY = 0.15f; // Delay before updating animator when falling naturally (stairs)
    #endregion

    /// <summary>
    /// 
    /// </summary>
    protected override void Awake()
    {
        base.Awake();

        FetchHashes();
    }

    /// <summary>
    /// Since the script could be disabled at start, we call this from an external script
    /// </summary>
    public override void Initialize()
    {
        if (_playerReferences == null) _playerReferences = transform.GetComponentInParent<bl_PlayerReferences>(true);
        if (_playerReferences == null) return;

        m_Transform = transform;
        PlayerRoot = _playerReferences.transform;
        initialLocalRot = m_Transform.localRotation;
        lastRootYaw = PlayerRoot.localEulerAngles.y;

        if (useFootSteps)
        {
            footstep = _playerReferences.firstPersonController.GetFootStep();
        }

        useTurnInPlace = bl_GameData.CoreSettings.useTurnInPlace;
    }

    /// <summary>
    /// 
    /// </summary>
    protected override void OnEnable()
    {
        base.OnEnable();
        bl_AnimatorReloadEvent.OnTPReload += OnTPReload;
    }

    /// <summary>
    /// 
    /// </summary>
    protected override void OnDisable()
    {
        base.OnDisable();
        bl_AnimatorReloadEvent.OnTPReload -= OnTPReload;
    }

    /// <summary>
    /// 
    /// </summary>
    void FetchHashes()
    {
        _bodyStateHash = Animator.StringToHash("BodyState");
        _verticalHash = Animator.StringToHash("Vertical");
        _horizontalHash = Animator.StringToHash("Horizontal");
        _speedHash = Animator.StringToHash("Speed");
        _isGroundHash = Animator.StringToHash("isGround");
        _upperStateHash = Animator.StringToHash("UpperState");
        _moveHash = Animator.StringToHash("Move");
        _gunTypeHash = Animator.StringToHash("GunType");
        _doubleJumpHash = Animator.StringToHash("Double Jump");
        _hitTypeHash = Animator.StringToHash("HitType");
        _hitHash = Animator.StringToHash("Hit");
        _flashedHash = Animator.StringToHash("Flashed");
        _reloadSpeedHash = 2036790888; // ReloadSpeed

        /* if (cachedRACs.ContainsKey(Animator.runtimeAnimatorController))
         {
             // to avoid create a new instance of the same animator controller for every player
             // we will use a cached copy of it.
             Animator.runtimeAnimatorController = cachedRACs[Animator.runtimeAnimatorController];
         }
         else
         {*/
        // create a instance copy of the animator controller to avoid override the original one.
        RuntimeAnimatorController copy = Instantiate(Animator.runtimeAnimatorController);
        // cachedRACs.Add(Animator.runtimeAnimatorController, copy);
        Animator.runtimeAnimatorController = copy;
        //  }
    }

    /// <summary>
    /// 
    /// </summary>
    void OnTPReload(bool enter, Animator theAnimator, AnimatorStateInfo stateInfo)
    {
        if (theAnimator != Animator || CurrentNetworkGun == null || CurrentNetworkGun.LocalGun == null) return;

        float duration = CurrentNetworkGun.LocalGun != null ? CurrentNetworkGun.LocalGun.GetReloadTime() : CurrentNetworkGun.Info.ReloadTime;
        reloadSpeed = enter ? (stateInfo.length / duration) : 1;
        Animator.SetFloat(_reloadSpeedHash, reloadSpeed);
    }

    /// <summary>
    /// 
    /// </summary>
    public override void OnUpdate()
    {
        deltaTime = Time.deltaTime;
        ControllerInfo();
        Animate();
        UpperControl();
        UpdateFootstep();
        DropPlayerAngle();
    }

    public override void OnLateUpdate()
    {
        LateUpdateTurnInPlace();
    }

    /// <summary>
    /// 
    /// </summary>
    void ControllerInfo()
    {
        if (BodyState == PlayerState.InVehicle)
        {
            Velocity = Vector3.zero;
            movementSpeed = vertical = horizontal = 0;
            return;
        }

        if (PlayerRoot != null)
            LocalVelocity = PlayerRoot.InverseTransformDirection(Velocity);

        VelocityMagnitude = Velocity.magnitude;
        float lerp = deltaTime * settings.blendSmoothness;

        vertical = Mathf.MoveTowards(vertical, LocalVelocity.z, lerp);
        horizontal = Mathf.MoveTowards(horizontal, LocalVelocity.x, lerp);
        movementSpeed = Mathf.MoveTowards(movementSpeed, VelocityMagnitude, lerp);

        UpdateTurnInPlace(deltaTime, movementSpeed);
    }

    public override void UpdateTIPFromFPV()
    {
        if (BodyState == PlayerState.InVehicle)
        {
            Velocity = Vector3.zero;
            movementSpeed = vertical = horizontal = 0;
            return;
        }

        deltaTime = Time.deltaTime;
        if (PlayerRoot == null)
        {
            PlayerRoot = PlayerReferences.transform;
            m_Transform = transform;
            initialLocalRot = Quaternion.identity;
            lastRootYaw = PlayerRoot.localEulerAngles.y;
            useTurnInPlace = bl_GameData.CoreSettings.useTurnInPlace;
        }

        float lerp = deltaTime * settings.blendSmoothness;
        movementSpeed = Mathf.MoveTowards(movementSpeed, _playerReferences.firstPersonController.VelocityMagnitude, lerp);
        UpdateTurnInPlace(deltaTime, movementSpeed, true);

        if (Time.frameCount % 2 == 0) LateUpdateTurnInPlace(true);
    }

    /// <summary>
    /// Updates the turn-in-place behavior for the player based on the current movement state and angular velocity.
    /// </summary>
    /// <remarks>This method handles the accumulation of yaw rotation while the player is idle and triggers
    /// turn-in-place animations when specific angular thresholds are met. It ensures that turns are triggered only when
    /// the player is stationary and within defined angular speed limits. The method also manages rearming and
    /// refractory periods to prevent multiple triggers in quick succession.</remarks>
    /// <param name="dt">The time delta, in seconds, since the last update. Used to calculate angular speed.</param>
    /// <param name="currentSpeed">The current movement speed of the player. Determines whether the player is idle.</param>
    private void UpdateTurnInPlace(float dt, float currentSpeed, bool fromFPV = false)
    {
        if (!fromFPV && !_playerReferences.photonView.IsMine) return;
        if (!IsTurnInPlaceActive()) return;
        // read root yaw delta & keep reference fresh every frame
        float nowYaw = PlayerRoot.localEulerAngles.y;
        float dYaw = Mathf.DeltaAngle(lastYaw, nowYaw);
        lastYaw = nowYaw;

        bool nowIdle = currentSpeed <= settings.idleSpeedThreshold && Time.time >= settings.idleGraceSeconds;
        if (nowIdle && !wasIdle)
        {
            // entering idle: flush all backlog and arm after a short delay
            accumYaw = 0f;
            pendingYaw = 0f;
            firedFromPending = false;
            wasUnderSpeed = false;
            refractoryUntil = 0f;
            yawAtLastFire = 0f;
            smoothedAngSpeed = 0f;
            armAt = Time.time + 0.12f;
        }

        // do NOT accumulate or fire while moving or during the arm delay
        if (!nowIdle || Time.time < armAt)
        {
            wasIdle = nowIdle;
            return;
        }
        wasIdle = true;

        // accumulate signed yaw (bounded just to avoid runaway)
        accumYaw = Mathf.Clamp(accumYaw + dYaw, -720f, 720f);

        // angular speed gating
        float angSpeed = Mathf.Abs(dYaw) / Mathf.Max(1e-4f, dt);
        smoothedAngSpeed = Mathf.Lerp(smoothedAngSpeed, angSpeed, 0.2f);

        bool underSpeed = smoothedAngSpeed <= settings.maxAngularSpeedToTrigger;
        bool rotatingAgain = smoothedAngSpeed >= settings.rearmAngularSpeed || currentSpeed > settings.idleSpeedThreshold;

        // entering "stopped" window
        if (underSpeed && !wasUnderSpeed)
        {
            settleStartTime = Time.time;
            pendingYaw = accumYaw;   // backlog built before stopping
            firedFromPending = false;
        }

        // leaving "stopped" window (re-arm)
        if (rotatingAgain)
        {
            wasUnderSpeed = false;
            firedFromPending = false;
            return;
        }

        if (underSpeed)
        {
            wasUnderSpeed = true;
            bool settled = (Time.time - settleStartTime) >= settings.settleSeconds;
            if (!settled || Time.time < refractoryUntil) return;

            if (!firedFromPending)
            {
                float src = (Mathf.Abs(pendingYaw) >= settings.stepAngle) ? pendingYaw : accumYaw;
                if (Mathf.Abs(src) >= settings.stepAngle)
                {
                    bool right = src > 0f;
                    TriggerTurn(right, fromFPV);

                    refractoryUntil = Time.time + settings.refractorySeconds;
                    firedFromPending = true;

                    // consume one step and baseline for subsequent slow steps
                    float s = Mathf.Sign(src);
                    accumYaw -= s * settings.stepAngle;
                    yawAtLastFire = accumYaw;
                    pendingYaw = 0f; // dump leftover backlog so we don't multi-fire after a fast spin
                }
                return;
            }

            // require NEW yaw since last fire to reach a step.
            float deltaSinceFire = accumYaw - yawAtLastFire;
            if (Mathf.Abs(deltaSinceFire) >= settings.stepAngle)
            {
                bool right = deltaSinceFire > 0f;
                TriggerTurn(right);

                refractoryUntil = Time.time + settings.refractorySeconds;

                float s = Mathf.Sign(deltaSinceFire);
                accumYaw -= s * settings.stepAngle;       // keep bounded
                yawAtLastFire = accumYaw;        // baseline moves forward
            }
        }
    }

    private void LateUpdateTurnInPlace(bool fromFPV = false)
    {
        if (!_playerReferences.photonView.IsMine || !IsTurnInPlaceActive()) return;
        if (PlayerRoot == null || BodyState == PlayerState.InVehicle) return;

        float nowYaw = PlayerRoot.localEulerAngles.y;
        float dYaw = Mathf.DeltaAngle(lastRootYaw, nowYaw);
        lastRootYaw = nowYaw;

        bool idle = movementSpeed <= settings.idleSpeedThreshold && Time.time >= settings.idleGraceSeconds;
        bool inTurnState = IsInTurnState(fromFPV);

        if (!idle && !inTurnState && !releasing)
        {
            // moving: smoothly return model to neutral
            modelYawOffset = Mathf.MoveTowards(modelYawOffset, 0f, 360f * deltaTime);
            ApplyLocalYaw();
            return;
        }

        if (idle && !releasing && !inTurnState)
        {
            modelYawOffset = Mathf.Clamp(modelYawOffset - dYaw, -settings.maxHoldDegrees, settings.maxHoldDegrees);
            ApplyLocalYaw();
            return;
        }

        if (releasing || inTurnState)
        {
            if (!releasing)
            {
                // Auto-schedule: move modelYawOffset toward zero by one step
                ScheduleReleaseTowardZero();
            }

            releaseT += deltaTime / Mathf.Max(0.01f, settings.turnClipDuration);
            modelYawOffset = Mathf.Lerp(releaseStart, releaseTarget, Mathf.SmoothStep(0f, 1f, releaseT));
            ApplyLocalYaw();

            if (releaseT >= 1f - 1e-4f)
            {
                releasing = false;
                modelYawOffset = releaseTarget;
                ApplyLocalYaw();
            }
        }
    }

    void Animate()
    {
        if (Animator == null)
            return;

        CheckPlayerStates();
        UpdateGroundStateForAnimator();

        Animator.SetInteger(_bodyStateHash, (int)BodyState);
        Animator.SetFloat(_verticalHash, vertical);
        Animator.SetFloat(_horizontalHash, horizontal);
        Animator.SetFloat(_speedHash, movementSpeed);
        Animator.SetBool(_isGroundHash, animatorIsGrounded);
    }

    /// <summary>
    /// Update the ground state for the animator with delay logic to prevent jitter on stairs
    /// </summary>
    void UpdateGroundStateForAnimator()
    {
        // Detect ground state change
        if (IsGrounded != lastIsGrounded)
        {
            groundStateChangeTime = Time.time;
            lastIsGrounded = IsGrounded;

            // Determine if this change is from a player action (jumping)
            // Player is jumping if: was grounded -> now not grounded AND in jumping state
            groundChangeFromPlayerAction = !IsGrounded && (BodyState == PlayerState.Jumping);

            // If player is landing (not grounded -> grounded), always update immediately
            if (IsGrounded)
            {
                animatorIsGrounded = true;
            }
        }

        // If currently not grounded in the controller
        if (!IsGrounded)
        {
            // If the change was from a player action (jump), update animator immediately
            if (groundChangeFromPlayerAction)
            {
                animatorIsGrounded = false;
            }
            // Otherwise (natural fall like stairs), wait for the delay period
            else if (Time.time - groundStateChangeTime >= GROUND_STATE_ANIMATION_DELAY)
            {
                animatorIsGrounded = false;
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void CheckPlayerStates()
    {
        if (BodyState != lastBodyState)
        {
            if (lastBodyState == PlayerState.Sliding && BodyState != PlayerState.Sliding)
            {
                Animator.CrossFade(_moveHash, 0.2f, 0);
            }
            if (BodyState == PlayerState.Sliding)
            {
                Animator.Play(_slideStateHash, 0, 0);
            }
            else if (OnEnterPlayerState(PlayerState.Dropping))
            {
                Animator.Play(_emptyUpperStateHash, 1, 0);
            }
            else if (OnEnterPlayerState(PlayerState.Gliding))
            {
                Animator.Play(_emptyUpperStateHash, 1, 0);
                Animator.CrossFade(_glidingStateHash, 0.33f, 0);
            }
            else if (OnEnterPlayerState(PlayerState.InVehicle))
            {
                SnapToRootYaw();
            }

            if (OnExitPlayerState(PlayerState.Dropping))
            {
                m_Transform.localRotation = Quaternion.identity;
            }

            lastBodyState = BodyState;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public bool OnEnterPlayerState(PlayerState playerState)
    {
        if (BodyState == playerState && lastBodyState != playerState)
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// 
    /// </summary>
    public bool OnExitPlayerState(PlayerState playerState)
    {
        if (lastBodyState == playerState && BodyState != playerState)
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// 
    /// </summary>
    void UpperControl()
    {
        int _fpState = (int)FPState;
        if (_fpState == 9) { _fpState = 1; }

        if (_fpState != lastFPState)
        {
            Animator.SetInteger(_upperStateHash, _fpState);
            lastFPState = _fpState;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void DropPlayerAngle()
    {
        if (BodyState != PlayerState.Dropping) return;

        Vector3 pangle = m_Transform.localEulerAngles;
        float tilt = settings.dropTiltAngleCurve.Evaluate(Mathf.Clamp01(VelocityMagnitude / (PlayerReferences.firstPersonController.GetSpeedOnState(PlayerState.Dropping) - 10)));
        pangle.x = Mathf.Lerp(0, 70, tilt);
        m_Transform.localRotation = Quaternion.Slerp(m_Transform.localRotation, Quaternion.Euler(pangle), deltaTime * 4);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="command"></param>
    public override void CustomCommand(PlayerAnimationCommands command, string arg = "", bool callFromLocal = true)
    {
        // if it is called locally, sync the command to the network and return.
        if (callFromLocal && bl_MFPS.IsMultiplayerScene)
        {
            PlayerReferences.playerNetwork.ReplicatePlayerAnimationCommand(command, arg);
            return;
        }

        if (!gameObject.activeSelf)
        {
            OnCustomCommand?.Invoke(command, arg);
            return;
        }

        // Hashes are fetched in Awake
        switch (command)
        {
            case PlayerAnimationCommands.PlayDoubleJump:
                Animator.CrossFade(_doubleJumpHash, 0.14f, 0);
                break;
            case PlayerAnimationCommands.OnFlashbang:
            case PlayerAnimationCommands.OnUnFlashed:
                Animator.SetBool(_flashedHash, command == PlayerAnimationCommands.OnFlashbang);
                break;
            case PlayerAnimationCommands.QuickMelee:
                int knifeGunId = int.Parse(arg);
                PlayerReferences.playerNetwork.TemporarySwitchTPWeapon(knifeGunId, 1000);
                Animator.Play(_quickFireKnifeStateHash, 1, 0);
                break;
            case PlayerAnimationCommands.QuickGrenade:
                int grenadeGunId = int.Parse(arg);
                PlayerReferences.playerNetwork.TemporarySwitchTPWeapon(grenadeGunId, 1000);
                Animator.Play(_quickFireGrenadeStateHash, 1, 0);
                break;
        }

        OnCustomCommand?.Invoke(command, arg);
    }

    /// <summary>
    /// 
    /// </summary>
    public override void BlockWeapons(int blockState)
    {
        if (PlayerReferences == null || PlayerReferences.playerIK == null) return;

        bool baredHands = blockState == 1;

        // Do not control the arms with IK when the player is not using weapons.
        PlayerReferences.playerIK.ControlArmsWithIK = !baredHands;

        if (blockState != 2)
        {
            // -1 is the ID for play the bared arms animations in the player animator controller.
            int id = baredHands ? -1 : (int)cacheWeaponType;
            Animator.SetInteger(_gunTypeHash, id);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public override void OnGetHit()
    {
        int r = Random.Range(0, 2);
        int hit = (r == 1) ? _rightHitHash : _leftHitHash;
        Animator.Play(hit, 2, 0);
    }

    private void TriggerTurn(bool right, bool fromFPV = false)
    {
        if (!fromFPV)
        {
            // One-shot triggers; animator handles full, un-interrupted play
            if (right) Animator.SetTrigger(HashTurnRight);
            else Animator.SetTrigger(HashTurnLeft);
        }
        ScheduleReleaseTowardZero();

        int delta = right ? +1 : -1;
        turnStepIdx = (byte)((turnStepIdx + delta + YAW_STEPS) % YAW_STEPS);
        lastTIPTrigger = Time.time;
    }

    private void ScheduleReleaseTowardZero()
    {
        releasing = true;
        releaseT = 0f;
        releaseStart = modelYawOffset;

        // Move toward zero by exactly stepAngle (but don't overshoot)
        float remaining = Mathf.Abs(modelYawOffset);
        float consume = Mathf.Min(settings.stepAngle, remaining);
        float sign = (modelYawOffset > 0f) ? 1f : -1f;
        releaseTarget = releaseStart - (sign * consume); // toward zero
    }

    private bool IsInTurnState(bool fromFPV)
    {
        if (fromFPV)
        {
            return lastTIPTrigger - Time.time > 0.5f;
        }
        var st = Animator.GetCurrentAnimatorStateInfo(0);
        return st.IsName("TurnLeft") || st.IsName("TurnRight");
    }

    private void ApplyLocalYaw()
    {
        // Apply offset relative to the model's initial local rotation
        m_Transform.localRotation = Quaternion.Euler(0f, modelYawOffset, 0f) * initialLocalRot;
    }

    /// <summary>
    /// Resets the yaw rotation of the model to align with the root object's current yaw rotation.
    /// </summary>
    /// <remarks>This method stops any ongoing release process, resets the model's yaw offset, and re-applies
    /// the initial local rotation.  It also clears any pending animation triggers for turning actions to ensure a
    /// consistent state.</remarks>
    public void SnapToRootYaw()
    {
        // stop any ongoing release
        releasing = false;
        releaseT = 0f;

        // zero visual offset and rebase yaw reference
        modelYawOffset = 0f;
        lastRootYaw = PlayerRoot.localEulerAngles.y;

        // hard-apply neutral local rotation
        m_Transform.localRotation = initialLocalRot;

        // optional: clear pending triggers in case caller fired one earlier
        Animator.ResetTrigger("TurnLeft");
        Animator.ResetTrigger("TurnRight");
    }

    /// <summary>
    /// 
    /// </summary>
    private void UpdateFootstep()
    {
        if (!useFootSteps || BodyState == PlayerState.InVehicle || footstep == null) return;
        if (VelocityMagnitude < 0.3f) return;

        bool isClimbing = (BodyState == PlayerState.Climbing);
        if ((!IsGrounded && !isClimbing) || BodyState == PlayerState.Sliding)
            return;

        if (BodyState == PlayerState.Stealth)
        {
            if (footstep.settings != null) footstep.SetVolumeMuliplier(footstep.settings.stealthModeVolumeMultiplier);
        }
        else
        {
            footstep.SetVolumeMuliplier(1f);
        }

        footstep.UpdateStep(movementSpeed);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="typ"></param>
    public override void PlayFireAnimation(GunType typ, FireReplicationFlag flag = FireReplicationFlag.None)
    {
        // Check if the network weapon is using custom animations
        if (CurrentNetworkGun != null && CurrentNetworkGun.useCustomPlayerAnimations)
        {
            if (!string.IsNullOrEmpty(CurrentNetworkGun.customFireAnimationName))
            {
                string anim = CurrentNetworkGun.customFireAnimationName;
                if (CurrentNetworkGun.customFireAnimationsCount > 1)
                {
                    int r = Random.Range(0, CurrentNetworkGun.customFireAnimationsCount);
                    if (r > 0) anim = $"{CurrentNetworkGun.customFireAnimationName} {r}";
                }
                Animator.Play(anim, 1, 0);
                return;
            }
        }

        switch (typ)
        {
            case GunType.Melee:
                Animator.Play(_fireKnifeHash, 1, 0);
                break;
            case GunType.Machinegun:
                if (flag.IsByteEnumFlagPresent(FireReplicationFlag.BashAttack)) Animator.Play(_rifleMeleeAttackHash, 1, 0);
                else Animator.Play(_rifleFireHash, 1, 0);
                break;
            case GunType.Pistol:
                if (flag.IsByteEnumFlagPresent(FireReplicationFlag.BashAttack)) Animator.Play(_pistolMeleeAttackHash, 1, 0);
                else Animator.Play(_pistolFireHash, 1, 0);
                break;
            case GunType.Launcher:
                Animator.Play(_launcherFireHash, 1, 0);
                break;
            case GunType.Shotgun:
                if (flag.IsByteEnumFlagPresent(FireReplicationFlag.BashAttack)) Animator.Play(_shotgunMeleeAttackHash, 1, 0);
                else Animator.Play(_shotgunFireHash, 1, 0);
                break;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void HitPlayer()
    {
        if (Animator != null)
        {
            HitType = !HitType;
            int ht = (HitType) ? 1 : 0;
            Animator.SetInteger(_hitTypeHash, ht);
            Animator.SetTrigger(_hitHash);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public override void UpdateAnimatorParameters()
    {
        ControllerInfo();
        Animate();
        UpperControl();
    }

    /// <summary>
    /// 
    /// </summary>
    public override void SetNetworkGun(GunType weaponType, bl_NetworkGun networkGun)
    {
        if (Animator == null || !Animator.gameObject.activeInHierarchy) return;

        cacheWeaponType = weaponType;
        CurrentNetworkGun = networkGun;

        // Hashes are fetched in Awake

        if (networkGun != null && Animator != null)
        {
            Animator.SetInteger(_gunTypeHash, networkGun.GetUpperStateID());
            Animator.Play(_equipStateHash, 1, 0);
        }

        if (settings != null)
        {
            // check if the current weapon type does need to override default animation clips, e.g the base sprint animation.
            if (settings.CheckOverrides(bl_PlayerAnimationSettings.OverrideAnimationClips.TriggerReason.WeaponType, networkGun.GetUpperStateID(), out var replacement))
            {
                weaponTypeOverride = replacement;
                ReplaceAnimationClip(replacement.DefaultClip, replacement.OverrideClip);
            }
            else if (settings.CheckOverrides(bl_PlayerAnimationSettings.OverrideAnimationClips.TriggerReason.WeaponId, networkGun.GetWeaponID, out var replacement2))
            {
                weaponTypeOverride = replacement2;
                ReplaceAnimationClip(replacement2.DefaultClip, replacement2.OverrideClip);
            }
            else
            {
                if (weaponTypeOverride != null)
                {
                    AnimationClip defaultClip = null;
                    if (defaultWeaponTyperOverrides != null && defaultWeaponTyperOverrides.ContainsKey(weaponTypeOverride.DefaultClip))
                    {
                        defaultClip = defaultWeaponTyperOverrides[weaponTypeOverride.DefaultClip];
                    }
                    // back to the default animation clip
                    ReplaceAnimationClip(weaponTypeOverride.DefaultClip, defaultClip);
                    weaponTypeOverride = null;
                }
            }
        }

        if (CurrentNetworkGun == null || CurrentNetworkGun.LocalGun == null)
        {
            reloadSpeed = 1;
        }

        // @TODO: Find the issue that cause Missing Reference exception when this conditional is not present.
        if (this != null)
        {
            // Do not control the arms with IK while the 'Equip' animation is playing.
            PlayerReferences.playerIK.ControlArmsWithIK = false;

            CancelInvoke(nameof(ResetHandsIK));
            Invoke(nameof(ResetHandsIK), 0.3f);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="defaultName"></param>
    /// <param name="newClip"></param>
    public void ReplaceAnimationClip(AnimationClip defaultClip, AnimationClip newClip)
    {
        if (defaultClip == null) return;

        if (overrideController == null)
            overrideController = Animator.runtimeAnimatorController as AnimatorOverrideController;

        if (overrideClips == null)
        {
            overrideClips = new();
            overrideController.GetOverrides(overrideClips);
        }

        for (int i = 0; i < overrideClips.Count; i++)
        {
            var item = overrideClips[i];
            if (item.Key == null || item.Key.name != defaultClip.name) continue;

            defaultWeaponTyperOverrides ??= new();
            if (!defaultWeaponTyperOverrides.ContainsKey(item.Key))
            {
                defaultWeaponTyperOverrides.Add(item.Key, item.Value);
            }

            overrideClips[i] = new KeyValuePair<AnimationClip, AnimationClip>(item.Key, newClip);
            break;
        }

        overrideController.ApplyOverrides(overrideClips);
    }

    public override void HandleRemoteTurnIndex(byte newIdx)
    {
        if (newIdx == _prevTurnStepIdx || BodyState == PlayerState.InVehicle) return;

        int diff = (newIdx - _prevTurnStepIdx + YAW_STEPS) % YAW_STEPS;
        // shortest direction around the ring
        int steps, dir;
        if (diff == 0) return;
        if (diff <= YAW_STEPS / 2) { dir = +1; steps = diff; }
        else { dir = -1; steps = YAW_STEPS - diff; }

        // Queue exactly 'steps' clips in 'dir'
        for (int i = 0; i < steps; i++)
            TriggerTurn(dir > 0); // just play the animation; yaw is already synced by ApplyNetworkModelYaw

        _prevTurnStepIdx = newIdx;
    }

    public override void PauseTurnInPlace(bool value)
    {
        pauseTIP = value;
    }

    /// <summary>
    /// 
    /// </summary>
    void ResetHandsIK() { PlayerReferences.playerIK.ControlArmsWithIK = true; }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public GunType GetCurretWeaponType() { return cacheWeaponType; }

    public override byte GetTurnStepIndex() { return turnStepIdx; }

    private bool IsTurnInPlaceActive()
    {
        return useTurnInPlace && !pauseTIP;
    }

    private bl_PlayerReferences _playerReferences = null;
    private bl_PlayerReferences PlayerReferences
    {
        get
        {
            if (_playerReferences == null) _playerReferences = transform.GetComponentInParent<bl_PlayerReferences>();
            return _playerReferences;
        }
    }
}