using MFPS.Internal;
using MFPS.Internal.Scriptables;
using MFPSEditor;
using System.Collections;
using UnityEngine;
using static MFPS.Internal.Scriptables.bl_LadderSettings;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MFPS.Runtime.Level
{
    public class bl_Ladder : bl_MonoBehaviour
    {

        public enum LadderStatus
        {
            None,
            Attaching,
            Climbing,
            Detaching,
        }

        [ScriptableDrawer] public bl_LadderSettings settings;
        public Vector3 topExitOffset = new(0f, 0f, 0.5f);   // Offset applied when exiting at the top
        public Vector3 bottomExitOffset = Vector3.zero;

        [Header("References")]
        [SerializeField] private Transform startExitPoint = null;
        [SerializeField] private Transform endExitPoint = null;

        [Space]
        public bl_EventHandler.UEvent onClimbStep;
        public bl_EventHandler.UEvent onExitLadder;
        public bl_EventHandler.UEvent onStartClimbing;

        public LadderStatus Status
        {
            get;
            set;
        } = LadderStatus.None;

        public bool Exiting
        {
            get;
            set;
        } = false;

        private float LastTime = 0;
        private bl_PlayerReferences player;
        private bool isPlayerInRange = false;
        private float climbTimer = 0f;
        private bool isPaused = false;
        private float lastVerticalInput = 0f;

        protected override void Awake()
        {
            // require in order to not run the update method when the player is not in the ladder
        }

        protected override void OnEnable()
        {
            // require in order to not run the update method when the player is not in the ladder
        }

        /// <summary>
        /// Called by the ladder trigger when the player enters or exits the trigger area
        /// </summary>
        /// <param name="player"></param>
        /// <param name="enter"></param>
        public void OnPlayerTrigger(bl_PlayerReferences player, bool enter)
        {
            if (!CanUse || Status != LadderStatus.None) return;

            if (enter)
            {
                this.player = player;
                isPlayerInRange = true;
                bl_UpdateManager.AddItem(this);

                if (settings.activeMode == ActiveMode.Input)
                {
                    bl_InputInteractionIndicator.ShowIndication(bl_Input.GetButtonName("Interact"), bl_GameTexts.ClimbLadder.Localized(275), () =>
                    {
                        StartClimb();
                    });
                }
                else if (settings.activeMode == ActiveMode.Auto)
                {
                    StartClimb();
                }
            }
            else
            {
                if (settings.activeMode != ActiveMode.Input) return;

                player = null;
                isPlayerInRange = false;
                bl_UpdateManager.RemoveSpecificItem(this);
                bl_InputInteractionIndicator.SetActive(false);
            }
        }

        /// <summary>
        /// This update is ONLY called every frame when the player is inside the ladder trigger area or climbing.
        /// </summary>
        public override void OnUpdate()
        {
            if ((isPlayerInRange || Status == LadderStatus.Climbing) && player == null)
            {
                Status = LadderStatus.None;
                OnOffLadder();
                return;
            }

            if (Status == LadderStatus.Climbing)
            {
                Climb();
            }

            InputListen();
        }

        /// <summary>
        /// 
        /// </summary>
        private void InputListen()
        {
            if (isPlayerInRange && settings.activeMode == ActiveMode.Input)
            {
                if (bl_GameInput.Interact() && CanUse)
                {
                    StartClimb();
                }
            }

            if (Status == LadderStatus.Climbing)
            {
                if (bl_GameInput.Jump())
                {
                    JumpOut();
                }
            }
        }

        /// <summary>
        /// Called when the player starts climbing the ladder
        /// </summary>
        public void StartClimb()
        {
            if (player == null)
            {
                Debug.LogWarning("Player reference is null. Cannot start climbing.");
                OnOffLadder();
                Status = LadderStatus.None;
                return;
            }

            Status = LadderStatus.Climbing;
            isPlayerInRange = false;
            player.firstPersonController.Stop();
            player.firstPersonController.State = PlayerState.Climbing;

            Vector3 playerCenter = GetPlayerCenterPosition();
            Vector3 nearestPoint = GetNearestPointOnLineWithOffset(startExitPoint.position, endExitPoint.position, playerCenter);

            // Calculate the movement needed to get to the nearest point
            Vector3 movementToNearestPoint = (nearestPoint - playerCenter);
            player.characterController.Move(movementToNearestPoint);

            // Get the desired facing direction
            Vector3 faceDirection = GetFacingDirection();
            Vector3 finalRot = player.firstPersonController.GetMouseLook().SetCharacterForward(faceDirection);

            float playerYRotation = finalRot.y;
            player.firstPersonController.GetMouseLook().SetActiveHorizontalClamp(true, playerYRotation, settings.viewAngleRange);

            // Reset climbing movement variables
            climbTimer = 0f;
            isPaused = false;

            // Reset last vertical input
            lastVerticalInput = 0f;

            player.firstPersonController.IsControlable = false;
            if (settings.hideWeapons)
            {
                player.gunManager.BlockAllWeapons();
            }

            bl_InputInteractionIndicator.ShowIndication(bl_Input.GetButtonName("Jump"), bl_GameTexts.JumpOffLadder, () =>
            {
                JumpOut();
            });

#if MFPSM
            bl_TouchHelper.OnJump += OnJumpPress;
#endif

            onStartClimbing?.Invoke();
        }

        /// <summary>
        /// Called every frame when the player is climbing the ladder
        /// </summary>
        void Climb()
        {
            float verticalInput = bl_GameInput.Vertical;

#if MFPSM
            if (bl_UtilityHelper.isMobile)
            {
                verticalInput = bl_MFPSMobileControl.GetMovementAxis().y;
            }
#endif

            // Update the last non-zero vertical input
            if (Mathf.Abs(verticalInput) > 0.1f)
            {
                lastVerticalInput = verticalInput;
            }

            if (Mathf.Abs(verticalInput) > 0.1f)
            {
                if (!isPaused)
                {
                    Vector3 climbDirection = (endExitPoint.position - startExitPoint.position).normalized;
                    Vector3 climbMovement = settings.climbSpeed * Time.deltaTime * verticalInput * climbDirection;

                    climbTimer += Time.deltaTime;

                    if (climbTimer >= settings.climbInterval)
                    {
                        climbTimer = 0f;
                        isPaused = true;

                        onClimbStep?.Invoke();

                        player.firstPersonController.GetFootStep().PlayStepForTag(GetTag());
                    }

                    player.characterController.Move(climbMovement * 150 * Time.deltaTime);
                }
                else
                {
                    // Pause movement briefly to simulate hand change
                    climbTimer += Time.deltaTime;

                    if (climbTimer >= settings.pauseDuration) // Pause duration
                    {
                        climbTimer = 0f;
                        isPaused = false;
                    }

                    player.characterController.Move(Vector3.zero);
                }
            }
            else
            {
                // Reset timers if no input
                climbTimer = 0f;
                isPaused = false;

                player.characterController.Move(Vector3.zero);
            }

            // Check if player reached an exit point
            if (ReachedExitPoint())
            {
                EndClimbing();
            }
        }

        /// <summary>
        /// Called when the player reaches the top or bottom of the ladder
        /// </summary>
        void EndClimbing()
        {
            OnOffLadder();
            // Determine closest exit point based on climbing direction
            Transform exitPoint;
            Vector3 exitOffset;

            if (lastVerticalInput > 0)
            {
                // Climbing up, so exit at the top
                exitPoint = endExitPoint;
                exitOffset = topExitOffset;
            }
            else
            {
                // Climbing down, so exit at the bottom
                exitPoint = startExitPoint;
                exitOffset = bottomExitOffset;
            }

            // Apply the offset relative to the ladder's rotation
            Vector3 relativeOffset = transform.TransformDirection(exitOffset);

            Vector3 targetPosition = exitPoint.position + relativeOffset - player.transform.TransformVector(player.characterController.center);

            StartCoroutine(MoveToExitPoint(targetPosition));
        }

        /// <summary>
        /// Ends the climbing state and detaches the player from the ladder right away.
        /// </summary>
        public void JumpOut()
        {
            LastTime = Time.time;
            Status = LadderStatus.None;
            Exiting = false;

            if (player != null)
            {
                Vector3 jumpDir = -player.transform.forward * settings.jumpBackDistance;
                jumpDir.y = settings.jumpUpDistance;
                player.firstPersonController.IsControlable = true;
                player.firstPersonController.AddForce(jumpDir);
            }

            OnOffLadder(true);
            onExitLadder?.Invoke();
        }

        /// <summary>
        /// 
        /// </summary>
        private void OnOffLadder(bool resetPlayer = false)
        {
            if (resetPlayer && player != null)
            {
                player.firstPersonController.IsControlable = true;
                player.firstPersonController.State = PlayerState.Idle;
                player.firstPersonController.GetMouseLook().SetActiveHorizontalClamp(false);
                if (player != null)
                {
                    if (settings.hideWeapons) { player.gunManager.ReleaseWeapons(false); }
                }
                player = null;
            }

            bl_UpdateManager.RemoveSpecificItem(this);
            isPlayerInRange = false;
            bl_InputInteractionIndicator.SetActive(false);
#if MFPSM
            bl_TouchHelper.OnJump -= OnJumpPress;
#endif
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="targetPosition"></param>
        /// <returns></returns>
        IEnumerator MoveToExitPoint(Vector3 targetPosition)
        {
            Status = LadderStatus.Detaching;
            float transitionDuration = 0.5f;
            float elapsedTime = 0f;
            Vector3 startingPosition = player.transform.position;

            while (elapsedTime < transitionDuration)
            {
                player.transform.position = Vector3.Lerp(startingPosition, targetPosition, (elapsedTime / transitionDuration));
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            player.transform.position = targetPosition;
            // Re-enable character controller after moving to exit point
            OnOffLadder(true);
            Status = LadderStatus.None;
            onExitLadder?.Invoke();

        }

        /// <summary>
        /// 
        /// </summary>
        private void OnJumpPress()
        {
            if (Status == LadderStatus.Climbing)
            {
                JumpOut();
            }
        }

        Vector3 GetPlayerCenterPosition()
        {
            return player.transform.position + player.transform.TransformVector(player.characterController.center);
        }

        private bool ReachedExitPoint()
        {
            Vector3 playerCenterPosition = GetPlayerCenterPosition();
            Vector3 lineStart = startExitPoint.position;
            Vector3 lineEnd = endExitPoint.position;
            Vector3 lineDirection = lineEnd - lineStart;
            float lineLength = lineDirection.magnitude;
            lineDirection.Normalize();

            Vector3 startToPlayer = playerCenterPosition - lineStart;
            float projectedLength = Vector3.Dot(startToPlayer, lineDirection);
            float t = projectedLength / lineLength;

            float tolerance = 0.05f;

            // Adjust exit detection based on climbing direction
            if (lastVerticalInput > 0)
            {
                // Climbing up
                if (t >= 1 - tolerance)
                {
                    return true;
                }
            }
            else if (lastVerticalInput < 0)
            {
                // Climbing down
                if (t <= 0 + tolerance)
                {
                    return true;
                }
            }

            return false;
        }

        private Vector3 GetNearestPointOnLineWithOffset(Vector3 linePoint1, Vector3 linePoint2, Vector3 point)
        {
            Vector3 lineDirection = linePoint2 - linePoint1;
            float lineLength = lineDirection.magnitude;
            lineDirection.Normalize();

            Vector3 startToPoint = point - linePoint1;
            float projectedLength = Vector3.Dot(startToPoint, lineDirection);

            float t = projectedLength / lineLength;

            // Apply offset to t to prevent placing the player too close to the exit points
            float minT = settings.exitPointOffset / lineLength;
            float maxT = 1 - (settings.exitPointOffset / lineLength);
            t = Mathf.Clamp(t, minT, maxT);

            return linePoint1 + (lineDirection * (t * lineLength));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string GetTag()
        {
            return gameObject.tag;
        }

        private Vector3 GetFacingDirection()
        {
            switch (settings.facingDirection)
            {
                case FacingDirection.Forward:
                    return transform.forward;
                case FacingDirection.Backward:
                    return -transform.forward;
                case FacingDirection.Left:
                    return -transform.right;
                case FacingDirection.Right:
                    return transform.right;
                case FacingDirection.Up:
                    return transform.up;
                case FacingDirection.Down:
                    return -transform.up;
                default:
                    return transform.forward;
            }
        }

        public bool CanUse
        {
            get
            {
                return ((Time.time - LastTime) > 1.5f);
            }
        }
#if UNITY_EDITOR
        void OnDrawGizmos()
        {
            if (Application.isPlaying) { return; }

            if (startExitPoint != null && endExitPoint != null)
            {
                Gizmos.color = Color.green;
                // Draw line between start and end exit points
                Gizmos.DrawLine(startExitPoint.position, endExitPoint.position);

                // Draw spheres at exit points
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(startExitPoint.position, 0.15f);
                Gizmos.DrawSphere(endExitPoint.position, 0.15f);

                if (settings == null) { return; }
                // Draw direction the player will face when starting to climb
                Gizmos.color = Color.yellow;
                Vector3 midPoint = (startExitPoint.position + endExitPoint.position) / 2;
                Vector3 faceDirection = GetFacingDirection();
                Gizmos.DrawLine(midPoint, midPoint + (faceDirection * 1f));
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (startExitPoint != null && endExitPoint != null)
            {
                // Draw exit position offsets
                Gizmos.color = Color.blue;
                Vector3 relativeOffset = transform.TransformDirection(bottomExitOffset);
                Handles.RectangleHandleCap(0, startExitPoint.position + relativeOffset, Quaternion.LookRotation(Vector3.up), 0.5f, EventType.Repaint);
                relativeOffset = transform.TransformDirection(topExitOffset);
                Handles.RectangleHandleCap(0, endExitPoint.position + relativeOffset, Quaternion.LookRotation(Vector3.up), 0.5f, EventType.Repaint);
            }
        }
#endif
    }
}