using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using JZK.Level;
using Levels;

namespace Level
{
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(LevelGrammarNodeReference))]
    public class LevelGrammarNodeReferencePropertyDrawer : PropertyDrawer
    {
        const string NONE_OPTION_TEXT = "<NONE>";
        const float FIELD_SIZE = 18;
        const float PADDING = 2;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return FIELD_SIZE + PADDING;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty prop_Id = property.FindPropertyRelative("_guidString");
            Guid entryId = string.IsNullOrEmpty(prop_Id.stringValue) ? Guid.Empty : Guid.Parse(prop_Id.stringValue);

            List<string> options = new List<string>() { NONE_OPTION_TEXT };

            foreach (var kvp in LevelGrammarDefinitionPropertyDrawer.LevelGrammarNodeGuidToId_LUT)
            {
                options.Add(kvp.Value);
            }

            int currentIndex = 0;

            if (entryId != Guid.Empty)
            {
                if (LevelGrammarDefinitionPropertyDrawer.LevelGrammarNodeGuidToId_LUT.TryGetValue(entryId, out string entryIdString))
                {
                    currentIndex = options.IndexOf(entryIdString);
                }
            }

            //could set a label with the index ID here
            string labelText = label.text;
            if (Guid.TryParse(labelText, out _))
            {
                labelText = "ID";
            }

            currentIndex = EditorGUI.Popup(position, labelText, currentIndex, options.ToArray());
            string selectedName;

            if (currentIndex < 0)
            {
                return;
            }

            selectedName = options[currentIndex];

            if (selectedName == NONE_OPTION_TEXT)
            {
                entryId = Guid.Empty;
            }
            else
            {
                entryId = LevelGrammarDefinitionPropertyDrawer.LevelGrammarNodeIdToGuid_LUT[selectedName];
                prop_Id.stringValue = entryId.ToString();
            }
        }
    }
#endif
}