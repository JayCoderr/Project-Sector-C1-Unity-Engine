using UnityEngine;

namespace MFPS.Internal.Scriptables
{
    [CreateAssetMenu(fileName = "Weapon Movement Settings", menuName = "MFPS/Settings/Weapon Movement")]
    public class bl_WeaponMovementSettings : ScriptableObject
    {
        public float accelerationMultiplier = 2.5f;
        public float InSpeed = 15;
        public float OutSpeed = 12;
        public float rotationSpeedMultiplier = 1;
        public AnimationCurve accelerationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    }
}