using MFPS.Internal;
using MFPS.Runtime.AI;
using MFPSEditor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[ExecuteInEditMode]
public class bl_AIAnimation : bl_AIAnimationBase
{
    #region Public members
    [Header("Head Look")]
    [Range(0, 1)] public float Weight = 0.8f;
    [Range(0, 1)] public float Body = 0.9f;
    [Range(0, 1)] public float Head = 1;

    [Header("Arm IK")]
    [LovattoToogle] public bool useArmIk = true;
    [Range(0, 1)] public float armIkWeight = 1;
    [Tooltip("Use this to adjust the rotation of the right hand IK."), FormerlySerializedAs("rightArmIkOffset")]
    public Vector3 HandRotationAdjust;

    [Header("Ragdoll"), FormerlySerializedAs("mRigidBody")]
    public List<Rigidbody> rigidbodies = new();
    [ReadOnly] public bl_NetworkGun currentWeapon;

    /// <summary>
    /// Set parent to null when the bot die?
    /// </summary>
    public bool DetachModelOnDeath
    {
        get;
        set;
    } = false;
    #endregion

    #region Private members
    private Vector3 localVelocity;
    private float vertical;
    private float horizontal;
    private float turnSpeed;
    private Vector3 velocity;
    private float lastYRotation;
    private float TurnLerp;
    private float movementSpeed;
    private Vector3 headTarget;
    private int[] animatorHashes;
    private float timeSinceLastMove = 0;
    private Vector3 fixedLookAtPosition = Vector3.zero;
    private Transform headBone;
    private Coroutine weightBlendCoroutine;
    private Transform rightUpperArm;
    private Animator m_animator;
    #endregion

