using System.Collections.Generic;
using UnityEngine;

namespace MFPS.Audio
{
    public class bl_AudioInstance : MonoBehaviour
    {
        private AudioSource aSource;
        public int ID { get; set; }
        public bl_AudioBank.AudioInfo playingAudio { get; private set; }
        private Queue<SequenceClip> sequenceClips;
        private Transform defaultParent;
        private bool isReparented = false;

        public class SequenceClip
        {
            public AudioClip Clip;
            public float Delay;
        }

        /// <summary>
        /// 
        /// </summary>
        public void Init()
        {
            if (aSource == null) { aSource = gameObject.AddComponent<AudioSource>(); }
        }

        /// <summary>
        /// Plays the audio clip associated with this instance.
        /// </summary>
        /// <remarks>The method activates the audio source, plays the clip, and schedules the instance to
        /// stop  automatically after the clip's adjusted duration, based on its pitch.</remarks>
        /// <returns>The current <see cref="bl_AudioInstance"/> to allow for method chaining.</returns>
        public bl_AudioInstance Play()
        {
            SetActive(true);
            var source = GetSource();
            source.Play();

            float adjustedLength = source.clip.length / source.pitch;
            Invoke(nameof(Stop), adjustedLength);
            return this;
        }

        /// <summary>
        /// Play the current audio setup
        /// </summary>
        public bl_AudioInstance PlayDelayed(float delay)
        {
            SetActive(true);
            var source = GetSource();
            source.PlayDelayed(delay);

            float adjustedLength = source.clip.length / source.pitch;
            Invoke(nameof(Stop), adjustedLength + delay);
            return this;
        }

