using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using JZK.Level;

namespace Levels
{
    [CustomPropertyDrawer(typeof(LevelGrammarNodeDefinition))]
    public class LevelGrammarNodeDefinitionPropertyDrawer : PropertyDrawer
    {
        const int LINE_HEIGHT = 20;

        bool _printDebug = false;

        Dictionary<string, float> _perPropertyHeights = new();


        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            string propertyPath = property.propertyPath;

            int numProperties = 0;

            Rect contentRect = new(position);

            EditorGUI.BeginProperty(position, label, property);

            SerializedProperty prop_NodeId = property.FindPropertyRelative("_nodeId");
            DrawPropertyField(ref contentRect, prop_NodeId, new("Display ID"), ref numProperties);

            SerializedProperty prop_UseFixedId = property.FindPropertyRelative("_useFixedId");
            DrawPropertyField(ref contentRect, prop_UseFixedId, new("Fix Room Prefab ID"), ref numProperties);

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

        void DrawPropertyField(ref Rect contentRect, SerializedProperty property, GUIContent label, ref int numProperties)
        {
            contentRect.height = LINE_HEIGHT;

            EditorGUI.PropertyField(contentRect, property, label);

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
