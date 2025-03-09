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

        List<ItemDefinition> _randomListItems = new();


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
            List<WeightedListItem> allDefs = new(_randomListItems);
            return (ItemDefinition)GameplayHelper.GetWeightedListItem(allDefs, random);
		}


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
            _randomListItems.Clear();

            foreach (ItemDefinitionSO defSO in definitionSOs)
            {
                ItemDefinition def = defSO.Definition.CreateCopy();
                def.Initialise();

                if (!def.HideInGame)
                {
                    _allIds.Add(def.Id);
                    _definition_LUT.Add(def.Id, def);
                    if(!def.ExcludeFromRandom)
                    {
                        _randomListItems.Add(def);
                    }
                    Debug.Log("[ITEMLOAD] added definition with ID " + def.Id + " to LUT");

                    ItemPoolingSystem.Instance.CreatePoolFromDef(def);
                }
            }

            FinishLoading(ELoadingState.Game);

        }

        #endregion //Load
    }
}