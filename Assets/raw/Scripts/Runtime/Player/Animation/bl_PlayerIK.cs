using MFPS.Internal;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// MFPS Default IK implementation
/// In order to use your own IK control system, simply create a new script
/// and inherited from <see cref="bl_PlayerIKBase"/> and override the methods you want to change."/>
/// </summary>
[ExecuteInEditMode]
public class bl_PlayerIK : bl_PlayerIKBase
{
    #region Public members
    [Header("HEAD")]
    [Range(0, 1)] public float Weight;
    [Range(0, 1)] public float Body;
    [Range(0, 1)] public float Head;
    [Range(1, 20)] public float Lerp = 8;
    [Range(1, 20)] public float AimPositionLerp = 10;
    [Range(1, 10)] public float IKWeightLerp = 6;

    [Header("ARMS")]
    [FormerlySerializedAs("HandOffset"), Tooltip("Adjust the right hand IK rotation.")]
    public Vector3 HandRotationAdjust;
    [FormerlySerializedAs("AimSightPosition"), Tooltip("The weapon aim position that will be used for any weapon that is not defined in the GlobalWeaponAimPosition list.")]
    public Vector3 GlobalWeaponAimPosition = new(0.02f, 0.19f, 0.02f);
    [Tooltip("The right hand position that will be used for any weapon that is not defined in the GlobalWeaponAimPosition list.")]
    public Vector3 globalRightHandPosition;
    [Tooltip("The right arm pole hint position")]
    public Vector3 rightPolePosition;
    [Tooltip("If you want to define a custom aim position for a specific weapon, you add the weapon to this list and define it's aim position.")]
    public List<WeaponAimPosition> customWeaponAimPositions;

    [Header("FEETS")]
    [LovattoToogle] public bool useFootPlacement = true;
    public LayerMask FootLayers;
    [Range(0.1f, 1)] public float FootHeight = 0.43f;
    [Range(-0.5f, 0.5f)] public float TerrainOffset = 0.13f;
    public Vector3 leftKneeTarget, rightKneeTarget;
    [LovattoToogle] public bool debugFootGizmos = true;

    public bool IsCustomHeadTarget { get; set; } = false;
    #endregion

    #region Private members
    private float legLeftWeightMul = 1;
    private float legRightWeightMul = 1;
    private float armLeftIKWeightMul = 1;
    private float armRightIKWeightMul = 1;
    private float rightArmBlend = 0;
    private Transform RightFoot;
    private Transform LeftFoot;
    private float lFootIKWeight, rFootIKWeight = 0;
    private Animator animator;
    private Vector3 targetPosition;
    private float armsIKWeight = 1;
    private float rightArmIKPositionWeight = 0;
    private Transform m_headTransform, rightUpperArm;
    private bl_PlayerAnimationsBase PlayerAnimation;
    private float deltaTime = 0;
    private Transform m_headTarget;
    private Transform m_leftArmRef;
    private Transform oldLHTarget;
    private Coroutine weightBlendCoroutine;
    private int lastGunID = -1;
    private WeaponAimPosition customPositions;

    //editor only
    [HideInInspector] public int editor_previewMode = 0;
    [HideInInspector] public float editor_weight = 1;
    [HideInInspector, NonSerialized] public int editor_preview_gun_id = -1;
    [HideInInspector, NonSerialized] public bool editor_is_aiming = false;
    [HideInInspector, NonSerialized] public bool editor_preview_global = false;
    #endregion

    #region Unity Methods
    /// <summary>
    /// 
    /// </summary>
    protected override void Awake()
    {
        base.Awake();

        Init();
    }

    /// <summary>
    /// 
    /// </summary>
    protected override void OnEnable()
    {
        base.OnEnable();
        bl_AnimatorIKBlendEvent.onMotionIKBlendModifier += OnIKMotionWeightModified;
    }

    /// <summary>
    /// 
    /// </summary>
    protected override void OnDisable()
    {
        base.OnDisable();
        bl_AnimatorIKBlendEvent.onMotionIKBlendModifier -= OnIKMotionWeightModified;
    }

    /// <summary>
    /// Called from the Animator after the animation update
    /// </summary>
    void OnAnimatorIK(int layer)
    {
        if (!EnableIK) return;

        onAnimatorIK?.Invoke(layer);
        if (animator == null)
            return;

        deltaTime = Time.deltaTime;

        if (layer == 0)
        {
            HeadIK();
            BottomBody();
        }
        else if (layer == 1) UpperBody();
    }
    #endregion

