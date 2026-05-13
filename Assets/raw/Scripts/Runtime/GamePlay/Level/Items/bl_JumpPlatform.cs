using MFPS.Audio;
using UnityEngine;

namespace MFPS.Runtime.Level
{
    public class bl_JumpPlatform : MonoBehaviour
    {
        public Vector3 ForceDirection;
        public float ForceMultiplier = 1;
        [LovattoToogle] public bool damageInmune = true;
        [SerializeField] private Transform directionIndicator = null;
        [SerializeField] private string bounceSoundKey = "jump-pad-bounce";

        public bl_EventHandler.UEvent onPlayerBounce;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        private void OnTriggerEnter(Collider other)
        {
            if (other.isLocalPlayerCollider())
            {
                var fpc = other.GetComponent<bl_FirstPersonControllerBase>();
                fpc.AddForce(ForceDirection * ForceMultiplier, damageInmune);
                if (!string.IsNullOrEmpty(bounceSoundKey))
                {
                    bl_AudioController.PlayClipAtPoint(bounceSoundKey, transform.position);
                }

                onPlayerBounce?.Invoke();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void OnDrawGizmos()
        {
#if UNITY_EDITOR
            // draw handle arrow
            UnityEditor.Handles.color = Color.green;
            UnityEditor.Handles.ArrowHandleCap(0, transform.position, Quaternion.LookRotation(ForceDirection.normalized), 1, EventType.Repaint);
#endif

            if (directionIndicator != null)
            {
                directionIndicator.LookAt(transform.position + (ForceDirection * ForceMultiplier));
            }
        }
    }
}