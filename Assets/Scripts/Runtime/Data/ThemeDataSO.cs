using JZK.Gameplay;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace JZK.Level
{
    [CreateAssetMenu(fileName = "ThemeDataSO", menuName = "LevelData/ThemeData")]

    [System.Serializable]
    public class ThemeDataSO : ScriptableObject
    {
        [SerializeField] ThemeDefinition _definition;
        public ThemeDefinition Definition => _definition;
    }

    [System.Serializable]
    public class ThemeDefinition
    {
        [SerializeField] ELevelTheme _theme;
        public ELevelTheme Theme => _theme;

        [SerializeField] TileBase _floorTile;
        public TileBase FloorTile => _floorTile;

        [SerializeField] TileBase _wallTile;
        public TileBase WallTile => _wallTile;

        public ThemeDefinition CreateCopy()
        {
            return new()
            {
                _floorTile = _floorTile,
                _wallTile = _wallTile,
            };
        }
    }
}