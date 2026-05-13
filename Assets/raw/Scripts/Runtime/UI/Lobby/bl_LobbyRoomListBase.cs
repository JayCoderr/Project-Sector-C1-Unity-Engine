using Photon.Realtime;
using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class bl_LobbyRoomListBase : MonoBehaviour
{
    /// <summary>
    /// This should be called when the cache room list is updated (add or remove a item)
    /// </summary>
    public Action<Dictionary<string, RoomInfo>> onRoomListUpdated;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="rooms"></param>
    public abstract void SetRoomList(List<RoomInfo> rooms);

    /// <summary>
    /// Clear all the rooms (including the cache) in the list.
    /// </summary>
    public abstract void ClearList();

    /// <summary>
    /// 
    /// </summary>
    private static bl_LobbyRoomListBase _instance;
    public static bl_LobbyRoomListBase Instance
    {
        get
        {
            if (_instance == null) _instance = FindAnyObjectByType<bl_LobbyRoomListBase>();
            if (_instance == null && bl_LobbyUI.Instance != null) _instance = bl_LobbyUI.Instance.GetComponentInChildren<bl_LobbyRoomListBase>(true);
            return _instance;
        }
    }
}
