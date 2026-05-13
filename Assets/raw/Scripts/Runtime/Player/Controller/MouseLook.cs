using MFPS.Internal.Scriptables;
using System;
using UnityEngine;

namespace MFPS.PlayerController
{
    [Serializable]
    public class MouseLook : MouseLookBase
    {
        public class FrameSmoothing
        {
            private readonly float[] frames;
            private int currentFrame = 0;
            private float total = 0;
            private float average = 0;
            private readonly int frameCount = 1;
            private int filledFrames = 0;

            public float GetValue() => average;

            public FrameSmoothing(int maxFrames)
            {
                frames = new float[maxFrames];
                frameCount = maxFrames;
                currentFrame = 0;
                total = 0;
                filledFrames = 0;
            }

            public void Set(float frameValue)
            {
                if (filledFrames >= frameCount)
                {
                    total -= frames[currentFrame];
                }
                else
                {
                    filledFrames++;
                }

                frames[currentFrame] = frameValue;
                total += frameValue;

                // Move index
                currentFrame = (currentFrame + 1) % frameCount;
                average = total / filledFrames;
            }
        }

        #region Public members
        public bool clampVerticalRotation = true;
        public float MinimumX = -90F;
        public float MaximumX = 90F;
        #endregion

        #region Private members
        private Quaternion m_CharacterTargetRot, m_CameraTargetRot;
        private bool InvertVertical, InvertHorizontal;
        private float verticalRotation, horizontalRotation;
        private float sensitivity, aimSensitivity = 3f;
        private Quaternion extraRotation = Quaternion.identity;
        private Transform m_CameraTransform, m_CharacterBody;
        private FrameSmoothing xSmoothing, ySmoothing;
        private bool ClampHorizontal = false;
        private Vector2 horizontalClamp = new(-360, 360);
        private bl_PlayerReferences playerReferences;
        private MouseLookSettings lookSettings;
        private float tiltValue, tiltTarget = 0;
        private float verticalOffset = 0;
        private bool isHorizontalClamp = false;
        private float horizontalClampCenter = 0f;
        private float horizontalClampRange = 40f;

        private bool isOrbitMode = false;
        private Vector3 defaultCameraLocalPos;
        private float currentOrbitDistance;
        private float orbitPitch, orbitYaw;

        private float m_Yaw, m_Pitch;
        #endregion

        public float CurrentSensitivity { get; set; } = 3;
        public bool OnlyCameraTransform { get; set; } = false;

        /// <summary>
        ///  Initialize the camera controller with the character initial rotation.
        /// </summary>
        public override void Init(Transform character, Transform camera)
        {
            m_CameraTransform = camera;
            m_CharacterBody = character;
            m_CharacterTargetRot = character.localRotation;
            m_CameraTargetRot = camera.localRotation;
            defaultCameraLocalPos = camera.localPosition;

            m_Yaw = m_CharacterTargetRot.eulerAngles.y;
            m_Pitch = NormalizeAngle(m_CameraTargetRot.eulerAngles.x);

            FetchSettings();
            CurrentSensitivity = sensitivity;
            playerReferences = character.GetComponent<bl_PlayerReferences>();
            lookSettings = bl_GameData.Instance.mouseLookSettings;
            xSmoothing = new FrameSmoothing(lookSettings.framesOfSmoothing);
            ySmoothing = new FrameSmoothing(lookSettings.framesOfSmoothing);
        }

