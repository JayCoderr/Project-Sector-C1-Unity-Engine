using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

namespace MFPS.InputManager
{
    public class bl_InputBindingUI : MonoBehaviour
    {
        public TextMeshProUGUI KeyNameText;
        public TextMeshProUGUI PrimaryKeyText;
        public TextMeshProUGUI AltKeyText;
        public GameObject PrimaryOverlay, AltOverlay;

        public ButtonData CachedData { get; set; }

        private bl_InputUI UIManager;
        private int waitingFor = 0;

        /// <summary>
        /// 
        /// </summary>
        public void Set(ButtonData data, bl_InputUI uimanager)
        {
            CachedData = data;
            UIManager = uimanager;
            ApplyUI();
        }

        /// <summary>
        /// 
        /// </summary>
        void ApplyUI()
        {
#if LOCALIZATION
            string description = CachedData.KeyName;
            bl_Localization.Instance.TryGetText($"input_{CachedData.KeyName.ToLower()}", ref description);
            KeyNameText.text = description;
#else
            KeyNameText.text = CachedData.Description;
#endif
            // if the primary input is a button
            if (!CachedData.PrimaryIsAxis)
            {
                string keyName = "None";
                if (bl_InputData.MappedInstance.ContainCustomButtonName(CachedData.PrimaryKey, out string buttonName))
                {
                    keyName = buttonName;
                }
                else
                {
                    if (CachedData.PrimaryKey != KeyCode.None) keyName = Regex.Replace(CachedData.PrimaryKey.ToString(), "[A-Z]", " $0").Trim();
                }
                PrimaryKeyText.text = keyName;
            }
            else
            {
                PrimaryKeyText.text = bl_InputData.MappedInstance.ContainCustomAxisName(CachedData.PrimaryAxis, CachedData.AxisValue > 0, out string axisName)
                    ? axisName
                    : CachedData.GetInputName();
            }

            if (!CachedData.AlternativeIsAxis)
            {
                string keyName = "None";
                if (bl_InputData.MappedInstance.ContainCustomButtonName(CachedData.AlternativeKey, out string buttonName))
                {
                    keyName = buttonName;
                }
                else
                {
                    if (CachedData.AlternativeKey != KeyCode.None) keyName = Regex.Replace(CachedData.AlternativeKey.ToString(), "[A-Z]", " $0").Trim();
                }

                AltKeyText.text = keyName;
            }
            else
            {
                AltKeyText.text = bl_InputData.MappedInstance.ContainCustomAxisName(CachedData.AlternativeAxis, CachedData.AxisValue > 0, out string axisName)
                    ? axisName
                    : CachedData.GetInputName();
            }

            PrimaryOverlay.SetActive(false);
            if (AltOverlay != null) AltOverlay.SetActive(false);
        }

        /// <summary>
        /// 
        /// </summary>
        public void OnEdit(bool primary)
        {
            if (UIManager.ChangeKeyFor(this))
            {
                PrimaryOverlay.SetActive(primary);
                AltOverlay.SetActive(!primary);
                if (primary) { PrimaryKeyText.text = ""; }
                else { AltKeyText.text = ""; }
                waitingFor = primary ? 1 : 2;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void OnChanged(bl_InputUI.PendingButton button)
        {
            if (waitingFor == 1)
            {
                if (button.isAKey)
                {
                    CachedData.PrimaryIsAxis = false;
                    CachedData.PrimaryKey = button.Key;
                    CachedData.PrimaryAxis = "";
                }
                else
                {
                    CachedData.PrimaryIsAxis = true;
                    CachedData.PrimaryAxis = button.Axis;
                    CachedData.PrimaryKey = KeyCode.None;
                    CachedData.AxisValue = button.AxValue;
                }
            }
            else if (waitingFor == 2)
            {
                if (button.isAKey)
                {
                    CachedData.AlternativeIsAxis = false;
                    CachedData.AlternativeKey = button.Key;
                    CachedData.AlternativeAxis = "";
                }
                else
                {
                    CachedData.AlternativeIsAxis = true;
                    CachedData.AlternativeAxis = button.Axis;
                    CachedData.AlternativeKey = KeyCode.None;
                    CachedData.AxisValue = button.AxValue;
                }
            }

            ApplyUI();
            waitingFor = 0;
        }

        /// <summary>
        /// 
        /// </summary>
        public void CancelChange()
        {
            ApplyUI();
        }

        /// <summary>
        /// 
        /// </summary>
        public void ResetKey(KeyCode key)
        {
            if (CachedData.PrimaryKey == key) { CachedData.PrimaryKey = KeyCode.None; }
            else if (CachedData.AlternativeKey == key) { CachedData.AlternativeKey = KeyCode.None; }
            ApplyUI();
        }

        /// <summary>
        /// 
        /// </summary>
        public void ResetAxis(string axis)
        {
            if (string.IsNullOrEmpty(axis)) return;

            if (CachedData.PrimaryAxis == axis) { CachedData.PrimaryAxis = ""; }
            else if (CachedData.AlternativeAxis == axis) { CachedData.AlternativeAxis = ""; }
            ApplyUI();
        }
    }
}