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