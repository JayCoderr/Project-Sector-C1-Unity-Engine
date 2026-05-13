using UnityEngine;

namespace MFPS.PlayerController
{
    public abstract class MouseLookBase
    {

        /// <summary>
        /// 
        /// </summary>
        public abstract Vector2 HorizontalLimits
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        public abstract Vector2 VerticalLimits
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="character"></param>
        /// <param name="camera"></param>
        public abstract void Init(Transform character, Transform camera);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="character"></param>
        /// <param name="camera"></param>
        /// <param name="ladder"></param>
        public abstract void UpdateLook(Transform character, Transform camera, bool withInput = true);

        /// <summary>
        /// 
        /// </summary>
        public abstract void LookAt(Transform reference, bool extrapolate = true);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="angle"></param>
        public abstract void SetTiltAngle(float angle);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        public abstract void ClampHorizontalRotation(float min, float max);

        /// <summary>
        /// 
        /// </summary>
        public abstract void UnClampHorizontal();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public abstract void SetVerticalOffset(float value);

        /// <summary>
        /// 
        /// </summary>
        public abstract void CombineVerticalOffset();

        /// <summary>
        /// Enable or disable the horizontal clamp rotation
        /// </summary>
        /// <param name="active"></param>
        /// <param name="centerAngle"></param>
        /// <param name="range"></param>
        public virtual void SetActiveHorizontalClamp(bool active, float centerAngle = 0, float range = 45)
        {
        }

        /// <summary>
        /// This method sets the character's rotation based on a forward vector
        /// </summary>
        /// <param name="forward"></param>
        public virtual Vector3 SetCharacterForward(Vector3 forward)
        {
            return Vector3.zero;
        }

        /// <summary>
        /// Enable or disable the orbiting camera mode.
        /// </summary>
        /// <param name="enable"></param>
        public virtual void SetOrbitMode(bool enable)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="minimun"></param>
        /// <param name="value"></param>
        public void SetVerticalClamp(bool minimun, float value)
        {
            var v = VerticalLimits;
            if (minimun) v.x = value;
            else v.y = value;

            VerticalLimits = v;
        }
    }
}