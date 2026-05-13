using UnityEngine;

[CreateAssetMenu(fileName = "Name Plate Style", menuName = "MFPS/UI/Name Plate Style")]
public class bl_NamePlateStyle : ScriptableObject
{
    [Header("Text Style")]
    public GUIStyle style;

    [Header("Icon")]
    public Texture2D IndicatorIcon;
    public float IndicatorIconSize = 15;
    public float verticalOffset = 0.4f;

    [Header("Health Bar")]
    public Texture2D HealthBarTexture;
    public float HealthBarWidth = 100;
    public float HealthBarThickness = 7;
    public Color HealthBackColor = new(0, 0, 0, 0.5f);
    public Color HealthBarColor = new(0, 1, 0, 0.9f);
    public Vector2 HealthBackOffset = Vector2.zero;

    [Header("Voice")]
    public Texture2D TalkingIcon;
}