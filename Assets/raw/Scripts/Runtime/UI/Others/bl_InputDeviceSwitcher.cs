using UnityEngine;
using MFPS.Runtime.Settings;

namespace MFPS.InputManager
{
    public class bl_InputDeviceSwitcher : MonoBehaviour
    {
        private bool init = false;
        private bl_SingleSettingsBinding settingsBinding;

        /// <summary>
        /// 
        /// </summary>
        private void Awake()
        {
            settingsBinding = GetComponent<bl_SingleSettingsBinding>();           
        }

        /// <summary>
        /// 
        /// </summary>
        private void OnEnable()
        {
            InitialSetup();
        }

        /// <summary>
        /// 
        /// </summary>
        void InitialSetup()
        {
            if (init) return;

            MFPSInputSource currentType = bl_InputData.Instance.mappedInstance.inputDevice;
            int id = 0; // keyboard
            if(currentType == MFPSInputSource.Xbox || currentType == MFPSInputSource.PlayStation)
            {
                id = 1;
            }

            settingsBinding.currentOption = id;
            settingsBinding.ApplyCurrentValue();
            init = true;
        }
    }
}