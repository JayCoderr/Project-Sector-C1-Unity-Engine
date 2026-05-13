using UnityEngine;

namespace MFPS.Runtime.UI
{
    public class bl_DamageIndicator : bl_DamageIndicatorBase
    {
        #region Public members
        [Range(1, 5)] public float FadeTime = 3;
        [Header("References")]
        public RectTransform indicatorPivot;
        public CanvasGroup indicatorAlpha;
        #endregion

        #region Private members
        private float alpha = 0.0f;
        Vector3 eulerAngle = Vector3.zero;
        private Vector3 attackDirection;
        #endregion

        /// <summary>
        /// 
        /// </summary>
        protected override void Awake()
        {
            base.Awake();
            enabled = bl_GameData.CoreSettings.showDamageIndicator;
            bl_EventHandler.onLocalPlayerSpawn += OnLocalSpawn;
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void OnDisable()
        {
            base.OnDisable();
            bl_EventHandler.onLocalPlayerSpawn -= OnLocalSpawn;
            if (indicatorAlpha != null)
                indicatorAlpha.alpha = 0;
        }

        /// <summary>
        /// 
        /// </summary>
        void OnLocalSpawn()
        {
            indicatorAlpha.alpha = 0;
            alpha = 0;
        }

        /// <summary>
        /// Use this to send a new direction of attack
        /// </summary>
        public override void SetHit(HitInfo hitInfo)
        {
            if (hitInfo.Direction == Vector3.zero)
                return;

            attackDirection = hitInfo.Direction;
            alpha = 3f;
        }

        /// <summary>
        /// if this is visible Update position
        /// </summary>
        public override void OnUpdate()
        {
            if (alpha <= 0) return;
            if (bl_MFPS.LocalPlayerReferences == null) return;

            alpha -= Time.deltaTime;
            UpdateDirection();
        }

        /// <summary>
        /// update direction as the arrow shows
        /// </summary>
        void UpdateDirection()
        {
            // Calculate the direction from the player to the attack source
            Vector3 attackDirectionNormalized = attackDirection - bl_MFPS.LocalPlayerReferences.PlayerCameraTransform.position;
            attackDirectionNormalized.y = 0;
            attackDirectionNormalized.Normalize();

            // Get the forward direction of the current camera or fallback to transform forward
            Vector3 forward = bl_CameraIdentity.CurrentCamera != null
                ? bl_CameraIdentity.CurrentCamera.transform.forward
                : transform.forward;
            forward.y = 0; // Ensure the forward vector is only horizontal
            forward.Normalize();

            // Calculate the angle between forward and the attack direction in the horizontal plane
            float angle = Vector3.SignedAngle(forward, attackDirectionNormalized, Vector3.up);

            if (indicatorPivot != null)
            {
                // Apply the rotation to the indicator
                indicatorAlpha.alpha = alpha; // Assuming alpha is defined elsewhere in your script
                eulerAngle.z = -angle; // Set the Z rotation to point the indicator correctly
                indicatorPivot.eulerAngles = eulerAngle;
            }
        }
    }
}