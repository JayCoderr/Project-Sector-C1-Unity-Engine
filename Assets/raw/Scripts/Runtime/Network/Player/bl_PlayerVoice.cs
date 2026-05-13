using Photon.Pun;
using UnityEngine;
#if !UNITY_WEBGL && PVOICE
using Photon.Voice.Unity;
#endif

public class bl_PlayerVoice : bl_MonoBehaviour
{
    private GameObject RecorderIcon;
#if !UNITY_WEBGL && PVOICE
    private Recorder VoiceRecorder;
    private Speaker Speaker;
    private bool PushToTalk = false;
    private bool voiceEnabled = false;
    private bool canHearEnemies = false;
#endif
    private PhotonView View;

    /// <summary>
    /// 
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        View = photonView;
        if (!bl_PhotonNetwork.InRoom || bl_PhotonNetwork.OfflineMode)
            return;

        RecorderIcon = bl_UIReferences.Instance.SpeakerIcon;
#if !UNITY_WEBGL && PVOICE
        VoiceRecorder = FindAnyObjectByType<Recorder>();
        Speaker = GetComponent<Speaker>();
        canHearEnemies = bl_GameData.CoreSettings.canHearEnemiesVoiceChat;
#endif
        OnSettingsSaved();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        bl_EventHandler.Gameplay.onGameSettingsSaved += OnSettingsSaved;
#if MFPSM
        if (View.IsMine)
        {
            bl_TouchHelper.OnTransmit += OnPushToTalkMobile;
        }
#endif
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        bl_EventHandler.Gameplay.onGameSettingsSaved -= OnSettingsSaved;
#if MFPSM
        if (View.IsMine)
        {
            bl_TouchHelper.OnTransmit -= OnPushToTalkMobile;
        }
#endif
    }

    public void OnPushToTalkMobile(bool transmit)
    {
#if !UNITY_WEBGL && PVOICE
        if (VoiceRecorder != null)
            VoiceRecorder.TransmitEnabled = transmit;
#endif       
    }

#if !UNITY_WEBGL && PVOICE
    /// <summary>
    /// 
    /// </summary>
    public override void OnUpdate()
    {
        if (VoiceRecorder == null) return;
        if (!bl_PhotonNetwork.InRoom || bl_PhotonNetwork.OfflineMode)
            return;

        if (View.IsMine)
        {
            RecorderIcon.SetActive(VoiceRecorder.TransmitEnabled && VoiceRecorder.IsCurrentlyTransmitting && VoiceRecorder.LevelMeter.CurrentPeakAmp > 0.1f);
            if (PushToTalk && !bl_UtilityHelper.isMobile)
            {
                VoiceRecorder.TransmitEnabled = bl_GameInput.Talk();
            }
        }
    }
#endif

    private void OnSettingsSaved()
    {
#if !UNITY_WEBGL && PVOICE
        if (View.IsMine)
        {
            voiceEnabled = (bool)bl_MFPS.Settings.GetSettingOf("Voice Chat");
            PushToTalk = (bool)bl_MFPS.Settings.GetSettingOf("PushToTalk");

            if (VoiceRecorder != null)
            {
                VoiceRecorder.TransmitEnabled = !PushToTalk;
                VoiceRecorder.enabled = voiceEnabled;
            }
            if (Speaker != null) Speaker.enabled = voiceEnabled;

#if MFPSM
            if (bl_TouchHelper.Instance != null)
            {
                bl_TouchHelper.Instance.OnPushToTalkChange(PushToTalk);
            }
#endif
        }
        else
        {
            if (!GetGameMode.IsOneTeamMode() && !canHearEnemies)
            {
                if (Speaker != null) Speaker.enabled = photonView.Owner.GetPlayerTeam() == bl_PhotonNetwork.LocalPlayer.GetPlayerTeam();
            }
        }
#else
        if (RecorderIcon) RecorderIcon.SetActive(false);
#endif
    }
}