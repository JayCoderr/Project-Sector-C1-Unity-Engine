using UnityEngine;

namespace MFPS.Audio
{
    /// <summary>
    /// Play an audio clip using multiple audio sources to avoid overlapping.
    /// </summary>
    public class bl_LayeredAudioSource : MonoBehaviour
    {
        [Tooltip("If true, the script will create a new audio source if all the current audio sources are playing a sound.")]
        [LovattoToogle] public bool increaseLayersOnDemand = true;
        [Range(0, 1)] public float volume = 1;
        [SerializeField] private AudioSource[] m_AudioSources;

        private int m_CurrentSourceIndex = 0;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="clip"></param>
        /// <param name="volume"></param>
        /// <param name="pitch"></param>
        /// <param name="spatialBlend"></param>
        public void Play(AudioClip clip, float volume = 1, float pitch = 1, float spatialBlend = -1)
        {
            if (clip == null) return;

            bool found = true;
            while (m_AudioSources[m_CurrentSourceIndex] == null || (m_AudioSources[m_CurrentSourceIndex].isPlaying && increaseLayersOnDemand))
            {
                m_CurrentSourceIndex = (m_CurrentSourceIndex + 1) % m_AudioSources.Length;

                if (m_CurrentSourceIndex == 0)
                {
                    found = false;
                    break;
                }
            }

            if (!found)
            {
                var newSources = new AudioSource[m_AudioSources.Length + 1];
                for (int i = 0; i < m_AudioSources.Length; i++)
                {
                    newSources[i] = m_AudioSources[i];
                }
                m_AudioSources = newSources;
                m_AudioSources[m_AudioSources.Length - 1] = gameObject.AddComponent<AudioSource>();
                m_CurrentSourceIndex = m_AudioSources.Length - 1;
                return;
            }

            m_AudioSources[m_CurrentSourceIndex].clip = clip;
            m_AudioSources[m_CurrentSourceIndex].volume = this.volume * volume;
            m_AudioSources[m_CurrentSourceIndex].pitch = pitch;
            if (spatialBlend > 0) m_AudioSources[m_CurrentSourceIndex].spatialBlend = spatialBlend;
            m_AudioSources[m_CurrentSourceIndex].Play();

            m_CurrentSourceIndex = (m_CurrentSourceIndex + 1) % m_AudioSources.Length;
        }
    }
}