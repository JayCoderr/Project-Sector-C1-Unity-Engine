using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public class MFPSMapSceneLoaderEditor
{
    static MFPSMapSceneLoaderEditor()
    {
        EditorSceneManager.sceneOpened += OnSceneOpened;
    }

    private static void OnSceneOpened(Scene scene, OpenSceneMode mode)
    {
        // Only run in the editor outside of Play mode
        if (Application.isPlaying)
            return;
        if (!bl_GameData.CoreSettings.automaticallyLoadRoomUI) { return; }

        // Check if the opened scene is a map scene
        if (IsMapScene(scene))
        {
            // Check if the UI scene is already loaded
            if (!IsSceneLoaded(bl_GameData.Instance.UIScene.Name))
            {
                // Load the UI scene additively
                string uiScenePath = GetUIScenePath();
                if (!string.IsNullOrEmpty(uiScenePath))
                {
                    EditorSceneManager.OpenScene(uiScenePath, OpenSceneMode.Additive);
                }
                else
                {
                    Debug.LogError("UI Scene path not found. Please check the path in GetUIScenePath().");
                }
            }
        }
    }

    private static bool IsMapScene(Scene scene)
    {
        var all = bl_GameData.Instance.AllScenes;
        for (int i = 0; i < all.Count; i++)
        {
            if (all[i].Path == scene.path)
            {
                return true;
            }
        }
        return false;
    }

    private static bool IsSceneLoaded(string sceneName)
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene loadedScene = SceneManager.GetSceneAt(i);
            if (loadedScene.name == sceneName)
            {
                return true;
            }
        }
        return false;
    }

    private static string GetUIScenePath()
    {
        return bl_GameData.Instance.UIScene.Path;
    }
}
