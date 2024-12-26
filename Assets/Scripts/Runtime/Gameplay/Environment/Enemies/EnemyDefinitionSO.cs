using System.Collections;
using System.Collections.Generic;
using UnityEditor.Build.Content;
using UnityEngine;

namespace JZK.Gameplay
{
    [CreateAssetMenu(fileName = "EnemyDefinition", menuName = "LevelData/Enemy")]
    [System.Serializable]
    public class EnemyDefinitionSO : ScriptableObject
    {
        [SerializeField] EnemyDefinition _definition;
        public EnemyDefinition Definition => _definition;
    }

    [System.Serializable]
    public class EnemyDefinition
    {
        [SerializeField] string _id;
        public string Id => _id;

        [SerializeField] bool _hideInGame;
        public bool HideInGame => _hideInGame;

        [SerializeField] GameObject _enemyPrefab;
        public GameObject EnemyPrefab => _enemyPrefab;

        [SerializeField] int _difficultyPoints;
        public int DifficultyPoints => _difficultyPoints;

        [SerializeField] ELevelTheme _theme;
        public ELevelTheme Theme => _theme;

        public EnemyDefinition CreateCopy()
        {
            return new()
            {
                _id = _id,
                _hideInGame = _hideInGame,
                _enemyPrefab = _enemyPrefab,
                _difficultyPoints = _difficultyPoints,
                _theme = _theme,
            };
        }
    }
}