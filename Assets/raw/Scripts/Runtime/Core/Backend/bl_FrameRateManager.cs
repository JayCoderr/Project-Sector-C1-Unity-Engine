using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// This class automatically setup a dropdown with the refresh rates options of the game
/// defined in GameData.
/// </summary>
public class bl_FrameRateManager : MonoBehaviour
{
    public Dropdown optionDropdown;
    private readonly string Unlimited = "Unlimited";

    /// <summary>
    /// 
    /// </summary>
    void Start()
    {
        if (optionDropdown != null && bl_MFPS.Settings != null)
        {
            optionDropdown.ClearOptions();
            List<Dropdown.OptionData> ol = new();
            int[] options = bl_MFPS.Settings.RefreshRates;
            for (int i = 0; i < options.Length; i++)
            {
                if (options[i] == 0) { ol.Add(new Dropdown.OptionData() { text = Unlimited.Localized("unlimited").ToUpper() }); continue; }
                ol.Add(new Dropdown.OptionData() { text = options[i].ToString() });
            }
            optionDropdown.AddOptions(ol);
            int df = PlayerPrefs.GetInt(PropertiesKeys.GetUniqueKey("framerateid"), (int)bl_MFPS.Settings.GetSettingOf("Frame Rate"));
            optionDropdown.value = df;
            Application.targetFrameRate = bl_MFPS.Settings.RefreshRates[df];
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void OnChange(int option)
    {
        Application.targetFrameRate = bl_MFPS.Settings.RefreshRates[option];
        PlayerPrefs.SetInt(PropertiesKeys.GetUniqueKey("framerateid"), option);
    }

    /// <summary>
    /// 
    /// </summary>
    public void OnChangeCustom(int option)
    {
        Application.targetFrameRate = bl_MFPS.Settings.RefreshRates[option];
    }
}