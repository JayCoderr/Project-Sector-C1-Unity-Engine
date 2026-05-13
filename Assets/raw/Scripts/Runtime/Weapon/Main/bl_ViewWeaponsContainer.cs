using System.Collections.Generic;
using UnityEngine;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class bl_ViewWeaponsContainer : bl_WeaponContainer
{
    public List<WeaponData> weapons;
    [Tooltip("In case you have separate containers")]
    public List<bl_ViewWeaponsContainer> childContainers;
    [SerializeField] private bl_WorldWeaponsContainer tpWeaponContainer;

    /// <summary>
    /// 
    /// </summary>
    public override bl_WeaponBase GetWeapon(int gunId, Transform parent, Avatar avatar = null)
    {
        int id = GetWeaponIndex(gunId);
        bl_ViewWeaponsContainer containerSource = this;

        // if the weapon is not found in this container, check the child containers
        if (id == -1 && childContainers != null && childContainers.Count > 0)
        {
            foreach (var childContainer in childContainers)
            {
                if (childContainer == null) continue;

                id = childContainer.GetWeaponIndex(gunId);
                if (id != -1)
                {
                    containerSource = childContainer;
                    break;
                }
            }
        }

        if (id == -1)
        {
            Debug.LogWarning($"The weapon ({gunId.GetWeaponName()}) is not integrated yet.");
            return null;
        }

        var weaponData = containerSource.weapons[id];
        var instance = Instantiate(weaponData.Weapon, parent);
        instance.name = instance.name.Replace("(Clone)", "");
        instance.transform.SetLocalPositionAndRotation(weaponData.Position, weaponData.Rotation);

        return instance.GetComponent<bl_WeaponBase>();
    }

    /// <summary>
    /// Get all the weapons prefabs in this container
    /// </summary>
    /// <param name="includeChildContainers">Include the weapons from child containers?</param>
    /// <returns></returns>
    public override List<bl_WeaponBase> GetAllWeaponsPrefabs(bool includeChildContainers = true)
    {
        var list = new List<bl_WeaponBase>();
        foreach (var weapon in weapons)
        {
            if (weapon.Weapon == null) continue;
            list.Add(weapon.Weapon);
        }

        if (includeChildContainers)
        {
            foreach (var item in childContainers)
            {
                if (item == null) continue;
                list.AddRange(item.GetAllWeaponsPrefabs(false));
            }
        }

        return list;
    }

    /// <summary>
    /// Get the list of weapons in this container.
    /// </summary>
    /// <param name="includeChildContainers">Include the weapons from child containers?</param>
    /// <returns>A list of WeaponData objects representing the weapons in this container.</returns>
    public List<WeaponData> GetWeapons(bool includeChildContainers = false)
    {
        var list = new List<WeaponData>(weapons);

        if (includeChildContainers && childContainers != null && childContainers.Count > 0)
        {
            foreach (var item in childContainers)
            {
                if (item == null) continue;
                list.AddRange(item.GetWeapons());
            }
        }

        return list;
    }

    /// <summary>
    /// Get the weapon index in the list of this container (not including child containers)
    /// return -1 if not found the weapon
    /// </summary>
    /// <param name="gunId"></param>
    /// <returns></returns>
    public int GetWeaponIndex(int gunId)
    {
        int id = weapons.FindIndex(x =>
        {
            if (x.Weapon == null) return false;
            return x.Weapon.GunID == gunId;
        });
        return id;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public bl_WorldWeaponsContainer GetWorldWeaponsContainer()
    {
        return tpWeaponContainer;
    }

    /// <summary>
    /// 
    /// </summary>
    public void FetchChildWeapons()
    {
        var childs = transform.GetComponentsInChildren<bl_WeaponBase>(true);
        foreach (var child in childs)
        {
            int id = -1;
            if (weapons == null || weapons.Count == 0)
            {
                weapons = new List<WeaponData>();
            }
            else
            {
                // check if the weapon is already registered
                id = weapons.FindIndex(x =>
                {
                    if (x.Weapon == null) return false;
                    return x.Weapon.GetGunID() == child.GunID;
                });
            }

            // if the weapon is not registered, add it
            if (id == -1)
            {
                var weaponData = new WeaponData();
                weaponData.FetchWeapon(child);
                weapons.Add(weaponData);

#if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(gameObject);
                UnityEditor.EditorUtility.SetDirty(this);
#endif

                continue;
            }

            // if the weapon is already registered, update it
            weapons[id].FetchWeapon(child);

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(gameObject);
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }
    }

#if UNITY_EDITOR
    private static GameObject _editorPrefabCached = null;
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>    
    public static GameObject GetEditorPrefab()
    {
        GameObject prefab = null;

        if (_editorPrefabCached != null)
        {
            prefab = _editorPrefabCached;
        }
        else
        {
            string refPath = AssetDatabase.GetAssetPath(bl_GameData.Instance);
            refPath = Directory.GetParent(refPath).FullName;
            refPath = Directory.GetParent(refPath).FullName;
            refPath = Path.Combine(refPath, "Content/Prefabs/Weapons/Containers/FP Weapons.prefab");
            // convert path to Unity project asset path
            refPath = "Assets" + refPath.Substring(Application.dataPath.Length);
            prefab = AssetDatabase.LoadAssetAtPath(refPath, typeof(GameObject)) as GameObject;
        }

        return prefab;
    }
#endif

    /// <summary>
    /// This function is an editor utility to set the GunID of all weapons in this container to 1 forward.
    /// useful when you made changes in the AllWeapons list and you want to update the GunID of all weapons.
    /// </summary>
    [ContextMenu("Set All Weapons GunID +1")]
    public void SetGunIDOneForward()
    {
        foreach (var w in GetComponentsInChildren<bl_WeaponBase>(true))
        {
            w.GunID++;
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(w);
#endif
        }

        SetDirty();
    }

    [ContextMenu("Set All Weapons GunID -1")]
    public void SetGunIDOneBackward()
    {
        foreach (var w in GetComponentsInChildren<bl_WeaponBase>(true))
        {
            w.GunID--;
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(w);
#endif
        }

        SetDirty();
    }
}
#if UNITY_EDITOR
[CustomEditor(typeof(bl_ViewWeaponsContainer))]
public class bl_ViewWeaponsContainerEditor : Editor
{
    public bl_ViewWeaponsContainer script;

    private void OnEnable()
    {
        script = (bl_ViewWeaponsContainer)target;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        GUILayout.Space(4);
        if (GUILayout.Button(new GUIContent("Update Child Weapons", "Fetch all child weapons, add the new weapons and update the information of all the registered ones, you have to click this every time you made a modification to any weapon.")))
        {
            script.FetchChildWeapons();
        }

        GUI.enabled = script.transform.GetComponentInParent<bl_PlayerPlaceholder>() == null && script.gameObject.activeInHierarchy;
        if (GUILayout.Button(new GUIContent("Preview Weapons", "Preview the weapons in the Game View, this will create an instance of the weapon container in the current active scene so you can modify, add, or remove weapons.")))
        {
            PreviewWeapons(script.gameObject);
        }
        GUI.enabled = true;

        GUI.enabled = HasOverrides();
        if (GUILayout.Button(new GUIContent("Commit Changes", "Commit/Save any modification to the weapon container")))
        {
            script.FetchChildWeapons();
            CommitWeaponContainerPrefab(script.gameObject, false);
        }
        GUI.enabled = true;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private bool HasOverrides()
    {
        bool isPrefabInstance = PrefabUtility.IsPartOfPrefabInstance(script.gameObject);
        if (!isPrefabInstance) return false;

        // check if the prefab instance has overrides
        bool hasOverrides = PrefabUtility.HasPrefabInstanceAnyOverrides(script.gameObject, false);

        return hasOverrides;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="containerPrefab"></param>
    public static bool CommitWeaponContainerPrefab(GameObject containerPrefab, bool destroyInstance = true)
    {
        if (!PrefabUtility.IsPartOfAnyPrefab(containerPrefab))
        {

            Debug.LogWarning("The weapon container instance has been unlinked from the source prefab, you have to link the instance to the prefab manually.");
            return false;
        }

        if (EditorUtility.DisplayDialog("Confirm Action", "Apply all modifications to the weapon container prefab? this action can't be undo.", "Yes", "Cancel"))
        {
            PrefabUtility.ApplyPrefabInstance(containerPrefab, InteractionMode.AutomatedAction);
            if (!PrefabUtility.IsOutermostPrefabInstanceRoot(containerPrefab))
            {
                var root = PrefabUtility.GetOutermostPrefabInstanceRoot(containerPrefab);
                PrefabUtility.UnpackPrefabInstanceAndReturnNewOutermostRoots(root, PrefabUnpackMode.OutermostRoot);
            }

            if (destroyInstance) DestroyImmediate(containerPrefab);
            return true;
        }

        return false;
    }

    /// <summary>
    /// 
    /// </summary>
    public static void PreviewWeapons(GameObject sourceContainer)
    {
        GameObject instance = GetContainerInstance(sourceContainer);
        if (instance == null)
        {
            Debug.LogError("The weapons container is not found.");
            return;
        }

        var container = instance.GetComponent<bl_ViewWeaponsContainer>();
        if (container == null)
        {
            // show warning and return
            Debug.LogWarning("The weapons container script is not found in the container prefab.");
        }

        var playerHolder = bl_PlayerPlaceholder.GetInstance();
        if (playerHolder == null)
        {
            Debug.LogError("The player placeholder is not found.");
            return;
        }

        playerHolder.PlaceFPWeaponContainer(container.transform);

        if (container.childContainers != null && container.childContainers.Count > 0)
        {
            foreach (var item in container.childContainers)
            {
                if (item == null) continue;

                var childInstance = PrefabUtility.InstantiatePrefab(item.gameObject) as GameObject;
                playerHolder.PlaceFPWeaponContainer(childInstance.transform);
            }
        }

        Selection.activeGameObject = container.gameObject;
        EditorGUIUtility.PingObject(container.gameObject);

        var view = (SceneView)SceneView.sceneViews[0];
        if (view != null) view.LookAt(container.transform.position);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sourceContainer"></param>
    /// <returns></returns>
    private static GameObject GetContainerInstance(GameObject sourceContainer)
    {
        // check if it is a prefab
        bool isPrefab = PrefabUtility.IsPartOfPrefabAsset(sourceContainer);
        if (!isPrefab) return sourceContainer;

        // if it is a prefab, check if it's instanced in an scene
        bool isPrefabInstance = PrefabUtility.IsPartOfPrefabInstance(sourceContainer);
        if (isPrefabInstance) return sourceContainer;

        // if is not a prefab instance, instance the prefab
        if (!isPrefabInstance)
        {
            GameObject prefab = bl_ViewWeaponsContainer.GetEditorPrefab();
            if (prefab == null)
            {
                Debug.LogError("The prefab of the weapons container is not found.");
                return null;
            }

            return PrefabUtility.InstantiatePrefab(prefab) as GameObject;
        }

        return null;
    }

    [MenuItem("MFPS/Actions/Preview FP Weapons", priority = 100)]
    static void PreviewWeaponsAction()
    {
        var containerPrefab = bl_ViewWeaponsContainer.GetEditorPrefab();
        if (containerPrefab == null)
        {
            Debug.LogError("The prefab of the weapons container is not found.");
            return;
        }

        PreviewWeapons(containerPrefab);
    }
}
#endif