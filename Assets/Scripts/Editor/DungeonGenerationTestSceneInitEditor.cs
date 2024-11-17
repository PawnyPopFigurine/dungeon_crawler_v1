using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using JZK.Framework;

[CustomEditor(typeof(DungeonGenerationTestSceneInit))]
public class DungeonGenerationTestSceneInitEditor : Editor
{
    DungeonGenerationTestSceneInit _sceneInit;

    private void Awake()
    {
        _sceneInit = (DungeonGenerationTestSceneInit)target;
    }
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if(GUILayout.Button("Clear Dungeon"))
        {
            _sceneInit.ClearTiles();
        }

        if (GUILayout.Button("Generate New Dungeon"))
        {
            _sceneInit.ClearTiles();
            _sceneInit.GenerateDungeon();
        }
    }
}
