using UnityEngine;

namespace MFPS.Audio
{
    [RequireComponent(typeof(AudioSource))]
    public class bl_AudioRandomPlayer : MonoBehaviour
    {
        [LovattoToogle] public bool playOnEnable = true;
        [LovattoToogle] public bool randomPitch = false;
        [SerializeField] private AudioClip[] clips = null;
        [SerializeField] private Vector2 pitchRange = new(0.94f, 1.12f);

        private AudioSource audioSource;

        /// <summary>
        /// 
        /// </summary>
        private void OnEnable()
        {
            if (playOnEnable) { PlayRandom(); }
        }

        /// <summary>
        /// 
        /// </summary>
        public void PlayRandom()
        {
            if (clips.Length <= 0) return;

            if (audioSource == null) { audioSource = GetComponent<AudioSource>(); }
            if (audioSource == null) { audioSource = gameObject.AddComponent<AudioSource>(); }

            int r = Random.Range(0, clips.Length);

            audioSource.clip = clips[r];
            if (randomPitch) { audioSource.pitch = Random.Range(pitchRange.x, pitchRange.y); }
            audioSource.Play();
        }
    }
}