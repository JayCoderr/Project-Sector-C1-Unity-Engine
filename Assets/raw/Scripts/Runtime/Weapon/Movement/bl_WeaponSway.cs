using MFPS.Runtime.Motion;
using MFPSEditor;
using UnityEngine;

public class bl_WeaponSway : bl_WeaponSwayBase
{
    #region Public members
    [ScriptableDrawer] public bl_SpringTransform spring;
    [Header(" Delay movement")]
    [Range(0.2f, 5)] public float delayAmplitude = 2;
    [Range(0.01f, 0.5f)] public float pushAmplutide = 0.2f;
    [Range(1, 7)] public float sideAngleAmplitude = 4;
    [Range(0.1f, 2), Tooltip("Define how much the weapon pitch react to the player vertical velocity (jump or falling).")]
    public float verticalDelayMultiplier = 1;
    public float crouchAngleTilt = 12;
    public float Smoothness = 3.0F;
    public float aimMultiplier = 0.05f;

    public float Amount { get; set; }
    #endregion

    #region Private members
    private Vector3 defaultPosition;
    private Vector3 targetVector = Vector3.zero;
    private Transform m_Transform;
    private float factorX, factorY, factorZ = 0;
    private Vector3 defaultEuler;
    private float deltaTime;
    private bool isAiming = false;
    private float amplitudeMultiplier = 1;
    private Vector3 factorRot;
    private Vector3 verticalvector = Vector3.zero;
    private bl_PlayerReferences playerRefs;
    private readonly float maxAmount = 0.05F;
    private WeaponSwayMode mode = WeaponSwayMode.RollMode;
    #endregion

