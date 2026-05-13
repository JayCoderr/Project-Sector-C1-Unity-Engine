using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class bl_FootStepsLibrary : ScriptableObject
{
    public AudioGroup[] Groups;
    public Vector2 pitchRange = new(0.95f, 1.05f);
    public Vector2 sprintPitchRange = new(1.02f, 1.1f);
    public Vector2 volumeRange = new(0.8f, 1f);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="tag"></param>
    /// <returns></returns>
    public AudioGroup GetGroupFor(string tag)
    {
        for (int i = 0; i < Groups.Length; i++)
        {
            if (Groups[i].Tag.Equals(tag))
            {
                return Groups[i];
            }
        }
        return Groups[0];
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="isSprinting"></param>
    /// <returns></returns>
    public float GetRandomPitch(bool isSprinting)
    {
        return isSprinting ? Random.Range(sprintPitchRange.x, sprintPitchRange.y) : Random.Range(pitchRange.x, pitchRange.y);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public float GetRandomVolume()
    {
        return UnityEngine.Random.Range(volumeRange.x, volumeRange.y);
    }

    [Serializable]
    public class AudioGroup
    {
        public string Tag;
        public AudioClip[] StepSounds;

        public AudioClip GetRandomClip()
        {
            return StepSounds[UnityEngine.Random.Range(0, StepSounds.Length)];
        }
    }
}