        /// <summary>
        /// Adds a subsequent audio clip to the playback sequence with an optional delay.
        /// </summary>
        /// <param name="nextClip">The audio clip to be played after the current clip.</param>
        /// <param name="delay">The delay, in seconds, before playing the next clip. Defaults to 0.</param>
        /// <returns>The current <see cref="bl_AudioInstance"/> instance, allowing for method chaining.</returns>
        public bl_AudioInstance Then(AudioClip nextClip, float delay = 0)
        {
            sequenceClips ??= new();
            sequenceClips.Enqueue(new SequenceClip() { Clip = nextClip, Delay = delay });
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        public int SetInstance(bl_AudioBank.AudioInfo info)
        {
            if (info == null || info.Clip == null) return ID;

            if (aSource == null) { aSource = gameObject.AddComponent<AudioSource>(); }

            aSource.clip = info.Clip;
            aSource.volume = info.Volume;
            aSource.spatialBlend = info.SpacialAudio ? 1 : 0;
            aSource.loop = info.Loop;

            aSource.Play();
            playingAudio = info;
            if (!info.Loop)
            {
                float adjustedLength = info.Clip.length / aSource.pitch;
                Invoke(nameof(Stop), adjustedLength);
            }
            return ID;
        }

        /// <summary>
        /// 
        /// </summary>
        public void SetInstance(AudioClip clip, float volume, bool loop, float spatial)
        {
            var source = GetSource();
            source.clip = clip;
            source.volume = volume;
            source.spatialBlend = spatial;
            source.loop = loop;
            source.Play();

            if (!loop)
            {
                Invoke(nameof(Stop), clip.length);
            }
        }

        /// <summary>
        /// Stops the current audio playback and deactivates the GameObject when no more audio clips are queued.
        /// </summary>
        /// <remarks>If there are queued audio clips, the next clip in the sequence will be played
        /// automatically,  with an optional delay before playback. The method continues processing the queue until all
        /// clips  have been played, at which point the audio source is stopped and the GameObject is
        /// deactivated.</remarks>
        public void Stop()
        {
            if (sequenceClips != null && sequenceClips.Count > 0)
            {
                var nextClip = sequenceClips.Dequeue();
                aSource.clip = nextClip.Clip;

                if (nextClip.Delay <= 0) aSource.Play();
                else aSource.PlayDelayed(nextClip.Delay);

                float adjustedLength = aSource.clip.length / aSource.pitch;
                Invoke(nameof(Stop), adjustedLength + nextClip.Delay);

                return;
            }

            aSource.Stop();

            if (isReparented)
            {
                transform.SetParent(defaultParent);
                isReparented = false;
            }

            gameObject.SetActive(false);
        }

        /// <summary>
        /// Sets the position of the audio instance in world space.
        /// </summary>
        /// <param name="pos">The new position to set, specified as a <see cref="Vector3"/>.</param>
        /// <returns>The current <see cref="bl_AudioInstance"/> instance, allowing for method chaining.</returns>
        public bl_AudioInstance SetPosition(Vector3 pos)
        {
            transform.position = pos;
            return this;
        }

        /// <summary>
        /// Sets the pitch of the audio source.
        /// </summary>
        /// <param name="pitch">The desired pitch value. Typically, values greater than 1.0 increase the pitch, while values less than 1.0
        /// decrease it.</param>
        /// <returns>The current <see cref="bl_AudioInstance"/> instance, allowing for method chaining.</returns>
        public bl_AudioInstance SetPitch(float pitch)
        {
            GetSource().pitch = pitch;
            return this;
        }

        /// <summary>
        /// Sets the volume level for the audio source.
        /// </summary>
        /// <remarks>The volume value is clamped internally by the audio system to ensure it remains
        /// within the valid range.</remarks>
        /// <param name="volume">The desired volume level, ranging from 0.0 (silent) to 1.0 (full volume).</param>
        /// <returns>The current <see cref="bl_AudioInstance"/> instance, allowing for method chaining.</returns>
        public bl_AudioInstance SetVolume(float volume)
        {
            GetSource().volume = volume;
            return this;
        }

        /// <summary>
        /// Sets whether the audio instance should loop during playback.
        /// </summary>
        /// <param name="loop">A value indicating whether the audio should loop.  <see langword="true"/> to enable looping; otherwise, <see
        /// langword="false"/>.</param>
        /// <returns>The current <see cref="bl_AudioInstance"/> instance, allowing for method chaining.</returns>
        public bl_AudioInstance SetLoop(bool loop)
        {
            GetSource().loop = loop;
            return this;
        }

        /// <summary>
        /// Sets the spatial blend of the audio source.
        /// </summary>
        /// <param name="spatial">The spatial blend value, ranging from 0.0 (2D sound) to 1.0 (3D sound).</param>
        /// <returns>The current <see cref="bl_AudioInstance"/> instance for method chaining.</returns>
        public bl_AudioInstance SetSpatialBlend(float spatial)
        {
            GetSource().spatialBlend = spatial;
            return this;
        }

        /// <summary>
        /// Configures the audio source's distance range and rolloff mode.
        /// </summary>
        /// <param name="min">The minimum distance at which the audio source starts to attenuate. Must be a non-negative value.</param>
        /// <param name="max">The maximum distance at which the audio source stops attenuating. Must be greater than or equal to <paramref
        /// name="min"/>.</param>
        /// <param name="rolloffMode">The mode used to calculate the attenuation of the audio source over distance. Defaults to <see
        /// cref="AudioRolloffMode.Linear"/>.</param>
        /// <returns>The current <see cref="bl_AudioInstance"/> instance, allowing for method chaining.</returns>
        public bl_AudioInstance SetRange(float min, float max, AudioRolloffMode rolloffMode = AudioRolloffMode.Linear)
        {
            var source = GetSource();
            source.minDistance = min;
            source.maxDistance = max;
            source.rolloffMode = rolloffMode;
            return this;
        }

        /// <summary>
        /// Sets the pitch of the audio source to a random value within the specified range.
        /// </summary>
        /// <param name="min">The minimum pitch value. Must be less than or equal to <paramref name="max"/>.</param>
        /// <param name="max">The maximum pitch value. Must be greater than or equal to <paramref name="min"/>.</param>
        /// <returns>The current <see cref="bl_AudioInstance"/> instance, allowing for method chaining.</returns>
        public bl_AudioInstance SetPitch(float min, float max)
        {
            GetSource().pitch = Random.Range(min, max);
            return this;
        }

        /// <summary>
        /// Sets the audio clip to be played by the audio instance.
        /// </summary>
        /// <param name="clip">The <see cref="AudioClip"/> to assign to the audio source. Cannot be null.</param>
        /// <returns>The current <see cref="bl_AudioInstance"/> instance, allowing for method chaining.</returns>
        public bl_AudioInstance SetClip(AudioClip clip)
        {
            GetSource().clip = clip;
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="clips"></param>
        /// <returns></returns>
        public bl_AudioInstance SetRandomClip(AudioClip[] clips)
        {
            if (clips.Length == 0) return this;
            int randomIndex = Random.Range(0, clips.Length);
            GetSource().clip = clips[randomIndex];
            return this;
        }

        /// <summary>
        /// Attaches the current object to the specified target transform.
        /// </summary>
        /// <param name="target">The transform to which the current object will be attached.</param>
        /// <param name="resetPosition">A boolean value indicating whether the local position of the current object should be reset to  Vector3.zero
        /// after being attached.  true to reset the position; otherwise, false.</param>
        /// <returns>The current instance of bl_AudioInstance for method chaining.</returns>
        public bl_AudioInstance AttachTo(Transform target, bool resetPosition = false)
        {
            if (target == null) return this;

            if (defaultParent == null)
            {
                defaultParent = transform.parent;
            }
            transform.SetParent(target);
            if (resetPosition) transform.localPosition = Vector3.zero;
            isReparented = true;

            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="active"></param>
        public void SetActive(bool active)
        {
            gameObject.SetActive(active);
        }

        /// <summary>
        /// 
        /// </summary>
        public bool IsPlaying
        {
            get
            {
                if (aSource == null) return false;

                return aSource.isPlaying;
            }
        }

        /// <summary>
        /// Get the AudioSource component
        /// </summary>
        /// <returns></returns>
        public AudioSource GetSource()
        {
            if (aSource == null) { aSource = gameObject.AddComponent<AudioSource>(); }
            return aSource;
        }

        public bool IsPlayingClip(string audioName) { return (IsPlaying && (playingAudio.Name.ToLower() == audioName.ToLower())); }
    }
}