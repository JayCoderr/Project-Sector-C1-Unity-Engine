using TMPro;
using UnityEngine;

namespace MFPS.Runtime.UI
{
    public class bl_RoomPropertiesUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI roomNameText = null;
        [SerializeField] private TextMeshProUGUI mapNameText = null;
        [SerializeField] private TextMeshProUGUI gameModeText = null;
        [SerializeField] private TextMeshProUGUI maxPlayersText = null;
        [SerializeField] private TextMeshProUGUI roundTimeText = null;
        [SerializeField] private TextMeshProUGUI goalText = null;

        /// <summary>
        /// 
        /// </summary>
        private void Start()
        {
            if (!bl_PhotonNetwork.InRoom) return;

            Setup();
        }

        /// <summary>
        /// 
        /// </summary>
        void Setup()
        {
            var ri = bl_RoomSettings.Instance.CurrentRoomInfo;
            if (roomNameText != null) roomNameText.text = ri.roomName;
            if (mapNameText != null) mapNameText.text = ri.GetMapInfo().ShowName;
            if (gameModeText != null) gameModeText.text = ri.gameMode.GetName();
            if (roundTimeText != null) roundTimeText.text = bl_StringUtility.GetTimeFormat(ri.time);

            if (goalText != null)
            {
                string goal = ri.gameMode.GetModeInfo().GoalName;
                goal = goal.Localized(goal.ToLower(), false, goal);
                goal = $"{ri.goal} {goal}";
                goalText.text = goal;
            }

            if (maxPlayersText != null)
            {
                int vs = (!bl_RoomSettings.Instance.isOneTeamMode) ? ri.maxPlayers / 2 : ri.maxPlayers - 1;
                maxPlayersText.text = (!bl_RoomSettings.Instance.isOneTeamMode) ? string.Format("{0} VS {1}", vs, vs) : string.Format("1 VS {0}", vs);
            }
        }
    }
}