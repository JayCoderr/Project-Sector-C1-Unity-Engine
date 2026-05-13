using System.Collections;
using UnityEngine;

public class bl_PhotonAudioDisabler : MonoBehaviour
{
    public bool isGlobal = false;

    private void Awake()
    {
        bl_PhotonAudioDisabler p = FindAnyObjectByType<bl_PhotonAudioDisabler>();
        if (p != null && p != this)
        {
            Destroy(gameObject);
        }
    }

    private IEnumerator Start()
    {
        if (!bl_GameData.isDataCached)
        {
            while (!bl_GameData.isDataCached) { yield return null; }
        }

        if (!bl_GameData.CoreSettings.UseVoiceChat)
        {
            Destroy(gameObject);
        }
        else if (isGlobal)
        {
            DontDestroyOnLoad(gameObject);
        }
    }
}