using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using JZK.Level;
using Codice.CM.Client.Differences.Graphic;

namespace Levels
{
    [CustomPropertyDrawer(typeof(LevelGrammarNodeDefinition))]
    public class LevelGrammarNodeDefinitionPropertyDrawer : PropertyDrawer
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

            SerializedProperty prop_NodeId = property.FindPropertyRelative("_nodeId");
            string nodeId = prop_NodeId.stringValue;

            DrawLabel(ref contentRect, new(nodeId), ref numProperties, FontStyle.Bold);

            DrawPropertyField(ref contentRect, prop_NodeId, new("Node ID"), ref numProperties);

            SerializedProperty prop_UseFixedId = property.FindPropertyRelative("_useFixedId");
            bool useFixedID = prop_UseFixedId.boolValue;
            DrawPropertyField(ref contentRect, prop_UseFixedId, new("Set Room Prefab ID"), ref numProperties);

            if(useFixedID)
            {
                EditorGUI.indentLevel++;
                SerializedProperty prop_FixedId = property.FindPropertyRelative("_fixedId");
                DrawPropertyField(ref contentRect, prop_FixedId, new("Room Prefab ID"), ref numProperties);
                EditorGUI.indentLevel--;
            }

            else
            {
                SerializedProperty prop_UseFixedRoomType = property.FindPropertyRelative("_useFixedRoomType");
                bool useFixedRoomType = prop_UseFixedRoomType.boolValue;
                DrawPropertyField(ref contentRect, prop_UseFixedRoomType, new("Set Room Type"), ref numProperties);

                if(useFixedRoomType)
                {
                    EditorGUI.indentLevel++;
                    SerializedProperty prop_FixedRoomType = property.FindPropertyRelative("_fixedRoomType");
                    DrawPropertyField(ref contentRect, prop_FixedRoomType, new("Room Type"), ref numProperties);
                    EditorGUI.indentLevel--;
                }
            }

            SerializedProperty prop_UseOverrideTheme = property.FindPropertyRelative("_useOverrideTheme");
            bool useOverrideTheme = prop_UseOverrideTheme.boolValue;
            DrawPropertyField(ref contentRect, prop_UseOverrideTheme, new("Override Level Theme"), ref numProperties);

            if(useOverrideTheme)
            {
                EditorGUI.indentLevel++;
                SerializedProperty prop_OverrideTheme = property.FindPropertyRelative("_overrideTheme");
                DrawPropertyField(ref contentRect, prop_OverrideTheme, new("Theme"), ref numProperties);
                EditorGUI.indentLevel--;
            }

            SerializedProperty prop_FixedEnemySpawns = property.FindPropertyRelative("_fixedEnemySpawns");
            DrawPropertyField(ref contentRect, prop_FixedEnemySpawns, new("Fixed Enemy Spawns"), ref numProperties);

            

            SerializedProperty prop_SpawnRandomEnemies = property.FindPropertyRelative("_spawnRandomEnemies");
            bool spawnRandomEnemies = prop_SpawnRandomEnemies.boolValue;
            DrawPropertyField(ref contentRect, prop_SpawnRandomEnemies, new("Spawn Random Enemies"), ref numProperties);

            if(spawnRandomEnemies)
            {
                SerializedProperty prop_RandomEnemySpawnData = property.FindPropertyRelative("_randomEnemySpawnData");
                DrawPropertyField(ref contentRect, prop_RandomEnemySpawnData, new("Random Enemy Spawn Data"), ref numProperties);
            }

            SerializedProperty prop_RoomLinkData = property.FindPropertyRelative("_roomLinkData");
            DrawPropertyField(ref contentRect, prop_RoomLinkData, new("Link Data"), ref numProperties);

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

