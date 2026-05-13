using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class bl_PauseMenu : bl_PauseMenuBase
{
    #region Public members    
    public string firstWindow = "scoreboard";

    [Header("Windows")]
    public List<MenuWindow> windows;

    [Header("Events")]
    public bl_EventHandler.UEvent onOpen;
    public bl_EventHandler.UEvent onClose;

    [Header("References")]
    [SerializeField] GameObject content = null;
    [SerializeField] private GameObject[] layoutRoots = null;
    #endregion

    #region Private members
    private bool isMatchFinish = false;
    #endregion

    /// <summary>
    /// 
    /// </summary>
    private void Start()
    {
        if (!IsMenuOpen) CloseMenu();
    }

    /// <summary>
    /// 
    /// </summary>
    private void OnEnable()
    {
        bl_EventHandler.Match.onMatchOver += OnMatchEnd;
        bl_EventHandler.onRoomPropertiesReset += OnMatchReset;
    }

    /// <summary>
    /// 
    /// </summary>
    private void OnDisable()
    {
        bl_EventHandler.Match.onMatchOver -= OnMatchEnd;
        bl_EventHandler.onRoomPropertiesReset -= OnMatchReset;
    }

    /// <summary>
    /// Open the pause menu
    /// </summary>
    public override void OpenMenu(string windowOpen = "")
    {
        if (isMatchFinish && !bl_GameData.CoreSettings.allowMenuWhenMatchOver) return; // can't open the menu if the match is over

        var win = string.IsNullOrEmpty(windowOpen) ? firstWindow : windowOpen;
        SetActiveLayouts(LayoutPart.Header | LayoutPart.Body | LayoutPart.Footer);
        OpenWindow(win);
        isMenuOpen = true;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="windowName"></param>
    public override void OpenWindow(string windowName)
    {
        var id = windows.FindIndex(x => x.Name == windowName);
        if (id == -1)
        {
            Debug.LogWarning($"Pause window {windowName} doesn't exist.");
            return;
        }
        OpenWindow(id);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="windowName"></param>
    public override void OpenWindow(int windowIndex)
    {
        if (windowIndex == CurrentWindow && isMenuOpen) return;

        content.SetActive(true);
        CurrentWindow = windowIndex;

        windows.ForEach(x => x.SetActive(false));

        // if the current game mode doesn't allow the player to see the scoreboard, don't open it
        if (windows[windowIndex].Name == "scoreboard" && !bl_MFPS.RoomGameMode.CurrentGameModeData.ShowPlayerScoreboard)
        {
            onOpen?.Invoke();
            return;
        }

        var window = windows[CurrentWindow];
        window.SetActive(true);
        onOpen?.Invoke();
    }

    /// <summary>
    /// 
    /// </summary>
    public override void CloseMenu()
    {
        isMenuOpen = false;
        onClose?.Invoke();
        content.SetActive(false);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="windowName"></param>
    public override void CloseWindow(string windowName)
    {
        var id = windows.FindIndex(x => x.Name == windowName);
        if (id != -1)
        {
            var win = windows[id];
            if (win.Window != null) win.SetActive(false);
        }
    }

    /// <summary>
    /// Active the given menu layout parts only (disable the not given parts)
    /// </summary>
    /// <param name="layouts"></param>
    public override void SetActiveLayouts(LayoutPart layouts)
    {
        layoutRoots[0].SetActive(layouts.IsEnumFlagPresent(LayoutPart.Header));
        layoutRoots[1].SetActive(layouts.IsEnumFlagPresent(LayoutPart.Body));
        layoutRoots[2].SetActive(layouts.IsEnumFlagPresent(LayoutPart.Footer));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="windowName"></param>
    /// <returns></returns>
    public bool IsWindowOpen(string windowName)
    {
        if (CurrentWindow < 0) return false;
        return windows[CurrentWindow].Name == windowName;
    }

    /// <summary>
    /// 
    /// </summary>
    void OnMatchEnd()
    {
        CloseMenu();
        isMatchFinish = true;
    }

    /// <summary>
    /// 
    /// </summary>
    void OnMatchReset()
    {
        isMatchFinish = false;
    }

    [Serializable]
    public class MenuWindow
    {
        public string Name;
        public GameObject Window;
        public Button OpenButton;
        public bl_EventHandler.UEvent onOpen;

        public void SetActive(bool active)
        {
            if (Window != null) Window.SetActive(active);
            if (OpenButton != null) OpenButton.interactable = !active;
            onOpen?.Invoke();
        }
    }
}