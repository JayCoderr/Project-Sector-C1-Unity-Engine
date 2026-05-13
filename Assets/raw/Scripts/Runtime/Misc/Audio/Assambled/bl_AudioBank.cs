using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace MFPS.Audio
{
    [CreateAssetMenu(fileName = "Audio Bank", menuName = "MFPS/Audio/Bank", order = 301)]
    public class bl_AudioBank : ScriptableObject
    {
        public List<AudioInfo> AudioBank = new();

        public AudioInfo PlayAudioInSource(AudioSource source, string bankInfo, float customVolume = -1, float pitch = 1)
        {
            var info = GetInfoOf(bankInfo);
            if (info == null) return null;

            source.volume = customVolume == -1 ? info.Volume : customVolume;
            source.loop = info.Loop;
            source.spatialBlend = info.SpacialAudio ? 1 : 0;
            source.pitch = pitch;
            source.clip = info.Clip;
            source.outputAudioMixerGroup = info.AudioMixerGroup;
            source.Play();
            return info;
        }

        /// <summary>
        /// 
        /// </summary>
        public AudioInfo GetInfoOf(string bankName)
        {
            return AudioBank.Find(x => x.Name.ToLower() == bankName.ToLower());
        }

        [System.Serializable]
        public class AudioInfo
        {
            public string Name;
            public AudioClip Clip;
            [Range(0, 1)] public float Volume = 0.9f;
            public bool SpacialAudio = false;
            public bool Loop = false;
            public AudioMixerGroup AudioMixerGroup = null;
        }
    }
}