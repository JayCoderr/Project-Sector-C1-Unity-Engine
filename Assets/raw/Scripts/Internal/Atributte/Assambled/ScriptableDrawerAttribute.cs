using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MFPSEditor
{
    public class ScriptableDrawerAttribute : PropertyAttribute
    {
        public ScriptableDrawerAttribute()
        {
        }
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(ScriptableDrawerAttribute))]
    public class ScriptableDrawerAttributeDrawer : PropertyDrawer
    {
        private Editor cachedEditor;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            EditorGUI.BeginChangeCheck();
            if (property.objectReferenceValue == null)
            {
                EditorGUI.PropertyField(position, property);
            }
            else
            {
                var name = property.displayName;
                if (name.Contains("Element"))
                {
                    name = property.objectReferenceValue.name;
                }
                property.isExpanded = MFPSEditorStyles.ContainerHeaderFoldout(name, property.isExpanded);
                if (property.isExpanded)
                {
                    EditorGUILayout.BeginVertical("box");
                    EditorGUI.indentLevel++;
                    EditorGUILayout.BeginHorizontal();
                    {
                        var lw = EditorGUIUtility.labelWidth;
                        EditorGUIUtility.labelWidth = 80;
                        EditorGUILayout.PropertyField(property, new GUIContent("Preset", "Assign the ScriptableObject preset here."));
                        EditorGUIUtility.labelWidth = lw;
                        if (GUILayout.Button("Create Copy", GUILayout.Width(100)))
                        {
                            if (EditorUtility.DisplayDialog("Create copy of preset?", "Do you want to create a new copy of this preset?", "Yes", "Cancel"))
                            {
                                property.objectReferenceValue = CreateAndSaveCopy(property.objectReferenceValue as ScriptableObject);
                            }
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                    DrawEditorOf(property);
                    EditorGUI.indentLevel--;
                    EditorGUILayout.EndVertical();
                }
            }
            if (EditorGUI.EndChangeCheck())
            {
                if (property.serializedObject != null)
                {
                    property.serializedObject.ApplyModifiedProperties();
                    EditorUtility.SetDirty(property.serializedObject.targetObject);
                }
            }
            EditorGUI.indentLevel = indent;
            EditorGUI.EndProperty();
        }

        void DrawEditorOf(SerializedProperty so)
        {
            if (so == null || so.objectReferenceValue == null) return;

            var editor = cachedEditor;
            if (editor == null)
            {
                editor = cachedEditor = Editor.CreateEditor(so.objectReferenceValue);
            }
            if (editor != null)
            {
                EditorGUILayout.BeginVertical("box");
                editor.OnInspectorGUI();
                EditorGUILayout.EndVertical();
            }
        }

        ScriptableObject CreateAndSaveCopy(ScriptableObject original)
        {
            var copy = ScriptableObject.Instantiate(original);
            copy.name = original.name;

            // Save the copy in the same folder as the original
            string path = AssetDatabase.GetAssetPath(original);
            string folder = System.IO.Path.GetDirectoryName(path);

            string finalPath = AssetDatabase.GenerateUniqueAssetPath(folder + "/" + copy.name + ".asset");
            AssetDatabase.CreateAsset(copy, finalPath);
            AssetDatabase.SaveAssets();

            var newCopy = AssetDatabase.LoadAssetAtPath<ScriptableObject>(finalPath);
            return newCopy;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (property.objectReferenceValue == null)
                return EditorGUIUtility.singleLineHeight;
            else return 0;
        }
    }
#endif
}