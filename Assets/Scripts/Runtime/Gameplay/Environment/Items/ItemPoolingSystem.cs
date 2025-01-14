using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JZK.Framework;

namespace JZK.Gameplay
{
    public class ItemPoolingSystem : PersistentSystem<ItemPoolingSystem>
    {
        private SystemLoadData _loadData = new SystemLoadData()
        {
            LoadStates = SystemLoadState.NoLoadingNeeded,
            UpdateAfterLoadingState = ELoadingState.Game
        };

        public override SystemLoadData LoadData => _loadData;

        public override void UpdateSystem()
        {
            base.UpdateSystem();

            foreach (ItemController controller in _activeControllers)
            {
                controller.UpdateController(Time.deltaTime);
            }
        }


        public static int MAX_ITEMS_PER_TYPE = 50;
        public Vector2 ITEM_RESET_POS => transform.position;

        private Dictionary<string, List<ItemController>> _controllerPool = new();
        private List<ItemController> _activeControllers = new();

        private List<string> _allItemIds = new();

        public void CreatePoolFromDef(ItemDefinition def)
        {
            GameObject prefab = def.ItemPrefab;

            if (prefab == null)
            {
                //complain
                return;
            }

            string id = def.Id;
            List<ItemController> controllersOfId = new(MAX_ITEMS_PER_TYPE);
            for (int controllerIndex = 0; controllerIndex < MAX_ITEMS_PER_TYPE; ++controllerIndex)
            {
                GameObject prefabGO = Instantiate(prefab);
                prefabGO.name = id + "_" + controllerIndex.ToString();
                ItemController controller = prefabGO.GetComponent<ItemController>();
                controller.Initialise();
                controllersOfId.Add(controller);
                prefabGO.SetActive(false);
                prefabGO.transform.SetParent(transform);
            }

            _controllerPool.Add(id, controllersOfId);
            _allItemIds.Add(id);
        }

        public bool RequestController(string enemyId, out ItemController controller)
        {
            controller = null;
            if (!_controllerPool.TryGetValue(enemyId, out List<ItemController> pool))
            {
                Debug.LogWarning("Tried to access item pool using invalid ID " + enemyId);
                return false;
            }

            if (pool.Count == 0)
            {
                Debug.LogWarning("Pool for item type " + enemyId + " is empty");
                return false;
            }

            controller = pool[0];
            pool.Remove(controller);
            _activeControllers.Add(controller);

            return true;
        }

        public void ReturnControllerToPool(ItemController controller)
        {
            if (!_activeControllers.Contains(controller))
            {
                return;
            }

            _controllerPool[controller.DefinitionId].Add(controller);
            _activeControllers.Remove(controller);
        }

        public void ClearController(ItemController controller)
        {
            if (!_activeControllers.Contains(controller))
            {
                Debug.LogWarning(this.name + " - tried to clear enemy controller " + controller.DefinitionId.ToString() + " but it is not active");
                return;
            }

            controller.transform.position = ITEM_RESET_POS;
            controller.gameObject.SetActive(false);
            controller.ResetController();
            controller.transform.parent = transform;

            ReturnControllerToPool(controller);
        }

        public void ClearAllControllers()
        {
            List<ItemController> activeCache = new(_activeControllers);

            foreach (ItemController controller in activeCache)
            {
                ClearController(controller);
            }

            _activeControllers.Clear();
        }
    }
}