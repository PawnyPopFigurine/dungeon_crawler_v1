using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using JZK.Gameplay;


namespace JZK.Level
{
    [CustomPropertyDrawer(typeof(RoomLinkData))]
    public class RoomLinkDataPropertyDrawer : PropertyDrawer
    {
        bool _printDebug = false;

        const int LINE_HEIGHT = 20;
        const int PADDING = 2;

        Dictionary<string, float> _perPropertyHeights = new();

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            string propertyPath = property.propertyPath;

            int numProperties = 0;

            Rect contentRect = new(position);

            EditorGUI.BeginProperty(position, label, property);

            SerializedProperty prop_FixedSide = property.FindPropertyRelative("_fixedSide");

            SerializedProperty prop_ParentNode = property.FindPropertyRelative("_parentNode");
            Guid parentNode = new(prop_ParentNode.stringValue);

            SerializedProperty prop_LinkToNode = property.FindPropertyRelative("_linkToNode");
            DrawPropertyField(ref contentRect, prop_LinkToNode, new("Link To Node"), ref numProperties);

            SerializedProperty prop_UseFixedSide = property.FindPropertyRelative("_useFixedSide");
            bool useFixedSide = prop_UseFixedSide.boolValue;
            DrawPropertyField(ref contentRect, prop_UseFixedSide, new("Use Fixed Side"), ref numProperties);
            if(prop_UseFixedSide.boolValue != useFixedSide)
			{
                TriggerUpdateOnMatchingLinkData(property);
            }

            if(useFixedSide)
			{
                EOrthogonalDirection fixedSide = (EOrthogonalDirection)prop_FixedSide.enumValueIndex;
                DrawPropertyField(ref contentRect, prop_FixedSide, new("Fixed Side"), ref numProperties);
                if((EOrthogonalDirection)prop_FixedSide.enumValueIndex != fixedSide)
				{
                    TriggerUpdateOnMatchingLinkData(property);
                }
            }

            SerializedProperty prop_LockedByKey = property.FindPropertyRelative("_lockedByKey");
            bool lockedByKey = prop_LockedByKey.boolValue;
            DrawPropertyField(ref contentRect, prop_LockedByKey, new("Locked By Key"), ref numProperties);

            if(lockedByKey != prop_LockedByKey.boolValue)
            {
                TriggerUpdateOnMatchingLinkData(property);
            }

            if(lockedByKey)
            {
                SerializedProperty prop_KeyIndex = property.FindPropertyRelative("_keyIndex");
                int keyIndex = prop_KeyIndex.intValue;
                DrawPropertyField(ref contentRect, prop_KeyIndex, new("Key Index"), ref numProperties);
                if(keyIndex != prop_KeyIndex.intValue)
                {
                    TriggerUpdateOnMatchingLinkData(property);
                }
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

        void TriggerUpdateOnMatchingLinkData(SerializedProperty property)
        {
            SerializedProperty prop_LinkToNode = property.FindPropertyRelative("_linkToNode");
            SerializedProperty prop_FixedSide = property.FindPropertyRelative("_fixedSide");
            SerializedProperty prop_UseFixedSide = property.FindPropertyRelative("_useFixedSide");
            SerializedProperty prop_ParentNode = property.FindPropertyRelative("_parentNode");
            SerializedProperty prop_LockedByKey = property.FindPropertyRelative("_lockedByKey");
            SerializedProperty prop_KeyIndex = property.FindPropertyRelative("_keyIndex");

            Guid parentNode = new(prop_ParentNode.stringValue);

            EOrthogonalDirection oppositeLinkSide = GameplayHelper.GetOppositeDirection((EOrthogonalDirection)prop_FixedSide.enumValueIndex);

            Guid linkToNodeGuid = new(prop_LinkToNode.FindPropertyRelative("_guidString").stringValue);

            LevelGrammarDefinition def = LevelGrammarDefinitionPropertyDrawer.Definition;

            foreach (LevelGrammarNodeDefinition node in def.Nodes)
            {
                if (node.NodeGuid != linkToNodeGuid)
                {
                    continue;
                }

                foreach (RoomLinkData linkData in node.RoomLinkData)
                {
                    if (linkData.LinkToNode.Id != parentNode)
                    {
                        continue;
                    }

                    linkData.UseFixedSide = prop_UseFixedSide.boolValue;

                    RoomLinkUpdateData updateData = new()
                    {
                        ParentNodeId = linkToNodeGuid,
                        LinkToNodeId = parentNode,
                        NewFixedSideEnum = oppositeLinkSide,
                        NewFixedSide = prop_UseFixedSide.boolValue,
                        NewLocked = prop_LockedByKey.boolValue,
                        NewKeyIndex = prop_KeyIndex.intValue,
                    };

                    LevelGrammarDefinitionPropertyDrawer.AddRoomLinkDataForUpdate(updateData);

                }

            }
        }

        void DrawPropertyField(ref Rect contentRect, SerializedProperty property, GUIContent label, ref int numProperties)
        {
            Rect useRect = contentRect;

            if(property.name == "_linkToNode")
			{
                useRect.height = 22;

            }

            EditorGUI.PropertyField(useRect, property, label, true);

            numProperties += 1;
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
