using MFPS.Internal.Structures;
using System.Collections.Generic;
using UnityEngine;

public class bl_LocalKillNotifier : MonoBehaviour
{
    [Range(1, 7)] public float IndividualShowTime = 3;
    public GameObject multipleNotifierRoot;
    public RectTransform panelRect;
    public GameObject notificationTemplate;
    public bl_LocalKillUI staticNotification;

    private List<KillInfo> localKillsQueque = new List<KillInfo>();

    /// <summary>
    /// 
    /// </summary>
    private void Start()
    {
        multipleNotifierRoot.SetActive(bl_GameData.CoreSettings.localKillsShowMode == LocalKillDisplay.List);
    }

    /// <summary>
    /// 
    /// </summary>
    private void OnEnable()
    {
        bl_EventHandler.onLocalKill += OnLocalKill;
        bl_EventHandler.onLocalNotification += OnLocalNotification;
        bl_EventHandler.onLocalKillAssist += OnKillAssist;
    }

    /// <summary>
    /// 
    /// </summary>
    private void OnDisable()
    {
        bl_EventHandler.onLocalKill -= OnLocalKill;
        bl_EventHandler.onLocalNotification -= OnLocalNotification;
        bl_EventHandler.onLocalKillAssist -= OnKillAssist;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="info"></param>
    void OnLocalKill(KillInfo info)
    {
        if (bl_GameData.CoreSettings.localKillsShowMode == LocalKillDisplay.List) InstanceSingleNotification(info);
        else if (bl_GameData.CoreSettings.localKillsShowMode == LocalKillDisplay.Queqe) ShowNotification(info);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="notification"></param>
    void OnLocalNotification(MFPSLocalNotification notification)
    {

    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="data"></param>
    void OnKillAssist(bl_EventHandler.KillAssistData data)
    {
        int score = bl_GameData.ScoreSettings.ScorePerKillAssist;
        string killAssistText = bl_GameTexts.KillAssist.Localized("killassist").ToUpper();
        if (bl_GameData.CoreSettings.localKillsShowMode == LocalKillDisplay.Queqe)
        {
            if (staticNotification.IsShowing)
            {
                staticNotification.SetTextLine($"{killAssistText} +{score}");
            }
            else
            {
                new MFPSLocalNotification($"{killAssistText} +{score}");
            }
        }
        else
        {
            new MFPSLocalNotification($"{killAssistText} +{score}");
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="info"></param>
    void InstanceSingleNotification(KillInfo info)
    {
        GameObject newkillfeedh = Instantiate(notificationTemplate) as GameObject;
        newkillfeedh.GetComponent<bl_LocalKillUI>().InitMultiple(info, info.byHeadShot);
        notificationTemplate.SetActive(true);
        newkillfeedh.transform.SetParent(panelRect, false);
        newkillfeedh.transform.SetAsFirstSibling();
        notificationTemplate.SetActive(false);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="info"></param>
    void ShowNotification(KillInfo info)
    {
        if (localKillsQueque.Count <= 0)
        {
            staticNotification.InitIndividual(info);
        }
        localKillsQueque.Add(info);
    }

    /// <summary>
    /// 
    /// </summary>
    public void LocalDisplayDone()
    {
        localKillsQueque.RemoveAt(0);
        if (localKillsQueque.Count > 0)
        {
            staticNotification.InitIndividual(localKillsQueque[0]);
        }
    }

    private static bl_LocalKillNotifier _instance;
    public static bl_LocalKillNotifier Instance
    {
        get
        {
            if (_instance == null) { _instance = FindAnyObjectByType<bl_LocalKillNotifier>(); }
            return _instance;
        }
    }

    [System.Serializable]
    public enum LocalKillDisplay
    {
        Queqe,
        List,
        Custom,
    }
}