using MFPS.Runtime.Motion;
using MFPSEditor;
using System.Collections;
using UnityEngine;

public class bl_CameraMotion : bl_CameraMotionBase
{
    [ScriptableDrawer] public bl_CameraMotionSettings motionSettings;

    #region Private members
    private bool wiggleMotion = true;
    private bool breathingMotion = false;
    private float m_breatingAmplitude;
    private float m_startBreathingTime;
    private Vector3 breathRotation = Vector3.zero;
    private float sideTilt = 0;
    private Quaternion rotationMotion;
    private bool isSliding = false;
    private bl_SpringTransform spring;
    private bl_Spring zoomSpring;
    private bl_PlayerReferences playerRefs;
    private Vector3 shakeVector = Vector3.zero;
    private Coroutine shakeRoutine;
    #endregion

    /// <summary>
    /// 
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        playerRefs = GetComponentInParent<bl_PlayerReferences>();
        m_breatingAmplitude = motionSettings.breatheAmplitude;
        rotationMotion = CachedTransform.localRotation;
        spring = motionSettings.spring;
        zoomSpring = motionSettings.zoomSpring;
        if (spring != null) spring = spring.InitAndGetInstance(CachedTransform);
        if (zoomSpring != null)
        {
            zoomSpring = zoomSpring.InitAndGetInstance();
            zoomSpring.Current = playerRefs.DefaultCameraFOV;
            zoomSpring.Target = playerRefs.DefaultCameraFOV;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    protected override void OnEnable()
    {
        base.OnEnable();
        bl_EventHandler.onPlayerLand += OnSmallImpact;
        bl_EventHandler.onLocalPlayerStateChanged += OnPlayerStateChanged;
        bl_EventHandler.Player.onLocalDoubleJump += OnDoubleJump;
        wiggleMotion = bl_GameData.CoreSettings.playerViewRolling;
    }

    /// <summary>
    /// 
    /// </summary>
    protected override void OnDisable()
    {
        base.OnDisable();
        bl_EventHandler.onPlayerLand -= OnSmallImpact;
        bl_EventHandler.onLocalPlayerStateChanged -= OnPlayerStateChanged;
        bl_EventHandler.Player.onLocalDoubleJump -= OnDoubleJump;
    }

    /// <summary>
    /// 
    /// </summary>
    public override void OnUpdate()
    {
        ZoomControl();
        Breathe();
        Wiggle();

        if (spring != null)
        {
            spring.RotationSpring.Target = breathRotation;
            spring.Update();
        }

        if (zoomSpring != null)
        {
            zoomSpring.UpdateSpring(Time.deltaTime);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void ZoomControl()
    {
        if (zoomSpring == null) return;

        playerRefs.playerCamera.fieldOfView = Mathf.Clamp(zoomSpring.Current, 1, 179);
    }

    /// <summary>
    /// 
    /// </summary>
    void Wiggle()
    {
        if (!wiggleMotion) return;

        float rollAngle = -bl_GameInput.MouseX * motionSettings.tiltAngle;
        if (isSliding) rollAngle = 0;
        rollAngle = Mathf.Clamp(rollAngle, -motionSettings.tiltAngle, motionSettings.tiltAngle);

        if (motionSettings.sideMoveEffector > 0)
        {
            sideTilt = Mathf.Lerp(sideTilt, -bl_GameInput.Horizontal * motionSettings.sideMoveEffector, Time.deltaTime * (motionSettings.smooth * 2));
            rollAngle += sideTilt;
        }

        breathRotation.z = bl_GameInput.Aim() ? rollAngle * motionSettings.aimMultiplier : rollAngle;
        rotationMotion = Quaternion.Slerp(rotationMotion, Quaternion.Euler(breathRotation), Time.smoothDeltaTime * motionSettings.smooth);
    }

    /// <summary>
    /// 
    /// </summary>
    void Breathe()
    {
        if (!breathingMotion) return;

        float theta = (Time.time - m_startBreathingTime) / motionSettings.breathePeriod;
        float vertical = m_breatingAmplitude * Mathf.Sin(theta);
        float horizontal = m_breatingAmplitude * Mathf.Cos(theta * 0.5f);
        breathRotation = (Vector3.up * vertical) + (Vector3.right * horizontal);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="rotation"></param>
    public override void AddCameraRotation(int layer, Vector3 rotation, bool autoReset = false)
    {
        rotation *= motionSettings.motionMultiplier;
        if (spring != null) spring.RotationSpring.SetTargetLayer(layer, rotation, autoReset);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="layer"></param>
    /// <param name="position"></param>
    /// <param name="autoReset"></param>
    public override void AddCameraPosition(int layer, Vector3 position, bool autoReset = false)
    {
        if (spring != null) spring.PositionSpring.SetTargetLayer(layer, position, autoReset);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="layer"></param>
    /// <param name="fov"></param>
    public override void AddCameraFOV(int layer, float extraFov, bool autoReset = false)
    {
        if (zoomSpring == null) return;

        zoomSpring.SetTargetLayer(layer, extraFov, autoReset);
        zoomSpring.SetMultiplier(zoomSpring.Target <= 16 ? 5f : 1);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="active"></param>
    public override void SetActiveBreathing(bool active, float amplitude = 0)
    {
        breathingMotion = active;

        if (!breathingMotion)
        {
            m_breatingAmplitude = motionSettings.breatheAmplitude;
            breathRotation = Vector3.zero;
        }
        else
        {
            m_breatingAmplitude = amplitude;
            m_startBreathingTime = Time.time;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="curve"></param>
    /// <param name="duration"></param>
    /// <param name="multiplier"></param>
    /// <param name="layer"></param>
    public override void PlayCurveShake(string curveName, float duration, float multiplier = 1, float delay = 0)
    {
        if (motionSettings == null) return;

        AnimationCurve curve = motionSettings.GetCurve(curveName);
        if (curve == null)
        {
            Debug.LogWarning("Curve with name: " + curveName + " not found in the settings.");
            return;
        }

        if (shakeRoutine != null) StopCoroutine(shakeRoutine);

        shakeRoutine = StartCoroutine(DoPlay());
        IEnumerator DoPlay()
        {
            if (delay > 0) yield return new WaitForSeconds(delay);
            float d = 0;
            while (d < 1)
            {
                d += Time.deltaTime / duration;
                d = Mathf.Clamp01(d);
                float value = curve.Evaluate(d);
                shakeVector.z = value * multiplier * motionSettings.motionMultiplier;

                if (spring == null)
                {
                    CachedTransform.localEulerAngles = shakeVector;
                }
                else
                {
                    CachedTransform.localRotation = Quaternion.Slerp(CachedTransform.localRotation, Quaternion.Euler(spring.RotationCurrent + shakeVector), 1);
                }
                yield return null;
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void OnSmallImpact(float impactAmount)
    {
        if (spring != null) spring.RotationSpring.SetTargetLayer(2, new Vector3(motionSettings.DownAmount, 0, 0), true);
        PlayCurveShake("sharp-triple", 0.4f, 10);
    }

    /// <summary>
    /// 
    /// </summary>
    void OnDoubleJump()
    {
        if (spring != null) spring.RotationSpring.SetTargetLayer(2, new Vector3(motionSettings.DownAmount * 0.5f, 0, 0), true);
        PlayCurveShake("osc-3", 0.4f, 1);
    }

    /// <summary>
    /// 
    /// </summary>
    void OnPlayerStateChanged(PlayerState from, PlayerState to)
    {
        isSliding = to == PlayerState.Sliding;
        if (to == PlayerState.Jumping)
        {
            if (spring != null) spring.RotationSpring.SetTargetLayer(2, new Vector3(motionSettings.DownAmount * 0.5f, 0, 0), true);
        }
    }
}