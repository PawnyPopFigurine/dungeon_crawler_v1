using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using JZK.Framework;

[CustomEditor(typeof(RandomWalkTestSceneInit))]
public class DungeonGenerationTestSceneInitEditor : Editor
{
    RandomWalkTestSceneInit _sceneInit;

    private void Awake()
    {
        _sceneInit = (RandomWalkTestSceneInit)target;
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
