/*using System.Collections;
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
            string nodeId = prop_NodeId.stringValue;
            DrawLabel(ref contentRect, new(nodeId), ref numProperties, FontStyle.Bold);

            DrawPropertyField(ref contentRect, prop_NodeId, new("Display ID"), ref numProperties);

            SerializedProperty prop_UseFixedId = property.FindPropertyRelative("_useFixedId");
            bool useFixedId = prop_UseFixedId.boolValue;
            DrawPropertyField(ref contentRect, prop_UseFixedId, new("Fix Room Prefab ID"), ref numProperties);

            if(useFixedId)
			{
                SerializedProperty prop_FixedId = property.FindPropertyRelative("_fixedId");
                DrawPropertyField(ref contentRect, prop_FixedId, new("Room Prefab ID"), ref numProperties);
            }

            else
			{
                DrawLabel(ref contentRect, new("Random Room Prefab Properties"), ref numProperties, FontStyle.Bold);

                SerializedProperty prop_UseFixedRoomType = property.FindPropertyRelative("_useFixedRoomType");
                bool useFixedRoomType = prop_UseFixedRoomType.boolValue;
                DrawPropertyField(ref contentRect, prop_UseFixedRoomType, new("Use Fixed Room Type"), ref numProperties);

                if(useFixedRoomType)
				{
                    SerializedProperty prop_FixedRoomType = property.FindPropertyRelative("_fixedRoomType");
                    DrawPropertyField(ref contentRect, prop_FixedRoomType, new("Room Type"), ref numProperties);
                }
            }

            SerializedProperty prop_FixedEnemySpawns = property.FindPropertyRelative("_fixedEnemySpawns");
            DrawPropertyField_Array(ref contentRect, prop_FixedEnemySpawns, new("Fixed Enemy Spawns"), ref numProperties);

            SerializedProperty prop_SpawnRandomEnemies = property.FindPropertyRelative("_spawnRandomEnemies");
            bool spawnRandomEnemies = prop_SpawnRandomEnemies.boolValue;
            DrawPropertyField(ref contentRect, prop_SpawnRandomEnemies, new("Spawn Random Enemies"), ref numProperties);

            if(spawnRandomEnemies)
			{
                SerializedProperty prop_RandomEnemySpawns = property.FindPropertyRelative("_randomEnemySpawnData");
                DrawPropertyField(ref contentRect, prop_RandomEnemySpawns, new("Random Enemy Spawn Data"), ref numProperties);
            }

            SerializedProperty prop_RoomLinkData = property.FindPropertyRelative("_roomLinkData");
            DrawPropertyField_Array(ref contentRect, prop_RoomLinkData, new("Room Link Data"), ref numProperties);

            *//*float propertyHeight = GetHeightForPropertyCount(numProperties);

            if (_perPropertyHeights.ContainsKey(propertyPath))
            {
                _perPropertyHeights[propertyPath] = propertyHeight;
            }
            else
            {
                _perPropertyHeights.Add(propertyPath, propertyHeight);
            }*//*

            EditorGUI.EndProperty();
        }

        void DrawPropertyField(ref Rect contentRect, SerializedProperty property, GUIContent label, ref int numProperties)
        {
            contentRect.height = LINE_HEIGHT;

            EditorGUILayout.PropertyField(property);
            // EditorGUI.PropertyField(contentRect, property, label);

            numProperties++;

            contentRect.y += LINE_HEIGHT;
        }

        void DrawPropertyField_Array(ref Rect contentRect, SerializedProperty property, GUIContent label, ref int numProperties)
        {
            int arrayCount = property.arraySize;

            Debug.Log("[EDITOR] array size is " + arrayCount + " property " + property.name + " is array? " + property.isArray);

            //contentRect.height = LINE_HEIGHT * arrayCount;

            EditorGUILayout.PropertyField(property);
            //EditorGUI.PropertyField(contentRect, property, label);

            numProperties+= arrayCount;

            //contentRect.y += LINE_HEIGHT * arrayCount;
        }

        void DrawLabel(ref Rect contentRect, GUIContent label, ref int numProperties, FontStyle styleEnum = FontStyle.Normal)
		{
            //contentRect.height = LINE_HEIGHT;

            GUIStyle style = new();
            style.fontStyle = styleEnum;
            style.normal.textColor = Color.white;

            EditorGUILayout.LabelField(label, style);
            //EditorGUI.LabelField(contentRect, label, style);

            numProperties++;

            //contentRect.y += LINE_HEIGHT;
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
*/