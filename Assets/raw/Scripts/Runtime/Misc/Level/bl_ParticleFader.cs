using System;
using System.Collections;
using UnityEngine;

namespace MFPS.Runtime.Misc
{
    public class bl_ParticleFader : MonoBehaviour
    {
        [LovattoToogle] public bool StartFadeEmit = false;
        [LovattoToogle] public bool DestroyAfterTime = false;
        [Range(0.1f, 5)] public float fadeDuration = 1;
        [Range(1, 10)] public float DestroyTime = 7;
        [SerializeField] private AudioSource audioLoopSource = null;
        public bl_EventHandler.UEvent onBeforeFadeOut;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private IEnumerator Start()
        {
            if (DestroyAfterTime)
            {
                Invoke(nameof(DestroyParticles), DestroyTime - 1);
            }
            if (!StartFadeEmit) yield break;

            ParticleSystem.EmissionModule e = GetComponent<ParticleSystem>().emission;
            float defaultEmission = e.rateOverTime.constant;
            float defaultVolume = audioLoopSource != null ? audioLoopSource.volume : 1;
            e.rateOverTime = 0;
            float t = 0;
            while (t < 1)
            {
                t += Time.deltaTime / fadeDuration;
                e.rateOverTime = t * defaultEmission;
                if (audioLoopSource != null)
                {
                    audioLoopSource.volume = t * defaultVolume;
                }
                yield return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        void DestroyParticles()
        {
            onBeforeFadeOut?.Invoke();
            StartCoroutine(FadeOut());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="onFinish"></param>
        public void DoFadeOut(bool autoDestroy = true, Action onFinish = null)
        {
            StopAllCoroutines();
            StartCoroutine(FadeOut(autoDestroy, onFinish));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IEnumerator FadeOut(bool autoDestroy = true, Action onFinish = null)
        {
            ParticleSystem.EmissionModule e = GetComponent<ParticleSystem>().emission;
            ParticleSystem.MinMaxCurve mc = e.rateOverTime;
            float original = mc.constant;
            float originalVolume = audioLoopSource != null ? audioLoopSource.volume : 1;
            float d = 0;
            while (d < 1)
            {
                d += Time.deltaTime / fadeDuration;
                e.rateOverTime = Mathf.Lerp(original, 0, d);
                if (audioLoopSource != null)
                {
                    audioLoopSource.volume = Mathf.Lerp(originalVolume, 0, d);
                }
                yield return null;
            }
            yield return new WaitForSeconds(GetComponent<ParticleSystem>().main.startLifetime.constant);
            onFinish?.Invoke();

            if (autoDestroy) Destroy(gameObject);
        }
    }
}