using TMPro;
using UnityEngine;

namespace MFPS.Runtime.UI
{
    /// <summary>
    /// Simply script to show the player name in the UI
    /// </summary>
    public class bl_PlayerNameText : MonoBehaviour
    {
        [LovattoToogle] public bool includeRoles = true;
        [SerializeField] private TextMeshProUGUI nameText = null;

        /// <summary>
        /// 
        /// </summary>
        private void OnEnable()
        {
            FetchName();
            bl_EventHandler.Lobby.onPlayerNameChanged += FetchName;
        }

        /// <summary>
        /// 
        /// </summary>
        private void OnDisable()
        {
            bl_EventHandler.Lobby.onPlayerNameChanged -= FetchName;
        }

        /// <summary>
        /// 
        /// </summary>
        public void FetchName()
        {
            if (nameText == null) return;

            if (includeRoles) nameText.text = bl_MFPS.LocalPlayer.FullNickName();
            else nameText.text = bl_PhotonNetwork.NickName;

           
        }
    }
}