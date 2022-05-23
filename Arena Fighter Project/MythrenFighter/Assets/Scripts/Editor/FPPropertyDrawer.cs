using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using FixedPoint;
using UnityEngine.UIElements;

namespace MythrenFighter
{
    [CustomPropertyDrawer(typeof(fp))]
    public class FPPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            int indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            SerializedProperty valueProperty = property.FindPropertyRelative("value");
            float newValue = EditorGUI.FloatField(position, valueProperty.longValue / 65536f);
            valueProperty.longValue = (long)(newValue * 65536f);
            valueProperty.serializedObject.ApplyModifiedProperties();

            EditorGUI.indentLevel = indent;

            EditorGUI.EndProperty();
        }
    }
}
