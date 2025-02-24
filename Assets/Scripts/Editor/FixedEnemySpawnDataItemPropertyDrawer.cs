using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using JZK.Level;
using Codice.CM.Client.Differences.Graphic;

namespace Levels
{
    [CustomPropertyDrawer(typeof(FixedEnemySpawnDataItem))]
    public class FixedEnemySpawnDataItemPropertyDrawer : PropertyDrawer
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

            SerializedProperty prop_EnemyId = property.FindPropertyRelative("_enemyId");
            DrawPropertyField(ref contentRect, prop_EnemyId, new("Enemy ID"), ref numProperties);

            SerializedProperty prop_UseFixedCoords = property.FindPropertyRelative("_useFixedCoords");
            bool useFixedCoords = prop_UseFixedCoords.boolValue;
            DrawPropertyField(ref contentRect, prop_UseFixedCoords, new("Fix Spawn Point"), ref numProperties);

            if(useFixedCoords)
			{
                EditorGUI.indentLevel++;
                SerializedProperty prop_FixedCoords = property.FindPropertyRelative("_fixedCoords");
                DrawPropertyField(ref contentRect, prop_FixedCoords, new("Spawn Coords"), ref numProperties);
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

        int GetLineCountForProperty(string parentPropertyName, SerializedProperty property)
        {
            switch (parentPropertyName)
            {
                case "_fixedEnemySpawns":
                    return 3;
                case "_roomLinkData":
                    return 3;
                default:
                    Debug.Log("[EDITOR] No line count set for property " + parentPropertyName);
                    return 1;
            }
        }
    }
}
