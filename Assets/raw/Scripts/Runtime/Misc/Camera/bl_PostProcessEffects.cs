#define USE_PPV2 // comment this line to stop using PostProcess references.
using UnityEngine;
#if UNITY_POST_PROCESSING_STACK_V2 && USE_PPV2
using UnityEngine.Rendering.PostProcessing;
#endif

/// <summary>
/// IF YOU HAVE A ERROR IN THE CONSOLE POINTING TO THIS SCRIPT
/// is probably due to the Post Processing package is not imported in the project.
/// If this is intended (not to use it) in order to fix the problem, go to the Unity Player Settings -> Other Settings -> Script Define Symbols ->
/// find and remove this string from the input field: UNITY_POST_PROCESSING_STACK_V2; -> hit Enter/Submit.
/// If it's not intended, go to Window -> Package Manager -> Find the 'Post Processing' package on the left panel -> Import it.
/// </summary>
public class bl_PostProcessEffects : MonoBehaviour
{
#if UNITY_POST_PROCESSING_STACK_V2 && USE_PPV2
    public PostProcessProfile processProfile;

    /// <summary>
    /// 
    /// </summary>
    private void Awake()
    {
        var sceneVolume = GetComponent<PostProcessVolume>();
        if (sceneVolume != null) sceneVolume.profile = processProfile;
    }

    /// <summary>
    /// 
    /// </summary>
    private void OnEnable()
    {
        bl_EventHandler.onEffectChange += OnPostEffect;
        bl_EventHandler.Player.onLocalDoubleJump += OnPlayerDoubleJump;
    }

    /// <summary>
    /// 
    /// </summary>
    private void OnDisable()
    {
        bl_EventHandler.onEffectChange -= OnPostEffect;
        bl_EventHandler.Player.onLocalDoubleJump -= OnPlayerDoubleJump;
    }

    /// <summary>
    /// 
    /// </summary>
    void OnPostEffect(bool chrab, bool anti, bool bloom, bool ssao, bool motionBlur)
    {
        if (processProfile == null) return;

        if (processProfile.HasSettings(typeof(ChromaticAberration)))
        {
            processProfile.GetSetting<ChromaticAberration>().active = chrab;
        }
        if (processProfile.HasSettings(typeof(Bloom)))
        {
            processProfile.GetSetting<Bloom>().active = bloom;
        }
        if (processProfile.HasSettings(typeof(AmbientOcclusion)))
        {
            processProfile.GetSetting<AmbientOcclusion>().active = ssao;
        }
        if (processProfile.HasSettings(typeof(MotionBlur)))
        {
            processProfile.GetSetting<MotionBlur>().active = motionBlur;
        }
    }

    void OnPlayerDoubleJump()
    {
        if (processProfile == null) return;

        float defaultIntensity = -1;
        // increase the chromatic aberration effect when the player double jumps
        if (processProfile.HasSettings(typeof(ChromaticAberration)))
        {
            var chromatic = processProfile.GetSetting<ChromaticAberration>();
            defaultIntensity = chromatic.intensity.value;
            chromatic.intensity.value = 1;
        }

        // fade out the chromatic aberration effect after a few seconds
        if (defaultIntensity != -1)
        {
            StartCoroutine(FadeOutChromatic(defaultIntensity, 1f));
        }
    }

    System.Collections.IEnumerator FadeOutChromatic(float target, float time)
    {
        float elapsedTime = 0;
        var chromatic = processProfile.GetSetting<ChromaticAberration>();
        float start = chromatic.intensity.value;
        while (elapsedTime < time)
        {
            elapsedTime += Time.deltaTime;
            chromatic.intensity.value = Mathf.Lerp(start, target, elapsedTime / time);
            yield return null;
        }
    }
#endif
}