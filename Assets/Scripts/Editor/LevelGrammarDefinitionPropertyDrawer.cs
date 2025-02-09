using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using JZK.Level;

namespace Levels
{
    [CustomPropertyDrawer(typeof(LevelGrammarDefinition))]
    public class LevelGrammarDefinitionPropertyDrawer : PropertyDrawer
    {
        public static LevelGrammarDefinition Definition;

        private static Dictionary<Guid, string> _levelGrammarNodeGuidToId_LUT = new();
        private static Dictionary<string, Guid> _levelGrammarNodeIdToGuid_LUT = new();

        public static Dictionary<Guid, string> LevelGrammarNodeGuidToId_LUT => _levelGrammarNodeGuidToId_LUT;
        public static Dictionary<string, Guid> LevelGrammarNodeIdToGuid_LUT => _levelGrammarNodeIdToGuid_LUT;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            ValidateNodes();
            RefreshLookups();

            EditorGUI.BeginProperty(position, label, property);

            SerializedProperty prop_Id = property.FindPropertyRelative("_id");
            EditorGUILayout.PropertyField(prop_Id);

            SerializedProperty prop_BaseLevelTheme = property.FindPropertyRelative("_baseLevelTheme");
            EditorGUILayout.PropertyField(prop_BaseLevelTheme);

            SerializedProperty prop_Nodes = property.FindPropertyRelative("_nodes");
            EditorGUILayout.PropertyField(prop_Nodes);

            EditorGUI.EndProperty();
        }

        void ValidateNodes()
        {
            List<Guid> existingNodeGuids = new();

            foreach(LevelGrammarNodeDefinition node in Definition.Nodes)
            {
                node.ValidateData(existingNodeGuids);
            }
        }

        void RefreshLookups()
        {
            _levelGrammarNodeGuidToId_LUT.Clear();
            _levelGrammarNodeIdToGuid_LUT.Clear();

            foreach(LevelGrammarNodeDefinition node in Definition.Nodes)
            {
                _levelGrammarNodeIdToGuid_LUT.TryAdd(node.Id, node.NodeGuid);
                _levelGrammarNodeGuidToId_LUT.TryAdd(node.NodeGuid, node.Id);
            }
        }
    }
}
