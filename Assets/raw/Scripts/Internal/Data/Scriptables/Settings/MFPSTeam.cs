using MFPSEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "MFPSTeam", menuName = "MFPS/Settings/Team")]
public class MFPSTeam : ScriptableObject
{
    public string Name;
    public Color TeamColor;
    [SpritePreview] public Sprite Icon;

    /// <summary>
    /// Get the team data by team enum (from the defined teams in bl_GameData)
    /// </summary>
    /// <param name="team"></param>
    /// <returns></returns>
    public static MFPSTeam Get(Team team)
    {
        if (team == Team.Team1) return bl_GameData.Instance.team1;
        else if (team == Team.Team2) return bl_GameData.Instance.team2;
        else return bl_GlobalReferences.I.globalTeam;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public string GetColorString()
    {
        return ColorUtility.ToHtmlStringRGBA(TeamColor);
    }

    /// <summary>
    /// Get the Luminance color of this team color
    /// </summary>
    /// <returns></returns>
    public Color GetLuminanceColor(float alpha = 1)
    {
        // Calculate the luminance of the background color
        float luminance = (0.2126f * TeamColor.r) + (0.7152f * TeamColor.g) + (0.0722f * TeamColor.b);

        // Invert the luminance to get a contrasting grayscale value
        float contrastingLuminance = 1.0f - luminance;

        // Return the grayscale color
        return new Color(contrastingLuminance, contrastingLuminance, contrastingLuminance, alpha);
    }
}