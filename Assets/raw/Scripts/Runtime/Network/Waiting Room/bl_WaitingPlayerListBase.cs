using Photon.Realtime;

public abstract class bl_WaitingPlayerListBase : bl_PhotonHelper
{
    /// <summary>
    /// 
    /// </summary>
    public abstract void InstancePlayerList();

    /// <summary>
    /// 
    /// </summary>
    public abstract void UpdatePlayerProperties();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="player"></param>
    public abstract void SetPlayerToList(Player player, Team team);
}