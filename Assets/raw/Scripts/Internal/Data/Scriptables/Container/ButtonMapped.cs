using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace MFPS.InputManager
{
    [Serializable, CreateAssetMenu(fileName = "Button Mapped", menuName = "MFPS/Input/Input Mapped")]
    public class ButtonMapped : ScriptableObject
    {
        [FormerlySerializedAs("inputType")]
        public MFPSInputSource inputDevice = MFPSInputSource.KeyboardAndMouse;
        public Mapped mapped = new();
        [Header("Static Buttons")]
        public string cameraVerticalAxis = "Mouse Y";
        public string cameraHorizontalAxis = "Mouse X";
        [KeyFinder] public KeyCode menuButton = KeyCode.Joystick1Button7;
        [KeyFinder] public KeyCode submitButton = KeyCode.Return;

        [Header("Device Buttons Names")]
        public bool hasCustomNames = false;
        [Tooltip("Custom names for buttons of this specific device")]
        public List<ButtonDeviceName> buttonsNames = new();
        [Tooltip("Special axis that can be rebinded")]
        public InputSourceAxis[] DeviceAxis;

        public List<ButtonData> ButtonMap { get { return mapped.ButtonMap; } }
        public Dictionary<string, ButtonData> ButtonMapDictionary;

        /// <summary>
        /// 
        /// </summary>
        public void Init()
        {
            ButtonMapDictionary = new Dictionary<string, ButtonData>();
            foreach (ButtonData item in ButtonMap)
            {
                ButtonMapDictionary.Add(item.KeyName, item);
            }
        }

        /// <summary>
        /// Does the button contain a custom name to display?
        /// </summary>
        /// <param name="button"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool ContainCustomButtonName(KeyCode button, out string name)
        {
            name = string.Empty;
            if (!hasCustomNames) return false;

            foreach (ButtonDeviceName item in buttonsNames)
            {
                if (item.KeyCode == KeyCode.None) continue;

                if (item.KeyCode == button)
                {
                    name = item.DisplayName;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Does the axis contain a custom name to display?
        /// </summary>
        /// <param name="axis"></param>
        /// <param name="positive"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool ContainCustomAxisName(string axis, bool positive, out string name)
        {
            name = string.Empty;
            if (!hasCustomNames) return false;

            foreach (ButtonDeviceName item in buttonsNames)
            {
                if (string.IsNullOrEmpty(item.AxisName)) continue;

                if (item.AxisName == axis && item.AxisIsPositive == positive)
                {
                    name = item.DisplayName;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="button"></param>
        /// <returns></returns>
        public string GetButtonName(ButtonData button)
        {
            if (button == null) return string.Empty;

            if (button.PrimaryIsAxis)
            {
                if (ContainCustomAxisName(button.PrimaryAxis, button.AxisValue > 0, out string name))
                {
                    return name;
                }
                return button.PrimaryAxis;
            }
            else
            {
                if (ContainCustomButtonName(button.PrimaryKey, out string name))
                {
                    return name;
                }
                return button.PrimaryKey.ToString();
            }
        }
    }
}