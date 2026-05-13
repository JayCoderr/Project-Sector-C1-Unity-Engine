using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace MFPS.Runtime.UI
{
    public class bl_ServerRegionDropdown : MonoBehaviour
    {
        [Serializable]
        public class PhotonRegion
        {
            public string Name;
            public SeverRegionCode Identifier;
        }

        private TMP_Dropdown dropdown;
        private int requestedRegionID = -1;

        /// <summary>
        /// 
        /// </summary>
        private void Awake()
        {
            if (!PhotonNetwork.PhotonServerSettings.AppSettings.UseNameServer)
            {
                //disable the dropdown server selection since it doesn't work with Photon Server.
                dropdown.gameObject.SetActive(false);
                return;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private IEnumerator Start()
        {
            while (!bl_GameData.isDataCached) { yield return null; }

            dropdown = GetComponent<TMP_Dropdown>();
            dropdown.onValueChanged.AddListener(OnValueChanged);

            dropdown.ClearOptions();

            List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();
            var regions = bl_GameData.Instance.punRegions;

            foreach (var region in regions)
            {
                TMP_Dropdown.OptionData data = new TMP_Dropdown.OptionData();
                data.text = region.Name;
                options.Add(data);
            }

            dropdown.AddOptions(options);

            int usedRegion = bl_PhotonNetwork.GetPreferedRegion();
            if (usedRegion != -1) dropdown.SetValueWithoutNotify(usedRegion);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public void OnValueChanged(int value)
        {
            requestedRegionID = value;
            bl_LobbyUI.ShowConfirmationWindow(bl_GameTexts.ChangeRegionAsk.Localized(200), () =>
            {
                bl_Lobby.Instance.ConnectToServerRegion(requestedRegionID);
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public static SeverRegionCode GetRegionIdentifier(int index)
        {
            var regions = bl_GameData.Instance.punRegions;
            if (index >= regions.Length - 1) return SeverRegionCode.none;

            return bl_GameData.Instance.punRegions[index].Identifier;
        }
    }
}