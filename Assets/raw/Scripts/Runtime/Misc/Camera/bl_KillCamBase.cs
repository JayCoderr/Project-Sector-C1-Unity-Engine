using System.Collections.Generic;
using UnityEngine;

public abstract class bl_KillCamBase : bl_MonoBehaviour
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="active"></param>
    public virtual void SetActive(bool active)
    {
        gameObject.SetActive(active);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="info"></param>
    /// <returns></returns>
    public abstract bl_KillCamBase SetTarget(KillCamInfo info);

    /// <summary>
    /// Find a random player to spectate
    /// </summary>
    public abstract void FindATarget();

    /// <summary>
    /// Get a list of the available players to spectate
    /// </summary>
    /// <returns></returns>
    public virtual List<MFPSPlayer> GetSpectateList()
    {
        var list = bl_GameManager.Instance.OthersActorsInScene;
        if (bl_MFPS.RoomGameMode.CurrentGameModeData.AllowSpectateEnemies)
        {
            return list;
        }

        var teamList = new List<MFPSPlayer>();
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i] != null && list[i].Team == bl_MFPS.LocalPlayer.Team)
            {
                teamList.Add(list[i]);
            }
        }
        return teamList;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public MFPSPlayer GetAFriendInstance()
    {
        var list = bl_GameManager.Instance.OthersActorsInScene;
        for (int i = 0; i < list.Count; i++)
        {
            var player = list[i];
            if (player != null && player.Team == bl_MFPS.LocalPlayer.Team && player.Actor != null && player.isAlive)
            {
                return list[i];
            }
        }

        return null;
    }

    private static bl_KillCamBase _killcam;
    public static bl_KillCamBase Instance
    {
        get
        {
            if (_killcam == null) _killcam = FindAnyObjectByType<bl_KillCamBase>();
            return _killcam;
        }
    }

    public struct KillCamInfo
    {
        public Transform Target;
        public string TargetName;
        public int GunID;//with which the player was terminated
        public Transform FallbackTarget;

        public bool IsTargetChangeOnly;
        public Dictionary<string, object> CustomProperties;

        public KillCamInfo SetProperty(string key, object prop)
        {
            CustomProperties ??= new Dictionary<string, object>();
            if (CustomProperties.ContainsKey(key))
            {
                CustomProperties[key] = prop;
            }
            else
            {
                CustomProperties.Add(key, prop);
            }

            return this;
        }

        public object GetProperty(string key)
        {
            if (CustomProperties == null || !CustomProperties.ContainsKey(key)) return null;
            return CustomProperties[key];
        }
    }
}