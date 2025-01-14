using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JZK.Framework;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using JZK.Level;

namespace JZK.Gameplay
{
    public class ItemLoadSystem : PersistentSystem<ItemLoadSystem>
    {
        #region PersistentSystem

        private SystemLoadData _loadData = new SystemLoadData()
        {
            LoadStates = new SystemLoadState[] { new SystemLoadState { LoadStartState = ELoadingState.Game, BlockStateUntilFinished = ELoadingState.Game } },
            UpdateAfterLoadingState = ELoadingState.Game,
        };

        public override SystemLoadData LoadData
        {
            get { return _loadData; }
        }

        public override void StartLoading(ELoadingState state)
        {
            base.StartLoading(state);

            Addressables.LoadAssetsAsync<ItemDefinitionSO>("item_definition", LoadedAsset).Completed += LoadCompleted;
        }

        #endregion //PersistentSystem

        List<string> _allIds = new();
        Dictionary<string, ItemDefinition> _definition_LUT = new();

        public ItemDefinition GetDefinition(string id)
        {
            if (_definition_LUT.ContainsKey(id))
            {
                return _definition_LUT[id];
            }
            Debug.LogWarning("Failed to get item definition for ID " + id + " - returning null");
            return null;
        }

        public ItemDefinition GetRandomDefinition(System.Random random)
		{
            List<WeightedListItem> allDefs = new(_definition_LUT.Values);
            return (ItemDefinition)GameplayHelper.GetWeightedListItem(allDefs, random);
		}

        public string GetRandomId(System.Random random)
		{
            return GetRandomDefinition(random).Id;
		}

        /*public string GetRandomItemId(System.Random random)
		{
            int idIndex = random.Next(0, _allIds.Count);
            return _allIds[idIndex];
		}

        public ItemDefinition GetRandomDefinition(System.Random random)
		{
            string randomId = GetRandomItemId(random);
            return GetDefinition(randomId);
		}*/


        #region Load

        void LoadedAsset(ItemDefinitionSO asset)
        {

        }

        void LoadCompleted(AsyncOperationHandle<IList<ItemDefinitionSO>> assets)
        {
            if (!assets.IsDone)
            {
                //complain here
                return;
            }

            IList<ItemDefinitionSO> definitionSOs = assets.Result;
            if (definitionSOs == null)
            {
                //complain here
                return;
            }

            _allIds.Clear();
            _definition_LUT.Clear();

            foreach (ItemDefinitionSO defSO in definitionSOs)
            {
                ItemDefinition def = defSO.Definition.CreateCopy();
                def.Initialise();

                if (!def.HideInGame)
                {
                    _allIds.Add(def.Id);
                    _definition_LUT.Add(def.Id, def);
                    Debug.Log("[ITEMLOAD] added definition with ID " + def.Id + " to LUT");

                    ItemPoolingSystem.Instance.CreatePoolFromDef(def);
                }
            }

            FinishLoading(ELoadingState.Game);

        }

        #endregion //Load
    }
}