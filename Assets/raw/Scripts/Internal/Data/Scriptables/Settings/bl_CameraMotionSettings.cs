using MFPS.Runtime.Motion;
using MFPSEditor;
using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CameraMotionSettings", menuName = "MFPS/Camera/Motion Settings")]
public class bl_CameraMotionSettings : ScriptableObject
{
    [ScriptableDrawer] public bl_SpringTransform spring;
    [ScriptableDrawer] public bl_Spring zoomSpring;

    [Tooltip("A global multiplier for the motion rotations.")]
    [Range(0.01f, 10.0f)] public float motionMultiplier = 1;

    [Header("Wiggle")]
    public float smooth = 4f;
    public float tiltAngle = 6f;
    public float sideMoveEffector = 3;
    public float aimMultiplier = 0.5f;

    [Header("Breathe")]
    public float breatheAmplitude = 0.5f;
    public float breathePeriod = 1f;

    [Header("Fall Effect")]
    [Range(0.01f, 1.0f)]
    public float DownAmount = 8;

    public List<CurveItem> shakeCurves = new();

    [Serializable]
    public class CurveItem
    {
        public string Name;
        public AnimationCurve Curve;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public AnimationCurve GetCurve(string name)
    {
        for (int i = 0; i < shakeCurves.Count; i++)
        {
            if (shakeCurves[i].Name == name)
            {
                return shakeCurves[i].Curve;
            }
        }
        return null;
    }
}