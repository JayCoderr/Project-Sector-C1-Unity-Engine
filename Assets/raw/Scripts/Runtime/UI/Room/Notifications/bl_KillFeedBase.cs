using System;
using System.Collections.Generic;
using UnityEngine;
using HashTable = ExitGames.Client.Photon.Hashtable;

public abstract class bl_KillFeedBase : bl_PhotonHelper
{

    public List<CustomIcons> customIcons = new();

    /// <summary>
    /// 
    /// </summary>
    public struct FeedData
    {
        public string LeftText;
        public string CenterText;
        public string RightText;
        public Team Team;
        public Dictionary<string, object> Data;

        /// <summary>
        /// 
        /// </summary>
        public void AddData(string key, object value)
        {
            if (Data == null) Data = new Dictionary<string, object>();
            Data.Add(key, value);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class CustomIcons
    {
        public string Name;
        public Sprite Icon;
    }

    /// <summary>
    /// 
    /// </summary>
    public abstract void SendKillMessageEvent(FeedData feedData);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="message"></param>
    /// <param name="localOnly"></param>
    public abstract void SendMessageEvent(string message, bool localOnly = false);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="teamHighlightMessage"></param>
    /// <param name="normalMessage"></param>
    /// <param name="playerTeam"></param>
    public abstract void SendTeamHighlightMessage(string teamHighlightMessage, string normalMessage, Team playerTeam);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="data"></param>
    public abstract void OnMessageReceive(HashTable data);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="keyName"></param>
    /// <param name="icon"></param>
    public virtual int AddCustomIcon(string keyName, Sprite icon)
    {
        int indexOf = customIcons.FindIndex(x => x.Name == keyName);
        if (indexOf != -1) return indexOf;

        customIcons.Add(new CustomIcons()
        {
            Name = keyName,
            Icon = icon
        });
        return customIcons.Count - 1;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="keyName"></param>
    /// <returns></returns>
    public virtual int GetCustomIconIndex(string keyName)
    {
        return customIcons.FindIndex(x => x.Name == keyName);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public virtual CustomIcons GetCustomIconByIndex(int index)
    {
        return index > customIcons.Count - 1 ? null : customIcons[index];
    }

    /// <summary>
    /// 
    /// </summary>
    public virtual Sprite GetCustomIcon(string keyName)
    {
        Sprite spr = null;
        if (customIcons.Exists(x => x.Name == keyName))
        {
            spr = customIcons.Find(x => x.Name == keyName).Icon;
        }
        else { Debug.LogWarning($"Custom icon {keyName} has not been register in the custom icons list"); }

        return spr;
    }

    private static bl_KillFeedBase _instance;
    public static bl_KillFeedBase Instance
    {
        get
        {
            if (_instance == null) { _instance = FindAnyObjectByType<bl_KillFeedBase>(); }
            return _instance;
        }
    }
}