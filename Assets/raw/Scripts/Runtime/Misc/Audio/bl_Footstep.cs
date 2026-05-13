using MFPS.Audio;
using MFPS.Internal.Scriptables;
using MFPSEditor;
using UnityEngine;

public class bl_Footstep : MonoBehaviour
{
    public bl_FootStepsLibrary footStepsLibrary;
    [ScriptableDrawer] public bl_FootstepSettings settings;

    private AudioSource audioSource;
    private RaycastHit m_raycastHit;
    private Transform m_Transform;
    private string surfaceTag;
    private float nextRate = 0;
    private float lastStepTime = 0;
    private float volumeMultiplier = 1;
    private float timeOffset = 0;
    private bool silent = false;

    /// <summary>
    /// 
    /// </summary>
    private void Awake()
    {
        m_Transform = transform;
        if (!TryGetComponent(out audioSource))
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1;
        if (bl_AudioController.Instance != null)
        {
            audioSource.maxDistance = bl_AudioController.Instance.maxFootstepDistance;
            audioSource.rolloffMode = bl_AudioController.Instance.audioRolloffMode;
            audioSource.minDistance = bl_AudioController.Instance.maxFootstepDistance * 0.05f;
        }
        audioSource.spatialize = true;
    }

    /// <summary>
    /// 
    /// </summary>
    public void UpdateStep(float speed)
    {
        if (settings == null) return;
        if ((Time.time + timeOffset) - lastStepTime <= nextRate) return;

        bool isSprinting = speed > settings.walkSpeed + 1;

        if (isSprinting) nextRate = settings.runStepRate;
        else nextRate = speed < (settings.walkSpeed - 1) ? settings.crouchStepRate : settings.walkStepRate;

        DetectAndPlaySurface(isSprinting);

        lastStepTime = Time.time;
    }

    /// <summary>
    /// 
    /// </summary>
    public void DetectAndPlaySurface(bool sprinting = false)
    {
        if (Physics.Raycast(m_Transform.position, -Vector3.up, out m_raycastHit, 5, settings.surfaceLayers, QueryTriggerInteraction.Ignore))
        {
            surfaceTag = m_raycastHit.transform.tag;
            if (m_raycastHit.transform.TryGetComponent<bl_TerrainSurfaces>(out var ts))
            {
                surfaceTag = ts.GetSurfaceTag(m_raycastHit.point);
            }
            PlayStepForTag(surfaceTag, sprinting);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void PlayStepForTag(string tag, bool sprint = false)
    {
        if (silent) return;

        AudioClip step = GetStepSoundFor(tag);
        PlayStepSound(step, sprint);
    }

    /// <summary>
    /// 
    /// </summary>
    public void PlayStepSound(AudioClip clip, bool sprint = false)
    {
        if (silent) return;

        float volume = volumeMultiplier;
        if (footStepsLibrary != null)
        {
            audioSource.pitch = footStepsLibrary.GetRandomPitch(sprint);
            volume *= footStepsLibrary.GetRandomVolume();
        }

        audioSource.clip = clip;
        audioSource.volume = settings.stepsVolume * volume;
        audioSource.Play();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="multiplier"></param>
    public void SetVolumeMuliplier(float multiplier)
    {
        volumeMultiplier = multiplier;
    }

    /// <summary> 
    /// Get a random step sound clip for the given tag
    /// </summary>
    /// <returns></returns>
    public AudioClip GetStepSoundFor(string tag)
    {
        if (footStepsLibrary == null) return null;

        var stepsGroup = footStepsLibrary.GetGroupFor(tag);
        return stepsGroup.GetRandomClip();
    }

    /// <summary>
    /// Set if the footstep should be silent
    /// </summary>
    /// <param name="value"></param>
    public void SetSilent(bool value)
    {
        silent = value;
    }

    /// <summary>
    /// Set the time offset for the footstep
    /// </summary>
    public void SetTimeOffset()
    {
        timeOffset = Random.Range(0, 0.1f);
    }
}