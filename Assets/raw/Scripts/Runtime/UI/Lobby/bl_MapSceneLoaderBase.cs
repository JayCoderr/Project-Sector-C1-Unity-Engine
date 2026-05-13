using UnityEngine;

/// <summary>
/// This class is used to load a map scene from the lobby
/// Map scene loadings are done asynchronously and load 2 scenes:
/// The Room UI scene and the Map scene, that is why a special loader is needed.
/// </summary>
public abstract class bl_MapSceneLoaderBase : MonoBehaviour
{
    /// <summary>
    /// Load a map scene by name
    /// </summary>
    /// <param name="mapSceneName"></param>
    /// <returns></returns>
    public static bool LoadMapScene(string mapSceneName)
    {
        if (Instance == null) { return false; }

        Instance.LoadMap(mapSceneName);
        return true;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="mapSceneName"></param>
    public abstract void LoadMap(string mapSceneName);

    private static bl_MapSceneLoaderBase _instance;
    public static bl_MapSceneLoaderBase Instance
    {
        get
        {
            if (_instance == null) { _instance = FindAnyObjectByType<bl_MapSceneLoaderBase>(); }
            return _instance;
        }
    }
}
