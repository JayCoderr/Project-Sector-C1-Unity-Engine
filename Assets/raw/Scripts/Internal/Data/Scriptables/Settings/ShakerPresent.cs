using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MFPS.Core.Motion
{
    [System.Serializable, CreateAssetMenu(fileName = "Shaker Present", menuName = "MFPS/Presents/Shaker", order = 302)]
    public class ShakerPresent : ScriptableObject
    {
        public ShakeMethod shakeMethod = ShakeMethod.PerlinNoise;
        [Range(0.01f, 5)]
        public float Duration = 0.4f;
        [Range(0, 50)]
        public float amplitude = 1;
        [Range(0.00001f, 0.99999f)]
        public float frequency = 0.98f;
        [Range(1, 4)]
        public int octaves = 2;
        [Range(0.00001f, 5)]
        public float persistance = 0.2f;
        [Range(0.00001f, 100)]
        public float lacunarity = 20;
        [Range(0.00001f, 0.99999f)]
        public float burstFrequency = 0.5f;
        [Range(0, 5)]
        public int burstContrast = 2;
        [Range(0.01f, 1)] public float fadeInTime = 0.2f;
        public AnimationCurve curve = AnimationCurve.EaseInOut(0, 1, 1, 0);
        [LovattoToogle] public bool Loop = false;

        [NonSerialized] public float currentTime = 1;
        [NonSerialized] public float influence = 1;
        [NonSerialized] public bool starting = false;

        [Serializable]
        public enum ShakeMethod
        {
            PerlinNoise,
            RandomSet,
            Oscillation,
            Curve
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(ShakerPresent))]
    public class ShakerPresentEditor : Editor
    {
        private ShakerPresent script;

        void OnEnable()
        {
            script = (ShakerPresent)target;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Shake Method", EditorStyles.boldLabel);
            script.shakeMethod = (ShakerPresent.ShakeMethod)EditorGUILayout.EnumPopup("Shake Method", script.shakeMethod);
            EditorGUILayout.EndVertical();
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Shake Settings", EditorStyles.boldLabel);

            switch (script.shakeMethod)
            {
                case ShakerPresent.ShakeMethod.RandomSet:
                    DrawRandomSetSettings();
                    break;
                case ShakerPresent.ShakeMethod.PerlinNoise:
                    DrawPerlinNoiseSettings();
                    break;
                case ShakerPresent.ShakeMethod.Oscillation:
                    DrawOscillationSettings();
                    break;
                case ShakerPresent.ShakeMethod.Curve:
                    DrawCurveSettings();
                    break;
            }

            script.Loop = EditorGUILayout.Toggle("Loop", script.Loop);
            EditorGUILayout.EndVertical();

            serializedObject.ApplyModifiedProperties();

            if (GUI.changed)
            {
                EditorUtility.SetDirty(script);
            }
        }

        private void DrawRandomSetSettings()
        {
            script.amplitude = EditorGUILayout.Slider("Amplitude", script.amplitude, 0, 50);
            script.Duration = EditorGUILayout.Slider("Duration", script.Duration, 0.01f, 5);
            script.fadeInTime = EditorGUILayout.Slider("Fade In Time", script.fadeInTime, 0.01f, 1);
        }

        private void DrawPerlinNoiseSettings()
        {
            script.amplitude = EditorGUILayout.Slider("Amplitude", script.amplitude, 0, 50);
            script.Duration = EditorGUILayout.Slider("Duration", script.Duration, 0.01f, 5);
            script.frequency = EditorGUILayout.Slider("Frequency", script.frequency, 0.00001f, 0.99999f);
            script.octaves = EditorGUILayout.IntSlider("Octaves", script.octaves, 1, 4);
            script.persistance = EditorGUILayout.Slider("Persistance", script.persistance, 0.00001f, 5);
            script.lacunarity = EditorGUILayout.Slider("Lacunarity", script.lacunarity, 0.00001f, 100);
            script.burstFrequency = EditorGUILayout.Slider("Burst Frequency", script.burstFrequency, 0.00001f, 0.99999f);
            script.burstContrast = EditorGUILayout.IntSlider("Burst Contrast", script.burstContrast, 0, 5);
            script.fadeInTime = EditorGUILayout.Slider("Fade In Time", script.fadeInTime, 0.01f, 1);
        }

        private void DrawOscillationSettings()
        {
            script.amplitude = EditorGUILayout.Slider("Amplitude", script.amplitude, 0, 50);
            script.Duration = EditorGUILayout.Slider("Duration", script.Duration, 0.01f, 5);
            script.octaves = EditorGUILayout.IntSlider("Octaves", script.octaves, 1, 4);
            script.fadeInTime = EditorGUILayout.Slider("Fade In Time", script.fadeInTime, 0.01f, 1);
        }

        private void DrawCurveSettings()
        {
            script.curve = EditorGUILayout.CurveField("Curve", script.curve);
            script.amplitude = EditorGUILayout.Slider("Amplitude", script.amplitude, 0, 50);
            script.Duration = EditorGUILayout.Slider("Duration", script.Duration, 0.01f, 5);
            script.fadeInTime = EditorGUILayout.Slider("Fade In Time", script.fadeInTime, 0, 1);
        }
    }
#endif
}