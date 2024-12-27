using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using JZK.Framework;

[CustomEditor(typeof(LayoutGenerationTestSceneInit))]
public class LayoutGenerationTestSceneEditor : Editor
{
    LayoutGenerationTestSceneInit _sceneInit;

    private void Awake()
    {
        _sceneInit = (LayoutGenerationTestSceneInit)target;
    }
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Clear Dungeon"))
        {
            //_sceneInit.ClearTiles();
            _sceneInit.ClearRooms();
        }

        if (GUILayout.Button("Generate New Dungeon"))
        {
            //_sceneInit.ClearTiles();
            _sceneInit.ClearRooms();
            _sceneInit.ClearEnemies();
            _sceneInit.GenerateDungeon();
            _sceneInit.RespawnPlayer();
        }
    }
}
