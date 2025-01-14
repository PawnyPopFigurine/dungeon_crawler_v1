using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JZK.Gameplay
{
    public class ItemSpawnPoint : MonoBehaviour
    {
        string _spawnItemId;
        public string SpawnItemId => _spawnItemId;

        public void SetItemId(string newId)
		{
            _spawnItemId = newId;
		}

        bool _hasSpawnedItem;
        public bool HasSpawnedItem => _hasSpawnedItem;
    }
}