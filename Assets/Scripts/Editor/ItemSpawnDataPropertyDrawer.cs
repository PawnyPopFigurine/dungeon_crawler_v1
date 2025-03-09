using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using JZK.Level;
using Codice.CM.Client.Differences.Graphic;

namespace Levels
{
    [CustomPropertyDrawer(typeof(ItemSpawnDataEntry))]
    public class ItemSpawnDataEntryPropertyDrawer : PropertyDrawer
    {
        bool _printDebug = false;

        const int LINE_HEIGHT = 20;

        Dictionary<string, float> _perPropertyHeights = new();

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            string propertyPath = property.propertyPath;

            int numProperties = 0;

            Rect contentRect = new(position);

            EditorGUI.BeginProperty(position, label, property);

            SerializedProperty prop_UseFixedId = property.FindPropertyRelative("_useFixedId");
            bool useFixedId = prop_UseFixedId.boolValue;
            DrawPropertyField(ref contentRect, prop_UseFixedId, new("Use Fixed ID"), ref numProperties);

            if (useFixedId)
            {
                EditorGUI.indentLevel++;
                SerializedProperty prop_ItemId = property.FindPropertyRelative("_itemId");
                DrawPropertyField(ref contentRect, prop_ItemId, new("Item ID"), ref numProperties);
                EditorGUI.indentLevel--;
            }


            float propertyHeight = GetHeightForPropertyCount(numProperties);

            if (_perPropertyHeights.ContainsKey(propertyPath))
            {
                _perPropertyHeights[propertyPath] = propertyHeight;
            }
            else
            {
                _perPropertyHeights.Add(propertyPath, propertyHeight);
            }

            EditorGUI.EndProperty();
        }

        void DrawLabel(ref Rect contentRect, GUIContent label, ref int numProperties, FontStyle style = FontStyle.Normal)
        {
            contentRect.height = LINE_HEIGHT;

            GUIStyle labelStyle = new();
            labelStyle.normal.textColor = Color.white;
            labelStyle.fontStyle = style;

            EditorGUI.LabelField(contentRect, label, labelStyle);

            numProperties++;

            contentRect.y += LINE_HEIGHT;
        }

        void DrawPropertyField(ref Rect contentRect, SerializedProperty property, GUIContent label, ref int numProperties)
        {
            contentRect.height = LINE_HEIGHT;
            EditorGUI.PropertyField(contentRect, property, label, true);
            numProperties++;
            contentRect.y += LINE_HEIGHT;
        }

        float GetHeightForPropertyCount(int numProperties)
        {
            return numProperties * LINE_HEIGHT;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (!_perPropertyHeights.TryGetValue(property.propertyPath, out float height))
            {
                if (_printDebug)
                {
                    Debug.Log("No height value for property " + property.propertyPath);
                }
                return 0;
            }

            return height;
        }
    }
}
