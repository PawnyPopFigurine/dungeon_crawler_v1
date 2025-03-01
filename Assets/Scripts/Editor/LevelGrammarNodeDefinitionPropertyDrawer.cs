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

            SerializedProperty prop_NodeId = property.FindPropertyRelative("_nodeId");
            string nodeId = prop_NodeId.stringValue;

            DrawLabel(ref contentRect, new(nodeId), ref numProperties, FontStyle.Bold);

            DrawPropertyField(ref contentRect, prop_NodeId, new("Display Name"), ref numProperties);

            SerializedProperty prop_UseFixedId = property.FindPropertyRelative("_useFixedId");
            bool useFixedID = prop_UseFixedId.boolValue;
            DrawPropertyField(ref contentRect, prop_UseFixedId, new("Set Room Prefab ID"), ref numProperties);

            SerializedProperty prop_FixedRoomType = property.FindPropertyRelative("_fixedRoomType");
            ERoomType roomType = (ERoomType)prop_FixedRoomType.enumValueIndex;

            if (useFixedID)
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

            if(roomType == ERoomType.StandardCombat)
            {
                SerializedProperty prop_FixedEnemySpawns = property.FindPropertyRelative("_fixedEnemySpawns");
                DrawPropertyField(ref contentRect, prop_FixedEnemySpawns, new("Fixed Enemy Spawns"), ref numProperties);

                SerializedProperty prop_SpawnRandomEnemies = property.FindPropertyRelative("_spawnRandomEnemies");
                bool spawnRandomEnemies = prop_SpawnRandomEnemies.boolValue;
                DrawPropertyField(ref contentRect, prop_SpawnRandomEnemies, new("Spawn Random Enemies"), ref numProperties);

                if (spawnRandomEnemies)
                {
                    SerializedProperty prop_RandomEnemySpawnData = property.FindPropertyRelative("_randomEnemySpawnData");
                    DrawPropertyField(ref contentRect, prop_RandomEnemySpawnData, new("Random Enemy Spawn Data"), ref numProperties);
                }
            }


            SerializedProperty prop_RoomLinkData = property.FindPropertyRelative("_roomLinkData");
            DrawPropertyField(ref contentRect, prop_RoomLinkData, new("Link Data"), ref numProperties);

            float propertyHeight = GetHeightForPropertyCount(numProperties);

            foreach(int overrideHeight in _overrideSubPropertyHeights)
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
                case "_fixedEnemySpawns":
                    if (property.isExpanded)
                    {
                        if (property.arraySize == 0)
                        {
                            lineCount += 1;
                        }
                        else
                        {
                            for (int arrayIndex = 0; arrayIndex < property.arraySize; arrayIndex++)
                            {
                                SerializedProperty prop_ArrayElement = property.GetArrayElementAtIndex(arrayIndex);

                                int subLineCount = GetLineCountForProperty(property.name, prop_ArrayElement);
                                lineCount += subLineCount;
                            }
                        }
                    }

                    overrideHeight += PADDING;

                    for(int line = 0; line < lineCount; ++line)
					{
                        overrideHeight += LINE_HEIGHT;
                        overrideHeight += PADDING;

                    }
                    //Debug.Log("[HELLO] total line count for fixed enemy spawns is " + lineCount.ToString() + " - " + property.propertyPath + " - property height is " + lineCount * LINE_HEIGHT + " - override height " + overrideHeight);
                    break;
                case "_randomEnemySpawnData":
                    if (property.isExpanded)
                    {
                        lineCount = 4;

                        SerializedProperty prop_RestrictType = property.FindPropertyRelative("_poolRestrictType");
                        EEnemyPoolRestrictType restrictType = (EEnemyPoolRestrictType)prop_RestrictType.enumValueIndex;
                        switch(restrictType)
						{
                            case EEnemyPoolRestrictType.IDs:
                                SerializedProperty prop_IncludeIds = property.FindPropertyRelative("_poolIds");
                                if (prop_IncludeIds.isExpanded)
                                {
                                    if (prop_IncludeIds.arraySize == 0)
                                    {
                                        lineCount += 1;
                                    }
                                    else
                                    {
                                        lineCount += prop_IncludeIds.arraySize;
                                    }

                                    lineCount += 2;
                                }
                                break;
                            case EEnemyPoolRestrictType.LevelThemes:
                                SerializedProperty prop_IncludeThemes = property.FindPropertyRelative("_poolThemes");
                                if (prop_IncludeThemes.isExpanded)
                                {
                                    if (prop_IncludeThemes.arraySize == 0)
                                    {
                                        lineCount += 1;
                                    }
                                    else
                                    {
                                        lineCount += prop_IncludeThemes.arraySize;
                                    }

                                    lineCount += 2;
                                }
                                break;
						}

                        SerializedProperty prop_ExcludeIds = property.FindPropertyRelative("_excludeIds");
                        if (prop_ExcludeIds.isExpanded)
                        {
                            if (prop_ExcludeIds.arraySize == 0)
                            {
                                lineCount += 1;
                            }
                            else
                            {
                                lineCount += prop_ExcludeIds.arraySize;
                            }

                            lineCount += 2;
                        }
                    }

                    else
                    {
                        lineCount = 1;
                    }
                    //Debug.Log("[HELLO] total line count for random enemy spawn data is " + lineCount.ToString() + " - " + property.propertyPath + " - property height is " + lineCount * LINE_HEIGHT + " - override height " + overrideHeight);
                    break;
                case "_roomLinkData":
                    lineCount = 1;
                    if (property.isExpanded)
                    {
                        lineCount += 1;
                        lineCount += property.arraySize;

                        if (property.arraySize == 0)
                        {
                            lineCount += 1;
                        }
                        else
                        {
                            for (int arrayIndex = 0; arrayIndex < property.arraySize; arrayIndex++)
                            {
                                SerializedProperty prop_ArrayElement = property.GetArrayElementAtIndex(arrayIndex);

                                int subLineCount = GetLineCountForProperty(property.name, prop_ArrayElement);

                                if (prop_ArrayElement.isExpanded)
                                {
                                    lineCount += subLineCount;
                                }
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
            if(overrideHeight == 0)
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

        int GetLineCountForProperty(string parentPropertyName, SerializedProperty property)
        {
            switch(parentPropertyName)
            {
                case "_fixedEnemySpawns":
                    int baseNum = 2;
                    SerializedProperty prop_UseFixedCoords = property.FindPropertyRelative("_useFixedCoords");
                    if(prop_UseFixedCoords.boolValue)
					{
                        baseNum++;
					}
                    return baseNum;
                case "_roomLinkData":
                    return 3;
                default:
                    Debug.Log("[EDITOR] No line count set for property " + parentPropertyName);
                    return 1;
            }
        }
    }
}