    /// <summary>
    /// 
    /// </summary>
    public override void Init()
    {
        if (PlayerReferences == null) return;

        PlayerAnimation = PlayerReferences.playerAnimations;
        animator = PlayerReferences.PlayerAnimator;
        m_leftArmRef = PlayerReferences.leftArmTarget.transform;

        if (HeadLookTarget == null) HeadLookTarget = PlayerReferences.headLookTarget;

        m_headTransform = animator.GetBoneTransform(HumanBodyBones.Head);
        rightUpperArm = animator.GetBoneTransform(HumanBodyBones.RightUpperArm);
        if (useFootPlacement)
        {
            LeftFoot = animator.GetBoneTransform(HumanBodyBones.LeftFoot);
            RightFoot = animator.GetBoneTransform(HumanBodyBones.RightFoot);
        }

        editor_previewMode = 0;
        editor_weight = 0;
    }

    /// <summary>
    /// 
    /// </summary>
    void OnIKMotionWeightModified(bool enter, Animator sourceAnim, bl_AnimatorIKBlendEvent.Modifier modifier)
    {
        if (sourceAnim != animator) return;

        float smoothTime = enter ? modifier.SmoothTime : modifier.OutSmoothTime;

        if (smoothTime <= 0)
        {
            if (modifier.AffectLeftLeg) legLeftWeightMul = modifier.Weight;
            if (modifier.AffectLeftArm) armLeftIKWeightMul = modifier.Weight;
            if (modifier.AffectRightLeg) legRightWeightMul = modifier.Weight;
            if (modifier.AffectRightArm) armRightIKWeightMul = modifier.Weight;
            return;
        }

        if (weightBlendCoroutine != null) StopCoroutine(weightBlendCoroutine);
        weightBlendCoroutine = StartCoroutine(LerpWeight());
        IEnumerator LerpWeight()
        {
            float d = 0;
            while (d < 1)
            {
                d += Time.deltaTime / smoothTime;
                if (modifier.AffectLeftLeg) legLeftWeightMul = Mathf.Lerp(legLeftWeightMul, modifier.Weight, d);
                if (modifier.AffectLeftArm) armLeftIKWeightMul = Mathf.Lerp(armLeftIKWeightMul, modifier.Weight, d);
                if (modifier.AffectRightLeg) legRightWeightMul = Mathf.Lerp(legRightWeightMul, modifier.Weight, d);
                if (modifier.AffectRightArm) armRightIKWeightMul = Mathf.Lerp(armRightIKWeightMul, modifier.Weight, d);
                yield return null;
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void HeadIK()
    {
        if (HeadLookTarget == null) return;

        animator.SetLookAtWeight(Weight * armsIKWeight, Body, Head, 1, 0.4f);
        targetPosition = Vector3.Slerp(targetPosition, HeadLookTarget.position, deltaTime * AimPositionLerp);
        animator.SetLookAtPosition(targetPosition);
    }

    /// <summary>
    /// Control the legs IK
    /// </summary>
    void BottomBody()
    {
        if (useFootPlacement && (PlayerAnimation.IsGrounded && PlayerAnimation.LocalVelocity.sqrMagnitude <= 0.01f) || editor_previewMode == 1)
        {
            LegsIK();
        }
        else
        {
            animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 0);
            animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 0);
            animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 0);
            animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, 0);
        }
    }

    /// <summary>
    /// Control the arms and head IK bones
    /// </summary>
    void UpperBody()
    {
        //If there's another script handling the arms IK
        if (CustomArmsIKHandler != null)
        {
            CustomArmsIKHandler.OnUpdate();
        }
        else if (GetLeftHandTarget(out _) && ControlArmsWithIK)
        {
            ArmsIK();
        }
        else
        {
            ResetWeightIK();
        }
    }

    /// <summary>
    /// Control left and right arms
    /// </summary>
    void ArmsIK()
    {
        bl_NetworkGun networkGun = null;
        if (Application.isPlaying)
        {
            networkGun = PlayerSync.GetCurrentNetworkWeapon();
            // if the weapon is not visible, don't control the arms with IK and let it play the default animation.
            if (networkGun == null || !networkGun.IsVisible())
            {
                return;
            }
        }

        //control the arms only when the player is aiming or firing
        float weightTarget = IsIKPlayerState ? 1 : 0f;
        armsIKWeight = Mathf.Lerp(armsIKWeight, weightTarget, deltaTime * IKWeightLerp);

        animator.SetIKRotation(AvatarIKGoal.LeftHand, m_leftArmRef.rotation);
        animator.SetIKPosition(AvatarIKGoal.LeftHand, m_leftArmRef.position);

        if (armsIKWeight > 0)
        {
            // Make the right arm aim where the player is looking at
            // Get the look at direction
            Quaternion lookAt = Quaternion.LookRotation(targetPosition - rightUpperArm.position);
            lookAt *= Quaternion.Euler(HandRotationAdjust);
            animator.SetIKRotation(AvatarIKGoal.RightHand, lookAt);
        }

        if (networkGun.GetWeaponType() == GunType.Melee || networkGun.GetWeaponType() == GunType.Grenade)
        {
            ResetWeightIK();
            armsIKWeight = 1;
            animator.SetIKRotationWeight(AvatarIKGoal.RightHand, armsIKWeight * armRightIKWeightMul);
            return;
        }

        bool isAiming = (PlayerSync.FPState == PlayerFPState.Aiming || PlayerSync.FPState == PlayerFPState.FireAiming);
        float rpw = isAiming ? 0.8f : 0.5f;
        rightArmIKPositionWeight = Mathf.Lerp(rightArmIKPositionWeight, rpw, deltaTime * IKWeightLerp);
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            if (editor_previewMode == 1)
            {
                rightArmIKPositionWeight = editor_weight;
                isAiming = editor_is_aiming;
            }
        }
