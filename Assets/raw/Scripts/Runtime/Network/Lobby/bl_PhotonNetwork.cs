using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;

[DefaultExecutionOrder(-1002)]
public class bl_PhotonNetwork : bl_PhotonHelper
{

    [HideInInspector] public bool hasPingKick = false;
    public bool HasAFKKick { get; set; }
    static readonly RaiseEventOptions EventsAll = new();
    private readonly List<PhotonEventsCallbacks> callbackList = new();

    /// <summary>
    /// 
    /// </summary>
    private void Awake()
    {
        gameObject.name = "MFPS Network";
        DontDestroyOnLoad(gameObject);
        EventsAll.Receivers = ReceiverGroup.All;
        PhotonNetwork.NetworkingClient.EventReceived += OnEventCustom;

        PhotonPeer.RegisterType(typeof(DamageData), (byte)'T', DamageData.SerializeDamageData, DamageData.DeserializeDamageData);
        PhotonPeer.RegisterType(typeof(BulletData), (byte)'R', BulletData.SerializeDamageData, BulletData.DeserializeDamageData);
    }

    /// <summary>
    /// 
    /// </summary>
    private void OnDisable()
    {
        PhotonNetwork.NetworkingClient.EventReceived -= OnEventCustom;
    }

    /// <summary>
    /// 
    /// </summary>
    public void AddCallback(byte code, Action<Hashtable> callback)
    {
        callbackList.Add(new PhotonEventsCallbacks() { Code = code, Callback = callback });
    }

    /// <summary>
    /// 
    /// </summary>
    public static void AddNetworkCallback(byte code, Action<Hashtable> callback)
    {
        if (Instance == null) return;
        Instance.AddCallback(code, callback);
    }

