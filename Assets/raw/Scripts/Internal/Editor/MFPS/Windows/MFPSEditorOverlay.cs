using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Overlays;
using UnityEditor.SceneManagement;
using UnityEditor.Toolbars;
using UnityEngine;

[EditorToolbarElement(id, typeof(SceneView))]
class MFPSSceneOpener : EditorToolbarDropdown
{
    public const string id = "MFPS/SceneOpener";

    public MFPSSceneOpener()
    {
        icon = (Texture2D)EditorGUIUtility.IconContent("d_SceneAsset Icon").image;
        tooltip = "Open MFPS Scene";
        clicked += SceneSelector;
    }

    void SceneSelector()
    {
        string activeScene = EditorSceneManager.GetActiveScene().name;
        var scenes = bl_GameData.Instance.AllScenes;
        var menu = new GenericMenu();

        Dictionary<string, string> specialScenes = new Dictionary<string, string>()
        {
            { "Main Menu" , "Assets/MFPS/Scenes/MainMenu.unity" }
        };
#if ULSP
        specialScenes.Add("Login", "Assets/Addons/ULoginSystemPro/Content/Scenes/Login.unity");
        specialScenes.Add("Admin Panel", "Assets/Addons/ULoginSystemPro/Content/Scenes/AdminPanel.unity");
#endif
#if CLASS_CUSTOMIZER
        specialScenes.Add("Class Customizer", "Assets/Addons/ClassCustomization/Content/Scene/ClassCustomizer.unity");
#endif
#if CUSTOMIZER
        specialScenes.Add("Customizer", "Assets/Addons/Customizer/Content/Scene/Customizer.unity");
#endif

        foreach (var item in specialScenes)
        {
            menu.AddItem(new GUIContent(item.Key), item.Value == activeScene, () =>
            {
                if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                {
                    EditorSceneManager.OpenScene(item.Value);
                }
            });
        }
        menu.AddSeparator("");

        for (int i = 0; i < scenes.Count; i++)
        {
            string sceneName = scenes[i].ShowName;
            int index = i;
            menu.AddItem(new GUIContent(sceneName), scenes[i].RealSceneName == activeScene, () =>
            {
                if (scenes[index].m_Scene != null)
                {
                    // ask the user if they want to save the current scene
                    if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                    {
                        string path = AssetDatabase.GetAssetPath(scenes[index].m_Scene);
                        // load the scene
                        EditorSceneManager.OpenScene(path);
                    }
                }
            });
        }

        menu.ShowAsContext();
    }
}

[EditorToolbarElement(id, typeof(SceneView))]
class PreviewFPWeapons : EditorToolbarButton//, IAccessContainerWindow
{
    public const string id = "MFPS/PreviewFPWeapons";
    public PreviewFPWeapons()
    {
        icon = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/MFPS/Scripts/Internal/Editor/MFPS/Resources/content/Images/e_fp_view.png");
        tooltip = "Preview FP Weapons";
        clicked += OnClick;
    }

    void OnClick()
    {
        EditorApplication.ExecuteMenuItem("MFPS/Actions/Preview FP Weapons");
    }
}

[EditorToolbarElement(id, typeof(SceneView))]
class PreviewTPWeapons : EditorToolbarButton//, IAccessContainerWindow
{
    public const string id = "MFPS/PreviewTPWeapons";
    public PreviewTPWeapons()
    {
        icon = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/MFPS/Scripts/Internal/Editor/MFPS/Resources/content/Images/e_tp_view.png");
        tooltip = "Preview TP Weapons";
        clicked += OnClick;
    }

    void OnClick()
    {
        EditorApplication.ExecuteMenuItem("MFPS/Actions/Preview TP Weapons");
    }
}

[EditorToolbarElement(id, typeof(SceneView))]
class MFPSManagerButton : EditorToolbarButton//, IAccessContainerWindow
{
    public const string id = "MFPS/MFPSManager";
    public MFPSManagerButton()
    {
        icon = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/MFPS/Scripts/Internal/Editor/MFPS/Resources/content/Images/e_m.png");
        tooltip = "Open MFPS Manager";
        clicked += OnClick;
    }

    void OnClick()
    {
        EditorWindow.GetWindow<bl_MFPSManagerWindow>();
    }
}

[Overlay(typeof(SceneView), "MFPS Toolbar", true)]
public class EditorToolbarExample : ToolbarOverlay
{
    EditorToolbarExample() : base(
        PreviewFPWeapons.id,
        PreviewTPWeapons.id,
        MFPSManagerButton.id,
        MFPSSceneOpener.id
        )
    { }
}