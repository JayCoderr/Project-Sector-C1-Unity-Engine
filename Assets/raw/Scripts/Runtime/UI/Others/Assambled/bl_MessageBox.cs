using UnityEngine;

namespace MFPS.Runtime.UI
{
    public class bl_MessageBox : MonoBehaviour
    {
        [SerializeField] private GameObject content = null;
        [SerializeField] private TMPro.TextMeshProUGUI text = null;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="autoHideIn"></param>
        public void Show(string message, float autoHideIn = 0)
        {
            content.SetActive(true);
            text.text = message;

            CancelInvoke();
            if (autoHideIn > 0)
            {               
                Invoke(nameof(Hide), autoHideIn);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Hide()
        {
            content.SetActive(false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public void HideIf(string message)
        {
            if (text.text == message)
            {
                Hide();
            }
        }
    }
}