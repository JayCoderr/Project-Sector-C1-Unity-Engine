using MFPS.Runtime.Motion;
using MFPSEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "Weapon Bob Settings", menuName = "MFPS/Weapons/Bob/Settings")]
public class bl_WeaponBobSettings : ScriptableObject
{
    [ScriptableDrawer] public bl_SpringTransform spring;
    [Header("Walk")]
    [Range(0.1f, 2)] public float WalkSpeedMultiplier = 1f;
    [Range(0, 15)] public float EulerZAmount = 5;
    [Range(0, 15)] public float EulerXAmount = 5;
    [Range(0, 0.2f)] public float WalkOscillationAmount = 0.04f;
    public float WalkLerpSpeed = 2;

    [Header("Sprint")]
    [Range(0.1f, 2)] public float RunSpeedMultiplier = 1f;
    [Range(0, 15)] public float RunEulerZAmount = 5;
    [Range(0, 15)] public float RunEulerXAmount = 5;
    [Range(0, 0.2f)] public float RunOscillationAmount = 0.1f;
    public float RunLerpSpeed = 4;

    [Header("Idle")]
    public float idleBreathingAmplitude = 0.002f;
    public float idleBreathingFrequency = 1.4f;
    public float idleSwayAmplitude = 0.6f;
    public float idleAimMultiplier = 0.08f;
    public float idleCameraMultiplier = 0.1f;

    [Header("Noise Movement")]
    [LovattoToogle] public bool useNoiseMovement = true;
    public float maxNoiseIntensity = 0.1f;       // Intensity of the noise
    public float maxNoiseFrequency = 0.5f;       // Maximum frequency of the noise
    public float noiseFrequencyTransitionSpeed = 1f; // Speed of frequency transitions
    public float noiseStiffness = 200;             // Stiffness of the noise
    public float noiseDamping = 8;               // Damping of the noise
    public float noiseAimMultiplier = 0.1f;
    public float noiseZMultiplier = 1;        // Multiplier for the Z axis noise
    public Vector2 noiseIntervalRange = new(1f, 5f); // Interval between noise events
    public Vector2 noiseDurationRange = new(0.1f, 1f); // Duration of noise events

    [Header("Misc")]
    public float AimIntensity = 0.01f;
    public float aimRotationIntensity = 0.33f;
    [LovattoToogle] public bool pitchTowardUp = true;

    public AnimationCurve rollCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public AnimationCurve pitchCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
}