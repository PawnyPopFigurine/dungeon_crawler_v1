using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JZK.Gameplay;
using System;
using Unity.VisualScripting;

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
        bool _useFixedSeed;
        public bool UseFixedSeed => _useFixedSeed;

        [SerializeField]
        int _fixedSeed;
        public int FixedSeed => _fixedSeed;

        [SerializeField]
        ELevelTheme _baseLevelTheme;
        public ELevelTheme BaseLevelTheme => _baseLevelTheme;

        [SerializeField] private List<LevelGrammarNodeDefinition> _nodes = new();
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

        public RoomLinkData GetOppositeLinkData(RoomLinkData inData, LevelGrammarNodeDefinition inDataParent)
        {
            bool foundLink = false;

            foreach(LevelGrammarNodeDefinition node in _nodes)
            {
                if(foundLink)
                {
                    continue;
                }

                if(node.NodeGuid != inData.LinkToNode.Id)
                {
                    continue;
                }

                foreach(RoomLinkData linkData in node.RoomLinkData)
                {
                    if(linkData.LinkToNode.Id != inDataParent.NodeGuid)
                    {
                        continue;
                    }
                    foundLink = true;
                    return linkData;
                }
            }
            return null;
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

        [SerializeField] bool _useFixedId;
        public bool UseFixedId => _useFixedId;

        [SerializeField] string _fixedId;
        public string FixedId => _fixedId;

        [SerializeField] bool _useFixedRoomType;
        public bool UseFixedRoomType => _useFixedRoomType;

        [SerializeField] ERoomType _fixedRoomType;
        public ERoomType FixedRoomType => _fixedRoomType;

        [SerializeField] bool _useOverrideTheme;
        public bool UseOverrideTheme => _useOverrideTheme;

        [SerializeField] ELevelTheme _overrideTheme;
        public ELevelTheme OverrideTheme => _overrideTheme;

        [SerializeField] List<FixedEnemySpawnDataItem> _fixedEnemySpawns = new();
        public List<FixedEnemySpawnDataItem> FixedEnemySpawns => _fixedEnemySpawns;

        [SerializeField] bool _spawnRandomEnemies;
        public bool SpawnRandomEnemies => _spawnRandomEnemies;

        [SerializeField] RandomEnemySpawnData _randomEnemySpawnData;
        public RandomEnemySpawnData RandomEnemySpawnData => _randomEnemySpawnData;

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

        [SerializeField] Vector2Int _fixedCoords;
        public Vector2Int FixedCoords => _fixedCoords;
    }

    [System.Serializable]
    public enum EEnemyPoolRestrictType
	{
        None = 0,
        IDs = 1,
        LevelThemes = 2,
	}

    [System.Serializable]
    public class RandomEnemySpawnData
    {
        [SerializeField] int _difficultyPoints; //TODO: Give min/max values
        public int DifficultyPoints => _difficultyPoints;

        [SerializeField] EEnemyPoolRestrictType _poolRestrictType;
        public EEnemyPoolRestrictType RestrictType => _poolRestrictType;

        [SerializeField] List<string> _poolIds = new();
        public List<string> IncludeIds => _poolIds;

        [SerializeField] List<ELevelTheme> _poolThemes = new();
        public List<ELevelTheme> IncludeThemes => _poolThemes;

        [SerializeField] List<string> _excludeIds = new();
        public List<string> ExcludeIds => _excludeIds;
    }

    [System.Serializable]
    public class RoomLinkData
    {
        [SerializeField] LevelGrammarNodeReference _linkToNode;
        public LevelGrammarNodeReference LinkToNode => _linkToNode;

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