using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using JZK.Level;

namespace Levels
{
    [CustomEditor(typeof(LevelGrammarDefinitionSO))]
    public class LevelGrammarDefinitionSOEditor : Editor
    {
        static LevelGrammarDefinitionSO _asset;

        private void OnEnable()
        {
            _asset = (LevelGrammarDefinitionSO)target;
            LevelGrammarDefinitionPropertyDrawer.Definition = _asset.Definition;
        }
    }
}