    /// <summary>
    /// 
    /// </summary>
    public void RemoveCallback(Action<Hashtable> callback)
    {
        PhotonEventsCallbacks e = callbackList.Find(x => x.Callback == callback);
        if (e != null) { callbackList.Remove(e); }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="callback"></param>
    public static void RemoveNetworkCallback(Action<Hashtable> callback)
    {
        if (Instance == null) return;
        Instance.RemoveCallback(callback);
    }

    /// <summary>
    /// 
    /// </summary>
    public void OnEventCustom(EventData data)
    {
        if (data.CustomData == null) return;

        switch (data.Code)
        {
            case PropertiesKeys.KickPlayerEvent:
                OnKick();
                break;
            case PropertiesKeys.KillFeedEvent:
                if (bl_KillFeedBase.Instance != null) bl_KillFeedBase.Instance.OnMessageReceive((Hashtable)data.CustomData);
                break;
            default:
                if (callbackList.Count > 0)
                {
                    for (int i = 0; i < callbackList.Count; i++)
                    {
                        if (callbackList[i].Code == data.Code)
                        {
                            Hashtable hastTable = (Hashtable)data.CustomData;
                            callbackList[i].Callback.Invoke(hastTable);
                        }
                    }
                }
                break;
        }
    }

    /// <summary>
    /// Send an event to be invoke in all clients subscribed to the callback
    /// Similar to PhotonView.RPC but without needed of a PhotonView
    /// </summary>
    /// <param name="code"></param>
    /// <param name="data"></param>
    public static void SendNetworkEvent(byte code, Hashtable data)
    {
        if (Instance == null) return;
        Instance.SendDataOverNetwork(code, data);
    }

    /// <summary>
    /// Send an event to be invoke in a specific client subscribed to the callback
    /// </summary>
    /// <param name="code"></param>
    /// <param name="data"></param>
    public static void SendNetworkEventToPlayer(byte code, Hashtable data)
    {
        if (Instance == null) return;
        Instance.SendDataOverNetworkToPlayer(code, data, LocalPlayer);
    }

    /// <summary>
    /// Send an event to be invoke in all clients subscribed to the callback
    /// Similar to PhotonView.RPC but without needed of a PhotonView
    /// </summary>
    public void SendDataOverNetwork(byte code, Hashtable data)
    {
        var so = new SendOptions();
        PhotonNetwork.RaiseEvent(code, data, EventsAll, so);
    }

    /// <summary>
    /// Send an event to be invoke in a specific client subscribed to the callback
    /// Similar to PhotonView.RPC but without needed of a PhotonView
    /// </summary>
    public void SendDataOverNetworkToPlayer(byte code, Hashtable data, Player targetPlayer)
    {
        var so = new SendOptions();
        var reo = new RaiseEventOptions
        {
            TargetActors = new int[1] { targetPlayer.ActorNumber },
            CachingOption = EventCaching.DoNotCache
        };
        PhotonNetwork.RaiseEvent(code, data, reo, so);
    }

    /// <summary>
    /// RPC Wrapper for NetworkView.RPC
    /// </summary>
    /// <param name="view">The NetworkView</param>
    /// <param name="method">The RPC Method</param>
    /// <param name="target"></param>
    /// <param name="parameters"></param>
    public void RPC(PhotonView view, MethodInfo method, RpcTarget target, params object[] parameters)
    {
        view.RPC(nameof(method), target, parameters);
    }

    /// <summary>
    /// Get room/game server property
    /// A property in this context is a key/value pair that persists throughout the lifetime of a room/game server.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public static object GetRoomCustomProperties(string key)
    {
        if (!IsConnectedInRoom) return null;

        if (CurrentRoom.CustomProperties.TryGetValue(key, out var properties))
        {
            return properties;
        }
        return null;
    }

    /// <summary>
    /// 
    /// </summary>
    public void OnPingKick()
    {
        hasPingKick = true;
    }

    /// <summary>
    /// 
    /// </summary>
    public static void KickPlayer(Player p)
    {
        var so = new SendOptions();
        var data = bl_UtilityHelper.CreatePhotonHashTable();
        PhotonNetwork.RaiseEvent(PropertiesKeys.KickPlayerEvent, data, new RaiseEventOptions() { TargetActors = new int[] { p.ActorNumber } }, so);
    }

    /// <summary>
    /// 
    /// </summary>
    void OnKick()
    {
        if (InRoom)
        {
            PlayerPrefs.SetInt(PropertiesKeys.KickKey, 1);
            LeaveRoom(false);
        }
    }

    /// <summary>
    /// Open or Close the current room, when close the room is not visible in the lobby
    /// and other players can't join.
    /// </summary>
    public static void SetRoomAvailable(bool available)
    {
        if (!IsMasterClient || !InRoom)
        {
            // Only master client can change the visibility of the room
            return;
        }

        CurrentRoom.IsOpen = available;
        CurrentRoom.IsVisible = available;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public static int GetPreferedRegion()
    {
        var regions = bl_GameData.Instance.punRegions;
        string[] Regions = regions.Select(x => x.Identifier.ToString()).ToArray();
        string key = PlayerPrefs.GetString(PropertiesKeys.GetUniqueKey("preferredregion2"), bl_Lobby.Instance.DefaultServer.ToString());
        int usedRegion = -1;
        for (int i = 0; i < Regions.Length; i++)
        {
            if (key == Regions[i])
            {
                usedRegion = i;
                bl_Lobby.Instance.ServerRegionID = usedRegion;
                break;
            }
        }

        return usedRegion;
    }

    public static PhotonView FindView(int viewID)
    {
        return PhotonView.Find(viewID);
    }

    /// <summary>
    /// 
    /// </summary>
    private void OnApplicationQuit()
    {
        bl_GameData.Instance.ResetInstance();
        Resources.UnloadUnusedAssets();
    }

    #region Wrapper
    // Wrapper for PhotonNetwork, to make easier in the future change the networking system
    public static new Player LocalPlayer { get { return PhotonNetwork.LocalPlayer; } }

    /// <summary>
    /// Local player nick name
    /// </summary>
    public static string NickName
    {
        get => PhotonNetwork.NickName;
        set
        {
            PhotonNetwork.NickName = value;
            bl_EventHandler.Lobby.onPlayerNameChanged?.Invoke();
        }
    }

    /// <summary>
    /// Is connected to the master server?
    /// </summary>
    public static new bool IsConnected { get { return PhotonNetwork.IsConnected; } }

    /// <summary>
    /// Is connected in a room/game server?
    /// </summary>
    public static bool IsConnectedInRoom { get { return IsConnected && InRoom; } }

    /// <summary>
    /// Is the local player the master client?
    /// </summary>
    public static bool IsMasterClient => PhotonNetwork.IsMasterClient;

    /// <summary>
    /// Is the local player in a room/game server?
    /// </summary>
    public static bool InRoom => PhotonNetwork.InRoom;

    /// <summary>
    /// Is the local player currently connected to the lobby?
    /// </summary>
    public static bool InLobby => PhotonNetwork.InLobby;

    /// <summary>
    /// Return the current room/game server info.
    /// </summary>
    public static Room CurrentRoom => PhotonNetwork.CurrentRoom;

    /// <summary>
    /// List of players in this room/game server
    /// </summary>
    public static Player[] PlayerList => PhotonNetwork.PlayerList;

    /// <summary>
    /// List of players in this room/game server, except the local player
    /// </summary>
    public static Player[] PlayerListOthers => PhotonNetwork.PlayerListOthers;

    /// <summary>
    /// Server time in milliseconds
    /// </summary>
    public static double Time => PhotonNetwork.Time;
    public static bool OfflineMode
    {
        get => PhotonNetwork.OfflineMode;
        set => PhotonNetwork.OfflineMode = value;
    }
    public static bool IsMessageQueueRunning
    {
        get => PhotonNetwork.IsMessageQueueRunning;
        set => PhotonNetwork.IsMessageQueueRunning = value;
    }

    public static bool IsConnectedAndReady => PhotonNetwork.IsConnectedAndReady;

    public static void LeaveRoom(bool becomeInactive = true) => PhotonNetwork.LeaveRoom(becomeInactive);

    /// <summary>
    /// Add a new callback target to the network callbacks
    /// </summary>
    /// <param name="target"></param>
    public static void AddCallbackTarget(object target)
    {
        PhotonNetwork.AddCallbackTarget(target);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="target"></param>
    public static void RemoveCallbackTarget(object target)
    {
        PhotonNetwork.RemoveCallbackTarget(target);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public static Hashtable GetRoomProperties()
    {
        return CurrentRoom.CustomProperties;
    }

    public static void SetRoomProperties(Hashtable propertiesToSet, Hashtable expectedProperties = null, WebFlags webFlags = null)
    {
        CurrentRoom.SetCustomProperties(propertiesToSet, expectedProperties, webFlags);
    }

    public static void CleanBuffer()
    {
        PhotonNetwork.OpCleanActorRpcBuffer(LocalPlayer.ActorNumber);
        PhotonNetwork.OpRemoveCompleteCacheOfPlayer(LocalPlayer.ActorNumber);
    }

    public static int GetPing() => PhotonNetwork.GetPing();

    public static void Destroy(GameObject targetGo) => PhotonNetwork.Destroy(targetGo);

    public static void FindFriends(string[] friendsToFind)
    {
        PhotonNetwork.FindFriends(friendsToFind);
    }

    /// <summary>
    /// 
    /// </summary>
    public static void Disconnect()
    {
        PhotonNetwork.Disconnect();
    }
    #endregion

    public class PhotonEventsCallbacks
    {
        public byte Code;
        public Action<Hashtable> Callback;
    }

    private static bl_PhotonNetwork _instance;
    public static bl_PhotonNetwork Instance
    {
        get
        {
            if (_instance == null)
                _instance = FindAnyObjectByType<bl_PhotonNetwork>();
            return _instance;
        }
    }
}