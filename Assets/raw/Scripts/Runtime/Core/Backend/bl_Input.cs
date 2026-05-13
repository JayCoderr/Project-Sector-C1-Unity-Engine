using MFPS.InputManager;
using UnityEngine;
using UnityEngine.UI;

public static class bl_Input
{
    public static bl_InputData InputData => bl_InputData.Instance;

    public static MFPSInputSource InputType
    {
        get { return bl_InputData.Instance.InputType; }
    }

    /// <summary>
    /// 
    /// </summary>
    public static void Initialize()
    {
        bl_InputData.Instance.Initialize();
    }

    /// <summary>
    /// 
    /// </summary>
    public static bool isButton(string keyName)
    {
        return bl_InputData.Instance.GetButton(keyName);
    }

    /// <summary>
    /// 
    /// </summary>
    public static bool isButtonDown(string keyName)
    {
        return bl_InputData.Instance.GetButtonDown(keyName);
    }

    /// <summary>
    /// 
    /// </summary>
    public static bool isButtonUp(string keyName)
    {
        return bl_InputData.Instance.GetButtonUp(keyName);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="keyName"></param>
    /// <returns></returns>
    public static string GetButtonName(string keyName)
    {
        return bl_InputData.Instance.GetButtonName(keyName);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public static MFPSInputSource GetInputType()
    {
        string[] names = Input.GetJoystickNames();
        MFPSInputSource t = MFPSInputSource.KeyboardAndMouse;
        for (int i = 0; i < names.Length; i++)
        {
            Debug.Log("Joystick: " + names[i]);
        }
        return t;
    }

    /// <summary>
    /// Use this instead of Input.GetAxis("Vertical");
    /// </summary>
    public static float VerticalAxis
    {
        get
        {
            if (!IsGamePad)
            {
                bool isForward = isButton("Forward");
                bool isBackward = isButton("Backward");

                return isForward ? isBackward ? 0.5f : 1 : isBackward ? -1 : 0;
            }
            else
            {
                return Input.GetAxis("Vertical");
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public static float MouseX
    {
        get
        {
            return Input.GetAxis(bl_InputData.MouseXAxis);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public static float MouseY
    {
        get
        {
            return Input.GetAxis(bl_InputData.MouseYAxis);
        }
    }

    /// <summary>
    /// start button on game pad controllers
    /// </summary>
    public static bool IsStartPad
    {
        get
        {
            return Input.GetKeyDown(bl_InputData.PauseKeyCode);
        }
    }

    /// <summary>
    /// Use this instead of Input.GetAxis("Horizontal");
    /// </summary>
    public static float HorizontalAxis
    {
        get
        {
            if (!IsGamePad)
            {
                bool isRight = isButton("Right");
                bool isLeft = isButton("Left");
                return isRight ? isLeft ? 0.5f : 1 : isLeft ? -1 : 0;
            }
            else
            {
                return Input.GetAxis("Horizontal");
            }
        }
    }

    public static void CheckGamePadRequired()
    {
        if (bl_InputData.Instance.mappedInstance == null || !bl_InputData.Instance.useGamePadNavigation) return;
        if (bl_InputData.Instance.mappedInstance.inputDevice != MFPSInputSource.Xbox && bl_InputData.Instance.mappedInstance.inputDevice != MFPSInputSource.PlayStation)
            return;

        bl_GamePadPointerModule dpm = Object.FindAnyObjectByType<bl_GamePadPointerModule>();
        if (dpm == null)
        {
            GameObject go = GameObject.Instantiate(bl_InputData.Instance.GamePadInputModule) as GameObject;
            dpm = go.GetComponent<bl_GamePadPointerModule>();
            dpm.CheckCanvas();
        }

        bl_GamePadPointer gmp = bl_GamePadPointer.Instance;
        if (gmp == null)
        {
            bl_UIReferences uir = bl_UIReferences.Instance;
            GameObject go = GameObject.Instantiate(bl_InputData.Instance.GamePadPointerPrefab) as GameObject;
            go.GetComponent<bl_GamePadPointer>();
            if (uir != null)
            {
                Transform parent = uir.transform.GetChild(1);
                go.transform.SetParent(parent, false);
                go.transform.SetAsLastSibling();
            }
            else
            {
                GraphicRaycaster c = GameObject.FindAnyObjectByType<GraphicRaycaster>();
                Transform parent = c.transform;
                go.transform.SetParent(parent, false);
                go.transform.SetAsLastSibling();
                go.transform.localPosition = Vector3.zero;
            }
        }
    }

    public static bool IsGamePad { get { return bl_InputData.Instance.InputType == MFPSInputSource.PlayStation || bl_InputData.Instance.InputType == MFPSInputSource.Xbox; } }

}