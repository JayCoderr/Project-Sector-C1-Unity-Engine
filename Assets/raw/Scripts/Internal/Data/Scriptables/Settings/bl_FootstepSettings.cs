using UnityEngine;

namespace MFPS.Internal.Scriptables
{
    [CreateAssetMenu(fileName = "Footstep Settings", menuName = "MFPS/Settings/Footstep")]
    public class bl_FootstepSettings : ScriptableObject
    {
        public LayerMask surfaceLayers;
        [Range(0, 1)] public float stepsVolume = 0.7f;
        [Range(0, 1)] public float stealthModeVolumeMultiplier = 0.1f;
        public float walkSpeed = 4;
        [Range(0.1f, 1)] public float walkStepRate = 0.44f;
        [Range(0.1f, 1)] public float runStepRate = 0.34f;
        [Range(0.1f, 1)] public float crouchStepRate = 0.54f;
    }
}