using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Automatically apply a team color to an UI element 
/// Or team name to an UI Text
/// </summary>
public class bl_GraphicTeamColor : MonoBehaviour
{
    public Team team = Team.All;
    public bool applyColor = true;
    public bool applyTeamName = false;
    public bool toUpper = true;
    [SerializeField] private bool luminanceColorText = false;
    [SerializeField] private Image iconImg = null;
    [SerializeField] private bool useIconAscentMonoColor = true;
    [SerializeField] private TextMeshProUGUI textUI = null;
    private Graphic graphic;

    /// <summary>
    /// 
    /// </summary>
    private void OnEnable()
    {
        Apply();
    }

    /// <summary>
    /// 
    /// </summary>
    public void Apply()
    {
        if (graphic == null) { graphic = GetComponent<Graphic>(); }
        var teamInfo = MFPSTeam.Get(team);
        if (applyColor)
        {
            graphic.color = teamInfo.TeamColor;
        }

        if (applyTeamName || luminanceColorText)
        {
            var t = GetComponent<Text>();
            if (t != null)
            {
                if (applyTeamName) t.text = toUpper ? teamInfo.Name.ToUpper() : teamInfo.Name;
                if (luminanceColorText)
                {
                    t.color = teamInfo.GetLuminanceColor();
                }
            }
            else
            {
                if (textUI == null) textUI = GetComponent<TextMeshProUGUI>();
                if (textUI != null)
                {
                    if (applyTeamName) textUI.text = toUpper ? teamInfo.Name.ToUpper() : teamInfo.Name;
                    if (luminanceColorText)
                    {
                        textUI.color = teamInfo.GetLuminanceColor();
                    }
                }
            }
        }

        if (iconImg != null && (team == Team.Team1 || team == Team.Team2))
        {
            iconImg.sprite = teamInfo.Icon;
            if (useIconAscentMonoColor)
            {
                iconImg.color = teamInfo.GetLuminanceColor();
            }
        }
    }
}