using MFPSEditor;
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
#if !UNITY_WEBGL && PVOICE
using Photon.Voice.Unity;
using Photon.Voice.PUN;
#endif

public class PhotonVoiceAddon : MonoBehaviour
{

    private const string DEFINE_KEY = "PVOICE";

#if !PVOICE
    [MenuItem("MFPS/Addons/Voice/Enable")]
    private static void Enable()
    {
        bl_GameData.CoreSettings.UseVoiceChat = true;
        Type type = AppDomain.CurrentDomain.GetAssemblies()
    .SelectMany(a => a.GetTypes())
    .FirstOrDefault(t => t.FullName == "Photon.Voice.Unity.Recorder");
        if (type != null)
        {
            EditorUtility.SetDirty(bl_GameData.Instance);
            EditorUtils.SetEnabled(DEFINE_KEY, true);
        }
        else
        {
            // EditorWindow.GetWindow<IntegratePVoiceTutorial>();
            Debug.LogWarning("Can't enable Photon Voice yet, please import the required files first.");
        }
    }
#endif

#if PVOICE
    [MenuItem("MFPS/Addons/Voice/Disable")]
    private static void Disable()
    {
        bl_GameData.CoreSettings.UseVoiceChat = false;
        EditorUtility.SetDirty(bl_GameData.Instance);
        EditorUtils.SetEnabled(DEFINE_KEY, false);
    }
#endif

    [MenuItem("MFPS/Addons/Voice/Integrate")]
    private static void Instegrate()
    {

#if PVOICE
        //setup the player 1
        SetUpPlayerPrefab(bl_GameData.Instance.Player1.gameObject);
        SetUpPlayerPrefab(bl_GameData.Instance.Player2.gameObject);

#if PSELECTOR
        foreach(var p in MFPS.Addon.PlayerSelector.bl_PlayerSelectorData.Instance.AllPlayers)
        {
            if (p.Prefab == null ) continue;
         SetUpPlayerPrefab(p.Prefab.gameObject);
        }
#endif

        if (AssetDatabase.IsValidFolder("Assets/MFPS/Scenes"))
        {
            UnityEditor.SceneManagement.EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
            string path = "Assets/MFPS/Scenes/MainMenu.unity";
            UnityEditor.SceneManagement.EditorSceneManager.OpenScene(path, UnityEditor.SceneManagement.OpenSceneMode.Single);
            bl_Lobby lb = FindFirstObjectByType<bl_Lobby>();
            if (lb != null)
            {
                GameObject old = GameObject.Find("PhotonVoice");
                if (old != null)
                {
                    DestroyImmediate(old);
                    Debug.Log("Remove old setup");
                }
                if (FindFirstObjectByType<PunVoiceClient>() == null)
                {
                    GameObject nobj = new("PhotonVoice");
                    var pvs = nobj.AddComponent<PunVoiceClient>();
                    pvs.AutoConnectAndJoin = true;
                    pvs.ApplyDontDestroyOnLoad = true;
                    pvs.KeepAliveInBackground = 5000;
                    pvs.ApplyDontDestroyOnLoad = true;

                    Recorder r = nobj.AddComponent<Recorder>();
                    r.MicrophoneType = Recorder.MicType.Unity;
                    r.TransmitEnabled = true;
                    r.VoiceDetection = true;
                    r.DebugEchoMode = false;

                    pvs.PrimaryRecorder = r;
                    nobj.AddComponent<bl_PhotonAudioDisabler>().isGlobal = true;
                    EditorUtility.SetDirty(nobj);
                    EditorUtility.SetDirty(pvs);
                    UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
                }
                Debug.Log("Photon Voice Integrated, enable it on GameData.");
            }
            else
            {
                Debug.Log("Can't found Menu scene.");
            }
        }
        else
        {
            Debug.LogWarning("Can't complete the integration of the addons because MFPS folder structure has been change, please do the manual integration.");
        }
#else
        Debug.LogWarning("Enable Photon Voice addon before integrate.");
#endif
    }

#if PVOICE
    [MenuItem("MFPS/Addons/Voice/Setup Players")]
    private static void SetupPlayers()
    {
        SetUpPlayerPrefab(bl_GameData.Instance.Player1.gameObject);
        SetUpPlayerPrefab(bl_GameData.Instance.Player2.gameObject);
#if PSELECTOR
        var allPlayers = bl_PlayerSelector.Data.AllPlayers;
        foreach (var p in allPlayers)
        {
            if (p.Prefab == null) continue;
            SetUpPlayerPrefab(p.Prefab.gameObject);
        }
#endif
        Debug.Log("All player prefabs has been updated.");
    }

    static void SetUpPlayerPrefab(GameObject prefab)
    {
        if (prefab == null) return;

        GameObject p1 = prefab;
        if (!p1.TryGetComponent<PhotonVoiceView>(out var pvv))
        {
            p1.AddComponent<PhotonVoiceView>();
        }

        Speaker speaker = p1.GetComponentInChildren<Speaker>();
        if (speaker == null)
        {
            var holder = p1.transform.GetChild(0).gameObject;
            var audioSource = holder.GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = holder.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
                audioSource.loop = false;
                audioSource.spatialBlend = 1; // proximity chat by default
                audioSource.rolloffMode = AudioRolloffMode.Logarithmic;
                audioSource.minDistance = 2f;
                audioSource.maxDistance = 35f;
            }
            holder.AddComponent<Speaker>();
        }
        EditorUtility.SetDirty(p1);
    }
#endif

#if UNITY_POST_PROCESSING_STACK_V2
    [MenuItem("MFPS/Tools/Delete Post-Processing")]
    private static void DeletePP()
    {
        UnityEditor.PackageManager.Client.Remove("com.unity.postprocessing");
        EditorUtils.SetEnabled("UNITY_POST_PROCESSING_STACK_V2", false);
    }
#endif

}