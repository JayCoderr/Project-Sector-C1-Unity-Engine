using MFPSEditor;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(bl_NetworkGun))]
public class bl_NetworkGunEditor : Editor
{
    private bl_NetworkGun script;
    private bl_PlayerReferences playerReferences;
    private readonly List<string> FPWeaponsAvailable = new();
    private bl_Gun[] LocalGuns;
    private int selectLG = 0;
    public bool isInEditMode = false;
    public int editMode = 0;
    private string[] editModeNames = new string[] { "Left Hand", "Weapon" };

    /// <summary>
    /// 
    /// </summary>
    private void OnEnable()
    {
        script = (bl_NetworkGun)target;
        playerReferences = script.transform.root.GetComponent<bl_PlayerReferences>();
        if (playerReferences != null && playerReferences.gunManager != null)
        {
            var weaponContainer = playerReferences.gunManager.weaponContainer;
            if (weaponContainer != null)
            {
                var weapons = weaponContainer.GetAllWeaponsPrefabs();
                LocalGuns = new bl_Gun[weapons.Count];
                for (int i = 0; i < weapons.Count; i++)
                {
                    LocalGuns[i] = weapons[i] as bl_Gun;
                    string weaponName = $"{weapons[i].Info.Type}/{weapons[i].name}";
                    FPWeaponsAvailable.Add(weaponName);
                }
            }
            else
            {
                LocalGuns = playerReferences.gunManager.transform.GetComponentsInChildren<bl_Gun>(true);
                for (int i = 0; i < LocalGuns.Length; i++)
                {
                    FPWeaponsAvailable.Add(LocalGuns[i].name);
                }
            }
        }

        SceneView.duringSceneGui -= this.OnSceneGUI;
        SceneView.duringSceneGui += this.OnSceneGUI;
    }

