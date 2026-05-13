using MFPS.Runtime.AI;
using MFPSEditor;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(bl_AIAnimation))]
public class bl_AIAnimationEditor : Editor
{
    bl_AIAnimation script;

    /// <summary>
    /// 
    /// </summary>
    private void OnEnable()
    {
        script = (bl_AIAnimation)target;
    }

    /// <summary>
    /// 
    /// </summary>
    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();

        base.OnInspectorGUI();

        GUILayout.Space(10);

        EditorGUILayout.HelpBox("Click this button to preview the bot IK, use the 'Right Arm Ik Offset' to adjust the right arm rotation in case it looks wrong.", MessageType.Info);
        if (GUILayout.Button("Preview IK"))
        {
            bl_AIAnimation aia = (script.References.aiAnimation as bl_AIAnimation);

            if (script.References.weaponRoot != null)
            {
                script.defaultWeaponRootPosition = script.References.weaponRoot.localEulerAngles;
                var all = script.References.weaponRoot.GetComponentsInChildren<bl_AIWeapon>(true);
                bl_AIWeapon activeWeapon = null;
                for (int i = 0; i < all.Length; i++)
                {
                    if (all[i].gameObject.activeSelf && activeWeapon == null)
                    {
                        activeWeapon = all[i];
                        continue;
                    }
                    all[i].gameObject.SetActive(false);
                }

                if (activeWeapon == null)
                {
                    activeWeapon = all[0];
                    activeWeapon.gameObject.SetActive(true);
                }

                // aia.currentWeapon = activeWeapon;
            }

            var window = (AnimatorRunner)EditorWindow.GetWindow(typeof(AnimatorRunner));
            window.Show();
            Animator anim = script.gameObject.GetComponent<Animator>();
            window.SetAnim(anim, () =>
            {
                script.References.weaponRoot.localEulerAngles = script.defaultWeaponRootPosition;
                aia.currentWeapon = null;
            }, true);
        }

        EditorGUILayout.HelpBox("Click this button to get all the colliders in the bot player model.", MessageType.Info);
        if (GUILayout.Button("Refresh Rigidbody list"))
        {
            script.GetRigidBodys();
        }

        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(target);
        }
    }
}