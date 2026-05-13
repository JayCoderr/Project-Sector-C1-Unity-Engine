using MFPS.Runtime.Settings;
using MFPSEditor;
using System.Collections.Generic;
using UnityEngine;

namespace MFPS.InputManager
{
    [CreateAssetMenu(fileName = "InputManager", menuName = "MFPS/Input/Manager")]
    public class bl_InputData : ScriptableObject
    {

        [SerializeField, ScriptableDrawer] private ButtonMapped Mapped = null;
        public string inputVersion = "1.0.0";
        [LovattoToogle] public bool useGamePadNavigation = false;
        [LovattoToogle] public bool runWithButton = false;
        [Tooltip("If true, when a Playstation or Xbox controller is connected, the game will automatically change its input source to that controller.")]
        [LovattoToogle] public bool detectConnectedGamepads = true;

        [Header("References")]
        public GameObject GamePadInputModule;
        public GameObject GamePadPointerPrefab;
        [SerializeField] private GameObject gamepadDetector = null;
        [Header("Mapped Options")]
        public ButtonMapped[] mappedOptions;

        public ButtonMapped mappedInstance { get; set; }
        private readonly Dictionary<string, ButtonData> cachedKeys = new();
        public const string KEYS = "mfps.input.bindings";
        private const string NONE = "None";

        public MFPSInputSource InputType
        {
            get
            {
                return Mapped != null ? Mapped.inputDevice : MFPSInputSource.KeyboardAndMouse;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Initialize()
        {
            cachedKeys.Clear();
            mappedInstance = null;
            LoadMapped();
            if (Instance.detectConnectedGamepads && bl_GamepadDetector.Instance == null)
            {
                if (gamepadDetector != null) Instantiate(gamepadDetector);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        void LoadMapped()
        {
            if (PlayerPrefs.HasKey(MappedBindingKey))
            {
                string json = PlayerPrefs.GetString(MappedBindingKey);
                mappedInstance = Instantiate(Mapped);
                mappedInstance.mapped = JsonUtility.FromJson<Mapped>(json);
            }
            else
            {
                mappedInstance = Instantiate(Mapped);
            }
            mappedInstance.Init();
        }

        /// <summary>
        /// 
        /// </summary>
        public void ChangeMapped(int mappedID)
        {
            Mapped = mappedOptions[mappedID];
            Initialize();
            if (Mapped.inputDevice != MFPSInputSource.KeyboardAndMouse)
            {
                Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            }
            bl_Input.CheckGamePadRequired();
        }

        /// <summary>
        /// Change the current input source to the given input source device mapped (if exists).
        /// </summary>
        /// <param name="inputSource"></param>
        public static void ChangeToDevice(MFPSInputSource inputSource)
        {
            if (MappedInstance != null && MappedInstance.inputDevice == inputSource) return;

            foreach (var item in Instance.mappedOptions)
            {
                if (item.inputDevice == inputSource)
                {
                    Instance.ChangeMapped(System.Array.IndexOf(Instance.mappedOptions, item));
                    break;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool GetButton(string key)
        {
            return IsCached(key, out ButtonData button) && button.IsButton();
        }

        /// <summary>
        /// 
        /// </summary>
        public bool GetButtonDown(string key)
        {
            return IsCached(key, out ButtonData button) && button.IsButtonDown();
        }

        /// <summary>
        /// 
        /// </summary>
        public bool GetButtonUp(string key)
        {
            return IsCached(key, out ButtonData button) && button.IsButtonUp();
        }

        /// <summary>
        /// Get the name of the primary button name
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string GetButtonName(string key)
        {
            return !IsCached(key, out ButtonData button) ? NONE : mappedInstance.GetButtonName(button);
        }

        /// <summary>
        /// 
        /// </summary>
        private bool IsCached(string key, out ButtonData button)
        {
            if (mappedInstance == null) { Initialize(); }

            if (!cachedKeys.TryGetValue(key, out var buttonData))
            {
                if (mappedInstance.ButtonMapDictionary.TryGetValue(key, out buttonData))
                {
                    cachedKeys.Add(key, buttonData);
                }
                else
                {
                    Debug.Log($"Key <color=yellow>{key}</color> has not been mapped in the InputManager.");
                    button = null;
                    return false;
                }
            }
            button = buttonData;
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        public void SaveMappedInstance()
        {
            string json = JsonUtility.ToJson(mappedInstance.mapped);
            PlayerPrefs.SetString(MappedBindingKey, json);
        }

        /// <summary>
        /// 
        /// </summary>
        public void RevertMappedInstance()
        {
            LoadMapped();
        }

        public static string MouseYAxis => Instance.Mapped.cameraVerticalAxis;
        public static string MouseXAxis => Instance.Mapped.cameraHorizontalAxis;

        public static KeyCode PauseKeyCode => Instance.mappedInstance != null ? Instance.mappedInstance.menuButton : KeyCode.Escape;

        public string MappedBindingKey => Mapped == null ? KEYS : $"{KEYS}.{(short)Mapped.inputDevice}.{inputVersion}";
        public ButtonMapped DefaultMapped => Mapped;

        public static ButtonMapped MappedInstance => Instance.mappedInstance;


        private static bl_InputData m_Data;
        public static bl_InputData Instance
        {
            get
            {
                if (m_Data == null)
                {
                    m_Data = Resources.Load("InputManager", typeof(bl_InputData)) as bl_InputData;
                }
                return m_Data;
            }
        }
    }
}