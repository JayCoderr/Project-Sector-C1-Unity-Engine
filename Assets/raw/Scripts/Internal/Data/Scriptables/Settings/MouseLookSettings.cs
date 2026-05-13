using System.Collections.Generic;
using UnityEngine;

namespace MFPS.Internal.Scriptables
{
    [CreateAssetMenu(fileName = "Mouse Settings", menuName = "MFPS/Camera/Mouse Settings")]
    public class MouseLookSettings : ScriptableObject
    {
        [LovattoToogle] public bool useSmoothing = true;
        [Range(2, 12)] public int framesOfSmoothing = 5;
        [LovattoToogle] public bool lerpMovement = false;
        [Range(2, 12)] public float smoothTime = 5f;
        [Tooltip("Relative: To the player camera field of view\nFixed: to a fixed value")]
        public AimSensitivityAdjust aimSensitivityAdjust = AimSensitivityAdjust.Relative;
        [Header("Orbit Mode")]
        public float orbitDistance = 5f;
        public float minOrbitDistance = 1f;
        public Vector3 orbitPivotOffset = new Vector3(0, 1.5f, 0);
        public LayerMask orbitCollisionMask = 1;
        public float orbitSmoothTime = 10f;

        public Texture2D customCursor;

        [System.Serializable]
        public enum AimSensitivityAdjust
        {
            Relative,
            Fixed
        }
    }
}