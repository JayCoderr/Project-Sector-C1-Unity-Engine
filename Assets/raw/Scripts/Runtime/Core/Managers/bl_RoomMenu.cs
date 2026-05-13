using UnityEngine;

[DefaultExecutionOrder(-800)]
public class bl_RoomMenu : bl_MonoBehaviour
{
    public enum ControlFocus
    {
        None,
        Chat,
        InputBinding
    }

    /// <summary>
    /// Return which part of the game is currently using the Input/Control/Keyboard
    /// </summary>
    public ControlFocus CurrentControlFocus
    {
        get;
        set;
    } = ControlFocus.None;

    /// <summary>
    /// Is currently the game showing the pause menu?
    /// </summary>
    public bool isPaused
    {
        get => bl_PauseMenuBase.IsMenuOpen;
    }

    /// <summary>
    /// Is the game quitting?
    /// </summary>
    public bool IsApplicationQuitting
    {
        get;
        private set;
    } = false;

    /// <summary>
    /// 
    /// </summary>
    protected override void Awake()
    {
        if (!IsConnected)
            return;

        base.Awake();
        bl_MFPSDatabase.PlayTime.StartRecordingTime();
        bl_Input.CheckGamePadRequired();
    }

    /// <summary>
    /// 
    /// </summary>
    protected override void OnEnable()
    {
#if MFPSM
        bl_TouchHelper.OnPause += TogglePause;
#endif
    }

    /// <summary>
    /// 
    /// </summary>
    protected override void OnDisable()
    {
#if MFPSM
        bl_TouchHelper.OnPause -= TogglePause;
#endif       
    }

    /// <summary>
    /// 
    /// </summary>
    public override void OnUpdate()
    {
        bl_GameInput.IsCursorLocked = bl_UtilityHelper.GetCursorState;
        PauseControll();
        ScoreboardInput();
    }

    /// <summary>
    /// Check the input for open/hide the pause menu
    /// </summary>
    void PauseControll()
    {
        // Force lock cursor when press left Alt + left mouse button
        if (Input.GetKey(KeyCode.LeftAlt))
        {
            if (Input.GetMouseButtonDown(0))
            {
                bl_UtilityHelper.LockCursor(true);
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            bl_UtilityHelper.LockCursor(false);
        }

        if (CurrentControlFocus == ControlFocus.InputBinding) return;

        if (bl_GameInput.Pause()) TogglePause();
    }

    /// <summary>
    /// Toggle the current paused state
    /// </summary>
    public void TogglePause()
    {
        if (bl_GameManager.Instance.GameFinish) return;

        bool paused = isPaused;
        paused = !paused;

        bl_PauseMenuBase.ShowPauseMenu(paused);
        bl_UtilityHelper.LockCursor(!paused);
        bl_CrosshairBase.Instance?.Show(!paused);
        bl_EventHandler.DispatchGamePauseEvent(paused);
    }

    /// <summary>
    /// 
    /// </summary>
    void ScoreboardInput()
    {
        if (bl_GameManager.Instance.GameFinish || bl_PauseMenuBase.IsMenuOpen) return;

        if (bl_GameInput.Scoreboard())
        {
            bl_PauseMenuBase.Instance.SetActiveLayouts(bl_PauseMenuBase.LayoutPart.Body);
            bl_PauseMenuBase.Instance.OpenWindow("scoreboard");
        }
        else if (bl_GameInput.Scoreboard(GameInputType.Up))
        {
            bl_PauseMenuBase.Instance.CloseMenu();
        }
    }

    /// <summary>
    /// Leave the current room (if exist) and return to the lobby
    /// </summary>
    public void LeaveRoom(RoomLeaveCause leaveCause, bool save = true)
    {
        if (save)
        {
            // store match stats
            bl_MFPSDatabase.User.StorePlayerMatchStats();
            bl_MFPSDatabase.PlayTime.StopRecordingPlayTime();
        }

        bl_EventHandler.Match.onLeaveRoom?.Invoke(leaveCause);

        //Good place to save info before reset statistics
        if (bl_PhotonNetwork.IsConnected && bl_PhotonNetwork.InRoom)
        {
            bl_PhotonNetwork.CleanBuffer();
            bl_PhotonNetwork.LeaveRoom();
        }
        else
        {
#if UNITY_EDITOR
            if (IsApplicationQuitting) { return; }
#endif
            bl_UtilityHelper.LoadLevel(bl_GameData.CoreSettings.MainMenuScene);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void OnApplicationQuit()
    {
        IsApplicationQuitting = true;
    }

    private static bl_RoomMenu _instance;
    public static bl_RoomMenu Instance
    {
        get
        {
            if (_instance == null) { _instance = FindAnyObjectByType<bl_RoomMenu>(); }
            return _instance;
        }
    }
}