#endif
        Vector3 weaponAimPos = GlobalWeaponAimPosition;
        Vector3 rightHandPos = globalRightHandPosition;

        if (PlayerReferences != null)
        {
            // if the player is using a different weapon, get the custom aim position for that weapon
            if (lastGunID != PlayerReferences.playerNetwork.NetworkGunID)
            {
                lastGunID = PlayerReferences.playerNetwork.NetworkGunID;

                customPositions = GetCustomPositionsFor(lastGunID);
            }

#if UNITY_EDITOR
            if (!Application.isPlaying && editor_preview_gun_id != -1)
            {
                if (editor_preview_global)
                {
                    weaponAimPos = GlobalWeaponAimPosition;
                    rightHandPos = globalRightHandPosition;
                }
                else
                {
                    customPositions = GetCustomPositionsFor(editor_preview_gun_id);
                    if (customPositions != null)
                    {
                        weaponAimPos = customPositions.AimPosition;
                        rightHandPos = customPositions.IdlePosition;
                    }
                }
            }
#endif
        }

        if (customPositions != null)
        {
            weaponAimPos = customPositions.AimPosition;
            rightHandPos = customPositions.IdlePosition;
        }

        rightArmBlend = Mathf.Lerp(rightArmBlend, isAiming ? 1 : 0, deltaTime * 10);
        Vector3 relativeAimPosition = Vector3.Lerp(m_headTransform.TransformPoint(rightHandPos), m_headTransform.TransformPoint(weaponAimPos), rightArmBlend);
        animator.SetIKPosition(AvatarIKGoal.RightHand, relativeAimPosition);

        animator.SetIKRotationWeight(AvatarIKGoal.RightHand, armsIKWeight * armRightIKWeightMul);
        animator.SetIKPositionWeight(AvatarIKGoal.RightHand, rightArmIKPositionWeight * armRightIKWeightMul * armsIKWeight);

        animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, armsIKWeight * armLeftIKWeightMul);
        animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, armsIKWeight * armLeftIKWeightMul);

        animator.SetIKHintPositionWeight(AvatarIKHint.RightElbow, armsIKWeight * armRightIKWeightMul);
        Vector3 rpp = PlayerReferences.transform.TransformPoint(rightPolePosition);
        animator.SetIKHintPosition(AvatarIKHint.RightElbow, rpp);
    }

    /// <summary>
    /// Detect the ground and place the foots using IK
    /// </summary>
    void LegsIK()
    {
        var tf = CachedTransform;
        var anim = animator;
        float dt = deltaTime;

        Vector3 upStart = Vector3.up * FootHeight;
        float detectionRange = FootHeight * 2f;

        Vector3 rootPos = tf.position;
        Vector3 rootFwd = tf.forward; // normalized

        float leftOffset = 0f;
        float rightOffset = 0f;

        // Frame-rate independent smoothing factors (k = response speed)
        float Exp01(float k) => 1f - Mathf.Exp(-k * dt);

        // Reusable foot-IK routine
        void ProcessFoot(Transform foot, AvatarIKGoal goal, AvatarIKHint hint, Vector3 kneeLocal, ref float w, ref float outOffset)
        {
            Vector3 start = foot.position + upStart;
            if (Physics.Raycast(start, Vector3.down, out var hit, detectionRange, FootLayers, QueryTriggerInteraction.Ignore))
            {
                // Position
                Vector3 targetPos = foot.position;
                targetPos.y = hit.point.y + TerrainOffset;
                anim.SetIKPosition(goal, targetPos);

                // Rotation (stable even when forward ≈ normal)
                Vector3 flatFwd = Vector3.ProjectOnPlane(rootFwd, hit.normal);
                if (flatFwd.sqrMagnitude < 1e-6f) flatFwd = Vector3.ProjectOnPlane(tf.forward, hit.normal);
                Quaternion targetRot = Quaternion.LookRotation(flatFwd, hit.normal);

                Quaternion curRot = anim.GetIKRotation(goal);
                anim.SetIKRotation(goal, Quaternion.Slerp(curRot, targetRot, Exp01(8f)));

                // Knee hint
                anim.SetIKHintPosition(hint, tf.TransformPoint(kneeLocal));

                // Weights (fast in, slower out handled in miss branch)
                w += (1f - w) * Exp01(10f);

                // Pelvis offset source
                outOffset = hit.point.y - rootPos.y;
            }
            else
            {
                // Fade out when no ground found
                w -= w * Exp01(4f);
                outOffset = 0f;
            }
        }

        // Feet
        ProcessFoot(LeftFoot, AvatarIKGoal.LeftFoot, AvatarIKHint.LeftKnee, leftKneeTarget, ref lFootIKWeight, ref leftOffset);
        ProcessFoot(RightFoot, AvatarIKGoal.RightFoot, AvatarIKHint.RightKnee, rightKneeTarget, ref rFootIKWeight, ref rightOffset);

        // Pelvis (average height; exp smoothing for stability across FPS)
        float tPelvis = Exp01(5f);
        Vector3 pelvis = rootPos;
        pelvis.y += ((leftOffset + rightOffset) * 0.5f) * tPelvis;
        tf.position = pelvis;

        // Final weights (single writes)
        float lw = lFootIKWeight * legLeftWeightMul;
        float rw = rFootIKWeight * legRightWeightMul;

        anim.SetIKPositionWeight(AvatarIKGoal.LeftFoot, lw);
        anim.SetIKRotationWeight(AvatarIKGoal.LeftFoot, lw);
        anim.SetIKHintPositionWeight(AvatarIKHint.LeftKnee, lw);

        anim.SetIKPositionWeight(AvatarIKGoal.RightFoot, rw);
        anim.SetIKRotationWeight(AvatarIKGoal.RightFoot, rw);
        anim.SetIKHintPositionWeight(AvatarIKHint.RightKnee, rw);
    }

    /// <summary>
    /// 
    /// </summary>
    void ResetWeightIK()
    {
        armsIKWeight = 0;
        animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 0.0f);
        animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0.0f);
        animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 0);
        animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 0);
    }

    public override void SetHeadLookTarget(Transform target)
    {
        if (target == null) HeadLookTarget = PlayerReferences.headLookTarget;
        else HeadLookTarget = target;
    }

    /// <summary>
    /// 
    /// </summary>
    public override Transform HeadLookTarget
    {
        get
        {
            if (m_headTarget == null)
            {
                m_headTarget = PlayerReferences.headLookTarget;
                IsCustomHeadTarget = false;
            }
            return m_headTarget;
        }
        set
        {
            if (value == null)
            {
                m_headTarget = PlayerReferences.headLookTarget;
                IsCustomHeadTarget = false;
            }
            else
            {
                m_headTarget = value;
                IsCustomHeadTarget = true;
            }
        }
    }

    /// <summary>
    /// If the player in an state where the arms should be controlled by IK
    /// </summary>
    private bool IsIKPlayerState
    {
        get
        {
            return (PlayerSync.FPState != PlayerFPState.Running && PlayerSync.FPState != PlayerFPState.Reloading);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private bool GetLeftHandTarget(out Transform target)
    {
        target = null;
        if (Application.isPlaying)
        {
            var currentWeapon = PlayerSync.GetCurrentNetworkWeapon();
            if (PlayerSync != null && currentWeapon != null)
            {
                CompareLeftArmTarget(currentWeapon.LeftHandPosition);
                target = currentWeapon.LeftHandPosition;
                if (currentWeapon.GetWeaponType() == GunType.Melee || currentWeapon.GetWeaponType() == GunType.Grenade)
                {
                    return true;
                }
            }
        }
        else//called from an editor script to simulate IK in editor
        {
            if (PlayerSync != null && PlayerReferences.playerAnimations != null && PlayerReferences.EditorSelectedGun)
            {
                if (m_leftArmRef == null) m_leftArmRef = PlayerReferences.leftArmTarget.transform;

                CompareLeftArmTarget(PlayerReferences.EditorSelectedGun.LeftHandPosition);
                target = PlayerReferences.EditorSelectedGun.LeftHandPosition;
            }
        }
        return target != null;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="laTarget"></param>
    private void CompareLeftArmTarget(Transform laTarget)
    {
        if (oldLHTarget != laTarget)
        {
            oldLHTarget = laTarget;
            while (PlayerReferences.leftArmTarget.sourceCount > 0)
                PlayerReferences.leftArmTarget.RemoveSource(0);

            if (PlayerReferences.leftArmTarget != null && oldLHTarget != null)
            {
                PlayerReferences.leftArmTarget.transform.SetPositionAndRotation(oldLHTarget.position, oldLHTarget.rotation);
                PlayerReferences.leftArmTarget.AddSource(new UnityEngine.Animations.ConstraintSource()
                {
                    sourceTransform = oldLHTarget,
                    weight = 1
                });
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="gunId"></param>
    /// <returns></returns>
    public Vector3 GetAimPositionFor(int gunId)
    {
        int index = customWeaponAimPositions.FindIndex(x => x.GunID == gunId);
        return index != -1 ? customWeaponAimPositions[index].AimPosition : GlobalWeaponAimPosition;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="gunId"></param>
    /// <returns></returns>
    public WeaponAimPosition GetCustomPositionsFor(int gunId)
    {
        int index = customWeaponAimPositions.FindIndex(x => x.GunID == gunId);
        return index != -1 ? customWeaponAimPositions[index] : null;

    }

#if UNITY_EDITOR
    /// <summary>
    /// 
    /// </summary>
    private void OnDrawGizmos()
    {
        if (animator == null) { animator = GetComponent<Animator>(); }

        FootGizmos();
    }

    /// <summary>
    /// 
    /// </summary>
    void FootGizmos()
    {
        if (!debugFootGizmos) return;

        if (LeftFoot == null) LeftFoot = animator.GetBoneTransform(HumanBodyBones.LeftFoot);
        if (RightFoot == null) RightFoot = animator.GetBoneTransform(HumanBodyBones.RightFoot);

        Vector3 leftPosition = LeftFoot.position;
        Vector3 rightPosition = RightFoot.position;
        Vector3 upStart = Vector3.up * FootHeight;

        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(leftPosition, 0.1f);
        Gizmos.DrawWireSphere(rightPosition, 0.1f);

        Vector3 lStartLine = leftPosition + upStart;
        Vector3 rStartLine = rightPosition + upStart;
        float distance = FootHeight * 2;
        Vector3 lineDirection = Vector3.down * distance;

        Gizmos.DrawRay(lStartLine, lineDirection);
        Gizmos.DrawRay(rStartLine, lineDirection);

        Transform leftKnee = animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg);
        if (leftKnee != null)
        {
            Vector3 hintPos = transform.TransformPoint(leftKneeTarget);
            UnityEditor.Handles.DrawDottedLine(leftKnee.position, hintPos, 3f);
            Gizmos.DrawWireSphere(hintPos, 0.07f);
        }

        leftKnee = animator.GetBoneTransform(HumanBodyBones.RightLowerLeg);
        if (leftKnee != null)
        {
            Vector3 hintPos = transform.TransformPoint(rightKneeTarget);
            UnityEditor.Handles.DrawDottedLine(leftKnee.position, hintPos, 3f);
            Gizmos.DrawWireSphere(hintPos, 0.07f);
        }
    }
#endif

    [Serializable]
    public class WeaponAimPosition
    {
        [GunID] public int GunID;
        public Vector3 AimPosition;
        public Vector3 IdlePosition;
    }

    private bl_PlayerReferences m_playerReferences;
    private bl_PlayerReferences PlayerReferences
    {
        get
        {
            if (m_playerReferences == null) m_playerReferences = CachedTransform.GetComponentInParent<bl_PlayerReferences>();
            return m_playerReferences;
        }
    }

    private bl_PlayerNetworkBase PlayerSync
    {
        get
        {
            return PlayerReferences != null ? PlayerReferences.playerNetwork : null;
        }
    }
}