using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace MFPS.Runtime.UI
{
    public class bl_LoadoutDropdown : MonoBehaviour
    {
        [SerializeField, LovattoToogle] private bool autoFetchClasses = true;
        private TMP_Dropdown dropdown;

        /// <summary>
        /// 
        /// </summary>
        private void Awake()
        {
            if (dropdown == null)
            {
                dropdown = GetComponent<TMP_Dropdown>();
            }

            if (autoFetchClasses)
            {
                dropdown.ClearOptions();
                var classes = Enum.GetValues(typeof(PlayerClass)).Cast<PlayerClass>().ToList();

                var options = new List<TMP_Dropdown.OptionData>();
                for (int i = 0; i < classes.Count; i++)
                {
                    var op = new TMP_Dropdown.OptionData()
                    {
                        text = classes[i].DisplayName().ToUpper(),
                        image = classes[i].Icon()
                    };
                    options.Add(op);
                }

                dropdown.AddOptions(options);
            }

            int lid = (int)PlayerClass.Assault.GetSavePlayerClass();
            dropdown.value = lid;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public void OnChanged(int value)
        {
            var loadout = (PlayerClass)value;
            loadout.SavePlayerClass();
#if CLASS_CUSTOMIZER
            bl_ClassManager.Instance.CurrentPlayerClass = loadout;
#endif
            bl_EventHandler.Player.onPlayerClassChanged?.Invoke(loadout);
        }
    }
}