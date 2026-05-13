using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;

namespace MFPS.Runtime.UI
{
    public class bl_RoomListItemUI : bl_RoomListItemUIBase
    {
        public TextMeshProUGUI RoomNameText = null;
        public TextMeshProUGUI MapNameText = null;
        public TextMeshProUGUI PlayersText = null;
        public TextMeshProUGUI GameModeText = null;
        public TextMeshProUGUI PingText = null;
        [SerializeField] private TextMeshProUGUI MaxKillText = null;
        public GameObject JoinButton = null;
        public GameObject FullText = null;
        [SerializeField] private GameObject PrivateUI = null;

        private RoomInfo cacheInfo = null;
        private MFPSRoomInfo cacheMfpsInfo = null;

        /// <summary>
        /// This method assign the RoomInfo and get the properties of it
        /// to display in the UI
        /// </summary>
        /// <param name="info"></param>
        public override void SetInfo(RoomInfo info)
        {
            cacheInfo = info;
            if (RoomNameText != null) RoomNameText.text = info.Name;
            var mapInfo = bl_GameData.Instance.AllScenes[(int)info.CustomProperties[PropertiesKeys.RoomSceneID]];
            if (MapNameText != null) MapNameText.text = mapInfo.ShowName;
            if (GameModeText != null) GameModeText.text = ((GameMode)info.CustomProperties[PropertiesKeys.GameModeKey]).ToString();
            if (PlayersText != null) PlayersText.text = info.PlayerCount + "/" + info.MaxPlayers;
            string goalName = info.GetGameMode().GetModeInfo().GoalName;
            goalName = goalName.Localized(goalName.ToLower());
            if (MaxKillText != null) MaxKillText.text = string.Format("{0} {1}", info.CustomProperties[PropertiesKeys.RoomGoal], goalName);
            if (PingText != null) PingText.text = ((int)info.CustomProperties[PropertiesKeys.MaxPing]).ToString() + " ms";

            bool _active = (info.PlayerCount < info.MaxPlayers) ? true : false;
            PrivateUI.SetActive((string.IsNullOrEmpty((string)cacheInfo.CustomProperties[PropertiesKeys.RoomPassword]) == false));
            JoinButton.SetActive(_active);
            FullText.SetActive(!_active);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="info"></param>
        public override void SetMFPSInfo(MFPSRoomInfo info, int playerCount = 0)
        {
            cacheMfpsInfo = info;
            if (RoomNameText != null) RoomNameText.text = info.roomName;
            var mapInfo = bl_GameData.Instance.AllScenes[info.sceneId];
            if (MapNameText != null) MapNameText.text = mapInfo.ShowName;
            if (GameModeText != null) GameModeText.text = info.gameMode.ToString();
            if (PlayersText != null) PlayersText.text = playerCount + "/" + info.maxPlayers;
            string goalName = info.gameMode.GetModeInfo().GoalName;
            goalName = goalName.Localized(goalName.ToLower());
            if (MaxKillText != null) MaxKillText.text = string.Format("{0} {1}", info.goal, goalName);
            if (PingText != null) PingText.text = info.maxPing + " ms";

            bool _active = (playerCount < info.maxPlayers) ? true : false;
            PrivateUI.SetActive(!string.IsNullOrEmpty(info.password));
            JoinButton.SetActive(_active);
            FullText.SetActive(!_active);
        }

        /// <summary>
        /// Join to the room that this UI Row represent
        /// </summary>
        public void JoinRoom()
        {
            if (roomFlag.IsEnumFlagPresent(RoomFlag.Persisten))
            {
                if (cacheMfpsInfo == null)
                {
                    Debug.LogWarning("Room info was not found.");
                    return;
                }

                //If the local player ping is higher than the max allowed in the room
                if (PhotonNetwork.GetPing() >= cacheMfpsInfo.maxPing)
                {
                    //display the message and Don't join to the room
                    bl_LobbyUI.Instance.ShowPopUpWindow("max room ping");
                    return;
                }

                bl_Lobby.Instance.CreateRoom(cacheMfpsInfo);
                return;
            }

            //If the local player ping is higher than the max allowed in the room
            if (PhotonNetwork.GetPing() >= (int)cacheInfo.CustomProperties[PropertiesKeys.MaxPing])
            {
                //display the message and Don't join to the room
                bl_LobbyUI.Instance.ShowPopUpWindow("max room ping");
                return;
            }

            //if the room doesn't require a password
            if (string.IsNullOrEmpty((string)cacheInfo.CustomProperties[PropertiesKeys.RoomPassword]))
            {
                if (cacheInfo.PlayerCount < cacheInfo.MaxPlayers)
                {
                    bl_LobbyUI.Instance.blackScreenFader.FadeIn(1);
                    bl_EventHandler.Lobby.onBeforeJoiningRoom?.Invoke();
                    PhotonNetwork.JoinRoom(cacheInfo.Name);
                }
                else
                {
                    Debug.Log("Room is full");
                }
            }
            else
            {

                if (bl_LobbyRoomPasswordWindow.Instance != null) bl_LobbyRoomPasswordWindow.Instance.VerifyRoom(cacheInfo);
                else
                {
                    Debug.LogWarning("Room password window was not found.");
                }
            }
        }
    }
}