using UnityEngine;
#if !UNITY_WEBGL && PVOICE
using Photon.Voice.Unity;
using Photon.Voice.PUN;
#endif
using UnityEngine.Serialization;

/// <summary>
/// Default MFPS Name Plate (Name above head) script
/// If you want to make a different approach like use UGUI instead of OnGUI
/// Create a custom script and inherited it from bl_NamePlateBase.cs
/// </summary>
public class bl_NamePlateDrawer : bl_NamePlateBase
{
    #region Public members
    [Header("Settings")]
    public float distanceModifier = 1;
    [FormerlySerializedAs("hideDistance"), Tooltip("Max distance to show the player name text, after this distance the indicator icon will be show.")]
    public float playerNameDrawDistance = 25;
    [Tooltip("Max distance to show the position indication icon on teammates, 0 = no limit")]
    public float positionIndicationMaxDistance = 75;

    [Header("References")]
    public bl_NamePlateStyle StylePresent;
    [FormerlySerializedAs("m_Target")]
    public Transform positionReference;
    #endregion

    #region Private members
    private float distance;
#if !UNITY_WEBGL && PVOICE
    private PhotonVoiceView voiceView;
    private float RightPand = 0;
    private float NameSize = 0;
#endif
    private bl_PlayerReferencesCommon playerRefs;
    private bool ShowHealthBar = false;
    private bool isFinish = false;
    float screenHeight = 0;
    Vector3 screenPoint;
    private bool isDrawing = true;
    private bool hasCustomColor = false;
    private Color customColor;
    private bool init = false;
    private float halfIconSize = 0.5f;
    private float halfHealthBarWidth = 0.5f;
    private Transform headTransform;
    #endregion

    /// <summary>
    /// 
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
#if !UNITY_WEBGL && PVOICE
         voiceView = GetComponent<PhotonVoiceView>();
#endif
        TryGetComponent(out playerRefs);
        ShowHealthBar = bl_GameData.CoreSettings.ShowTeamMateHealthBar;
        if (bl_MatchTimeManagerBase.Instance != null) isFinish = bl_MatchTimeManagerBase.Instance.IsTimeUp();

        if (bl_CameraIdentity.CurrentCamera != null)
            distance = bl_UtilityHelper.Distance(CachedTransform.localPosition, bl_CameraIdentity.CurrentCamera.transform.position);

        if (IsMine)
        {
            SetActive(false);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    protected override void OnEnable()
    {
        base.OnEnable();
        bl_EventHandler.onRoundEnd += OnGameFinish;
    }

    /// <summary>
    /// 
    /// </summary>
    protected override void OnDisable()
    {
        base.OnDisable();
        bl_EventHandler.onRoundEnd -= OnGameFinish;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="DrawName"></param>
    public override void SetName(string DrawName)
    {
        PlayerName = DrawName;
#if !UNITY_WEBGL && PVOICE
        NameSize = StylePresent.style.CalcSize(new GUIContent(PlayerName)).x;
        RightPand = (NameSize / 2) + 2;
#endif
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="active"></param>
    public override void SetActive(bool active)
    {
        isDrawing = active;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="color"></param>
    public override void SetColor(Color color)
    {
        if (color == default)
        {
            hasCustomColor = false;
        }
        else
        {
            hasCustomColor = true;
            customColor = color;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void OnGameFinish()
    {
        isFinish = true;
    }

    /// <summary>
    /// 
    /// </summary>
    void OnGUI()
    {
        if (!isDrawing || BlockDraw || bl_CameraIdentity.CurrentCamera == null || isFinish || bl_PauseMenuBase.IsMenuOpen)
            return;
        if (StylePresent == null) return;

        if (!init)
        {
            halfIconSize = StylePresent.IndicatorIconSize * 0.5f;
            halfHealthBarWidth = StylePresent.HealthBarWidth * 0.5f;
            var headBox = playerRefs.GetBodyHitbox(HumanBodyBones.Head);
            if (headBox != null)
            {
                headTransform = headBox.transform;
            }
            init = true;
        }

        Vector3 pos;
        if (headTransform != null)
        {
            pos = headTransform.position;
            pos.y += StylePresent.verticalOffset;
        }
        else pos = positionReference.position;

        screenPoint = bl_CameraIdentity.CurrentCamera.WorldToScreenPoint(pos);

        if (screenPoint.z > 0)
        {
            int vertical = ShowHealthBar ? 15 : 10;
            screenHeight = Screen.height;
            if (distance < playerNameDrawDistance)
            {
                float distanceDifference = Mathf.Clamp(distance - 0.1f, 1, 12);
                screenPoint.y += (distanceDifference * distanceModifier) + StylePresent.HealthBackOffset.y;

                GUI.color = hasCustomColor ? customColor : Color.white;
                float y = screenHeight - screenPoint.y - vertical;

                // draw player name
                GUI.Label(new Rect(screenPoint.x - 5, y, 10, 11), PlayerName, StylePresent.style);
                if (ShowHealthBar)
                {
                    y += vertical;
                    GUI.color = StylePresent.HealthBackColor;
                    GUI.DrawTexture(new Rect(screenPoint.x - halfHealthBarWidth, y, StylePresent.HealthBarWidth, StylePresent.HealthBarThickness), StylePresent.HealthBarTexture);
                    GUI.color = hasCustomColor ? customColor : StylePresent.HealthBarColor;
                    GUI.DrawTexture(new Rect(screenPoint.x - halfHealthBarWidth, y, StylePresent.HealthBarWidth * ((float)playerRefs.GetHealth() / (float)playerRefs.GetMaxHealth()), StylePresent.HealthBarThickness), StylePresent.HealthBarTexture);
                    GUI.color = Color.white;
                }

#if !UNITY_WEBGL && PVOICE
                //voice chat icon
                if (voiceView != null && voiceView.IsSpeaking)
                {
                    GUI.DrawTexture(new Rect(screenPoint.x + RightPand, (screenHeight - screenPoint.y) - vertical, 14, 14), StylePresent.TalkingIcon);
                }
#endif
            }
            else if ((distance <= positionIndicationMaxDistance || positionIndicationMaxDistance == 0) && playerRefs.IsTeamMateOfLocalPlayer())
            {
                float iconSize = StylePresent.IndicatorIconSize;
                GUI.DrawTexture(new Rect(screenPoint.x - halfIconSize, screenHeight - screenPoint.y - halfIconSize, iconSize, iconSize), StylePresent.IndicatorIcon);
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public override void OnSlowUpdate()
    {
        if (bl_CameraIdentity.CurrentCamera == null || !isDrawing)
            return;

        distance = bl_UtilityHelper.Distance(CachedTransform.position, bl_CameraIdentity.CurrentCamera.transform.position);
    }
}