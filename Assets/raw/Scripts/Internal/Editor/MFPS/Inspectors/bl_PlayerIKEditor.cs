using MFPSEditor;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(bl_PlayerIK))]
public class bl_PlayerIKEditor : Editor
{
    bl_PlayerIK script;
    private Animator animator;
    public Transform headTransform;
    public Transform foreArmTransform;
    private Vector3 aimPos, oldAimPos;
    private Vector3 idlePos, oldIdlePos;
    public bl_PlayerReferences playerReferences;
    private bool hasTPWeapons = false;
    private bl_NetworkGun watchingWeapon;

    public bool isAiming = false;
    public bool editLeftHandPos = false;
    private readonly string[] previewStates = new string[] { "Idle", "Aiming" };
    public int currentPreviewState = 0;

    /// <summary>
    /// 
    /// </summary>
    private void OnEnable()
    {
        script = (bl_PlayerIK)target;
        animator = script.GetComponent<Animator>();
        playerReferences = script.GetComponentInParent<bl_PlayerReferences>();
        if (playerReferences != null)
        {
            var rw = script.GetComponentInChildren<bl_RemoteWeapons>(true);
            if (rw != null)
            {
                hasTPWeapons = rw.transform.childCount > 0;
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();

        base.OnInspectorGUI();

        PreviewButtons();

        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(target);
        }
    }

    void PreviewButtons()
    {
        if (animator == null) return;

        if (!hasTPWeapons)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.HelpBox("The TPWeapons are not instanced yet in this player or no weapon is enabled, if you want to preview the Aim Position you first have to instance the TPWeapon container in this player instance and enable a weapon, click in the Preview button to instance the TPWeapons automatically.", MessageType.Info);
            if (GUILayout.Button("Preview", GUILayout.Width(75)))
            {
                EditorUtility.DisplayDialog("Remember", "You are instanced the TPWeapon Container, make sure once you finish your operation, delete the TPWeapon container instances from your player prefab.", "Ok");

                var rw = script.GetComponentInChildren<bl_RemoteWeapons>(true);
                if (rw != null)
                {
                    rw.InstanceContainer(null, script.GetComponent<Animator>().avatar);
                    hasTPWeapons = rw.transform.childCount > 0;
                    EditorGUIUtility.PingObject(rw.gameObject);
                    Repaint();
                }
                else
                {
                    Debug.LogWarning("No TPWeapons was found active in this player, can't preview the arms IK without a weapon active!");
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        GUILayout.Space(6);
        if (script.editor_previewMode == 1)
        {
            script.editor_weight = EditorGUILayout.Slider("Preview IK Weight", script.editor_weight, 0, 1);
        }
        GUI.enabled = hasTPWeapons;
        if (GUILayout.Button("Preview Aim Position"))
        {
            AnimatorRunner window = (AnimatorRunner)EditorWindow.GetWindow(typeof(AnimatorRunner));
            window.Show();

            script.editor_preview_gun_id = -1;
            window.SetAnim(animator, () =>
            {
                script.editor_weight = 0;
                script.editor_preview_gun_id = -1;
                if (animator != null)
                {
                    animator.Update(0);
                    animator.Update(0);
                }
                script.editor_previewMode = 0;
                script.editor_is_aiming = false;
                script.editor_preview_global = false;
                editLeftHandPos = false;
                currentPreviewState = 0;
                isAiming = false;
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(target);
            });
            // change the editor handle tool to move
            Tools.current = Tool.View;
            Selection.activeObject = script.gameObject;
            watchingWeapon = script.transform.GetComponentInChildren<bl_NetworkGun>();
            if (watchingWeapon != null)
            {
                script.GetComponentInParent<bl_PlayerReferences>().EditorSelectedGun = watchingWeapon;
                script.editor_preview_gun_id = watchingWeapon.GetGunID();
            }
            else
            {
                Debug.LogWarning("No TPWeapons was found active in this player, can't preview the arms IK without a weapon active!");
            }
            if (script.TryGetComponent<Animator>(out var anim))
            {
                script.CustomArmsIKHandler = null;
                headTransform = anim.GetBoneTransform(HumanBodyBones.Head);
                foreArmTransform = anim.GetBoneTransform(HumanBodyBones.RightLowerArm);
                Vector3 weaponAimPos = script.GlobalWeaponAimPosition;
                Vector3 rightHandPos = script.globalRightHandPosition;

                if (watchingWeapon != null)
                {
                    var customPosData = script.GetCustomPositionsFor(watchingWeapon.GetGunID());
                    if (customPosData != null)
                    {
                        weaponAimPos = customPosData.AimPosition;
                        rightHandPos = customPosData.IdlePosition;
                    }
                }
                aimPos = headTransform.TransformPoint(weaponAimPos);
                idlePos = headTransform.TransformPoint(rightHandPos);

                script.Init();
                script.editor_previewMode = 1;
                script.editor_weight = 1;
            }
            else { Debug.Log("Can't preview without an animator attached"); }
        }
        GUI.enabled = true;
    }

    private void OnSceneGUI()
    {
        if (script.editor_previewMode == 1 && headTransform != null)
        {

            // draw a GUI button for the aim position
            Handles.BeginGUI();
            Vector3 aboveHeadPos = headTransform.position + (Vector3.up * 0.15f);
            Vector2 guiPoint = HandleUtility.WorldToGUIPoint(aboveHeadPos);
            Rect rect = new Rect(guiPoint.x - 70, guiPoint.y - 50, 140, 20);
            currentPreviewState = GUI.Toolbar(rect, currentPreviewState, previewStates);
            script.editor_is_aiming = currentPreviewState == 1;
            rect.y -= 25;
            EditorGUI.BeginChangeCheck();
            script.editor_preview_global = GUI.Toggle(rect, script.editor_preview_global, new GUIContent("Preview Global", "Modify the global values instead of the values for the current active weapon only."), EditorStyles.miniButton);
            if (EditorGUI.EndChangeCheck())
            {
                if (headTransform != null)
                {
                    if (script.editor_preview_global)
                    {
                        aimPos = headTransform.TransformPoint(script.GlobalWeaponAimPosition);
                        idlePos = headTransform.TransformPoint(script.globalRightHandPosition);
                    }
                    else
                    {
                        if (watchingWeapon != null)
                        {
                            var customPositions = script.GetCustomPositionsFor(watchingWeapon.GetGunID());
                            if (customPositions != null)
                            {
                                idlePos = headTransform.TransformPoint(customPositions.IdlePosition);
                                aimPos = headTransform.TransformPoint(customPositions.AimPosition);
                            }
                            else
                            {
                                idlePos = headTransform.TransformPoint(script.globalRightHandPosition);
                                aimPos = headTransform.TransformPoint(script.GlobalWeaponAimPosition);
                            }

                        }
                    }
                }
            }

            rect.y -= 25;
            editLeftHandPos = GUI.Toggle(rect, editLeftHandPos, "Edit Left Hand", EditorStyles.miniButton);

            Handles.EndGUI();

            Handles.DrawDottedLine(idlePos, aimPos, 2);

            if (foreArmTransform != null && playerReferences != null)
            {
                Vector3 polePos = playerReferences.transform.TransformPoint(script.rightPolePosition);
                Handles.DrawDottedLine(foreArmTransform.position, polePos, 2);
                Vector3 rhs = foreArmTransform.position - polePos;
                Handles.ConeHandleCap(0, polePos, Quaternion.LookRotation(rhs, Vector3.up), 0.1f, EventType.Repaint);
            }

            if (editLeftHandPos)
            {
                if (watchingWeapon != null && watchingWeapon.LeftHandPosition != null)
                {
                    watchingWeapon.LeftHandPosition.GetPositionAndRotation(out Vector3 lhp, out Quaternion lhr);
                    Handles.TransformHandle(ref lhp, ref lhr);
                    watchingWeapon.LeftHandPosition.SetPositionAndRotation(lhp, lhr);
                    return;
                }
            }

            if (script.editor_is_aiming) PreviewAiming();
            else PreviewIdle();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void PreviewIdle()
    {
        idlePos = Handles.PositionHandle(idlePos, Quaternion.identity);
        if (oldIdlePos != idlePos)
        {
            if (watchingWeapon != null && !script.editor_preview_global)
            {
                int gunId = watchingWeapon.GetGunID();
                if (gunId != -1)
                {
                    int index = script.customWeaponAimPositions.FindIndex(x => x.GunID == gunId);
                    if (index != -1)
                    {
                        script.customWeaponAimPositions[index].IdlePosition = headTransform.InverseTransformPoint(idlePos);
                    }
                    else
                    {
                        script.globalRightHandPosition = headTransform.InverseTransformPoint(idlePos);
                    }
                }
            }
            else
            {
                script.globalRightHandPosition = headTransform.InverseTransformPoint(idlePos);
            }
            oldIdlePos = idlePos;
        }

        Handles.Label(idlePos + (Vector3.down * 0.1f), "Idle Position");
    }

    /// <summary>
    /// 
    /// </summary>
    void PreviewAiming()
    {
        aimPos = Handles.PositionHandle(aimPos, Quaternion.identity);
        if (oldAimPos != aimPos)
        {
            if (watchingWeapon != null && !script.editor_preview_global)
            {
                int gunId = watchingWeapon.GetGunID();
                if (gunId != -1)
                {
                    int index = script.customWeaponAimPositions.FindIndex(x => x.GunID == gunId);
                    if (index != -1)
                    {
                        script.customWeaponAimPositions[index].AimPosition = headTransform.InverseTransformPoint(aimPos);
                    }
                    else
                    {
                        script.GlobalWeaponAimPosition = headTransform.InverseTransformPoint(aimPos);
                    }
                }
            }
            else
            {
                script.GlobalWeaponAimPosition = headTransform.InverseTransformPoint(aimPos);
            }
            oldAimPos = aimPos;
        }
        Handles.Label(aimPos + (Vector3.down * 0.1f), "Aim Position");
    }
}