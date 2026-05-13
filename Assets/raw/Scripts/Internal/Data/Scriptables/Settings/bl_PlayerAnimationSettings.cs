using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Player Animation Settings", menuName = "MFPS/Player/Animation Settings")]
public class bl_PlayerAnimationSettings : ScriptableObject
{
    public AnimationCurve dropTiltAngleCurve = AnimationCurve.Linear(0, 0, 1, 1);
    public float blendSmoothness = 10;
    [Tooltip("Degrees per turn clip (usually 45)")]
    public float stepAngle = 55f;
    [Tooltip("Max speed to be considered idle")]
    public float idleSpeedThreshold = 0.08f;
    [Tooltip("How long after last movement to allow turns")]
    public float idleGraceSeconds = 0.25f;
    [Tooltip("Minimal spacing between consecutive turn fires")]
    public float refractorySeconds = 0.18f;
    public float rearmAngularSpeed = 140f;        // deg/s to consider "rotating again"
    public float maxAngularSpeedToTrigger = 120f; // deg/s considered "not rotating"
    public float settleSeconds = 0.12f; // must stay under threshold this long

    [Header("Tuning")]
    public float turnClipDuration = 0.20f;  // seconds of your 45° clip
    public float maxHoldDegrees = 80f;      // do not store infinite offset
    public List<OverrideAnimationClips> overrideClips;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="bl_PlayerAnimations"></param>
    public bool CheckOverrides(OverrideAnimationClips.TriggerReason reason, int value, out OverrideAnimationClips replacement)
    {
        foreach (var item in overrideClips)
        {
            if (item.Reason != reason || item.Reason == OverrideAnimationClips.TriggerReason.None) continue;

            if (item.WhenValue == value)
            {
                replacement = item;
                return true;
            }
        }

        replacement = null;
        return false;
    }

    [Serializable]
    public class OverrideAnimationClips
    {
        public TriggerReason Reason;
        public int WhenValue = 0;
        public AnimationClip DefaultClip;
        public AnimationClip OverrideClip;

        public enum TriggerReason
        {
            None = 0,
            WeaponType = 1,
            UpperState = 2,
            WeaponId = 3,
        }
    }
}