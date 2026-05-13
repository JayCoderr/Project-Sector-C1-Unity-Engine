using System;
using UnityEngine;
using UnityEngine.Events;

namespace MFPS.Internal.Utility
{
    public class bl_EventInvoker : MonoBehaviour
    {
        [Serializable] public class UEvent : UnityEvent { }
        public UEvent[] onEvent;

        [SerializeField] private Vector3[] vectors;
        [SerializeField] private AudioClip[] audioClips;
        [SerializeField] private ShakeMotion[] shakeMotions;

        private AudioSource audioSource;
        private bl_PlayerReferences playerRefs;

        [Serializable]
        public class ShakeMotion
        {
            public string ShakeID;
            public float Duration = 0.4f;
            public int Intensity = 500;
        }

        /// <summary>
        /// 
        /// </summary>
        public void InvokeEvent(int eventId)
        {
            if (eventId >= onEvent.Length) { return; }
            onEvent[eventId]?.Invoke();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="vectorId"></param>

        public void SetCameraRotation(int vectorId)
        {
            PlayerRefs.cameraMotion.AddCameraRotation(3, vectors[vectorId], true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="audioId"></param>
        public void PlayAudio(int audioId)
        {
            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
                if (audioSource == null)
                {
                    audioSource = gameObject.AddComponent<AudioSource>();
                }
            }

            audioSource.PlayOneShot(audioClips[audioId]);
        }

        public void PlayerMotionShake(int shakeId)
        {
            if (shakeId >= shakeMotions.Length) { return; }
            ShakeMotion sm = shakeMotions[shakeId];
            if (playerRefs == null) { playerRefs = GetComponentInParent<bl_PlayerReferences>(); }
            if (playerRefs != null)
            {

                playerRefs.cameraMotion.PlayCurveShake(sm.ShakeID, sm.Duration, sm.Intensity);
            }
        }

        private bl_PlayerReferences _playerRefs = null;
        private bl_PlayerReferences PlayerRefs
        {
            get
            {
                if (_playerRefs == null) _playerRefs = GetComponentInParent<bl_PlayerReferences>();
                return _playerRefs;
            }
        }
    }
}