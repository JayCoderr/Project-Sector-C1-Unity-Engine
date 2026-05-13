using MFPS.InputManager;
using UnityEngine;

namespace MFPS.Runtime.Settings
{
    public class bl_GamepadDetector : MonoBehaviour
    {
        public float checkInterval = 2f;

        /// <summary>
        /// 
        /// </summary>
        private void Start()
        {
            DontDestroyOnLoad(gameObject);
            if (bl_InputData.Instance.detectConnectedGamepads)
                InvokeRepeating(nameof(DetectControllerType), 0, checkInterval);
        }

        /// <summary>
        /// 
        /// </summary>
        void DetectControllerType()
        {
            string[] joystickNames = Input.GetJoystickNames();

            for (int i = 0; i < joystickNames.Length; i++)
            {
                if (!string.IsNullOrEmpty(joystickNames[i]))
                {
                    // Debug.Log($"Joystick {i + 1}: {joystickNames[i]}");

                    if (IsPlayStationController(joystickNames[i]))
                    {
                        Debug.Log($"Joystick {i + 1} is a PlayStation controller.");
                        CheckIfNeedToSwitchTo(MFPSInputSource.PlayStation);

                    }
                    else if (IsXboxController(joystickNames[i]))
                    {
                        Debug.Log($"Joystick {i + 1} is an Xbox controller.");
                        CheckIfNeedToSwitchTo(MFPSInputSource.Xbox);
                    }
                    else
                    {
                        // Implement your own logic here.
                        // Debug.Log($"Joystick {i + 1} is an unknown controller.");
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inputSource"></param>
        private void CheckIfNeedToSwitchTo(MFPSInputSource inputSource)
        {
            // if the player is already using that device or any other gamepad, do not switch automatically.
            if (bl_Input.InputType == inputSource || bl_Input.IsGamePad) return;

            // if the player is using a keyboard and the gamepad is connected, switch to it.
            bl_InputData.ChangeToDevice(inputSource);
        }

        bool IsPlayStationController(string joystickName)
        {
            return joystickName.ToLower().Contains("wireless controller") ||
                   joystickName.ToLower().Contains("dualshock");
        }

        bool IsXboxController(string joystickName)
        {
            return joystickName.ToLower().Contains("xbox") ||
                   joystickName.ToLower().Contains("microsoft");
        }

        private static bl_GamepadDetector _instance;
        public static bl_GamepadDetector Instance
        {
            get
            {
                if (_instance == null) { _instance = FindAnyObjectByType<bl_GamepadDetector>(); }
                return _instance;
            }
        }
    }
}