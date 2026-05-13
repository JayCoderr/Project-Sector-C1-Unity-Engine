using MFPSEditor;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine;

[CustomEditor(typeof(bl_GunManager))]
public class bl_GunManagerEditor : Editor
{
    private AnimBool AssaultAnim;
    protected static bool ShowAssault;
    private AnimBool EnginnerAnim;
    protected static bool ShowEngi;
    private AnimBool ReconAnim;
    protected static bool ShowRecon;
    private AnimBool SupportAnim;
    protected static bool ShowSupport;

    private void OnEnable()
    {
        bl_GunManager script = (bl_GunManager)target;

        AssaultAnim = new AnimBool(ShowAssault);
        AssaultAnim.valueChanged.AddListener(Repaint);
        EnginnerAnim = new AnimBool(ShowEngi);
        EnginnerAnim.valueChanged.AddListener(Repaint);
        ReconAnim = new AnimBool(ShowRecon);
        ReconAnim.valueChanged.AddListener(Repaint);
        SupportAnim = new AnimBool(ShowSupport);
        SupportAnim.valueChanged.AddListener(Repaint);
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUI.BeginChangeCheck();
        bl_GunManager script = (bl_GunManager)target;
        bool allowSceneObjects = !EditorUtility.IsPersistent(script);


        EditorGUILayout.BeginVertical("box");
        DrawNetworkGunsList(script);
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical("box");

#if CLASS_CUSTOMIZER
        GUILayout.BeginHorizontal();
        GUILayout.Label("Class Customization is enabled, set default weapons here: ", EditorStyles.miniLabel);
        if (GUILayout.Button("ClassManager")) { Selection.activeObject = bl_ClassManager.Instance; EditorGUIUtility.PingObject(bl_ClassManager.Instance); }
        GUILayout.EndHorizontal();
        GUI.enabled = false;
#endif
        EditorGUILayout.BeginVertical("box");
        ShowAssault = MFPSEditorStyles.ContainerHeaderFoldout("Assault Class", ShowAssault);
        AssaultAnim.target = ShowAssault;
        if (EditorGUILayout.BeginFadeGroup(AssaultAnim.faded))
        {
            var so = serializedObject.FindProperty("m_AssaultClass");
            DrawLoadoutField(so);
            DrawEditorOf(so);
        }
        EditorGUILayout.EndFadeGroup();
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical("box");
        ShowEngi = MFPSEditorStyles.ContainerHeaderFoldout("Engineer Class", ShowEngi);
        EnginnerAnim.target = ShowEngi;
        if (EditorGUILayout.BeginFadeGroup(EnginnerAnim.faded))
        {
            var so = serializedObject.FindProperty("m_EngineerClass");
            DrawLoadoutField(so);
            DrawEditorOf(so);
        }
        EditorGUILayout.EndFadeGroup();
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical("box");
        ShowRecon = MFPSEditorStyles.ContainerHeaderFoldout("Recon Class", ShowRecon);
        ReconAnim.target = ShowRecon;
        if (EditorGUILayout.BeginFadeGroup(ReconAnim.faded))
        {
            var so = serializedObject.FindProperty("m_ReconClass");
            DrawLoadoutField(so);
            DrawEditorOf(so);
        }
        EditorGUILayout.EndFadeGroup();
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical("box");
        ShowSupport = MFPSEditorStyles.ContainerHeaderFoldout("Support Class", ShowSupport);
        SupportAnim.target = ShowSupport;
        if (EditorGUILayout.BeginFadeGroup(SupportAnim.faded))
        {
            var so = serializedObject.FindProperty("m_SupportClass");
            DrawLoadoutField(so);
            DrawEditorOf(so);
        }
        EditorGUILayout.EndFadeGroup();
        EditorGUILayout.EndVertical();

        EditorGUILayout.EndVertical();
        GUI.enabled = true;

        GUILayout.BeginVertical("box");
        script.unarmedWeaponID = EditorGUILayout.Popup("Unarmed Weapon", script.unarmedWeaponID, bl_GameData.Instance.AllWeaponStringList(true, true));

        script.SwichTime = EditorGUILayout.Slider("Switch Time", script.SwichTime, 0.1f, 5);
        script.PickUpTime = EditorGUILayout.Slider("Pick Up Time", script.PickUpTime, 0.1f, 5);
        GUILayout.EndVertical();

        GUILayout.BeginVertical("box");
        script.HeadAnimator = EditorGUILayout.ObjectField("Head Animator", script.HeadAnimator, typeof(Animator), allowSceneObjects) as Animator;
        GUILayout.EndVertical();

        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(target);
        }
    }

    void DrawEditorOf(SerializedProperty so)
    {
        if (so == null || so.objectReferenceValue == null) return;

        var editor = Editor.CreateEditor(so.objectReferenceValue);
        if (editor != null)
        {
            EditorGUILayout.BeginVertical("box");
            editor.DrawDefaultInspector();
            EditorGUILayout.EndVertical();
        }
    }

    void DrawLoadoutField(SerializedProperty so)
    {
        EditorGUILayout.BeginHorizontal("box");
        EditorGUILayout.PropertyField(so);
        if (so.objectReferenceValue == null)
        {
            if (GUILayout.Button("Create", EditorStyles.toolbarButton, GUILayout.Width(60)))
            {
                string path = "Assets/MFPS/Content/Prefabs/Weapons/Loadouts";
                if (!AssetDatabase.IsValidFolder(path))
                {
                    path = EditorUtility.OpenFolderPanel("Save Folder", "Assets", "Assets");
                }
                path = bl_UtilityHelper.CreateAsset<bl_PlayerClassLoadout>(path, false, "Player Class Loadout");
                bl_PlayerClassLoadout pcl = AssetDatabase.LoadAssetAtPath(path, typeof(bl_PlayerClassLoadout)) as bl_PlayerClassLoadout;
                so.objectReferenceValue = pcl;
                EditorUtility.SetDirty(target);
            }
        }
        else
        {
            if (GUILayout.Button("NEW", EditorStyles.toolbarButton, GUILayout.Width(50)))
            {
                bl_PlayerClassLoadout old = so.objectReferenceValue as bl_PlayerClassLoadout;
                string path = "Assets/MFPS/Content/Prefabs/Weapons/Loadouts";
                if (!AssetDatabase.IsValidFolder(path))
                {
                    path = EditorUtility.OpenFolderPanel("Save Folder", "Assets", "Assets");
                }
                path = bl_UtilityHelper.CreateAsset<bl_PlayerClassLoadout>(path, false, $"{so.objectReferenceValue.name} copy");
                bl_PlayerClassLoadout pcl = AssetDatabase.LoadAssetAtPath(path, typeof(bl_PlayerClassLoadout)) as bl_PlayerClassLoadout;
                so.objectReferenceValue = pcl;
                pcl.Primary = old.Primary;
                pcl.Secondary = old.Primary;
                pcl.Perks = old.Perks;
                pcl.Letal = old.Letal;
                EditorUtility.SetDirty(target);
                EditorUtility.SetDirty(pcl);
            }
        }
        EditorGUILayout.EndHorizontal();
    }

    void DrawNetworkGunsList(bl_GunManager script)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label("WEAPON MANAGER", EditorStyles.toolbarButton);
        GUILayout.Space(5);
        if (GUILayout.Button(new GUIContent("IMPORT", EditorGUIUtility.IconContent("ol plus").image), EditorStyles.toolbarButton, GUILayout.Width(70)))
        {
            EditorWindow.GetWindow<bl_ImportExportWeapon>("Import", true).PrepareToImport(script.transform.root.GetComponent<bl_PlayerNetworkBase>(), null);
        }
        GUILayout.EndHorizontal();
        GUILayout.Space(4);

        EditorGUILayout.BeginVertical("box");
        script.weaponContainer = EditorGUILayout.ObjectField("Weapon Container", script.weaponContainer, typeof(bl_WeaponContainer), false) as bl_WeaponContainer;
        if (script.weaponContainer != null)
        {
            if (script._editorWeaponContainerInstance != null)
            {
                EditorGUILayout.HelpBox("The current weapons are an instance of the weapon container prefab, once finish the modifications, make sure to commit the changes or delete the instance (FP Weapons) manually from the hierarchy window.", MessageType.Info);
                GUI.color = Color.green;
                if (GUILayout.Button("Commit Weapon Container", GUILayout.Height(30)))
                {
                    if (bl_ViewWeaponsContainerEditor.CommitWeaponContainerPrefab(script._editorWeaponContainerInstance))
                    {
                        script._editorWeaponContainerInstance = null;
                    }
                }
                GUI.color = Color.white;
                if (GUILayout.Button("Cancel Weapon Changes"))
                {
                    if (!PrefabUtility.IsOutermostPrefabInstanceRoot(script._editorWeaponContainerInstance))
                    {
                        var root = PrefabUtility.GetOutermostPrefabInstanceRoot(script._editorWeaponContainerInstance);
                        PrefabUtility.UnpackPrefabInstanceAndReturnNewOutermostRoots(root, PrefabUnpackMode.OutermostRoot);
                    }
                    DestroyImmediate(script._editorWeaponContainerInstance);
                    script._editorWeaponContainerInstance = null;
                }
                GUI.color = Color.white;
            }
            else
            {
                if (GUILayout.Button("Preview Weapons", GUILayout.Height(30)))
                {
                    script._editorWeaponContainerInstance = PrefabUtility.InstantiatePrefab(script.weaponContainer.gameObject, script.transform) as GameObject;
                    script._editorWeaponContainerInstance.transform.localPosition = Vector3.zero;
                    script._editorWeaponContainerInstance.transform.localRotation = Quaternion.identity;
                }
            }
        }
        EditorGUILayout.EndVertical();

        SerializedProperty listProperty = serializedObject.FindProperty("AllGuns");
        if (listProperty == null)
        {
            return;
        }

        EditorGUI.indentLevel++;
        EditorGUILayout.PropertyField(listProperty, true);
        EditorGUI.indentLevel--;

        serializedObject.ApplyModifiedProperties();
    }
}