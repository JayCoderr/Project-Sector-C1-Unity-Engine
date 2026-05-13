using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class bl_FlashbangUI : MonoBehaviour
{
    [SerializeField] private AnimationCurve flashAlphaCurve;
    [Header("References")]
    [SerializeField] private GameObject content = null;
    [SerializeField] private CanvasGroup flashAlpha = null;
    [SerializeField] private CanvasGroup frameAlpha = null;
    [SerializeField] private RawImage frameImg = null;

    private AudioSource flashAudio;

    /// <summary>
    /// 
    /// </summary>
    private void OnEnable()
    {
        bl_EventHandler.onLocalPlayerDeath += OnLocalPlayerDeath;
    }

    /// <summary>
    /// 
    /// </summary>
    private void OnDisable()
    {
        bl_EventHandler.onLocalPlayerDeath -= OnLocalPlayerDeath;
    }

    /// <summary>
    /// 
    /// </summary>
    void OnLocalPlayerDeath()
    {
        if (!content.activeSelf) return;

        StopAllCoroutines();
        content.SetActive(false);
        if (flashAudio) flashAudio.Stop();
        var defaultSnapshot = bl_GlobalReferences.I.MFPSAudioMixer.FindSnapshot("Default");
        if (defaultSnapshot != null) { defaultSnapshot.TransitionTo(0.1f); }
    }

    /// <summary>
    /// 
    /// </summary>
    public void DoFlash(float amount, Texture2D frameTexture, bl_FlashExplosion source)
    {
        if (amount <= 0) return;

        frameImg.texture = frameTexture;
        flashAlpha.alpha = 1;
        frameAlpha.alpha = 0.85f;
        content.SetActive(true);
        if (flashAudio == null) { TryGetComponent(out flashAudio); }
        flashAudio.volume = 1;
        flashAudio.Play();

        StopAllCoroutines();
        StartCoroutine(DoEffect());

        IEnumerator DoEffect()
        {
            float blindTime = source.maxBlindDuration * amount;
            yield return new WaitForSeconds(blindTime);
            float d = 0;
            float t;
            float duration = source.maxFadeDuration * amount;
            while (d < 1)
            {
                d += Time.deltaTime / duration;
                t = flashAlphaCurve.Evaluate(d);
                flashAlpha.alpha = Mathf.Lerp(1, 0, t);
                frameAlpha.alpha = Mathf.Lerp(0.85f, 0, d);
                flashAudio.volume = Mathf.Lerp(1, 0.1f, d);
                yield return null;
            }
            content.SetActive(false);
            var defaultSnapshot = bl_GlobalReferences.I.MFPSAudioMixer.FindSnapshot("Default");
            if (defaultSnapshot != null) { defaultSnapshot.TransitionTo(2f); }
            d = 0;
            while (d < 1)
            {
                d += Time.deltaTime;
                flashAudio.volume = Mathf.Lerp(0.1f, 0, d);
                yield return null;
            }

            flashAudio.Stop();
            if (bl_MFPS.LocalPlayerReferences != null) bl_MFPS.LocalPlayerReferences.playerAnimations.CustomCommand(PlayerAnimationCommands.OnUnFlashed);
        }
    }

    private static bl_FlashbangUI instance;
    public static bl_FlashbangUI Instance
    {
        get
        {
            if (instance == null) { instance = FindAnyObjectByType<bl_FlashbangUI>(); }
            return instance;
        }
    }
}