    /// <summary>
    /// 
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        m_Transform = transform;
        playerRefs = GetComponentInParent<bl_PlayerReferences>();
        defaultPosition = m_Transform.localPosition;
        Amount = delayAmplitude;
        defaultEuler = m_Transform.localEulerAngles;
        if (spring != null) spring = spring.InitAndGetInstance(CachedTransform);
        mode = bl_GameData.CoreSettings.weaponSwayMode;
    }

    /// <summary>
    /// 
    /// </summary>
    public override void OnUpdate()
    {
        deltaTime = Time.smoothDeltaTime;
        DelayMovement();
        SideMovement();
        VerticalForce();

        if (spring != null)
        {
            spring.SetRotationTarget(Vector3.Slerp(spring.RotationCurrent, (defaultEuler + factorRot), 1));
            spring.Update();
        }
    }

    /// <summary>
    /// The delay effect movement when move the camera with the mouse.
    /// </summary>
    void DelayMovement()
    {
        if (mode == WeaponSwayMode.NoSway) return;

        float mx = bl_GameInput.MouseX;
        float my = bl_GameInput.MouseY;
        float vertical = bl_GameInput.Vertical;
        float mul = 1;

        if (bl_UtilityHelper.isMobile)
        {
#if MFPSM
            mx = bl_TouchPad.Instance.Horizontal;
            my = bl_TouchPad.Instance.Vertical;
            vertical = bl_MFPSMobileControl.GetMovementAxis().y;
            mul = 0.015f;
#endif
        }

        if (mode == WeaponSwayMode.RollMode)
        {
            factorX = -mx * deltaTime * Amount * amplitudeMultiplier * mul;
            factorY = -my * deltaTime * Amount * amplitudeMultiplier * mul;
            factorZ = -vertical * (isAiming ? pushAmplutide * 0.1f : pushAmplutide) * amplitudeMultiplier * mul;
            factorX = Mathf.Clamp(factorX, -maxAmount, maxAmount);
            factorY = Mathf.Clamp(factorY, -maxAmount, maxAmount);
            targetVector.Set(defaultPosition.x + factorX, defaultPosition.y, defaultPosition.z + factorZ);
            factorRot = new Vector3(factorY * 20, 0, factorX * 200);
        }
        else if (mode == WeaponSwayMode.YawAndPitchMode)
        {
            factorX = -mx * deltaTime * Amount * amplitudeMultiplier;
            factorY = -my * deltaTime * Amount * amplitudeMultiplier;
            factorZ = -vertical * (isAiming ? pushAmplutide * 0.1f : pushAmplutide) * amplitudeMultiplier;
            factorX = Mathf.Clamp(factorX, -maxAmount, maxAmount);
            factorY = Mathf.Clamp(factorY, -maxAmount, maxAmount);
            targetVector.Set(defaultPosition.x + factorX, defaultPosition.y + factorY, defaultPosition.z + factorZ);
            factorRot = new Vector3(factorY * 100, -factorX * 100, 0);
        }
        else if (mode == WeaponSwayMode.CounterStrikeStyle)
        {
            factorX = -mx * deltaTime * Amount * amplitudeMultiplier;
            factorY = -my * deltaTime * Amount * amplitudeMultiplier;
            factorZ = -vertical * (isAiming ? pushAmplutide * 0.1f : pushAmplutide) * amplitudeMultiplier;
            factorX = Mathf.Clamp(factorX, -maxAmount, maxAmount);
            targetVector.Set(defaultPosition.x + (factorX * 0.5f), defaultPosition.y + (factorY * 0.1f), defaultPosition.z + factorZ);
        }

        if (spring != null) spring.SetPositionTarget(targetVector);
    }

    /// <summary>
    /// 
    /// </summary>
    void VerticalForce()
    {
        if (spring == null) return;

        float aimMul = isAiming ? 0.1f : 1;
        verticalvector.x = playerRefs.characterController.velocity.y * verticalDelayMultiplier * aimMul;
        spring.RotationSpring.SetTargetLayer(2, verticalvector);
    }

    /// <summary>
    /// The angle oscillation movement when the player move sideway
    /// </summary>
    void SideMovement()
    {
        if (mode == WeaponSwayMode.NoSway || mode == WeaponSwayMode.CounterStrikeStyle) return;

        factorX = bl_GameInput.Horizontal;
        defaultEuler.z = factorX * sideAngleAmplitude * amplitudeMultiplier;
        defaultEuler.z = -defaultEuler.z;
    }

    /// <summary>
    /// 
    /// </summary>
    protected override void OnEnable()
    {
        base.OnEnable();
        bl_EventHandler.onLocalAimChanged += OnLocalAimChanged;
        bl_EventHandler.onChangeWeapon += OnLocalWeaponChanged;
        bl_EventHandler.onLocalPlayerStateChanged += OnLocalPlayerStateChanged;
    }

    /// <summary>
    /// 
    /// </summary>
    protected override void OnDisable()
    {
        base.OnDisable();
        bl_EventHandler.onLocalAimChanged -= OnLocalAimChanged;
        bl_EventHandler.onChangeWeapon -= OnLocalWeaponChanged;
        bl_EventHandler.onLocalPlayerStateChanged -= OnLocalPlayerStateChanged;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="aim"></param>
    void OnLocalAimChanged(bool aim)
    {
        isAiming = aim;
        if (!aim)
        {
            if (spring != null)
            {
                spring.RotationSpring.StartDecayingSineWave(Vector3.forward, 9, 4, 3.8f);
                if (playerRefs.firstPersonController.State == PlayerState.Crouching || playerRefs.firstPersonController.State == PlayerState.Sliding)
                    spring.RotationSpring.SetTargetLayer(3, new Vector3(0, 0, crouchAngleTilt));
            }
        }
        else
        {
            if (spring != null)
            {
                spring.RotationSpring.StopSineWave();
                spring.RotationSpring.SetTargetLayer(3, Vector3.zero);
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="newWeapon"></param>
    void OnLocalWeaponChanged(int newWeapon)
    {
        isAiming = false;
        if (spring != null)
        {
            spring.RotationSpring.StopSineWave();
            this.InvokeAfter(0.2f, () =>
            {
                spring.RotationSpring.StartDecayingSineWave(new Vector3(0.1f, 0, 1), 10, 3.5f, 5f);
            });
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="from"></param>
    /// <param name="to"></param>
    void OnLocalPlayerStateChanged(PlayerState from, PlayerState to)
    {
        if ((to == PlayerState.Crouching || to == PlayerState.Sliding) && !isAiming)
        {
            if (spring != null) spring.RotationSpring.SetTargetLayer(3, new Vector3(0, 0, crouchAngleTilt));
        }
        else if (from == PlayerState.Crouching || (from == PlayerState.Sliding && to != PlayerState.Crouching))
        {
            if (spring != null) spring.RotationSpring.SetTargetLayer(3, Vector3.zero);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="multiplier"></param>
    public override void SetMotionMultiplier(float multiplier)
    {
        amplitudeMultiplier = multiplier;
    }

    /// <summary>
    /// 
    /// </summary>
    public override void UseAimSettings()
    {
        amplitudeMultiplier = aimMultiplier;
    }

    /// <summary>
    /// 
    /// </summary>
    public override void ResetSettings()
    {
        Amount = delayAmplitude;
        amplitudeMultiplier = 1;
    }
}