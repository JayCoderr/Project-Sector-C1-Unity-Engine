using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Use this script to enable or disable a GameObject depending on the current game mode.
/// </summary>
public class bl_GameModeObject : bl_PhotonHelper
{
    public Mode enableIf = Mode.IfIsGameMode;
    [LovattoToogle] public bool allowOfflineMode = true;
    public List<GameMode> m_GameModes;
    public GameObject TargetObject;

    public enum Mode
    {
        IfIsGameMode,
        IfIsNotGameMode,
    }

    /// <summary>
    /// 
    /// </summary>
    private void Awake()
    {
        if (!bl_PhotonNetwork.IsConnectedInRoom && !allowOfflineMode)
        {
            GetGameModeObject().SetActive(false);
            return;
        }

        DoCheck();
    }

    /// <summary>
    /// 
    /// </summary>
    public void DoCheck()
    {
        if (m_GameModes == null || m_GameModes.Count <= 0) return;

        bool gameModeExist = m_GameModes.Contains(GetGameMode);

        if (enableIf == Mode.IfIsGameMode)
        {
            GetGameModeObject().SetActive(gameModeExist);
        }
        else
        {
            GetGameModeObject().SetActive(!gameModeExist);
        }
    }

    private GameObject GetGameModeObject()
    {
        if (TargetObject == null) return gameObject;
        return TargetObject;
    }

}