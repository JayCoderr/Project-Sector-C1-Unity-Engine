using TMPro;
using UnityEngine;

namespace MFPS.Runtime.UI
{
    public class bl_RoomTimeText : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI timeText = null;
        private const int SECOND = 60;

        /// <summary>
        /// 
        /// </summary>
        private void OnEnable()
        {
            if (bl_MFPS.RoomGameMode.CurrentGameModeData.UnlimitedTime)
            {
                gameObject.SetActive(false);
                return;
            }

            bl_EventHandler.Match.onMatchTimeChanged += OnTimeChanged;
        }

        /// <summary>
        /// 
        /// </summary>
        private void OnDisable()
        {
            bl_EventHandler.Match.onMatchTimeChanged -= OnTimeChanged;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="time"></param>
        void OnTimeChanged(int seconds)
        {
            if (timeText == null) return;

            timeText.text = bl_StringUtility.GetTimeFormat(Mathf.FloorToInt(seconds / SECOND), Mathf.FloorToInt(seconds % SECOND));
        }
    }
}