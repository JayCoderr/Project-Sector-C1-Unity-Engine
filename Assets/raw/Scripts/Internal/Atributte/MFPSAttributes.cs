using System.Linq;
using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MFPSEditor
{
    #region MFPSCoin Attribute
    public class MFPSCoinIDAttribute : PropertyAttribute
    {
        public MFPSCoinIDAttribute()
        {
        }
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(MFPSCoinIDAttribute))]
    public class MFPSCoinIDAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            if (property.hasMultipleDifferentValues)
            {
                EditorGUI.PropertyField(position, property);
            }
            else
            {
                var names = bl_GameData.Instance.gameCoins.Select(x =>
                {
                    if (x == null) return "Empty";

                    return x.CoinName;
                }).ToArray();
                property.intValue = EditorGUI.Popup(position, property.displayName, property.intValue, names);
            }
            EditorGUI.EndProperty();
        }
    }
#endif
    #endregion

    #region ReadOnly Attribute
    public class ReadOnlyAttribute : PropertyAttribute { }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
    public class ReadOnlyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            GUI.enabled = false;
            bool isString = property.propertyType == SerializedPropertyType.String && !string.IsNullOrEmpty(property.stringValue);
            if (isString) position.width -= 20f;
            EditorGUI.PropertyField(position, property, label, true);
            GUI.enabled = true;
            if (isString)
                if (GUI.Button(new Rect(position.xMax, position.y, 20f, position.height), "C"))
                {
                    GUIUtility.systemCopyBuffer = property.stringValue;
                }
        }
    }
#endif
    #endregion

    #region FlagEnum Attribute
    public class FlagEnumAttribute : PropertyAttribute
    {
        public FlagEnumAttribute() { }
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(FlagEnumAttribute))]
    public class FlagEnumAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect _position, SerializedProperty _property, GUIContent _label)
        {
            _property.intValue = EditorGUI.MaskField(_position, _label, _property.intValue, _property.enumNames);
        }
    }
#endif 
    #endregion

    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public class MFPSGameItemCollectorAttribute : Attribute
    {
    }
}