    /// <summary>
    /// 
    /// </summary>
    private void OnDisable()
    {
        SceneView.duringSceneGui -= this.OnSceneGUI;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        bool allowSceneObjects = !EditorUtility.IsPersistent(script);

        EditorGUI.BeginChangeCheck();
        if (script.LocalGun != null)
        {
            //script.gameObject.name = bl_GameData.Instance.GetWeapon(script.LocalGun.GunID).Name;
        }

        EditorGUILayout.BeginVertical("box");
        script.LocalGun = EditorGUILayout.ObjectField("Local Weapon", script.LocalGun, typeof(bl_Gun), allowSceneObjects) as bl_Gun;
        EditorGUILayout.EndVertical();

        if (script.LocalGun != null)
        {
            if (script.LocalGun.Info.Type != GunType.Melee)
            {
                EditorGUILayout.BeginVertical("box");
                if (script.LocalGun.Info.Type == GunType.Grenade)
                {
                    script.Bullet = EditorGUILayout.ObjectField("Bullet", script.Bullet, typeof(GameObject), allowSceneObjects) as GameObject;
                }
                if (script.LocalGun.Info.Type != GunType.Grenade)
                {
                    script.MuzzleFlash = EditorGUILayout.ObjectField("MuzzleFlash", script.MuzzleFlash, typeof(ParticleSystem), allowSceneObjects) as ParticleSystem;
                }
                script.tpFirePoint = EditorGUILayout.ObjectField(new GUIContent("TP Fire Point", "If null, the muzzleflash will be used as point, if muzzle is null, the transform itself will be used as reference."), script.tpFirePoint, typeof(Transform), allowSceneObjects) as Transform;
                if (script.LocalGun.Info.Type == GunType.Grenade)
                {
                    script.DesactiveOnOffAmmo = EditorGUILayout.ObjectField("Desactive On No Ammo", script.DesactiveOnOffAmmo, typeof(GameObject), allowSceneObjects) as GameObject;
                }
                EditorGUILayout.EndVertical();
            }

            GUILayout.BeginVertical("box");
            script.useCustomPlayerAnimations = EditorGUILayout.ToggleLeft("Use Custom Player Animations", script.useCustomPlayerAnimations, EditorStyles.toolbarButton);
            if (script.useCustomPlayerAnimations)
            {
                GUILayout.Space(2);
                EditorGUILayout.HelpBox("Check the Player Animations > Weapon Animation, documentation for setup this.", MessageType.Info);
                script.customUpperAnimationID = EditorGUILayout.IntField("Custom Animator State ID", script.customUpperAnimationID);
                script.customFireAnimationName = EditorGUILayout.TextField("Custom Fire Animation Name", script.customFireAnimationName);
                script.customFireAnimationsCount = EditorGUILayout.IntField("Custom Fire Animations Count", script.customFireAnimationsCount);
            }
            GUILayout.EndVertical();

            if (script.LocalGun.Info.Type != GunType.Grenade && script.LocalGun.Info.Type != GunType.Melee)
            {
                GUILayout.BeginVertical("box");
                if (script.LeftHandPosition != null)
                {
                    if (GUILayout.Button("Edit Positions"))
                    {
                        OpenIKWindow(script);
                    }
                }
                else
                {
                    if (GUILayout.Button("Set Up Hand IK"))
                    {
                        GameObject gobject = new GameObject("LeftHandPoint");
                        gobject.transform.parent = script.transform;
                        gobject.transform.position = bl_UtilityHelper.CalculateCenter(script.transform);
                        gobject.transform.localEulerAngles = Vector3.zero;
                        script.LeftHandPosition = gobject.transform;
                        OpenIKWindow(script);
                    }
                }
                if (IsInContainer(out var container))
                {
                    if (!container.weapons.Exists((x) =>
                    {
                        return x.Weapon != null && x.Weapon == script;
                    }))
                    {
                        if (GUILayout.Button("Enlist TPWeapon"))
                        {
                            container.FetchChildWeapons();
                            Repaint();
                        }
                    }
                }
                GUILayout.EndVertical();
            }
            else
            {
                if (IsInContainer(out var container))
                {
                    if (!container.weapons.Exists((x) =>
                    {
                        return x.Weapon != null && x.Weapon == script;
                    }))
                    {
                        if (GUILayout.Button("Enlist TPWeapon", EditorStyles.toolbarButton))
                        {
                            container.FetchChildWeapons();
                            Repaint();
                        }
                    }
                }
            }
        }
        else
        {
            if (playerReferences != null && playerReferences.gunManager != null)
            {
                GUILayout.BeginVertical("box");
                GUILayout.Label("Select the local weapon of this TPWeapon");
                GUILayout.BeginHorizontal();
                GUILayout.Label("FPWeapon:", GUILayout.Width(100));
                selectLG = EditorGUILayout.Popup(selectLG, FPWeaponsAvailable.ToArray());
                if (GUILayout.Button("Select", EditorStyles.toolbarButton, GUILayout.Width(75)))
                {
                    script.LocalGun = LocalGuns[selectLG];
                }
                GUILayout.EndHorizontal();
                GUILayout.EndVertical();
            }
            else
            {
                if (GUILayout.Button("Open FPWeapons", EditorStyles.toolbarButton))
                {
                    bl_GunManager gm = script.transform.root.GetComponentInChildren<bl_GunManager>();
                    Selection.activeObject = gm.transform.GetChild(0).gameObject;
                    EditorGUIUtility.PingObject(gm.transform.GetChild(0).gameObject);
                }
            }
        }
        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(target);
        }
    }

    void OnSceneGUI(SceneView sceneView)
    {
        if (playerReferences == null || playerReferences.playerCamera == null) return;

        Vector3 origin = script.transform.position;
        Vector3 target = playerReferences.playerCamera.transform.position + (playerReferences.playerCamera.transform.forward * 25);

        Handles.color = Color.yellow;
        Handles.DrawDottedLine(origin, target, 3f);
        Handles.color = Color.white;

        DrawEditSceneButtons();
    }

    void DrawEditSceneButtons()
    {
        if (!isInEditMode) return;

        Handles.BeginGUI();

        Vector3 abovePoint = script.transform.position + (Vector3.up * 0.15f);
        Vector2 guiPoint = HandleUtility.WorldToGUIPoint(abovePoint);
        Rect rect = new Rect(guiPoint.x - 70, guiPoint.y - 50, 140, 20);

        int mode = GUI.Toolbar(rect, editMode, editModeNames);

        if (mode != editMode)
        {
            editMode = mode;
            if (editMode == 0)
            {
                Selection.activeObject = script.LeftHandPosition.gameObject;
            }
            else
            {
                Selection.activeObject = script.gameObject;
            }
        }

        Handles.EndGUI();
    }

    void OpenIKWindow(bl_NetworkGun script)
    {
        AnimatorRunner window = (AnimatorRunner)EditorWindow.GetWindow(typeof(AnimatorRunner));
        window.Show();
        bl_PlayerReferences pa = script.transform.root.GetComponent<bl_PlayerReferences>();
        Animator anim = pa.playerAnimations.Animator;
        pa.EditorSelectedGun = script;

        var pis = pa.playerAnimations.GetComponentsInChildren<bl_PlayerIKBase>(true);
        if (pis == null || pis.Length == 0)
        {
            Debug.LogWarning("Couldn't found the player IK script inside the player prefab!");
            return;
        }

        bl_PlayerIKBase hm = pis[0];
        if (pis.Length > 1)
        {
            for (int i = 0; i < pis.Length; i++)
            {
                if (pis[i] == null || !pis[i].gameObject.activeSelf) continue;

                hm = pis[i];
                break;
            }
        }

        if (hm != null)
        {
            hm.enabled = true;
            hm.Init();
            hm.CustomArmsIKHandler = null;
        }

        window.SetAnim(anim, () =>
        {
            Selection.activeObject = script.gameObject;
            ActiveEditorTracker.sharedTracker.isLocked = false;
            isInEditMode = false;
        });

        isInEditMode = true;
        ActiveEditorTracker.sharedTracker.isLocked = true;
        Selection.activeObject = script.LeftHandPosition.gameObject;
    }

    private bool IsInContainer(out bl_WorldWeaponsContainer container)
    {
        container = script.GetComponentInParent<bl_WorldWeaponsContainer>();
        return container != null;
    }
}