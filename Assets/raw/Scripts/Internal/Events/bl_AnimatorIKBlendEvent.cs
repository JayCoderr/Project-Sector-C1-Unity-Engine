using System;
using UnityEngine;

namespace MFPS.Internal
{
    public class bl_AnimatorIKBlendEvent : StateMachineBehaviour
    {
        public struct Modifier
        {
            public float Weight;
            public bool AffectLeftArm;
            public bool AffectRightArm;
            public bool AffectLeftLeg;
            public bool AffectRightLeg;
            public float SmoothTime;
            public float OutSmoothTime;
        }

        [Flags]
        public enum AffectParts
        {
            LeftArm = 1,
            RightArm = 2,
            LeftLeg = 4,
            RightLeg = 8,
        }

        public float onEnterIKWeight = 0;
        public float onExitIKWeight = 1;
        public AffectParts affectParts;
        public float smoothness = -1;
        public float outSmoothness = -1;

        public static Action<bool, Animator, Modifier> onMotionIKBlendModifier;

        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            Modifier modifier = new()
            {
                Weight = onEnterIKWeight,
                AffectLeftArm = affectParts.IsEnumFlagPresent(AffectParts.LeftArm),
                AffectRightArm = affectParts.IsEnumFlagPresent(AffectParts.RightArm),
                AffectLeftLeg = affectParts.IsEnumFlagPresent(AffectParts.LeftLeg),
                AffectRightLeg = affectParts.IsEnumFlagPresent(AffectParts.RightLeg),
                SmoothTime = smoothness,
                OutSmoothTime = outSmoothness
            };

            onMotionIKBlendModifier?.Invoke(true, animator, modifier);
        }

        override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            Modifier modifier = new()
            {
                Weight = onExitIKWeight,
                AffectLeftArm = affectParts.IsEnumFlagPresent(AffectParts.LeftArm),
                AffectRightArm = affectParts.IsEnumFlagPresent(AffectParts.RightArm),
                AffectLeftLeg = affectParts.IsEnumFlagPresent(AffectParts.LeftLeg),
                AffectRightLeg = affectParts.IsEnumFlagPresent(AffectParts.RightLeg),
                SmoothTime = smoothness,
                OutSmoothTime = outSmoothness
            };
            onMotionIKBlendModifier?.Invoke(false, animator, modifier);
        }
    }
}
