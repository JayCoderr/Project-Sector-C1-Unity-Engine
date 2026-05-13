using System.Collections.Generic;
using UnityEngine;

public class bl_SniperScope : bl_SniperScopeBase
{
    #region Public members
    [Range(0.02f, 0.5f)] public float transitionDuration = 0.2f;
    [Range(0, 0.5f)] public float fadeInDelay = 0.2f;
    public float breathingAmplitude = 0.14f;
    public float maxHoldBreathTime = 3;
    [Tooltip("Objects to disable when the scope shown, usually the weapon and arms meshes.")]
    public List<GameObject> OnScopeDisable = new List<GameObject>();
    #endregion

    #region Private members
    private bl_Gun m_gun;
    private bool returnedAim = true;
    private bool aiming = false;
    private bool isHoldingBreath = false;
    private const float HoldColdownTime = 5;
    private KeyCode holdBreathKey = KeyCode.LeftControl;
    private float holdingBreathTime = 0;
    private float breathingColdown = 0;
    #endregion

    /// <summary>
    /// 
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        m_gun = GetComponent<bl_Gun>();
    }

    /// <summary>
    /// 
    /// </summary>
    protected override void OnEnable()
    {
        base.OnEnable();
        if (bl_ScopeUIBase.Instance != null)
            bl_ScopeUIBase.Instance.SetupWeapon(this);
        aiming = false;
    }

    /// <summary>
    /// 
    /// </summary>
    protected override void OnDisable()
    {
        base.OnDisable();
        if (bl_ScopeUIBase.Instance != null) bl_ScopeUIBase.Instance.SetActive(false);
        aiming = false;
    }

    /// <summary>
    /// 
    /// </summary>
    public override void OnUpdate()
    {
        if (m_gun == null || ScopeTexture == null || !IsScopeEnabled)
            return;

        DisplayControl();
        HoldBreathingControl();
    }

    /// <summary>
    /// 
    /// </summary>
    void HoldBreathingControl()
    {
        if (breathingColdown > 0)
        {
            breathingColdown -= Time.deltaTime;
            if (isHoldingBreath)
            {
                isHoldingBreath = false;
                m_gun.PlayerReferences.cameraMotion.SetActiveBreathing(true, breathingAmplitude);
            }
            return;
        }

        if (m_gun.isAiming && bl_UtilityHelper.GetCursorState)
        {
            if (Input.GetKeyDown(holdBreathKey))
            {
                isHoldingBreath = true;
                m_gun.PlayerReferences.cameraMotion.SetActiveBreathing(false);
            }
            if (Input.GetKey(holdBreathKey))
            {
                holdingBreathTime += Time.deltaTime;
                if (holdingBreathTime >= maxHoldBreathTime)
                {
                    m_gun.PlayerReferences.cameraMotion.SetActiveBreathing(false);
                    breathingColdown = HoldColdownTime;
                    holdingBreathTime = 0;
                }
            }
            if (Input.GetKeyUp(holdBreathKey))
            {
                isHoldingBreath = false;
                m_gun.PlayerReferences.cameraMotion.SetActiveBreathing(true, breathingAmplitude);
                holdingBreathTime = 0;
            }
        }
        else
        {
            if (isHoldingBreath)
            {
                isHoldingBreath = false;
                m_gun.PlayerReferences.cameraMotion.SetActiveBreathing(true, breathingAmplitude);
                holdingBreathTime = 0;
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void DisplayControl()
    {
        if (m_gun.isAiming && !m_gun.isReloading)
        {
            if (!aiming)
            {
                returnedAim = false;
                if (bl_ScopeUIBase.Instance != null)
                    bl_ScopeUIBase.Instance.Crossfade(true, transitionDuration, fadeInDelay, null, () =>
                    {
                        foreach (GameObject go in OnScopeDisable)
                        {
                            if (go == null) continue;
                            go.SetActive(false);
                        }
                    });

                // Active breathing effect
                if (breathingAmplitude > 0)
                {
                    m_gun.PlayerReferences.cameraMotion.SetActiveBreathing(true, breathingAmplitude);
                }

                aiming = true;
            }
        }
        else
        {
            if (aiming)
            {
                float inverse = bl_MathUtility.InverseLerp(m_gun.GetDefaultPosition(), m_gun.AimPosition, m_gun.transform.localPosition);
                bool wasFullAiming = inverse >= 0.78f;
                if (bl_ScopeUIBase.Instance != null)
                    bl_ScopeUIBase.Instance.Crossfade(false, transitionDuration, 0, () =>
                {
                    if (!returnedAim && m_gun.Info != null)
                    {
                        if (wasFullAiming) m_gun.SetToAim();
                        returnedAim = true;
                    }

                    foreach (GameObject go in OnScopeDisable)
                    {
                        if (go == null) continue;
                        if (bl_MFPS.LocalPlayer.IsThirdPersonView()) continue;

                        go.SetActive(true);
                    }

                });
                m_gun.PlayerReferences.cameraMotion.SetActiveBreathing(false);
            }
            aiming = false;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="text"></param>
    private void ShowText(string text)
    {
        if (bl_ScopeUIBase.Instance == null) return;

        bl_ScopeUIBase.Instance.ShowIndication(text);
    }
}