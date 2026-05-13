using TMPro;
using UnityEngine;

namespace MFPS.Runtime.UI
{
    public class bl_KillZoneUI : bl_KillZoneUIBase
    {
        [SerializeField] private GameObject content = null;
        [SerializeField] private TextMeshProUGUI countText = null;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="active"></param>
        public override void SetActive(bool active)
        {
           content.SetActive(active);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="count"></param>
        public override void SetCount(int count)
        {
            if (countText == null) return;

            countText.text = count.ToString();
        }
    }
}