        /// <summary>
        /// Updates the character and camera rotation based on the player input.
        /// </summary>
        public override void UpdateLook(Transform character, Transform camera, bool withInput = true)
        {
            // When use a mobile device or Unity Remote
            if (bl_UtilityHelper.isMobile)
            {
#if MFPSM
                Vector2 input = bl_TouchPad.Instance.GetInput(CurrentSensitivity);
                input.x = !InvertHorizontal ? input.x : (input.x * -1f);
                input.y = InvertVertical ? (input.y * -1f) : input.y;

                Move(input.x, input.y, character, camera);
#endif
            }
            else
            {
                float inputX = bl_GameInput.MouseX;
                float inputY = bl_GameInput.MouseY;
                if (!bl_GameInput.IsCursorLocked || !withInput)
                {
                    inputX = 0;
                    inputY = 0;
                }

                Move(inputX, inputY, character, camera);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Move(float inputX, float inputY, Transform character, Transform camera)
        {
            if (isOrbitMode)
            {
                HandleOrbitMode(inputX, inputY, character, camera);
                return;
            }

            horizontalRotation = inputX * CurrentSensitivity;
            horizontalRotation = (InvertHorizontal) ? (horizontalRotation * -1f) : horizontalRotation;

            if (lookSettings.useSmoothing)
            {
                xSmoothing.Set(horizontalRotation);
                inputX = xSmoothing.GetValue();
            }
            else inputX = horizontalRotation;

            m_Yaw += inputX;

            verticalRotation = inputY * CurrentSensitivity;
            verticalRotation = InvertVertical ? (verticalRotation * -1f) : verticalRotation;
            if (lookSettings.useSmoothing)
            {
                ySmoothing.Set(verticalRotation);
                inputY = ySmoothing.GetValue();
            }
            else inputY = verticalRotation;

            m_Pitch -= inputY;
            m_Pitch = Mathf.Clamp(m_Pitch, MinimumX, MaximumX);

            // Handle horizontal clamping
            if (isHorizontalClamp)
            {
                float minAngle = horizontalClampCenter - (horizontalClampRange / 2f);
                float maxAngle = horizontalClampCenter + (horizontalClampRange / 2f);
                m_Yaw = ClampAngle(m_Yaw, minAngle, maxAngle);
            }
            else if (ClampHorizontal)
            {
                m_Yaw = ClampAngle(m_Yaw, horizontalClamp.x, horizontalClamp.y);
            }

            if (!OnlyCameraTransform)
            {
                m_CharacterTargetRot = Quaternion.Euler(0f, m_Yaw, 0f);
                m_CameraTargetRot = Quaternion.Euler(m_Pitch, 0f, 0f);
            }
            else
            {
                // In detached camera mode, we rotate the camera relative to the body's locked rotation.
                // We calculate the delta between the desired global yaw (m_Yaw) and the body's current locked yaw.
                float bodyYaw = m_CharacterTargetRot.eulerAngles.y;
                float localCameraYaw = Mathf.DeltaAngle(bodyYaw, m_Yaw);

                m_CameraTargetRot = Quaternion.Euler(m_Pitch, localCameraYaw, 0f);
            }

            // Apply rotation with optional smoothing
            if (lookSettings.lerpMovement)
            {
                if (character != null && !OnlyCameraTransform)
                {
                    character.localRotation = Quaternion.Slerp(character.localRotation, m_CharacterTargetRot, lookSettings.smoothTime * Time.deltaTime);
                }
                if (camera != null)
                {
                    // Apply extra rotation (tilt/headbob) on top of target rotation
                    var finalCamRot = m_CameraTargetRot * extraRotation;
                    camera.localRotation = Quaternion.Slerp(camera.localRotation, finalCamRot, lookSettings.smoothTime * Time.deltaTime);
                }
            }
            else
            {
                if (!OnlyCameraTransform)
                {
                    character.localRotation = m_CharacterTargetRot;
                }

                // Directly apply combined rotation
                camera.localRotation = m_CameraTargetRot * extraRotation;
            }
        }

        /// <summary>
        /// Enable or disable the orbiting camera mode.
        /// While in orbit mode only the camera will rotate based on the mouse input, the character body rotation will remain static.
        /// </summary>
        public override void SetOrbitMode(bool enable)
        {
            if (isOrbitMode == enable) return;

            isOrbitMode = enable;
            if (isOrbitMode)
            {
                Vector3 angles = m_CameraTransform.eulerAngles;
                orbitYaw = angles.y;
                orbitPitch = angles.x;

                if (orbitPitch > 180) orbitPitch -= 360;
                currentOrbitDistance = 0f;
            }
            else
            {
                if (m_CameraTransform != null)
                {
                    m_CameraTransform.localPosition = defaultCameraLocalPos;
                    m_CameraTransform.localRotation = m_CameraTargetRot;
                }
            }
        }

        /// <summary>
        /// Handle the orbital camera movement.
        /// </summary>
        private void HandleOrbitMode(float x, float y, Transform character, Transform camera)
        {
            orbitYaw += x * CurrentSensitivity;
            orbitPitch -= y * CurrentSensitivity;
            orbitPitch = ClampAngle(orbitPitch, MinimumX, MaximumX);

            Quaternion rotation = Quaternion.Euler(orbitPitch, orbitYaw, 0);
            Vector3 pivot = character.TransformPoint(lookSettings.orbitPivotOffset);

            Vector3 dir = rotation * Vector3.back;
            Vector3 desiredPos = pivot + (dir * lookSettings.orbitDistance);
            float targetDist = lookSettings.orbitDistance;

            if (Physics.Linecast(pivot, desiredPos, out RaycastHit hit, lookSettings.orbitCollisionMask))
            {
                targetDist = Mathf.Clamp(hit.distance - 0.2f, lookSettings.minOrbitDistance, lookSettings.orbitDistance);
            }

            if (targetDist < currentOrbitDistance)
            {
                currentOrbitDistance = Mathf.Lerp(currentOrbitDistance, targetDist, Time.deltaTime * 20f);
            }
            else
            {
                currentOrbitDistance = Mathf.Lerp(currentOrbitDistance, targetDist, Time.deltaTime * lookSettings.orbitSmoothTime);
            }

            camera.position = pivot + (dir * currentOrbitDistance);
            camera.rotation = rotation;
        }

        /// <summary>
        /// 
        /// </summary>
        public void Update()
        {
            tiltValue = Mathf.Lerp(tiltValue, tiltTarget, Time.deltaTime * 4);
            extraRotation = Quaternion.Euler(verticalOffset, 0, tiltValue);
        }

        /// <summary>
        /// 
        /// </summary>
        public override void SetVerticalOffset(float amount)
        {
            verticalOffset = amount;
            extraRotation = Quaternion.Euler(amount, 0, tiltValue);
        }

        /// <summary>
        /// 
        /// </summary>
        public override void CombineVerticalOffset()
        {
            m_CameraTargetRot *= extraRotation;
            verticalOffset = 0;
            extraRotation = Quaternion.Euler(verticalOffset, 0, tiltValue);

            if (clampVerticalRotation)
            {
                m_CameraTargetRot = ClampRotationAroundXAxis(m_CameraTargetRot);
            }
        }

        /// <summary>
        /// Don't rotate the character body, only the Camera/Head
        /// </summary>
        public void UseOnlyCameraRotation()
        {
            OnlyCameraTransform = true;
        }

        /// <summary>
        /// Port the Current Camera Rotation to separate the vertical and horizontal rotation in the body and head
        /// horizontal rotation for the body and vertical for the camera/head
        /// That should only be called when OnlyCameraRotation was used before.
        /// </summary>
        public void PortBodyOrientationToCamera()
        {
            OnlyCameraTransform = false;

            // Apply the accumulated m_Yaw (which represents the camera's look direction)
            // to the character body as its new forward direction.
            m_CharacterTargetRot = Quaternion.Euler(0f, m_Yaw, 0f);
            m_CharacterBody.localRotation = m_CharacterTargetRot;

            // Reset the camera's local Yaw to 0 since the body is now aligned with the look direction.
            // Pitch remains as is.
            m_CameraTargetRot = Quaternion.Euler(m_Pitch, 0f, 0f);

            // Apply the new camera rotation immediately combined with any extra rotation (e.g., tilt/headbob)
            m_CameraTransform.localRotation = m_CameraTargetRot * extraRotation;
        }

        /// <summary>
        /// Forces the character to look at a position in the world.
        /// </summary>
        public override void LookAt(Transform reference, bool extrapolate = true)
        {
            // Update accumulators
            m_Yaw = reference.eulerAngles.y;
            // Calculate relative pitch
            Quaternion relative = Quaternion.Inverse(Quaternion.identity) * reference.rotation;
            m_Pitch = NormalizeAngle(relative.eulerAngles.x);

            m_CharacterTargetRot = Quaternion.Euler(0f, m_Yaw, 0f);
            m_CameraTargetRot = Quaternion.Euler(m_Pitch, 0, 0);

            if (extrapolate)
            {
                m_CharacterBody.localRotation = m_CharacterTargetRot;
                m_CameraTransform.localRotation = m_CameraTargetRot;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void LookAt(Vector3 direction, bool extrapolate = true, float influence = 1)
        {
            Quaternion rotate = Quaternion.LookRotation(direction);
            if (bl_MFPS.LocalPlayer.IsThirdPersonView())
            {
                rotate = Quaternion.FromToRotation(bl_MFPS.LocalPlayerReferences.PlayerCameraTransform.forward, direction);
                rotate *= m_CameraTransform.rotation;
            }

            rotate = Quaternion.Slerp(m_CameraTransform.rotation, rotate, influence);

            // Update accumulators
            m_Yaw = rotate.eulerAngles.y;
            m_Pitch = NormalizeAngle(rotate.eulerAngles.x);

            m_CharacterTargetRot = Quaternion.Euler(0f, m_Yaw, 0f);
            m_CameraTargetRot = Quaternion.Euler(m_Pitch, 0, 0);

            if (extrapolate)
            {
                m_CharacterBody.localRotation = m_CharacterTargetRot;
                m_CameraTransform.localRotation = m_CameraTargetRot;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="angle"></param>
        public override void SetTiltAngle(float angle)
        {
            tiltTarget = angle;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="forward"></param>
        public override Vector3 SetCharacterForward(Vector3 forward)
        {
            // Calculate the rotation based on the forward vector
            Quaternion targetRotation = Quaternion.LookRotation(forward);

            m_Yaw = targetRotation.eulerAngles.y;

            // Update internal variables
            m_CharacterTargetRot = Quaternion.Euler(0f, m_Yaw, 0f);

            // Apply rotation
            if (!OnlyCameraTransform)
            {
                m_CharacterBody.localRotation = m_CharacterTargetRot;
            }
            else
            {
                // If only the camera is rotating, adjust accordingly
                var fixEuler = m_CameraTargetRot.eulerAngles;
                fixEuler.y = m_Yaw;
                m_CameraTargetRot = Quaternion.Euler(fixEuler);
                m_CameraTransform.localRotation = m_CameraTargetRot * extraRotation;
            }
            return m_CharacterTargetRot.eulerAngles;
        }

        /// <summary>
        /// 
        /// </summary>
        public void FetchSettings()
        {
            sensitivity = (float)bl_MFPS.Settings.GetSettingOf("Sensitivity");
            aimSensitivity = (float)bl_MFPS.Settings.GetSettingOf("Aim Sensitivity");
            InvertHorizontal = (bool)bl_MFPS.Settings.GetSettingOf("MouseH Invert");
            InvertVertical = (bool)bl_MFPS.Settings.GetSettingOf("MouseV Invert");
            CurrentSensitivity = sensitivity;
        }

        /// <summary>
        /// 
        /// </summary>
        public void OnAimChange(bool isAiming)
        {
            CurrentSensitivity = isAiming ? sensitivity * aimSensitivity : sensitivity;

            if (isAiming && lookSettings.aimSensitivityAdjust == MouseLookSettings.AimSensitivityAdjust.Relative)
                AdjustSensitivityBasedOnFOV();
        }

        /// <summary>
        /// Adjust the mouse sensitivity based on the camera field of view.
        /// </summary>
        public void AdjustSensitivityBasedOnFOV()
        {
            if (playerReferences == null) return;

            float fovPercentage = playerReferences.playerCamera.fieldOfView / playerReferences.DefaultCameraFOV;
            CurrentSensitivity *= fovPercentage;
        }

        /// <summary>
        /// 
        /// </summary>
        public override void ClampHorizontalRotation(float min, float max)
        {
            horizontalClamp = new Vector2(min, max);
            ClampHorizontal = true;
        }

        /// <summary>
        /// 
        /// </summary>
        public override void UnClampHorizontal() => ClampHorizontal = false;

        /// <summary>
        /// Enables the climbing clamp mode with a center angle and range.
        /// </summary>
        public override void SetActiveHorizontalClamp(bool active, float centerAngle = 0, float range = 45)
        {
            isHorizontalClamp = active;
            if (!active) return;

            horizontalClampCenter = centerAngle;
            horizontalClampRange = range;
        }

        /// <summary>
        /// Clamps an angle between a minimum and maximum value, handling angle wrapping.
        /// </summary>
        private float ClampAngle(float angle, float min, float max)
        {
            angle = NormalizeAngle(angle);
            min = NormalizeAngle(min);
            max = NormalizeAngle(max);

            // Check if the angle range crosses the -180/180 boundary
            bool angleRangeCrossesBoundary = min > max;

            if (!angleRangeCrossesBoundary)
            {
                // Standard clamping
                return Mathf.Clamp(angle, min, max);
            }
            else
            {
                // Angle range crosses the boundary
                // Check if the angle is outside the allowed range
                if (angle > max && angle < min)
                {
                    // Decide whether to clamp to min or max based on which is closer
                    float diffToMin = Mathf.Abs(Mathf.DeltaAngle(angle, min));
                    float diffToMax = Mathf.Abs(Mathf.DeltaAngle(angle, max));
                    return (diffToMin < diffToMax) ? min : max;
                }
                else
                {
                    // Angle is within the allowed range
                    return angle;
                }
            }
        }

        private float NormalizeAngle(float angle)
        {
            angle = angle % 360;
            if (angle > 180)
                angle -= 360;
            else if (angle < -180)
                angle += 360;
            return angle;
        }

        public float VerticalAngle => m_CameraTransform.localEulerAngles.x;

        Quaternion ClampRotationAroundXAxis(Quaternion q) => bl_MathUtility.ClampRotationAroundAxis(q, MinimumX, MaximumX, UnityEngine.Animations.Axis.X);

        public override Vector2 HorizontalLimits { get => horizontalClamp; set => horizontalClamp = value; }
        public override Vector2 VerticalLimits
        {
            get => new(MinimumX, MaximumX);
            set
            {
                MinimumX = value.x;
                MaximumX = value.y;
            }
        }
    }
}