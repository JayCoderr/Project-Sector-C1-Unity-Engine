using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(bl_WeaponMovements))]
public class bl_EditorWeaponMovement : Editor
{
    private bool isRecording = false;
    Vector3 defaultPosition = Vector3.zero;
    Quaternion defaultRotation = Quaternion.identity;
    bl_WeaponMovements script;
    SerializedProperty previewProp;
    public bool isPreviwing = false;

    private void OnEnable()
    {
        previewProp = serializedObject.FindProperty("_previewWeight");
        previewProp.isExpanded = false;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        script = (bl_WeaponMovements)target;

        EditorGUILayout.BeginVertical("box");
        GUILayout.BeginHorizontal("box");
        Color c = isRecording ? Color.red : Color.white;
        GUI.color = c;
        if (GUILayout.Button(new GUIContent(" Edit Pose", EditorGUIUtility.IconContent("d_EditCollider").image, "Active/Deactive the Edit mode.")))
        {
            isRecording = !isRecording;
            if (isRecording)
            {
                defaultPosition = script.transform.localPosition;
                defaultRotation = script.transform.localRotation;
                if (script.moveTo != Vector3.zero)
                {
                    script.transform.localPosition = script.moveTo;
                    script.transform.localRotation = Quaternion.Euler(script.rotateTo);
                }
                Tools.current = Tool.Transform;
                ActiveEditorTracker.sharedTracker.isLocked = true;
                isPreviwing = false;
            }
            else
            {
                script.transform.localPosition = defaultPosition;
                script.transform.localRotation = defaultRotation;
                ActiveEditorTracker.sharedTracker.isLocked = false;
                EditorUtility.SetDirty(target);
            }
        }
        GUI.color = Color.white;
        GUILayout.EndHorizontal();

        GUILayout.Label("On Run weapon position", EditorStyles.helpBox);
        script.moveTo = EditorGUILayout.Vector3Field("Position", script.moveTo);
        script.rotateTo = EditorGUILayout.Vector3Field("Rotation", script.rotateTo);
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Get Actual Position"))
        {
            script.moveTo = script.transform.localPosition;
            script.rotateTo = script.transform.localEulerAngles;
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical("box");
        GUILayout.Label("On Run and Reload weapon position", EditorStyles.helpBox);
        script.moveToReload = EditorGUILayout.Vector3Field("Position", script.moveToReload);
        script.rotateToReload = EditorGUILayout.Vector3Field("Rotation", script.rotateToReload);
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Get Actual Position"))
        {
            script.moveToReload = script.transform.localPosition;
            script.rotateToReload = script.transform.localRotation.eulerAngles;
        }
        if (GUILayout.Button("Copy"))
        {
            script.moveToReload = script.moveTo;
            script.rotateToReload = script.rotateTo;
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();

        GUILayout.BeginVertical("box");
        script.createUniquePivot = bl_GunEditor.Toggle("Use Unique Pivot", script.createUniquePivot);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("settings"), true);
        GUILayout.EndVertical();

        if (!isRecording)
        {
            var l = EditorGUI.indentLevel;
            EditorGUI.indentLevel++;
            GUILayout.BeginVertical("box");
            previewProp.isExpanded = EditorGUILayout.Foldout(previewProp.isExpanded, "Preview Movement");
            if (isPreviwing != previewProp.isExpanded)
            {
                defaultPosition = script.transform.localPosition;
                defaultRotation = script.transform.localRotation;
                script._previewWeight = 0;
                isPreviwing = previewProp.isExpanded;
            }
            if (previewProp.isExpanded)
            {
                EditorGUI.BeginChangeCheck();
                script._previewWeight = EditorGUILayout.Slider("Weight", script._previewWeight, 0, 1);
                if (EditorGUI.EndChangeCheck() && !Application.isPlaying)
                {
                    script.transform.localPosition = Vector3.Lerp(defaultPosition, script.moveTo, script._previewWeight);
                    script.transform.localRotation = Quaternion.Slerp(defaultRotation, Quaternion.Euler(script.rotateTo), script._previewWeight);
                }
            }
            GUILayout.EndVertical();
            EditorGUI.indentLevel = l;
        }

        if (GUI.changed)
        {
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(target);
        }
    }

    Vector3 CalculateCenter()
    {
        var renderers = script.transform.GetComponentsInChildren<Renderer>();
        Bounds b = new Bounds(renderers[0].bounds.center, renderers[0].bounds.size);
        foreach (var r in renderers)
        {
            if (r.GetComponent<ParticleSystem>() != null) continue;
            if (b.extents == Vector3.zero)
                b = r.bounds;

            b.Encapsulate(r.bounds);
        }
        return b.center;
    }
}