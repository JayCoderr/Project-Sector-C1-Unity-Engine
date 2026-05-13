using Photon.Realtime;
using Photon.Pun;
using TMPro;
using UnityEngine;

namespace MFPS.Runtime.UI
{
    public class bl_LobbyRoomPasswordWindow : MonoBehaviour
    {

        [SerializeField] private GameObject Window = null;
        [SerializeField] private TMP_InputField passwordInput = null;
        [SerializeField] private TextMeshProUGUI logText = null;

        private RoomInfo checkingRoom;

        /// <summary>
        /// 
        /// </summary>
        public void VerifyRoom(RoomInfo room)
        {
            checkingRoom = room;
            Window.SetActive(true);
            logText.text = string.Empty;
            passwordInput.text = string.Empty;
        }

        /// <summary>
        /// 
        /// </summary>
        public void SubmitPassword()
        {
            string pass = passwordInput.text;
            if (string.IsNullOrEmpty(pass)) return;

            if (checkingRoom == null)
            {
                Debug.LogWarning("Checking room is not assigned more!");
                return;
            }

            if ((string)checkingRoom.CustomProperties[PropertiesKeys.RoomPassword] == pass && checkingRoom.PlayerCount < checkingRoom.MaxPlayers)
            {
                logText.text = string.Empty;

                if (PhotonNetwork.GetPing() < (int)checkingRoom.CustomProperties[PropertiesKeys.MaxPing])
                {
                    bl_LobbyUI.Instance.blackScreenFader.FadeIn(1);
                    if (checkingRoom.PlayerCount < checkingRoom.MaxPlayers)
                    {
                        bl_Lobby.Instance.JoinedWithPassword = true;
                        PhotonNetwork.JoinRoom(checkingRoom.Name);
                        Window.SetActive(false);

                    }
                }
                else
                {
                    logText.text = bl_GameTexts.PingTooHighToJoin;
                    passwordInput.text = string.Empty;
                }
            }
            else
            {
                logText.text = bl_GameTexts.WrongRoomPassword;
                passwordInput.text = string.Empty;
            }
        }

        private static bl_LobbyRoomPasswordWindow instance;
        public static bl_LobbyRoomPasswordWindow Instance
        {
            get
            {
                if (instance == null) { instance = FindAnyObjectByType<bl_LobbyRoomPasswordWindow>(); }
                return instance;
            }
        }
    }
}