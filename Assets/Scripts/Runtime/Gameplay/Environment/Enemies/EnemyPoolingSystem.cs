using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JZK.Framework;

namespace JZK.Gameplay
{
    public class EnemyPoolingSystem : PersistentSystem<EnemyPoolingSystem>
    {
        private SystemLoadData _loadData = new SystemLoadData()
        {
            LoadStates = SystemLoadState.NoLoadingNeeded,
            UpdateAfterLoadingState = ELoadingState.Game
        };

        public override SystemLoadData LoadData => _loadData;


        public static int MAX_ENEMIES_PER_TYPE = 25;
        public Vector2 ENEMY_RESET_POS => transform.position;

        private Dictionary<string, List<EnemyController>> _enemiesPool = new();
        private List<EnemyController> _activeEnemies = new();

        private List<string> _allEnemyIds = new();

        public void CreateEnemyPoolFromDef(EnemyDefinition def)
        {
            GameObject prefab = def.EnemyPrefab;

            if(prefab == null)
            {
                //complain
                return;
            }

            string enemyId = def.Id;
            List<EnemyController> enemiesOfId = new(MAX_ENEMIES_PER_TYPE);
            for(int enemyIndex = 0; enemyIndex < MAX_ENEMIES_PER_TYPE; ++enemyIndex)
            {
                GameObject prefabGO = Instantiate(prefab);
                prefabGO.name = enemyId + "_" + enemyIndex.ToString();
                EnemyController controller = prefabGO.GetComponent<EnemyController>();
                enemiesOfId.Add(controller);
                prefabGO.SetActive(false);
                prefabGO.transform.SetParent(transform);
            }

            _enemiesPool.Add(enemyId, enemiesOfId);
            _allEnemyIds.Add(enemyId);
        }

        public bool RequestEnemy(string enemyId, out EnemyController controller)
        {
            controller = null;
            if(!_enemiesPool.TryGetValue(enemyId, out List<EnemyController> pool))
            {
                Debug.LogWarning("Tried to access enemy pool using invalid ID " + enemyId);
                return false;
            }

            if(pool.Count == 0)
            {
                Debug.LogWarning("Pool for enemy type " + enemyId + " is empty");
                return false;
            }

            controller = pool[0];
            pool.Remove(controller);
            _activeEnemies.Add(controller);

            return true;
        }

        public void ReturnEnemyToPool(EnemyController controller)
        {
            if(!_activeEnemies.Contains(controller))
            {
                return;
            }

            _enemiesPool[controller.DefinitionId].Add(controller);
            _activeEnemies.Remove(controller);
        }

        public void ClearEnemy(EnemyController controller)
        {
            if(! _activeEnemies.Contains(controller))
            {
                Debug.LogWarning(this.name + " - tried to clear enemy controller " + controller.DefinitionId.ToString() + " but it is not active");
                return;
            }

            controller.transform.position = ENEMY_RESET_POS;
            controller.gameObject.SetActive(false);
            controller.ResetController();
            controller.transform.parent = transform;

            ReturnEnemyToPool(controller);
        }

        public void ClearAllEnemies()
        {
            List<EnemyController> activeCache = new(_activeEnemies);

            foreach(EnemyController controller in activeCache)
            {
                ClearEnemy(controller);
            }
        }
    }
}