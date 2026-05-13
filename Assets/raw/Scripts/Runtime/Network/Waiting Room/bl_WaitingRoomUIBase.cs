public abstract class bl_WaitingRoomUIBase : bl_PhotonHelper
{

    /// <summary>
    /// 
    /// </summary>
    /// <param name="show"></param>
    public abstract void ShowLoadingScreen(bool show);

    /// <summary>
    /// Show the waiting room menu
    /// </summary>
    public abstract void SetActive(bool active);

    /// <summary>
    /// 
    /// </summary>
    public abstract void UpdatePlayerCount();

    /// <summary>
    /// 
    /// </summary>
    public abstract void UpdateRoomInfoUI();

    /// <summary>
    /// Update the player list with the room players
    /// </summary>
    public abstract void UpdatePlayerList();

    /// <summary>
    /// Update the player list with the room players without instance them again
    /// </summary>
    public virtual void UpdatePlayers() { }

    private static bl_WaitingRoomUIBase _instance;
    public static bl_WaitingRoomUIBase Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindAnyObjectByType<bl_WaitingRoomUIBase>();
            }
            return _instance;
        }
    }
}