using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JZK.Gameplay;
using System;

namespace JZK.Level
{
    [CreateAssetMenu(fileName = "LevelGrammarDefinition", menuName = "LevelData/Level Grammar")]
    [System.Serializable]
    public class LevelGrammarDefinitionSO : ScriptableObject
    {
        [SerializeField]
        private LevelGrammarDefinition _definition;
        public LevelGrammarDefinition Definition => _definition;

        /*private void Awake()
        {
            _definition.InitialiseOnAwake();
        }*/

        
    }

    [System.Serializable]
    public class LevelGrammarDefinition
    {
        [SerializeField]
        string _id;
        public string Id => _id;

        [SerializeField]
        ELevelTheme _baseLevelTheme;
        public ELevelTheme ELevelTheme => _baseLevelTheme;

        [SerializeField] List<LevelGrammarNodeDefinition> _nodes = new();
        public List<LevelGrammarNodeDefinition> Nodes => _nodes;

        private Dictionary<Guid, LevelGrammarNodeDefinition> _nodes_LUT = new();
        public Dictionary<Guid, LevelGrammarNodeDefinition> Nodes_LUT => _nodes_LUT;

        public void Initialise()
        {
            _nodes_LUT.Clear();

            foreach(LevelGrammarNodeDefinition node in _nodes)
            {
                _nodes_LUT.Add(node.NodeGuid, node);
            }
        }
    }

    [System.Serializable]
    public class LevelGrammarNodeDefinition
    {
        [SerializeField] string _nodeId;
        public string Id => _nodeId;

        [SerializeField][HideInInspector] string _guidString = Guid.NewGuid().ToString();
        public Guid NodeGuid => string.IsNullOrEmpty(_guidString) ? Guid.Empty : Guid.Parse(_guidString);

#if UNITY_EDITOR
        public void ValidateData(List<Guid> existingGuids)
        {
            if (existingGuids.Contains(NodeGuid))
            {
                _guidString = Guid.NewGuid().ToString();
            }

            else
            {
                if (string.IsNullOrEmpty(_guidString))
                {
                    _guidString = Guid.NewGuid().ToString();
                }
            }

            existingGuids.Add(NodeGuid);
        }
#endif

        [SerializeField] string _roomId;
        public string RoomId => _roomId;

        [SerializeField] ERoomType _roomType;
        public ERoomType RoomType => _roomType;
        [SerializeField] ELevelTheme _overrideTheme;
        public ELevelTheme OverrideTheme => _overrideTheme;

        [SerializeField] List<FixedEnemySpawnDataItem> _fixedEnemySpawns = new();
        public List<FixedEnemySpawnDataItem> FixedEnemySpawns => _fixedEnemySpawns;

        [SerializeField] RandomEnemySpawnData _randomEnemySpawnData;

        [SerializeField] List<RoomLinkData> _roomLinkData = new();
        public List<RoomLinkData> RoomLinkData => _roomLinkData;

    }

    [System.Serializable]
    public class FixedEnemySpawnDataItem
    {
        [SerializeField] string _enemyId;
        public string EnemyId => _enemyId;

        [SerializeField] bool _useFixedCoords;
        public bool UseFixedCoords => _useFixedCoords;

        [SerializeField] Vector2 _fixedCoords;
        public Vector2 FixedCoords => _fixedCoords;
    }

    [System.Serializable]
    public class RandomEnemySpawnData
    {
        [SerializeField] int _difficultyPoints; //TODO: Give min/max values
        public int DifficultyPoints => _difficultyPoints;

        [SerializeField] int _enemyCount;   //TODO: min/max vals
        public int EnemyCount => _enemyCount;

        [SerializeField] List<string> _includeIds = new();
        public List<string> IncludeIds => _includeIds;

        [SerializeField] List<ELevelTheme> _includeThemes = new();
        public List<ELevelTheme> IncludeThemes => _includeThemes;

        [SerializeField] List<ELevelTheme> _excludeThemes = new();
        public List<ELevelTheme> ExcludeThemes => _excludeThemes;
    }

    [System.Serializable]
    public class RoomLinkData
    {
        [SerializeField] LevelGrammarNodeReference _linkToNode;
        public LevelGrammarNodeReference LinkToNode => _linkToNode;
        /*[SerializeField] string _linkNodeId;
        public string LinkNodeId => _linkNodeId;*/

        [SerializeField] bool _useFixedSide;
        public bool UseFixedSide => _useFixedSide;

        [SerializeField] EOrthogonalDirection _fixedSide;
        public EOrthogonalDirection FixedSide => _fixedSide;
    }

    [System.Serializable]
    public class LevelGrammarNodeReference
    {
        [SerializeField] string _guidString;
        public Guid Id => string.IsNullOrEmpty(_guidString) ? Guid.Empty : Guid.Parse(_guidString);

        public LevelGrammarNodeReference(LevelGrammarNodeReference copyFrom)
        {
            _guidString = copyFrom._guidString;
        }
    }
}