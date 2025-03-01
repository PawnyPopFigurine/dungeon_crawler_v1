using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using JZK.Level;
using Codice.CM.Client.Differences.Graphic;

namespace Levels
{
    [CustomPropertyDrawer(typeof(RandomEnemySpawnData))]
    public class RandomEnemySpawnDataPropertyDrawer : PropertyDrawer
    {
        bool _printDebug = false;

        const int LINE_HEIGHT = 20;
        const int PADDING = 2;

        Dictionary<string, float> _perPropertyHeights = new();

        List<int> _overrideSubPropertyHeights = new();

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            string propertyPath = property.propertyPath;

            _overrideSubPropertyHeights.Clear();

            int numProperties = 0;

            Rect contentRect = new(position);

            EditorGUI.BeginProperty(position, label, property);

            SerializedProperty prop_DifficultyPoints = property.FindPropertyRelative("_difficultyPoints");
            DrawPropertyField(ref contentRect, prop_DifficultyPoints, new("Difficulty Points"), ref numProperties);

            SerializedProperty prop_PoolRestrictType = property.FindPropertyRelative("_poolRestrictType");
            EEnemyPoolRestrictType restrictType = (EEnemyPoolRestrictType)prop_PoolRestrictType.enumValueIndex;
            DrawPropertyField(ref contentRect, prop_PoolRestrictType, new("Pool Restrict Type"), ref numProperties);

            switch(restrictType)
			{
                case EEnemyPoolRestrictType.None:
                    break;
                case EEnemyPoolRestrictType.IDs:
                    SerializedProperty prop_PoolIds = property.FindPropertyRelative("_poolIds");
                    DrawPropertyField(ref contentRect, prop_PoolIds, new("Pool IDs"), ref numProperties);
                    break;
                case EEnemyPoolRestrictType.LevelThemes:
                    SerializedProperty prop_PoolThemes = property.FindPropertyRelative("_poolThemes");
                    DrawPropertyField(ref contentRect, prop_PoolThemes, new("Pool Themes"), ref numProperties);
                    break;
			}

            SerializedProperty prop_ExcludeIds = property.FindPropertyRelative("_excludeIds");
            DrawPropertyField(ref contentRect, prop_ExcludeIds, new("Exclude IDs"), ref numProperties);

            float propertyHeight = GetHeightForPropertyCount(numProperties);

            foreach (int overrideHeight in _overrideSubPropertyHeights)
            {
                propertyHeight += overrideHeight;
            }

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

        int GetPropertyFieldLineCount(SerializedProperty property, out int overrideHeight)
        {
            int lineCount = 1;
            overrideHeight = 0;

            switch (property.name)
            {
                case "_poolIds":
                case "_poolThemes":
                case "_excludeIds":
                    if (property.isExpanded)
                    {
                        lineCount = 4;

                        if(property.arraySize > 1)
						{
                            for (int arrayIndex = 1; arrayIndex < property.arraySize; arrayIndex++)
                            {
                                lineCount += 1;
                            }
                        }
                    }
                    break;
            }

            return lineCount;
        }

        void DrawPropertyField(ref Rect contentRect, SerializedProperty property, GUIContent label, ref int numProperties)
        {
            int lineCount = GetPropertyFieldLineCount(property, out int overrideHeight);
            if (overrideHeight == 0)
            {
                contentRect.height += lineCount * LINE_HEIGHT;

                EditorGUI.PropertyField(contentRect, property, label, true);

                numProperties += lineCount;
                contentRect.y += LINE_HEIGHT * lineCount;
            }
            else
            {
                //DON'T add to numProperties if overriding height - supPropertyHeights list accounts for it

                contentRect.height += overrideHeight;

                EditorGUI.PropertyField(contentRect, property, label, true);
                contentRect.y += overrideHeight;

                _overrideSubPropertyHeights.Add(overrideHeight);
            }

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
