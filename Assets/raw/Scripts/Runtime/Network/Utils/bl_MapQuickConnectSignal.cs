using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class bl_MapQuickConnectSignal : MonoBehaviourPunCallbacks
{
    private Hashtable roomProps;
    private string nickName;

    private void Awake()
    {
        nickName = $"Player {Random.Range(0, 998)}";
        SceneManager.activeSceneChanged += OnSceneChanged;
        PhotonNetwork.AddCallbackTarget(this);
    }

    void OnDestroy()
    {
        SceneManager.activeSceneChanged -= OnSceneChanged;
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    void OnSceneChanged(Scene old, Scene newScene)
    {
        if (bl_Lobby.Instance == null) return;

        bl_Lobby.Instance.NickName = nickName;
        bl_PhotonNetwork.NickName = nickName;
    }

    public override void OnConnectedToMaster()
    {
        if (roomProps == null || bl_Lobby.Instance == null) return;

        var roomInfo = new MFPSRoomInfo
        {
            roomName = $"QuickConnectRoom",
            maxPlayers = 8
        };
        roomInfo.GetProps(roomProps);

        bl_PhotonNetwork.NickName = nickName;
        bl_Lobby.Instance.CreateRoom(roomInfo);
    }

    public static void CreateSignal(Hashtable roomProps)
    {
        var quickConnectSignal = new GameObject("QuickConnectSignal");
        var script = quickConnectSignal.AddComponent<bl_MapQuickConnectSignal>();
        script.roomProps = roomProps;
        DontDestroyOnLoad(quickConnectSignal);
    }
}