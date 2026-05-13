using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class bl_RemoteWeapons : MonoBehaviour
{
    public bl_WeaponContainer weaponContainer;

    [HideInInspector] public GameObject _editorWeaponContainerInstance = null;

    /// <summary>
    /// 
    /// </summary>
    public void Detach()
    {
        transform.parent = null;
    }

    /// <summary>
    /// Return the instance of the weapon prefab with the given ID
    /// </summary>
    /// <param name="gunID"></param>
    /// <returns></returns>
    public virtual bl_NetworkGun GetWeapon(int gunID, Avatar avatar = null)
    {
        if (weaponContainer == null) return null;

        var weapon = weaponContainer.GetWeapon(gunID, transform, avatar);
        var tpWeapon = weapon as bl_NetworkGun;

        if (tpWeapon != null)
        {
            if (PlayerReferences != null && PlayerReferences.gunManager != null)
            {
                int fpLocalId = PlayerReferences.gunManager.AllGuns.FindIndex(x => x != null && x.GunID == tpWeapon.GetWeaponID);
                if (fpLocalId != -1)
                {
                    tpWeapon.LocalGun = PlayerReferences.gunManager.AllGuns[fpLocalId];
                }
            }
        }

        return tpWeapon;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="gunID"></param>
    /// <param name="avatar"></param>
    /// <returns></returns>
    public virtual bl_NetworkGun EquipWeapon(int gunID, Avatar avatar = null)
    {
        var weapon = GetWeapon(gunID, avatar);
        if (weapon != null)
        {
            weapon.SetActive(true);
        }
        return weapon;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="weapon"></param>
    public void ActiveOnlyThisInChild(bl_WeaponBase weapon)
    {
        var all = transform.GetComponentsInChildren<bl_WeaponBase>();
        foreach (var item in all)
        {
            item.gameObject.SetActive(false);
        }
        if (weapon != null) weapon.gameObject.SetActive(true);
    }

#if UNITY_EDITOR
    public void InstanceContainer(bl_WeaponContainer containerInstance = null, Avatar avatar = null, bool isRootContainer = true, bool instanceChild = false)
    {
        if (containerInstance == null)
        {
            if (isRootContainer) _editorWeaponContainerInstance = PrefabUtility.InstantiatePrefab(weaponContainer.gameObject, transform) as GameObject;
            containerInstance = _editorWeaponContainerInstance.GetComponent<bl_WeaponContainer>();
        }
        else
        {
            containerInstance.transform.SetParent(transform, true);
            if (isRootContainer) _editorWeaponContainerInstance = containerInstance.gameObject;
        }

        if (containerInstance == null) return;

        containerInstance.transform.localPosition = Vector3.zero;
        containerInstance.transform.localRotation = Quaternion.identity;
        containerInstance.transform.localScale = Vector3.one;

        if (avatar != null) (containerInstance as bl_WorldWeaponsContainer).SetupAllChildToAvatar(avatar);

        int childCount = containerInstance.transform.childCount;
        var childs = new List<GameObject>();
        for (int i = 0; i < childCount; i++)
        {
            var c = containerInstance.transform.GetChild(i).gameObject;
            if (c.activeSelf)
            {
                childs.Add(c);
            }
        }
        // make sure only one child is active
        if (childs.Count > 1)
        {
            foreach (var item in childs)
            {
                item.SetActive(false);
            }
            childs[0].gameObject.SetActive(true);
        }
        else if (childs.Count == 0)
        {
            var childT = containerInstance.transform.GetComponentsInChildren<Transform>(true);
            if (childT != null && childT.Length > 1)
            {
                childT[1].gameObject.SetActive(true);
            }
        }

        if (!isRootContainer || !instanceChild) return;

        var container = containerInstance.GetComponent<bl_WorldWeaponsContainer>();
        if (container.childContainers != null && container.childContainers.Count > 0)
        {
            foreach (var item in container.childContainers)
            {
                if (item == null) continue;

                var childInstance = PrefabUtility.InstantiatePrefab(item.gameObject) as GameObject;
                InstanceContainer(childInstance.GetComponent<bl_WorldWeaponsContainer>(), avatar, false);
            }
        }
    }
#endif

    private bl_PlayerReferences _PlayerReferences = null;
    public bl_PlayerReferences PlayerReferences
    {
        get
        {
            if (_PlayerReferences == null)
            {
                _PlayerReferences = transform.GetComponentInParent<bl_PlayerReferences>();
            }
            return _PlayerReferences;
        }
    }
}
#if UNITY_EDITOR
[CustomEditor(typeof(bl_RemoteWeapons))]
public class bl_RemoteWeaponsEditor : Editor
{
    public bl_RemoteWeapons script;
    public bl_PlayerReferences playerReferences;

    /// <summary>
    /// 
    /// </summary>
    private void OnEnable()
    {
        script = (bl_RemoteWeapons)target;
        playerReferences = script.GetComponentInParent<bl_PlayerReferences>(true);
    }

    /// <summary>
    /// 
    /// </summary>
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        GUILayout.Space(10);

        if (script.weaponContainer != null)
        {
            if (script._editorWeaponContainerInstance != null)
            {
                EditorGUILayout.HelpBox("The current weapons are an instance of the weapon container prefab, once finish the modifications, make sure to commit the changes or cancel the changes of the instance.", MessageType.Info);
                GUI.color = Color.green;
                if (GUILayout.Button("Commit Weapon Container", GUILayout.Height(30)))
                {
                    if (PrefabUtility.IsPartOfAnyPrefab(script._editorWeaponContainerInstance))
                    {
                        if (EditorUtility.DisplayDialog("Confirm Action", "Apply all modifications to the weapon container prefab? this action can't be undo.", "Yes", "Cancel"))
                        {
                            PrefabUtility.ApplyPrefabInstance(script._editorWeaponContainerInstance, InteractionMode.AutomatedAction);
                            DestroyImmediate(script._editorWeaponContainerInstance);
                            script._editorWeaponContainerInstance = null;
                        }
                    }
                    else
                    {
                        Debug.LogWarning("The weapon container instance has been unlinked from the source prefab, you have to link the instance to the prefab manually.");
                    }
                }
                GUI.color = Color.white;
                if (GUILayout.Button("Cancel Weapon Changes"))
                {
                    DestroyImmediate(script._editorWeaponContainerInstance);
                    script._editorWeaponContainerInstance = null;
                }
                GUI.color = Color.white;
            }
            else
            {
                if (GUILayout.Button("Preview TP Weapons", GUILayout.Height(30)))
                {
                    Avatar avatar = null;
                    if (playerReferences != null) avatar = playerReferences.PlayerAnimator.avatar;

                    script.InstanceContainer(null, avatar, true, true);
                }
            }
        }
    }

    void OnSceneGUI()
    {
        if (playerReferences == null || playerReferences.playerCamera == null) return;

        Vector3 origin = script.transform.position;
        Vector3 target = playerReferences.playerCamera.transform.position + (playerReferences.playerCamera.transform.forward * 25);

        Handles.color = Color.yellow;
        Handles.DrawDottedLine(origin, target, 3f);
        Handles.color = Color.white;
    }
}
#endif