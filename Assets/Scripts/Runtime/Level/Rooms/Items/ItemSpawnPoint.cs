using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JZK.Gameplay
{
    public class ItemSpawnPoint : MonoBehaviour
    {
        [SerializeField]
        string _spawnItemId;
        public string SpawnItemId => _spawnItemId;

        public void SetItemId(string newId)
		{
            _spawnItemId = newId;
		}

		public void ResetSpawn()
		{
            _hasSpawnedItem = false;
		}

        public void TrySpawnItem()
		{
            if(_hasSpawnedItem)
			{
                return;
			}

            if(!ItemPoolingSystem.Instance.RequestController(_spawnItemId, out ItemController controller))
			{
                //complain here
                return;
			}

            controller.transform.SetParent(transform);
            controller.transform.localPosition = Vector3.zero;
            controller.gameObject.SetActive(true);

            _hasSpawnedItem = true;
		}

		bool _hasSpawnedItem;
        public bool HasSpawnedItem => _hasSpawnedItem;
    }
}