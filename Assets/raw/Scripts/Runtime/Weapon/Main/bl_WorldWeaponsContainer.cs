using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using MFPSEditor;



#if UNITY_EDITOR
using UnityEditor;
#endif

public class bl_WorldWeaponsContainer : bl_WeaponContainer
{
    [Serializable]
    public class WorldWeaponData
    {
        [MFPSEditor.ReadOnly] public string Name;
        public bl_WeaponBase Weapon;
        public bl_GunPickUpBase PickUpWeaponPrefab;
        public List<PlayerSpecificData> playerSpecificDatas = new List<PlayerSpecificData>();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public int GetGunID()
        {
            if (Weapon == null) return -1;
            return Weapon.GetGunID();
        }

        /// <summary>
        /// 
        /// </summary>
        public void ApplySpecificLocation(bl_WeaponBase weapon, Avatar avatar)
        {
            int index = playerSpecificDatas.FindIndex(x => x.Avatar != null && x.Avatar == avatar);
            if (index == -1)
            {
                Debug.LogWarning("There's no setup for this player model yet.", weapon);
                return;
            }

            var data = playerSpecificDatas[index];
            weapon.transform.SetLocalPositionAndRotation(data.Position, data.Rotation);

            if (weapon is bl_NetworkGun)
            {
                bl_NetworkGun n = (bl_NetworkGun)weapon;
                if (n.LeftHandPosition != null)
                {
                    n.LeftHandPosition.SetLocalPositionAndRotation(data.LeftHandPosition, data.LeftHandRotation);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="weapon"></param>
        public void FetchWeapon(bl_WeaponBase weapon)
        {
            if (weapon == null) return;

            Name = weapon.name;
            Weapon = weapon;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="weaponBase"></param>
        /// <returns></returns>
        public bool FetchSpecificData(bl_WeaponBase weaponBase, Transform leftHandRef)
        {
            if (weaponBase == null) return false;

            Animator animator = weaponBase.transform.GetComponentInParent<Animator>();
            if (animator == null)
            {
                Debug.LogWarning("This player doesn't have an animation component, weapon data won't be saved");
                return false;
            }
            if (animator.avatar == null)
            {
                Debug.LogWarning("This player doesn't have an Avatar assigned in the Animator, weapon data won't be saved");
                return false;
            }

            int index = playerSpecificDatas.FindIndex(x => x.Avatar != null && x.Avatar == animator.avatar);
            if (index != -1)
            {
                playerSpecificDatas[index].Position = weaponBase.transform.localPosition;
                playerSpecificDatas[index].Rotation = weaponBase.transform.localRotation;
                if (animator.avatar != null) playerSpecificDatas[index].AvatarName = animator.avatar.name;
                if (leftHandRef != null)
                {
                    playerSpecificDatas[index].LeftHandPosition = leftHandRef.localPosition;
                    playerSpecificDatas[index].LeftHandRotation = leftHandRef.localRotation;
                }
            }
            else
            {
                var specificData = new PlayerSpecificData();
                specificData.Avatar = animator.avatar;
                if (animator.avatar != null) specificData.AvatarName = animator.avatar.name;
                specificData.Position = weaponBase.transform.localPosition;
                specificData.Rotation = weaponBase.transform.localRotation;
                if (leftHandRef != null)
                {
                    specificData.LeftHandPosition = leftHandRef.localPosition;
                    specificData.LeftHandRotation = leftHandRef.localRotation;
                }
                playerSpecificDatas.Add(specificData);
            }

            return true;
        }

        [Serializable]
        public class PlayerSpecificData
        {
            public string AvatarName;
            public Avatar Avatar;
            public Vector3 Position;
            public Quaternion Rotation;
            public Vector3 LeftHandPosition;
            public Quaternion LeftHandRotation;
        }
    }

    public List<WorldWeaponData> weapons;
    [Tooltip("In case you have separate containers")]
    public List<bl_WorldWeaponsContainer> childContainers;
    public bl_ViewWeaponsContainer viewWeaponsContainer;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="gunID"></param>
    /// <param name="parent"></param>
    /// <returns></returns>
    public override bl_WeaponBase GetWeapon(int gunID, Transform parent, Avatar avatar = null)
    {
        int id = GetWeaponIndex(gunID);
        bl_WorldWeaponsContainer containerSource = this;

        // if the weapon is not found in this container, check the child containers
        if (id == -1 && childContainers != null && childContainers.Count > 0)
        {
            foreach (var childContainer in childContainers)
            {
                if (childContainer == null) continue;

                id = childContainer.GetWeaponIndex(gunID);
                if (id != -1)
                {
                    containerSource = childContainer;
                    break;
                }
            }
        }

        if (id == -1)
        {
            Debug.LogWarning($"The TP Weapon ({gunID}) is not integrated yet.");
            return null;
        }

        var weaponData = containerSource.weapons[id];
        var instance = Instantiate(weaponData.Weapon, parent);
        instance.name = instance.name.Replace("(Clone)", "");

        if (avatar == null)
        {
            var playerRef = parent.GetComponentInParent<bl_PlayerReferences>();
            if (playerRef != null)
            {
                avatar = playerRef.PlayerAnimator.avatar;
            }
        }

        weaponData.ApplySpecificLocation(instance, avatar);

        return instance.GetComponent<bl_WeaponBase>();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="gunId"></param>
    /// <returns></returns>
    public bl_GunPickUpBase GetWeaponPickUpOf(int gunId)
    {
        var weaponInfo = GetWeaponInfoOf(gunId, true);

        if (weaponInfo == null) return null;

        return weaponInfo.PickUpWeaponPrefab;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="gunId"></param>
    /// <returns></returns>
    private WorldWeaponData GetWeaponInfoOf(int gunId, bool includingChild = false)
    {
        int id = GetWeaponIndex(gunId);
        bl_WorldWeaponsContainer container = this;

        if (id == -1 && includingChild && childContainers != null && childContainers.Count > 0)
        {
            foreach (var childContainer in childContainers)
            {
                if (childContainer == null) continue;

                id = childContainer.GetWeaponIndex(gunId);
                if (id != -1)
                {
                    container = childContainer;
                    break;
                }
            }
        }

        if (id == -1)
        {
            Debug.LogWarning($"The tp weapon ({gunId}) is not integrated yet.");
            return null;
        }

        return container.weapons[id];
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

            return x.Weapon.GetGunID() == gunId;
        });

        return id;
    }

    /// <summary>
    /// Setup all the child weapons to the avatar specif setup (if exist)
    /// </summary>
    /// <param name="avatar"></param>
    public void SetupAllChildToAvatar(Avatar avatar)
    {
        foreach (var weapon in weapons)
        {
            if (weapon.Weapon == null) continue;

            weapon.ApplySpecificLocation(weapon.Weapon, avatar);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [ContextMenu("Fetch Child Weapons")]
    public void FetchChildWeapons()
    {
        var childs = transform.GetComponentsInChildren<bl_WeaponBase>(true);
        foreach (var child in childs)
        {
            bl_NetworkGun castWeapon = child as bl_NetworkGun;
            int id = -1;
            if (weapons == null || weapons.Count == 0)
            {
                weapons = new List<WorldWeaponData>();
            }
            else
            {
                id = weapons.FindIndex(x =>
                {
                    if (x.Weapon == null) return false;
                    return x.Weapon.GetGunID() == castWeapon.GetWeaponID;
                });
            }

            if (id == -1)
            {
                var weaponData = new WorldWeaponData();
                weaponData.FetchWeapon(child);
                weaponData.FetchSpecificData(child, castWeapon.LeftHandPosition);
                weapons.Add(weaponData);

                SetDirty();

                continue;
            }

            weapons[id].FetchWeapon(child);
            weapons[id].FetchSpecificData(child, castWeapon.LeftHandPosition);

            SetDirty();
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
        GameObject prefab;
        if (_editorPrefabCached != null)
        {
            prefab = _editorPrefabCached;
        }
        else
        {
            string refPath = AssetDatabase.GetAssetPath(bl_GameData.Instance);
            refPath = Directory.GetParent(refPath).FullName;
            refPath = Directory.GetParent(refPath).FullName;
            refPath = Path.Combine(refPath, "Content/Prefabs/Weapons/Containers/TP Weapons.prefab");
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
        foreach (var w in weapons)
        {
            if (w.PickUpWeaponPrefab == null) continue;

            var script = w.PickUpWeaponPrefab.GetComponent<bl_GunPickUpBase>();
            script.GunID += 1;
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(script);
#endif
        }
    }

}
#if UNITY_EDITOR
[CustomEditor(typeof(bl_WorldWeaponsContainer))]
public class bl_WorldWeaponsContainerEditor : Editor
{
    public bl_WorldWeaponsContainer script;

    private void OnEnable()
    {
        script = (bl_WorldWeaponsContainer)target;
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
    public static void PreviewWeapons(GameObject sourceContainer, bl_PlayerReferences playerToPreview = null)
    {
        if (playerToPreview == null)
        {
            var selectPlayerWindow = EditorWindow.GetWindow<bl_MFPSPlayerPrefabsList>();
            selectPlayerWindow.SetTitle("<b>SELECT PLAYER</b>\n<i><size=10><color=#767676>Select the player prefab to preview the TP Weapons.</color></size></i>");
            selectPlayerWindow.onClickPlayer = (player, selector) =>
            {
                selector.Close();
                PreviewWeapons(sourceContainer, player);
            };
            return;
        }

        if (playerToPreview.RemoteWeapons == null)
        {
            // warning
            Debug.LogWarning("The player prefab doesn't have the RemoteWeapons, the TP Weapons can't be preview in this player.");
            return;
        }

        GameObject instance = GetContainerInstance(sourceContainer);
        if (instance == null)
        {
            Debug.LogError("The weapons container is not found.");
            return;
        }

        var container = instance.GetComponent<bl_WorldWeaponsContainer>();
        if (container == null)
        {
            // show warning and return
            Debug.LogWarning("The weapons container script is not found in the container prefab.");
            return;
        }

        var playerInstance = PrefabUtility.InstantiatePrefab(playerToPreview.gameObject) as GameObject;
        playerInstance.transform.SetAsLastSibling();
        var playerInsRefs = playerInstance.GetComponent<bl_PlayerReferences>();
        Avatar avatar = playerInsRefs.PlayerAnimator.avatar;

        playerInsRefs.RemoteWeapons.InstanceContainer(container, avatar);
        if (container.childContainers != null && container.childContainers.Count > 0)
        {
            foreach (var item in container.childContainers)
            {
                if (item == null) continue;

                var childInstance = PrefabUtility.InstantiatePrefab(item.gameObject) as GameObject;
                playerInsRefs.RemoteWeapons.InstanceContainer(childInstance.GetComponent<bl_WorldWeaponsContainer>(), avatar, false);
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
            GameObject prefab = bl_WorldWeaponsContainer.GetEditorPrefab();
            if (prefab == null)
            {
                Debug.LogError("The prefab of the weapons container is not found.");
                return null;
            }

            return PrefabUtility.InstantiatePrefab(prefab) as GameObject;
        }

        return null;
    }

    [MenuItem("MFPS/Actions/Preview TP Weapons", priority = 100)]
    static void PreviewWeaponsAction()
    {
        var containerPrefab = bl_WorldWeaponsContainer.GetEditorPrefab();
        if (containerPrefab == null)
        {
            Debug.LogError("The prefab of the weapons container is not found.");
            return;
        }

        PreviewWeapons(containerPrefab);
    }
}
#endif