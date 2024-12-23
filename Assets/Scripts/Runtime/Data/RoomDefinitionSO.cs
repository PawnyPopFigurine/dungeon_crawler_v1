using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JZK.Gameplay;

namespace JZK.Level
{
    [CreateAssetMenu(fileName = "RoomDefinition", menuName = "LevelData/Room")]
    [System.Serializable]
    public class RoomDefinitionSO : ScriptableObject
    {
        [SerializeField]
        private RoomDefinition _definition;
        public RoomDefinition Definition => _definition;
    }

    public enum ERoomType
    {
        None = 0,
        StandardCombat = 1,
        NonCombat = 2,
        Start = 3,
        End = 4,
    }

    [System.Serializable]
    public class RoomDefinition
    {
        [SerializeField]
        string _id;
        public string Id => _id;

        [SerializeField]
        GameObject _prefabController;
        public GameObject PrefabController => _prefabController;

        [SerializeField]
        bool _hideInGame;
        public bool HideInGame => _hideInGame;

        [SerializeField]
        ERoomType _roomType;
        public ERoomType RoomType => _roomType;
        

        public RoomDefinition CreateCopy()
        {
            return new()
            {
                _id = _id,
                _hideInGame = _hideInGame,
                _prefabController = _prefabController,
            };
        }
    }
}