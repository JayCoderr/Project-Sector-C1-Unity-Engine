using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName ="Weapon Recoil Settings", menuName = "MFPS/Weapons/Recoil/Settings")]
public class bl_WeaponRecoilSettings: ScriptableObject
{
    public WeaponRecoilType recoilType = WeaponRecoilType.SimpleVertical;
    public AnimationCurve recoilPattern;
    public float recoilPerShot = 0.05f;
    public float recoilVerticalIntensity = 5f;
    public float recoilHorizontalIntensity = 5f;
    public float recoilDuration = 0.15f;
    public float maxRecoilValue = 5;
    public float recoilSmoothness = 6f;
}

#if UNITY_EDITOR
[CustomEditor(typeof(bl_WeaponRecoilSettings))]
public class bl_WeaponRecoilSettingsEditor : Editor
{
    public bl_WeaponRecoilSettings script;

    private void OnEnable()
    {
        script = (bl_WeaponRecoilSettings)target;
    }

    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();


        script.recoilType = (WeaponRecoilType)EditorGUILayout.EnumPopup("Recoil Type", script.recoilType);
        if (script.recoilType == WeaponRecoilType.Pattern)
        {
            script.recoilPattern = EditorGUILayout.CurveField("Recoil Pattern", script.recoilPattern);
            script.recoilVerticalIntensity = EditorGUILayout.FloatField("Recoil Vertical Intensity", script.recoilVerticalIntensity);
            script.recoilHorizontalIntensity = EditorGUILayout.FloatField("Recoil Horizontal Intensity", script.recoilHorizontalIntensity);
        }
        else if (script.recoilType == WeaponRecoilType.SimpleVertical)
        {
            script.recoilVerticalIntensity = EditorGUILayout.FloatField("Recoil Intensity", script.recoilVerticalIntensity);
        }
        else if (script.recoilType == WeaponRecoilType.Random2D)
        {
            script.recoilVerticalIntensity = EditorGUILayout.FloatField("Recoil Vertical Intensity", script.recoilVerticalIntensity);
            script.recoilHorizontalIntensity = EditorGUILayout.FloatField("Recoil Horizontal Intensity", script.recoilHorizontalIntensity);
        }
        EditorGUILayout.Space();
        script.recoilPerShot = EditorGUILayout.Slider("Recoil Per Shot", script.recoilPerShot, 0.005f, 1);
        script.recoilDuration = EditorGUILayout.Slider("Recoil Duration", script.recoilDuration, 0.001f, 0.4f);
        script.maxRecoilValue = EditorGUILayout.FloatField("Max Recoil Value", script.maxRecoilValue);
        script.recoilSmoothness = EditorGUILayout.FloatField("Recoil Smoothness", script.recoilSmoothness);

        if (EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(script);
        }
    }
}
#endif