using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MFPS.Runtime.FriendList
{
    public class bl_AddFriend : MonoBehaviour
    {
        public TMP_InputField nameInput;
        public TextMeshProUGUI logText;
        public Button addButton;

        /// <summary>
        /// 
        /// </summary>
        public void AddFriend()
        {
            var name = nameInput.text;
            if (string.IsNullOrEmpty(name)) return;

#if UNITY_EDITOR
            if (!bl_MFPSDatabase.IsUserLogged)
            {
                Debug.Log("Player is not logged, can't be checked if user exist in database.");
                Add(name);
                return;
            }
#endif

            addButton.interactable = false;
            bl_MFPSDatabase.Users.CheckIfUserExist("nick", name, (exist) =>
            {
                if (exist)
                {
                    Add(name);
                }
                else
                {
                    logText.text = $"Player '{name}' not exist.";
                }
                nameInput.text = string.Empty;
                addButton.interactable = true;
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="friendName"></param>
        private void Add(string friendName)
        {
            logText.text = string.Empty;
            bl_FriendListBase.Instance?.AddFriend(friendName);
            gameObject.SetActive(false);
        }
    }
}