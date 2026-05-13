using MFPS.Runtime.Motion;
using MFPSEditor;
using UnityEngine;

/// <summary>
/// MFPS Default recoil movement
/// If you want to use your custom recoil script, inherited your script from bl_RecoilBase.cs
/// Use this as reference only.
/// </summary>
public class bl_Recoil : bl_RecoilBase
{
    [LovattoToogle] public bool AutomaticallyComeBack = true;
    [ScriptableDrawer] public bl_SpringTransform spring;

    #region Private members
    private Transform m_Transform;
    private WeaponRecoilType currentRecoilType = WeaponRecoilType.SimpleVertical;
    private Vector3 RecoilRot;
    private float Recoil = 0;
    private float RecoilSpeed = 2;
    private float lerpRecoil = 0;
    private bool wasFiring = false;
    private float currentRecoilTime;
    private Vector2 targetRecoil = Vector2.zero;
    private Vector2 currentRecoil;
    #endregion

    /// <summary>
    /// 
    /// </summary>
    private void Start()
    {
        GameObject g = new GameObject("Recoil");
        m_Transform = g.transform;
        m_Transform.parent = transform.parent;
        m_Transform.localPosition = Vector3.zero;
        m_Transform.localEulerAngles = Vector3.zero;
        transform.parent = m_Transform;
#if !MFPSTPV
        transform.localPosition = Vector3.zero;
        transform.localEulerAngles = Vector3.zero;
#endif

        RecoilRot = m_Transform.localEulerAngles;
        if (spring != null) spring = spring.InitAndGetInstance(m_Transform);
    }

    /// <summary>
    /// 
    /// </summary>
    public override void OnUpdate()
    {
        PatternRecoilControl();
        SimpleRecoilControl();
        if (spring != null) spring.Update();
    }

    /// <summary>
    /// 
    /// </summary>
    void SimpleRecoilControl()
    {
        if (currentRecoilType != WeaponRecoilType.SimpleVertical) return;

        if (currentRecoilTime > 0)
        {
            currentRecoilTime -= Time.deltaTime;
            if (AutomaticallyComeBack)
            {
                if (spring != null) spring.SetRotationTarget(new Vector3(-Recoil, 0, 0));
            }
            else
            {
                lerpRecoil = Mathf.Lerp(lerpRecoil, Recoil, Time.deltaTime * RecoilSpeed);
                fpController.GetMouseLook().SetVerticalOffset(-lerpRecoil);
            }
            wasFiring = true;
        }
        else
        {
            if (AutomaticallyComeBack)
            {
                BackToOrigin();
            }
            else
            {
                if (wasFiring)
                {
                    Recoil = 0;
                    lerpRecoil = 0;
                    fpController.GetMouseLook().CombineVerticalOffset();
                    wasFiring = false;
                }
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void PatternRecoilControl()
    {
        if (currentRecoilType == WeaponRecoilType.SimpleVertical) return;

        if (currentRecoilTime > 0)
        {
            currentRecoilTime -= Time.deltaTime;
            currentRecoil = Vector2.Lerp(currentRecoil, targetRecoil, Time.deltaTime * RecoilSpeed);
            if (spring != null) spring.SetRotationTarget(currentRecoil);
        }
        else
        {
            Recoil = 0;
            if (AutomaticallyComeBack)
            {
                currentRecoil = Vector3.zero;
                if (spring != null) spring.RotationSpring.Target = Vector3.zero;
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void BackToOrigin()
    {
        if (m_Transform == null) return;

        if (spring != null) spring.SetRotationTarget(RecoilRot);
        Recoil = m_Transform.localEulerAngles.x;
    }

    /// <summary>
    /// Add recoil
    /// </summary>
    public override void SetRecoil(RecoilData data)
    {
#if MFPSM
        if (bl_UtilityHelper.isMobile && bl_MobileControlSettings.Instance.disableRecoil) return;
#endif
        var recoilSettings = data.RecoilSettings;
        if (recoilSettings == null)
        {
            currentRecoilTime = 0.2f;
            currentRecoilType = WeaponRecoilType.SimpleVertical;
        }
        else
        {
            currentRecoilTime = recoilSettings.recoilDuration;
            currentRecoilType = recoilSettings.recoilType;
        }

        if (currentRecoilType == WeaponRecoilType.SimpleVertical)
        {
            Recoil += data.Amount;
            Recoil = Mathf.Clamp(Recoil, 0, data.MaxValue);
            RecoilSpeed = data.Speed;
        }
        else if (currentRecoilType == WeaponRecoilType.Pattern)
        {
            Recoil += recoilSettings.recoilPerShot;
            Recoil = Mathf.Clamp(Recoil, 0, data.MaxValue);
            RecoilSpeed = recoilSettings.recoilSmoothness;
            targetRecoil.Set(-Recoil * recoilSettings.recoilVerticalIntensity, recoilSettings.recoilPattern.Evaluate(Recoil) * recoilSettings.recoilHorizontalIntensity);
        }
        else if (currentRecoilType == WeaponRecoilType.Random2D)
        {
            Recoil += recoilSettings.recoilPerShot;
            Recoil = Mathf.Clamp(Recoil, 0, data.MaxValue);
            RecoilSpeed = recoilSettings.recoilSmoothness;
            float x = Random.Range(-recoilSettings.recoilHorizontalIntensity, recoilSettings.recoilHorizontalIntensity);
            targetRecoil.Set(-Recoil * recoilSettings.recoilVerticalIntensity, x);
        }
    }

    private bl_FirstPersonControllerBase fpController => PlayerReferences.firstPersonController;

    private bl_PlayerReferences playerReferences;
    public bl_PlayerReferences PlayerReferences
    {
        get
        {
            if (playerReferences == null) playerReferences = transform.root.GetComponent<bl_PlayerReferences>();
            return playerReferences;
        }
    }
}