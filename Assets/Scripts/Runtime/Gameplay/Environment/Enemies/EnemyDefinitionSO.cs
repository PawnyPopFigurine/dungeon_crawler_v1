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
    public class EnemyDefinition : WeightedListItem
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

        private List<Vector3Int> _occupyPoints = new();
        public List<Vector3Int> OccupyPoints => _occupyPoints;

        public void Initialise()
        {
            _occupyPoints.Clear();

            EnemyController controller = _enemyPrefab.GetComponent<EnemyController>();
            for(int x = controller.OccupyTiles.cellBounds.xMin; x < controller.OccupyTiles.cellBounds.xMax; ++x)
            {
                for(int y = controller.OccupyTiles.cellBounds.yMin; y < controller.OccupyTiles.cellBounds.yMax; ++y)
                {
                    Vector3Int occupyPos = new(x, y);
                    if(controller.OccupyTiles.HasTile(occupyPos))
                    {
                        _occupyPoints.Add(occupyPos);
                    }
                }
            }
        }

        public EnemyDefinition CreateCopy()
        {
            EnemyDefinition copy = new()
            {
                _id = _id,
                _hideInGame = _hideInGame,
                _enemyPrefab = _enemyPrefab,
                _difficultyPoints = _difficultyPoints,
                _theme = _theme,
                _occupyPoints = _occupyPoints,
            };

            copy.SetWeighting(Weighting);

            return copy;
        }
    }
}