using MFPS.Internal.Scriptables;
using MFPSEditor;
using UnityEngine;

/// <summary>
/// Control the rotation of the FP weapon when the player sprint
/// This class has not direct references so you can use your custom script instead.
/// </summary>
public class bl_WeaponMovements : bl_MonoBehaviour
{

    #region Public members
    [Header("Weapon On Run Position")]
    [Tooltip("Weapon Position and Position On Run")]
    public Vector3 moveTo;
    [Tooltip("Weapon Rotation and Position On Run")]
    public Vector3 rotateTo;
    [Header("Weapon On Run and Reload Position")]
    [Tooltip("Weapon Position and Position On Run and Reload")]
    public Vector3 moveToReload;
    [Tooltip("Weapon Rotation and Position On Run and Reload")]
    public Vector3 rotateToReload;

    public bool createUniquePivot = true;
    [ScriptableDrawer] public bl_WeaponMovementSettings settings;
    [HideInInspector] public float _previewWeight = 0;
    #endregion

    #region Private members
    private float vel;
    private Transform transformPivot;
    private Quaternion DefaultRot;
    private Quaternion sprintRot, sprintReloadRot;
    private Vector3 DefaultPos;
    private bl_Gun Gun;
    private bl_FirstPersonControllerBase controller;
    private float acceleration = 1;
    private bool defaultState = true;
    private Vector3 currentPosition;
    private Quaternion currentQRotation;
    private bool holdingPos = false;
    #endregion

    /// <summary>
    /// 
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        Gun = CachedTransform.GetComponentInParent<bl_Gun>(true);
        controller = Gun.PlayerReferences.firstPersonController;
        sprintRot = Quaternion.Euler(rotateTo);
        sprintReloadRot = Quaternion.Euler(rotateToReload);

        if (createUniquePivot)
        {
            transformPivot = CachedTransform.CreateParent("Movement Pivot");
        }
        else
        {
            transformPivot = CachedTransform;
        }

        DefaultRot = transformPivot.localRotation;
        DefaultPos = transformPivot.localPosition;
    }

    /// <summary>
    /// 
    /// </summary>
    public override void OnUpdate()
    {
        if (holdingPos || controller == null || settings == null)
            return;

        vel = controller.VelocityMagnitude;
        RotateControl();
    }

    /// <summary>
    /// 
    /// </summary>
    void RotateControl()
    {
        if (transformPivot == null) return;

        float delta = Time.smoothDeltaTime;
        acceleration = Mathf.Lerp(acceleration, 1, delta * settings.accelerationMultiplier);
        float acc = settings.accelerationCurve.Evaluate(acceleration);

        if ((vel > 1f && controller.isGrounded) && controller.IsSprinting() && !Gun.isFiring && !Gun.isAiming)
        {
            if (defaultState)
            {
                acceleration = 0f;
                acc = 0;
                defaultState = false;
            }

            if (Gun.isReloading)
            {
                currentQRotation = Quaternion.Slerp(currentQRotation, sprintReloadRot, delta * (settings.InSpeed * settings.rotationSpeedMultiplier) * acc);
                currentPosition = Vector3.Lerp(currentPosition, moveToReload, delta * settings.InSpeed * acc);
            }
            else
            {
                currentQRotation = Quaternion.Slerp(currentQRotation, sprintRot, delta * (settings.InSpeed * settings.rotationSpeedMultiplier) * acc);
                currentPosition = Vector3.Lerp(currentPosition, moveTo, delta * settings.InSpeed * acc);
            }
        }
        else
        {
            defaultState = true;

            currentQRotation = Quaternion.Slerp(currentQRotation, DefaultRot, delta * (settings.OutSpeed * settings.rotationSpeedMultiplier));
            currentPosition = Vector3.Lerp(currentPosition, DefaultPos, delta * settings.OutSpeed);
        }

        transformPivot.localPosition = currentPosition;
        transformPivot.localRotation = currentQRotation;
    }

    /// <summary>
    /// 
    /// </summary>
    public void SetToOriginal()
    {
        transformPivot.localPosition = DefaultPos;
        transformPivot.localRotation = DefaultRot;
        currentPosition = DefaultPos;
        currentQRotation = DefaultRot;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="duration"></param>
    public void HoldPositionFor(float duration)
    {
        holdingPos = true;
        this.InvokeAfter(duration, () => { holdingPos = false; });
    }
}