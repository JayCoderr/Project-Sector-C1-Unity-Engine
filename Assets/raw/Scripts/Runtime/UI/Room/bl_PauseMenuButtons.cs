using UnityEngine;

public class bl_PauseMenuButtons : MonoBehaviour
{
    [SerializeField] private GameObject suicideButton = null;
    [SerializeField] private GameObject enterSpectatorButton = null;

    /// <summary>
    /// 
    /// </summary>
    private void OnEnable()
    {
        Setup();

        bl_EventHandler.Player.onLocalJoinAsPlayer += OnLocalJoinedAsPlayer;
        bl_EventHandler.Player.onLocalJoinTeam += OnLocalJoinTeam;
    }

    /// <summary>
    /// 
    /// </summary>
    private void OnDisable()
    {
        bl_EventHandler.Player.onLocalJoinAsPlayer -= OnLocalJoinedAsPlayer;
        bl_EventHandler.Player.onLocalJoinTeam -= OnLocalJoinTeam;
    }

    /// <summary>
    /// 
    /// </summary>
    private void Setup()
    {
        if (suicideButton != null)
        {
            suicideButton.SetActive(bl_RoomSettings.Instance.canSuicide && bl_GameManager.IsLocalPlaying);
        }
        if (enterSpectatorButton != null)
        {
            enterSpectatorButton.SetActive(!bl_GameManager.Joined);
        }
    }

    private void OnLocalJoinedAsPlayer()
    {
        if (enterSpectatorButton != null)
        {
            enterSpectatorButton.SetActive(false);
        }
    }

    private void OnLocalJoinTeam(Team team)
    {
        if (enterSpectatorButton != null)
        {
            enterSpectatorButton.SetActive(false);
        }
    }

    #region Button Callbacks
    /// <summary>
    /// 
    /// </summary>
    public void Suicide()
    {
        if (!bl_MFPS.LocalPlayer.Suicide())
        {
            bl_EventHandler.DispatchGamePauseEvent(false);
        }
        bl_UtilityHelper.LockCursor(true);
        bl_PauseMenuBase.ShowPauseMenu(false);
    }

    /// <summary>
    /// This should be called from the Leave Room button.
    /// </summary>
    public void LeaveRoom()
    {
        bl_UtilityHelper.LockCursor(false);
        bl_UIReferences.Instance.leaveRoomConfirmation.AskConfirmation(bl_GameTexts.LeaveMatchWarning.Localized("areusulega"), () => { bl_UIReferences.Instance.LeftRoom(true); }, () => { });
    }

    /// <summary>
    /// This should be called from the Enter In Spectator mode button.
    /// </summary>
    public void EnterInSpectatorMode()
    {
        if (bl_SpectatorModeBase.Instance == null) return;

        bl_SpectatorModeBase.Instance.SetActiveSpectatorMode(true);
    }
    #endregion
}