            switch(property.name)
            {
                case "_fixedEnemySpawns":
                    contentRect.height = LINE_HEIGHT;
                    if (property.isExpanded)
                    {
                        contentRect.height += LINE_HEIGHT;
                        contentRect.height += property.arraySize * LINE_HEIGHT;

                        if (property.arraySize == 0)
                        {
                            contentRect.height += LINE_HEIGHT;
                        }
                        else
                        {
                            for (int arrayIndex = 0; arrayIndex < property.arraySize; arrayIndex++)
                            {
                                SerializedProperty prop_ArrayElement = property.GetArrayElementAtIndex(arrayIndex);

                                int lineCount = GetLineCountForProperty(property.name, prop_ArrayElement);

                                if (prop_ArrayElement.isExpanded)
                                {
                                    contentRect.height += LINE_HEIGHT * lineCount;
                                }
                            }
                        }
                    }

                    EditorGUI.PropertyField(contentRect, property, label, true);

                    numProperties++;
                    contentRect.y += LINE_HEIGHT;
                    if (property.isExpanded)
                    {
                        numProperties += property.arraySize + 1;
                        contentRect.y += LINE_HEIGHT * property.arraySize + 1;

                        if (property.arraySize == 0)
                        {
                            numProperties += 1;
                            contentRect.y += LINE_HEIGHT;
                        }
                        else
                        {
                            for (int arrayIndex = 0; arrayIndex < property.arraySize; arrayIndex++)
                            {
                                SerializedProperty prop_ArrayElement = property.GetArrayElementAtIndex(arrayIndex);
                                if (prop_ArrayElement.isExpanded)
                                {
                                    int lineCount = GetLineCountForProperty(property.name, prop_ArrayElement);

                                    numProperties += lineCount;
                                    contentRect.y += LINE_HEIGHT * lineCount;
                                }
                            }
                        }
                    }

                    contentRect.y += LINE_HEIGHT;
                    numProperties++;
                    return;
                case "_randomEnemySpawnData":
                    if (property.isExpanded)
                    {
                        contentRect.height = LINE_HEIGHT * 6;

                        SerializedProperty prop_IncludeIds = property.FindPropertyRelative("_includeIds");
                        if (prop_IncludeIds.isExpanded)
                        {
                            if (prop_IncludeIds.arraySize == 0)
                            {
                                contentRect.height += LINE_HEIGHT;
                            }
                            else
                            {
                                contentRect.height += LINE_HEIGHT * prop_IncludeIds.arraySize;
                            }
                        }

                        SerializedProperty prop_IncludeThemes = property.FindPropertyRelative("_includeThemes");
                        if(prop_IncludeThemes.isExpanded)
                        {
                            if (prop_IncludeThemes.arraySize == 0)
                            {
                                contentRect.height += LINE_HEIGHT;
                            }
                            else
                            {
                                contentRect.height += LINE_HEIGHT * prop_IncludeThemes.arraySize;
                            }
                        }

                        SerializedProperty prop_ExcludeThemes = property.FindPropertyRelative("_excludeThemes");
                        if(prop_ExcludeThemes.isExpanded)
                        {
                            if(prop_ExcludeThemes.arraySize == 0)
                            {
                                contentRect.height += LINE_HEIGHT;
                            }
                            else
                            {
                                contentRect.height += LINE_HEIGHT * prop_ExcludeThemes.arraySize;
                            }
                        }
                    }

                    else
                    {
                        contentRect.height = LINE_HEIGHT;
                    }
                    EditorGUI.PropertyField(contentRect, property, label, true);

                    if(property.isExpanded)
                    {
                        numProperties += 6;
                        contentRect.y += LINE_HEIGHT * 6;
                        SerializedProperty prop_IncludeIds = property.FindPropertyRelative("_includeIds");
                        if (prop_IncludeIds.isExpanded)
                        {
                            numProperties += 2;
                            contentRect.y += LINE_HEIGHT * 2;

                            if (prop_IncludeIds.arraySize == 0)
                            {
                                numProperties++;
                                contentRect.y += LINE_HEIGHT;
                            }
                            else
                            {
                                numProperties += prop_IncludeIds.arraySize;
                                contentRect.y += LINE_HEIGHT * prop_IncludeIds.arraySize;
                            }
                        }
                        SerializedProperty prop_IncludeThemes = property.FindPropertyRelative("_includeThemes");
                        if (prop_IncludeThemes.isExpanded)
                        {
                            numProperties += 2;
                            contentRect.y += LINE_HEIGHT * 2;

                            if (prop_IncludeThemes.arraySize == 0)
                            {
                                numProperties++;
                                contentRect.y += LINE_HEIGHT;
                            }
                            else
                            {
                                numProperties += prop_IncludeThemes.arraySize;
                                contentRect.y += LINE_HEIGHT * prop_IncludeThemes.arraySize;
                            }
                        }
                        SerializedProperty prop_ExcludeThemes = property.FindPropertyRelative("_excludeThemes");
                        if (prop_ExcludeThemes.isExpanded)
                        {
                            numProperties += 2;
                            contentRect.y += LINE_HEIGHT * 2;

                            if (prop_ExcludeThemes.arraySize == 0)
                            {
                                numProperties++;
                                contentRect.y += LINE_HEIGHT;
                            }
                            else
                            {
                                numProperties += prop_ExcludeThemes.arraySize;
                                contentRect.y += LINE_HEIGHT * prop_ExcludeThemes.arraySize;
                            }
                        }
                    }
                    else
                    {
                        numProperties++;
                        contentRect.y += LINE_HEIGHT;
                    }
                    break;
                case "_roomLinkData":
                    contentRect.height = LINE_HEIGHT;
                    if (property.isExpanded)
                    {
                        contentRect.height += LINE_HEIGHT;
                        contentRect.height += property.arraySize * LINE_HEIGHT;

                        if (property.arraySize == 0)
                        {
                            contentRect.height += LINE_HEIGHT;
                        }
                        else
                        {
                            for (int arrayIndex = 0; arrayIndex < property.arraySize; arrayIndex++)
                            {
                                SerializedProperty prop_ArrayElement = property.GetArrayElementAtIndex(arrayIndex);

                                int lineCount = GetLineCountForProperty(property.name, prop_ArrayElement);

                                if (prop_ArrayElement.isExpanded)
                                {
                                    contentRect.height += LINE_HEIGHT * lineCount;
                                }
                            }
                        }
                    }

                    EditorGUI.PropertyField(contentRect, property, label, true);

                    if (property.isExpanded)
                    {
                        numProperties += property.arraySize + 1;
                        contentRect.y += LINE_HEIGHT * property.arraySize + 1;

                        if (property.arraySize == 0)
                        {
                            numProperties += 1;
                            contentRect.y += LINE_HEIGHT;
                        }
                        else
                        {
                            for (int arrayIndex = 0; arrayIndex < property.arraySize; arrayIndex++)
                            {
                                SerializedProperty prop_ArrayElement = property.GetArrayElementAtIndex(arrayIndex);
                                if (prop_ArrayElement.isExpanded)
                                {
                                    int lineCount = GetLineCountForProperty(property.name, prop_ArrayElement);

                                    numProperties += lineCount;
                                    contentRect.y += LINE_HEIGHT * lineCount;
                                }
                            }
                        }
                    }

                    contentRect.y += LINE_HEIGHT;
                    numProperties++;
                    break;
                default:
                    contentRect.height = LINE_HEIGHT;
                    EditorGUI.PropertyField(contentRect, property, label, true);
                    numProperties++;
                    contentRect.y += LINE_HEIGHT;
                    return;
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

        int GetLineCountForProperty(string parentPropertyName, SerializedProperty property)
        {
            switch(parentPropertyName)
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
