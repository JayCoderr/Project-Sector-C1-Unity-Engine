using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace MFPS.Runtime.FriendList
{
    public class bl_FriendList : bl_FriendListBase, IMatchmakingCallbacks, IConnectionCallbacks, ILobbyCallbacks
    {
        private readonly List<string> friendsNames = new List<string>();
        private bl_FriendListUIBase FriendUI;
        private bool firstBuild = false;
        private List<FriendInfo> friendList = new List<FriendInfo>();
        private Status m_status = Status.Idle;

        public override int FriendsCount => friendList.Count;

        /// <summary>
        /// 
        /// </summary>
        void Awake()
        {
            FriendUI = bl_LobbyUI.Instance.FriendUI;
            bl_PhotonNetwork.AddCallbackTarget(this);
            if (bl_PhotonNetwork.IsConnected && !string.IsNullOrEmpty(bl_PhotonNetwork.NickName))
            {
                GetFriendsStore();
                InvokeRepeating(nameof(UpdateList), 1, 1);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        void OnDisable()
        {
            bl_PhotonNetwork.RemoveCallbackTarget(this);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override Status GetStatus()
        {
            return m_status;
        }

        /// <summary>
        /// 
        /// </summary>
        void GetFriendsStore()
        {
            if (!bl_PhotonNetwork.IsConnectedAndReady || !bl_PhotonNetwork.InLobby)
                return;

            friendsNames.Clear();
            //Get all friends saved
            friendsNames.AddRange(bl_MFPSDatabase.Friends.GetFriends());

            //Find all friends names in photon list.
            if (friendsNames.Count > 0)
            {
                bl_PhotonNetwork.FindFriends(friendsNames.ToArray());
                //Update the list UI 
                FriendUI.UpdateFriendList(true);
            }
            else
            {
                // Debug.Log("No friends saved");
                return;
            }
        }

        /// <summary>
        /// Call For Update List of friends.
        /// </summary>
        void UpdateList()
        {
            if (!bl_PhotonNetwork.IsConnected || bl_PhotonNetwork.InRoom)
                return;

            if (friendsNames.Count > 0)
            {
                if (friendsNames.Count > 1 && friendsNames.Contains("Null"))
                {
                    friendsNames.Remove("Null");
                    friendsNames.Remove("Null");
                    SaveFriends();
                }
                if (bl_PhotonNetwork.IsConnectedAndReady && friendsNames.Count > 0)
                {
                    bl_PhotonNetwork.FindFriends(friendsNames.ToArray());
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void SaveFriends()
        {
            bl_MFPSDatabase.Friends.StoreFriends(friendsNames);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="field"></param>
        public override void AddFriend(TMP_InputField field)
        {
            AddFriend(field.text);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="field"></param>
        public override void AddFriend(string friend)
        {
            if (friendsNames.Contains(friend)) return;
            if (!CanAddMoreFriends())
            {
                FriendUI.ShowMessage("Max friends reached!");
                return;
            }
            string t = friend;
            if (string.IsNullOrEmpty(t))
                return;

            if (FriendUI != null && FriendUI.IsPlayerListed(t))
            {
                FriendUI.ShowMessage("Already has added this friend.");
                return;
            }
            if (t == bl_PhotonNetwork.NickName)
            {
                FriendUI.ShowMessage("You can't add yourself.");
                return;
            }

            friendsNames.Add(friend);
            PhotonNetwork.FindFriends(friendsNames.ToArray());
            FriendUI.UpdateFriendList(true);
            SaveFriends();
            m_status = Status.Fetching;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="friend"></param>
        public override void RemoveFriend(string friend)
        {
            if (friendsNames.Contains(friend))
            {
                friendsNames.Remove(friend);
                SaveFriends();
                if (friendsNames.Count > 0)
                {
                    if (friendsNames.Count > 1 && friendsNames.Contains("Null"))
                    {
                        friendsNames.Remove("Null");
                        friendsNames.Remove("Null");
                        SaveFriends();
                    }
                    if (friendsNames.Count > 0)
                        bl_PhotonNetwork.FindFriends(friendsNames.ToArray());
                }
                else
                {
                    AddFriend("Null");
                    if (friendsNames.Count > 0)
                        bl_PhotonNetwork.FindFriends(friendsNames.ToArray());
                }

                FriendUI.UpdateFriendList(true);
                m_status = Status.Fetching;
            }
            else { Debug.Log("This user doesn't exist"); }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override FriendInfo[] GetFriends()
        {
            return friendList.ToArray();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="playerNick"></param>
        /// <returns></returns>
        public override bool IsPlayerFriend(string playerNick)
        {
            return friendsNames.Contains(playerNick);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override bool CanAddMoreFriends()
        {
            return friendsNames.Count < (bl_GameData.CoreSettings.MaxFriendsAdded + ExtraFriends);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="friendList"></param>
        public void OnFriendListUpdate(List<FriendInfo> friendList)
        {
            if (string.IsNullOrEmpty(bl_PhotonNetwork.NickName)) return;

            // Fix issue with the first friend added now begins show up in the list.
            foreach (var item in friendList)
            {
                if (item.UserId == bl_PhotonNetwork.NickName) return;
            }

            bool build = (friendList.Count != this.friendList.Count);
            if (friendList.Count == 1)
            {
                if (friendList[0].UserId != "Null")
                {
                    if (!firstBuild) { build = true; firstBuild = true; }
                }
                else firstBuild = false;

            }
            else firstBuild = false;

            this.friendList = friendList;
            FriendUI.UpdateFriendList(build);
        }

        /// <summary>
        /// 
        /// </summary>
        public void StartFriendsUpdate()
        {
            GetFriendsStore();
            CancelInvoke();
            InvokeRepeating(nameof(UpdateList), 1, 1);
            FriendUI.SetActiveList(true);
        }

        #region Photon Callbacks
        public void OnCreatedRoom()
        {

        }

        public void OnCreateRoomFailed(short returnCode, string message)
        {

        }

        public void OnJoinRoomFailed(short returnCode, string message)
        {

        }

        public void OnJoinRandomFailed(short returnCode, string message)
        {

        }

        public void OnLeftRoom()
        {

        }

        public void OnJoinedRoom()
        {
            CancelInvoke(nameof(UpdateList));
        }

        public void OnConnected()
        {

        }

        public void OnConnectedToMaster()
        {

        }

        public void OnJoinedLobby()
        {
            if (!string.IsNullOrEmpty(bl_PhotonNetwork.NickName))
            {
                StartFriendsUpdate();
            }
        }

        public void OnDisconnected(DisconnectCause cause)
        {
            CancelInvoke(nameof(UpdateList));
        }

        public void OnRegionListReceived(RegionHandler regionHandler)
        {

        }

        public void OnCustomAuthenticationResponse(Dictionary<string, object> data)
        {

        }

        public void OnCustomAuthenticationFailed(string debugMessage)
        {

        }

        public void OnLeftLobby()
        {
            CancelInvoke(nameof(UpdateList));
        }

        public void OnRoomListUpdate(List<RoomInfo> roomList)
        {

        }

        public void OnLobbyStatisticsUpdate(List<TypedLobbyInfo> lobbyStatistics)
        {

        }
        #endregion
    }
}