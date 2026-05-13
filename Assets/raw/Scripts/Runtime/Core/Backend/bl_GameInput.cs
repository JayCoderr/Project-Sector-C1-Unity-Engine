using MFPS.InputManager;
using UnityEngine;

public enum GameInputType
{
    Down,
    Up,
    Hold,
}

public enum MFPSInputFocus
{
    Player,
    Interface,
    Other,
}

public class bl_GameInput
{
    public static MFPSInputFocus InputFocus = MFPSInputFocus.Player;

    /// <summary>
    /// Block the scroll of the weapon selection
    /// </summary>
    public static bool BlockWeaponScroll = false;

    /// <summary>
    /// Is the cursor locked?
    /// </summary>
    public static bool IsCursorLocked = false;

    /// <summary>
    /// Cache the name of the weapon slots to avoid string interpolation
    /// </summary>
    private static readonly string[] WeaponSlotNames = new string[]
    {
        "Weapon1", "Weapon2", "Weapon3", "Weapon4", "Weapon5", "Weapon6", "Weapon7", "Weapon8", "Weapon9",
    };

    public static bool Fire(GameInputType inputType = GameInputType.Hold)
    {
        return GetInputManager("Fire", inputType);
    }

    public static bool Run(GameInputType inputType = GameInputType.Hold)
    {
        return bl_InputData.Instance.runWithButton ? GetInputManager("Run", inputType) : Input.GetAxis("Vertical") >= 1f;
    }

    public static bool Aim(GameInputType inputType = GameInputType.Hold)
    {
        return GetInputManager("Aim", inputType);
    }

    public static bool Crouch(GameInputType inputType = GameInputType.Hold)
    {
        return GetInputManager("Crouch", inputType);
    }

    public static bool Stealth(GameInputType inputType = GameInputType.Hold)
    {
        return GetInputManager("Stealth", inputType);
    }

    public static bool Jump(GameInputType inputType = GameInputType.Down)
    {
        return GetInputManager("Jump", inputType);
    }

    public static bool Interact(GameInputType inputType = GameInputType.Down)
    {
        return GetInputManager("Interact", inputType);
    }

    public static bool Reload(GameInputType inputType = GameInputType.Down)
    {
        return GetInputManager("Reload", inputType);
    }

    public static bool WeaponSlot(int slotID, GameInputType inputType = GameInputType.Down)
    {
        return GetInputManager(WeaponSlotNames[slotID], inputType);
    }

    public static bool QuickMelee(GameInputType inputType = GameInputType.Down)
    {
        return GetInputManager("FastKnife", inputType);
    }

    public static bool QuickNade(GameInputType inputType = GameInputType.Down)
    {
        return GetInputManager("QuickNade", inputType);
    }

    public static bool DropWeapon(GameInputType inputType = GameInputType.Down)
    {
        return GetInputManager("DropWeapon", inputType);
    }

    public static bool Pause(GameInputType inputType = GameInputType.Down)
    {
        return bl_Input.IsGamePad ? bl_Input.IsStartPad : GetButton(KeyCode.Escape, inputType, true);
    }

    public static bool Scoreboard(GameInputType inputType = GameInputType.Down)
    {
        return GetInputManager("Scoreboard", inputType);
    }

    public static bool SwitchFireMode(GameInputType inputType = GameInputType.Down)
    {
        return GetInputManager("FireType", inputType);
    }

    public static bool GeneralChat(GameInputType inputType = GameInputType.Down)
    {
        return GetInputManager("GeneralChat", inputType);
    }

    public static bool ThrowKit(GameInputType inputType = GameInputType.Down)
    {
        return GetInputManager("ThrowItem", inputType);
    }

    public static bool TeamChat(GameInputType inputType = GameInputType.Down)
    {
        return GetInputManager("TeamChat", inputType);
    }

    public static bool Talk(GameInputType inputType = GameInputType.Hold)
    {
        return GetInputManager("VoiceTalk", inputType);
    }

    public static bool ChangeView(GameInputType inputType = GameInputType.Down)
    {
        return GetInputManager("SwitchView", inputType);
    }

    public static bool NextWeapon()
    {
        return bl_Input.isButtonDown("NextWeapon");
    }

    public static bool PreviousWeapon()
    {
        return bl_Input.isButtonDown("PreviousWeapon");
    }

    public static float Vertical
    {
        get
        {
            return !IsCursorLocked || bl_GameData.isChatting ? 0 : bl_Input.VerticalAxis;
        }
    }

    public static float Horizontal
    {
        get
        {
            return !IsCursorLocked || bl_GameData.isChatting ? 0 : bl_Input.HorizontalAxis;
        }
    }

    public static float MouseX
    {
        get
        {
            return !IsCursorLocked || bl_GameData.isChatting ? 0 : bl_Input.MouseX;
        }
    }

    public static float MouseY
    {
        get
        {
            return !IsCursorLocked || bl_GameData.isChatting ? 0 : bl_Input.MouseY;
        }
    }

    public static bool GetButton(KeyCode key, GameInputType inputType, bool overrideBlockers = false)
    {
        if (!overrideBlockers)
        {
            if (!IsCursorLocked || bl_GameData.isChatting) return false;
        }

        if (inputType == GameInputType.Hold) { return Input.GetKey(key); }
        else if (inputType == GameInputType.Down) { return Input.GetKeyDown(key); }
        else { return Input.GetKeyUp(key); }
    }

    public static bool GetButton(string key, GameInputType inputType, bool overrideBlockers = false)
    {

        if (!overrideBlockers)
        {
            if (!IsCursorLocked || bl_GameData.isChatting) return false;
        }

        if (inputType == GameInputType.Hold) { return Input.GetKey(key); }
        else if (inputType == GameInputType.Down) { return Input.GetKeyDown(key); }
        else { return Input.GetKeyUp(key); }
    }

    public static bool GetInputManager(string key, GameInputType inputType, bool overrideBlockers = false)
    {
        if (!overrideBlockers)
        {
            if (!IsCursorLocked || bl_GameData.isChatting) return false;
        }

        if (inputType == GameInputType.Hold) { return bl_Input.isButton(key); }
        else if (inputType == GameInputType.Down) { return bl_Input.isButtonDown(key); }
        else { return bl_Input.isButtonUp(key); }
    }
}