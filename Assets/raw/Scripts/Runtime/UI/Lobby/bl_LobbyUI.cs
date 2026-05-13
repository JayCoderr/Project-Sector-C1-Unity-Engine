using MFPS.Audio;
using MFPS.Runtime.FriendList;
using MFPS.Runtime.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class bl_LobbyUI : MonoBehaviour
{
    public string MainWindowName = "room list";
    public List<WindowUI> windows = new();
    public List<PopUpWindows> popUpWindows = new();

    [Header("References")]
    public CanvasGroup FadeAlpha;
    public GameObject SeekingMatchUI;
    public Image LevelIcon;
    public TMP_InputField PlayerNameField = null;
    public bl_FriendListUIBase FriendUI;
    public GameObject overAllEmptyLoading;
    public bl_ConfirmationWindow confirmationWindow;
    public bl_ConfirmationWindow messageWindow;
    public bl_ConfirmationWindow connectionPopupMessage;
    public bl_CanvasGroupFader blackScreenFader;
    public GameObject[] AddonsButtons;

    #region Private members
    private string currentWindow = "";
    private bool areWindowsSetup = false;
    private Coroutine fadeCoroutine;
    #endregion

    /// <summary>
    /// 
    /// </summary>
    public void InitialSetup()
    {
        bl_EventHandler.onGameSettingsChange += ApplyRuntimeSettings;
#if LOCALIZATION
        bl_Localization.Instance.OnLanguageChange += OnLanguageChange;
#endif
        if (!areWindowsSetup) windows.ForEach(x => { if (x.UIRoot != null) { x.UIRoot.SetActive(false); } });//disable all windows
        blackScreenFader.SetAlpha(1);
    }

    /// <summary>
    /// 
    /// </summary>
    private void OnEnable()
    {
#if ULSP
        if (bl_DataBase.IsUserLogged) bl_DataBase.OnUpdateData += OnUpdateDataBaseInfo;
#endif
    }

    /// <summary>
    /// 
    /// </summary>
    void OnDisable()
    {
        bl_EventHandler.onGameSettingsChange -= ApplyRuntimeSettings;
#if LOCALIZATION
        bl_Localization.Instance.OnLanguageChange -= OnLanguageChange;
#endif
#if ULSP
        if (bl_DataBase.IsUserLogged) bl_DataBase.OnUpdateData -= OnUpdateDataBaseInfo;
#endif
    }

    /// <summary>
    /// 
    /// </summary>
    public void ChangeWindow(string window)
    {
        if (window == currentWindow) return;//return if we are trying to open the opened window
        WindowUI wData = windows.Find(x => x.Name == window);
        if (wData == null) return;//the window with that windowName doesn't exist

        for (int i = 0; i < windows.Count; i++)
        {
            WindowUI w = windows[i];
            if (w.UIRoot == null) continue;

            if (w.isPersistent)
            {
                w.UIRoot.SetActive(!wData.hidePersistents);
            }
            else
            {
                if (w.MenuButton != null) w.MenuButton.interactable = true;
                w.UIRoot.SetActive(false);
            }
        }

        wData.UIRoot.SetActive(true);
        if (wData.MenuButton != null) wData.MenuButton.interactable = false;
        if (wData.showTopMenu)
        {
            SetEnableWindow("top menu", true);
        }

        if (wData.playFadeInOut)
        {
            if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
            fadeCoroutine = StartCoroutine(DoFadeIn());
        }

        currentWindow = window;
        areWindowsSetup = true;
        bl_Lobby.onMenuWindowChange?.Invoke(window);
    }

    /// <summary>
    /// 
    /// </summary>
    public void SetEnableWindow(string windowName, bool active)
    {
        WindowUI w = windows.Find(x => x.Name == windowName);
        if (w == null || w.UIRoot == null) return;//the window with that windowName doesn't exist

        w.UIRoot.SetActive(active);
        if (w.MenuButton != null) w.MenuButton.interactable = !active;
    }

    public void Home() { ChangeWindow(MainWindowName); bl_Lobby.Instance.onShowMenu?.Invoke(); }
    public void HideAll() { currentWindow = ""; windows.ForEach(x => { if (x.UIRoot != null) { x.UIRoot.SetActive(false); } }); }//disable all windows 

    /// <summary>
    /// 
    /// </summary>
    IEnumerator DoFadeIn()
    {
        float d = 0;
        while (d < 1)
        {
            d += Time.deltaTime * 2;
            FadeAlpha.alpha = d;
            yield return null;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void ShowPopUpWindow(string popUpName)
    {
        PopUpWindows w = popUpWindows.Find(x => x.Name == popUpName);
        if (w == null || w.Window == null) return;

        w.Window.SetActive(true);
    }

    /// <summary>
    /// 
    /// </summary>
    public void SignOut()
    {
        bl_Lobby.Instance.SignOut();
    }

    #region Settings

    /// <summary>
    /// 
    /// </summary>
    public void Setup()
    {
        LoadSettings();
        FullSetUp();
    }

    /// <summary>
    /// 
    /// </summary>
    public void LoadSettings()
    {
        ApplyRuntimeSettings();

        bool km = PlayerPrefs.GetInt(PropertiesKeys.KickKey, 0) == 1;
        if (km) { ShowPopUpWindow("kicked"); }
        if (bl_PhotonNetwork.Instance.hasPingKick) { ShowPopUpWindow("ping kick"); bl_PhotonNetwork.Instance.hasPingKick = false; }
        PlayerPrefs.SetInt(PropertiesKeys.KickKey, 0);
        bl_Lobby.Instance.rememberMe = !string.IsNullOrEmpty(PlayerPrefs.GetString(PropertiesKeys.GetUniqueKey("remembernick"), string.Empty));
        if (bl_PhotonNetwork.Instance.HasAFKKick) { ShowPopUpWindow("afk kick"); bl_PhotonNetwork.Instance.HasAFKKick = false; }

        bl_Input.Initialize();
        bl_Input.CheckGamePadRequired();

#if CUSTOMIZER
        SetActiveAddonObject(2, true);
#endif
    }

    /// <summary>
    /// 
    /// </summary>
    void ApplyRuntimeSettings()
    {
        if (bl_MFPS.Settings != null)
        {
            Application.targetFrameRate = bl_MFPS.Settings.RefreshRates[(int)bl_MFPS.Settings.GetSettingOf("Frame Rate")];
            bl_MFPS.MusicVolume = (float)bl_MFPS.Settings.GetSettingOf("Music Volume");
            if (bl_AudioController.Instance != null)
            {
                bl_AudioController.Instance.BackgroundVolume = bl_MFPS.MusicVolume;
                bl_AudioController.Instance.MaxBackgroundVolume = bl_MFPS.MusicVolume;
                bl_AudioController.Instance.PlayBackground();
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void FullSetUp()
    {
#if LM
        if (LevelIcon != null)
        {
            LevelIcon.gameObject.SetActive(true);
            var pli = bl_LevelManager.Instance.GetLevel();
            LevelIcon.sprite = pli.Icon;
            var plt = LevelIcon.GetComponentInChildren<TextMeshProUGUI>();
            if (plt != null) plt.text = pli.LevelID.ToString();
        }
#else
        if (LevelIcon != null) LevelIcon.gameObject.SetActive(false);
#endif

#if SHOP
        SetActiveAddonObject(9, true);
#endif
#if PSELECTOR
        SetActiveAddonObject(10, !bl_PlayerSelector.InMatch);
#endif
        SetActiveAddonObject(0, false);
#if CLASS_CUSTOMIZER
        SetActiveAddonObject(0, true);
#endif
    }

    /// <summary>
    /// 
    /// </summary>
    void OnLanguageChange(Dictionary<string, string> lang)
    {
#if LOCALIZATION
        bl_LobbyRoomCreatorUI.Instance.SetupSelectors();
#endif
    }

    /// <summary>
    /// 
    /// </summary>
    public void ResetSettings()
    {
        LoadSettings();
        FullSetUp();
    }
    #endregion

    #region UI Callbacks
    /// <summary>
    /// 
    /// </summary>
    public void EnterName(TMP_InputField field = null)
    {
        if (field == null || string.IsNullOrEmpty(field.text))
            return;

        bl_Lobby.Instance.SetPlayerName(field.text);
    }

    public void LoadLocalLevel(string level)
    {
        bl_Lobby.Instance.LoadLocalLevel(level);
    }

    public void LoadLocalLevelAdditive(string level)
    {
        bl_UtilityHelper.LoadLevelAdditive(level);
    }

    /// <summary>
    /// 
    /// </summary>
    public void ShowBuyCoins()
    {
#if SHOP
        MFPS.Shop.bl_CoinsWindow.Instance.SetActive(true);
#else
        Debug.Log("Require shop addon.");
#endif
    }

#if ULSP
    void OnUpdateDataBaseInfo(MFPS.ULogin.LoginUserInfo info)
    {
        bl_EventHandler.DispatchCoinUpdate(null);
    }
#endif

    /// <summary>
    /// 
    /// </summary>
    public void Quit()
    {
        confirmationWindow.AskConfirmation(bl_GameTexts.QuitGameConfirmation, () =>
         {
#if UNITY_EDITOR
             UnityEditor.EditorApplication.ExecuteMenuItem("Edit/Play");
#else
             Application.Quit();
#endif
         });
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <param name="window"></param>
    /// <param name="showTopMenu"></param>
    public WindowUI AddMainMenuOption(string name, GameObject window, bool showTopMenu = true)
    {
        var refWindow = windows.Find(x => x.Name == "home");

        if (window == null)
        {
            window = new GameObject(name);
            RectTransform rectTransform = window.AddComponent<RectTransform>();
            rectTransform.SetParent(refWindow.UIRoot.transform.parent, false);

            var refRect = refWindow.UIRoot.GetComponent<RectTransform>();
            rectTransform.anchorMin = refRect.anchorMin;
            rectTransform.anchorMax = refRect.anchorMax;
            rectTransform.pivot = refRect.pivot;
            rectTransform.sizeDelta = refRect.sizeDelta;
            rectTransform.anchoredPosition = refRect.anchoredPosition;
            try
            {
                rectTransform.SetSiblingIndex(6);
            }
            catch { }
        }

        var templateButton = refWindow.MenuButton;
        var btnNew = Instantiate(templateButton.gameObject, templateButton.transform.parent, false);
        btnNew.name = name;
        Button btn = btnNew.GetComponent<Button>();
        btn.onClick.RemoveAllListeners();

        if (!Application.isPlaying)
        {
#if UNITY_EDITOR
            UnityEditor.Events.UnityEventTools.UnregisterPersistentListener(btn.onClick, 0);
            var targetInfo = UnityEngine.Events.UnityEvent.GetValidMethodInfo(this, nameof(ChangeWindow), new Type[] { typeof(string) });
            UnityEngine.Events.UnityAction<string> methodDelegate = Delegate.CreateDelegate(typeof(UnityEngine.Events.UnityAction<string>), this, targetInfo) as UnityEngine.Events.UnityAction<string>;
            UnityEditor.Events.UnityEventTools.AddStringPersistentListener(btn.onClick, methodDelegate, name);
#endif
        }
        else
        {
            btn.onClick.AddListener(() => ChangeWindow(name));
        }

        var btnText = btn.GetComponentInChildren<TextMeshProUGUI>(true);
        btnText.text = name.ToUpper();
        if (btnText.TryGetComponent(out bl_LocalizationUI lui))
        {
            DestroyImmediate(lui);
        }

        var windowUI = new WindowUI()
        {
            Name = name,
            UIRoot = window,
            MenuButton = btn,
            showTopMenu = showTopMenu,
            playFadeInOut = true
        };
        windows.Add(windowUI);

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            UnityEditor.EditorUtility.SetDirty(btn);
            UnityEditor.EditorUtility.SetDirty(btnText);
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
        return windows[windows.Count - 1];
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="index"></param>
    /// <param name="active"></param>
    public void SetActiveAddonObject(int index, bool active)
    {
        if (index < 0 || index > AddonsButtons.Length - 1) return;
        if (AddonsButtons[index] == null) return;

        AddonsButtons[index].SetActive(active);
    }

    public static void ShowEmptyLoading(bool active) => Instance.overAllEmptyLoading.SetActive(active);
    public static void ShowOverAllMessage(string text) => Instance.messageWindow?.ShowMessage(text);
    public static void ShowConfirmationWindow(string text, Action onAccept, Action onCancel = null) => Instance.confirmationWindow?.AskConfirmation(text, onAccept, onCancel);

    public void AutoMatch() { bl_Lobby.Instance.AutoMatch(); }
    public void CreateRoom() { bl_Lobby.Instance.CreateRoom(); }
    public void SetRememberMe(bool value) { bl_Lobby.Instance.SetRememberMe(value); }
    #endregion

    #region Classes
    [System.Serializable]
    public class WindowUI
    {
        public string Name;
        public GameObject UIRoot;
        public Button MenuButton;

        public bool isPersistent = false;//this window will stay show up when change window?
        public bool hidePersistents = false;//force hide persistent windows
        public bool playFadeInOut = false;
        public bool showTopMenu = true;
    }

    [System.Serializable]
    public class PopUpWindows
    {
        public string Name;
        public GameObject Window;
    }

    private static bl_LobbyUI _instance;
    public static bl_LobbyUI Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindAnyObjectByType<bl_LobbyUI>();
            }
            return _instance;
        }
    }
    #endregion
}