    /// <summary>
    /// 
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        m_animator = BotAnimator; // the animator getter is called a lot of times, so cache it to skip the null check
        headBone = m_animator.GetBoneTransform(HumanBodyBones.Head);
        rightUpperArm = m_animator.GetBoneTransform(HumanBodyBones.RightUpperArm);
        SetKinecmatic();
        if (animatorHashes == null)
        {
            FetchHashes();
        }
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
    /// 
    /// </summary>
    /// <param name="enter"></param>
    /// <param name="sourceAnim"></param>
    /// <param name="state"></param>
    /// <param name="weight"></param>
    void OnIKMotionWeightModified(bool enter, Animator sourceAnim, bl_AnimatorIKBlendEvent.Modifier modifier)
    {
        if (sourceAnim != BotAnimator) return;

        if (modifier.SmoothTime <= 0)
        {
            armIkWeight = modifier.Weight;
            return;
        }

        if (weightBlendCoroutine != null) StopCoroutine(weightBlendCoroutine);
        weightBlendCoroutine = StartCoroutine(LerpWeight());
        IEnumerator LerpWeight()
        {
            float d = 0;
            while (d < 1)
            {
                d += Time.deltaTime / modifier.SmoothTime;
                armIkWeight = Mathf.Lerp(armIkWeight, modifier.Weight, d);
                yield return null;
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void FetchHashes()
    {
        //cache the hashes in a Array will be more appropriate but to be more readable for other users
        // I decide to cached them in a Dictionary with the key name indicating the parameter that contain
        animatorHashes = new int[]
        {
            Animator.StringToHash("Vertical"),
            Animator.StringToHash("Horizontal"),
            Animator.StringToHash("Speed"),
            Animator.StringToHash("Turn"),
            Animator.StringToHash("isGround")
        };
    }

    /// <summary>
    /// 
    /// </summary>
    public override void OnUpdate()
    {
        ControllerInfo();
        Animate();
    }

    /// <summary>
    /// 
    /// </summary>
    void ControllerInfo()
    {
        velocity = References.shooterNetwork.Velocity;
        float delta = Time.deltaTime;
        localVelocity = CachedTransform.InverseTransformDirection(velocity);
        localVelocity.y = 0;

        vertical = Mathf.Lerp(vertical, localVelocity.z, delta * 5);
        horizontal = Mathf.Lerp(horizontal, localVelocity.x, delta * 8);
        movementSpeed = velocity.magnitude;

        if (movementSpeed > 0.1f)
        {
            timeSinceLastMove = Time.time;
        }

        horizontal = bl_MathUtility.SnapSmallValueToZero(horizontal, 0.3f); // this help with the directional blend animation

        turnSpeed = Mathf.DeltaAngle(lastYRotation, CachedTransform.rotation.eulerAngles.y);
        TurnLerp = Mathf.Lerp(TurnLerp, turnSpeed, 7 * delta);

        if (Time.time - timeSinceLastMove < 1)
        {
            TurnLerp = 0;
        }

        if (Time.frameCount % 7 == 0) lastYRotation = lastYRotation = CachedTransform.rotation.eulerAngles.y;
    }

    /// <summary>
    /// 
    /// </summary>
    void Animate()
    {
        if (m_animator == null)
            return;

        m_animator.SetFloat(animatorHashes[0], vertical);
        m_animator.SetFloat(animatorHashes[1], horizontal);
        m_animator.SetFloat(animatorHashes[2], movementSpeed);
        m_animator.SetFloat(animatorHashes[3], TurnLerp);
    }

    /// <summary>
    /// 
    /// </summary>
    private void OnAnimatorIK(int layerIndex)
    {
        if (layerIndex == 1)
        {
            HeadIK();
        }

        ArmsIK();
    }

    /// <summary>
    /// 
    /// </summary>
    void HeadIK()
    {
        var references = References;
        if (!Application.isPlaying)
        {
            BotAnimator.SetLookAtWeight(Weight, Body, Head, 1, 0.4f);
            BotAnimator.SetLookAtPosition(references.lookAtTarget.position);
            return;
        }

        fixedLookAtPosition = references.aiShooter.LookAtPosition;
        if (references.aiShooter.LookingAt != AILookAt.Target && bl_MathUtility.Distance(fixedLookAtPosition, headBone.position) < Mathf.PI)
        {
            fixedLookAtPosition = (CachedTransform.position + (CachedTransform.forward * 10));
            fixedLookAtPosition.y = headBone.position.y;
        }

        m_animator.SetLookAtWeight(Weight, Body, Head, 1, 0.4f);
        headTarget = Vector3.Slerp(headTarget, fixedLookAtPosition, Time.deltaTime * 5);
        m_animator.SetLookAtPosition(headTarget);
        if (references.lookReference != null) references.lookReference.LookAt(headTarget, Vector3.up);
    }

    /// <summary>
    /// 
    /// </summary>
    void ArmsIK()
    {
        if (!useArmIk) return;
        if (References.lookAtTarget == null) return;
        if (currentWeapon == null)
        {
            currentWeapon = References.shooterWeapon.GetEquippedWeapon();
            return;
        }

        var target = References.lookAtTarget;

        // Calculate the direction from the rifle tip to the target.
        Vector3 directionToTarget = target.position - rightUpperArm.position;
        Quaternion desiredRotation = Quaternion.LookRotation(directionToTarget);

        // Set IK rotation weights for the right hand.
        m_animator.SetIKRotationWeight(AvatarIKGoal.RightHand, armIkWeight);

        desiredRotation *= Quaternion.Euler(HandRotationAdjust);
        // Set the IK rotation for the right hand.
        m_animator.SetIKRotation(AvatarIKGoal.RightHand, desiredRotation);

        if (currentWeapon.LeftHandPosition != null)
        {
            // Set IK rotation weights for the left hand.
            m_animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, armIkWeight);
            m_animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, armIkWeight);

            // Set the position of the left hand where the Rifle Grip is.
            m_animator.SetIKPosition(AvatarIKGoal.LeftHand, currentWeapon.LeftHandPosition.position);
            m_animator.SetIKRotation(AvatarIKGoal.LeftHand, currentWeapon.LeftHandPosition.rotation);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void SetKinecmatic()
    {
        for (int i = 0; i < rigidbodies.Count; i++)
        {
            var r = rigidbodies[i];
            if (r == null) continue;

            r.linearVelocity = Vector3.zero;
            r.angularVelocity = Vector3.zero;
            r.isKinematic = true;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="from"></param>
    /// <param name="isExplosion"></param>
    public override void Ragdolled(DamageData damageData)
    {
        m_animator.enabled = false;
        Vector3 rhs = damageData.GetRHSDirection();

        for (int i = 0; i < rigidbodies.Count; i++)
        {
            if (rigidbodies[i] == null) continue;

            var r = rigidbodies[i];
            r.isKinematic = false;
            r.detectCollisions = true;
            if (i != 0)
            {
                r.linearVelocity = Vector3.zero;
                r.angularVelocity = Vector3.zero;
            }

            if (bl_GameData.CoreSettings.applyForceToRagdolls)
            {
                r.AddForceAtPosition(rhs.normalized * 150, damageData.HitPoint);
                if (damageData.Cause.IsEnumFlagPresent(DamageCause.Explosion))
                {
                    r.AddExplosionForce(1000, damageData.HitPoint, 7);
                }
            }
        }

        bl_Hook.DoAction("Action_AfterRagdollPhysics", rigidbodies, CachedTransform, rhs, damageData.HitPoint);

        //ApplyYawSpin(rigidbodies, CachedTransform, rhs, damageData.HitPoint, 50, 1f);
    }

    static void ApplyYawSpin(List<Rigidbody> bodies, Transform root, Vector3 hitDir, Vector3 hitPoint, float baseImpulse, float spinMul = 1f)
    {
        if (bodies == null || bodies.Count == 0) return;

        // Use character up so it works on slopes
        Vector3 up = root.up;

        // Flatten hit direction onto the ground plane and measure "how much to the left/right"
        Vector3 dir = Vector3.ProjectOnPlane(hitDir, up).normalized;
        if (dir.sqrMagnitude < 1e-6f) return;

        Vector3 right = Vector3.Cross(up, root.forward).normalized;
        float lateral = Mathf.Clamp(Vector3.Dot(right, dir), -1f, 1f); // -1 = hit from left, +1 = from right

        // Spin amount: scale with hit strength & lateral factor
        float yawImpulse = baseImpulse * lateral * 0.7f * spinMul;
        if (Mathf.Abs(yawImpulse) < 0.01f) return;

        // Pick pelvis/chest if present, otherwise use first bodies
        Rigidbody pelvis = null, chest = null;
        for (int i = 0; i < bodies.Count; i++)
        {
            var r = bodies[i]; if (r == null) continue;
            string n = r.name.ToLowerInvariant();
            if (pelvis == null && (n.Contains("pelvis") || n.Contains("hips"))) pelvis = r;
            else if (chest == null && (n.Contains("chest") || n.Contains("spine") || n.Contains("torso"))) chest = r;
        }
        if (pelvis == null) pelvis = bodies[0];
        if (chest == null) chest = bodies[Mathf.Min(1, bodies.Count - 1)];

        // Let bodies actually spin (Unity default is ~7 rad/s)
        for (int i = 0; i < bodies.Count; i++) { var r = bodies[i]; if (r != null) r.maxAngularVelocity = 50f; }

        // Apply torque impulses around up axis; pelvis carries most of it
        pelvis.AddTorque(up * yawImpulse, ForceMode.Impulse);
        chest.AddTorque(up * (yawImpulse * 0.6f), ForceMode.Impulse);
    }

    /// <summary>
    /// 
    /// </summary>
    public override void OnGetHit()
    {
        int r = Random.Range(0, 2);
        string hit = (r == 1) ? "Right Hit" : "Left Hit";
        m_animator.CrossFade(hit, 0.1f, 2);
    }

#if UNITY_EDITOR
    [HideInInspector] public Vector3 defaultWeaponRootPosition;

#endif

    /// <summary>
    /// 
    /// </summary>
    public void GetRigidBodys()
    {
        Rigidbody[] R = this.transform.GetComponentsInChildren<Rigidbody>();

        foreach (Rigidbody rb in R)
        {
            if (!rigidbodies.Contains(rb))
            {
                rigidbodies.Add(rb);
            }
        }
    }

    private bl_AIShooterReferences _references = null;
    public bl_AIShooterReferences References
    {
        get
        {
            if (_references == null)
            {
                _references = CachedTransform.GetComponentInParent<bl_AIShooterReferences>();
            }
            return _references;
        }
    }
}