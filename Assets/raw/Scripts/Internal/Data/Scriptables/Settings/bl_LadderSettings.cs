using UnityEngine;

namespace MFPS.Internal.Scriptables
{
    [CreateAssetMenu(fileName = "LadderSettings", menuName = "MFPS/Settings/Ladder")]
    public class bl_LadderSettings : ScriptableObject
    {
        public enum FacingDirection
        {
            Forward,
            Backward,
            Left,
            Right,
            Up,
            Down
        }

        public enum ActiveMode
        {
            Input,
            Auto
        }

        public ActiveMode activeMode = ActiveMode.Input;
        [LovattoToogle] public bool hideWeapons = true;
        [Tooltip("The direction which the player will face when climbing")]
        public FacingDirection facingDirection = FacingDirection.Forward;
        public float climbSpeed = 3f;
        [Tooltip("Time between climb steps")]
        public float climbInterval = 0.25f;     // Time between climb steps
        [Tooltip("Duration of pause between steps")]
        public float pauseDuration = 0.2f;     // Duration of pause between steps
        [Tooltip("Distance to move back when jumping off")]
        public float jumpBackDistance = 6f;    // Distance to move back when jumping off
        [Tooltip("Distance to move up when jumping off")]
        public float jumpUpDistance = 4f;      // Distance to move up when jumping off
        [Tooltip("Mouse Look Angle Range")]
        public float viewAngleRange = 60f;     // Mouse Look Angle Range
        public float exitPointOffset = 0.1f;
    }
}