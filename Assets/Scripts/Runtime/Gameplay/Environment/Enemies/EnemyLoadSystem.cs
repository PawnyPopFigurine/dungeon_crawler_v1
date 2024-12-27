using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JZK.Framework;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using JZK.Level;

namespace JZK.Gameplay
{
    public class EnemyLoadSystem : PersistentSystem<EnemyLoadSystem>
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

            Addressables.LoadAssetsAsync<EnemyDefinitionSO>("enemy_definition", LoadedAsset).Completed += LoadCompleted;
        }

        #endregion //PersistentSystem

        List<string> _allEnemyIds = new();
        Dictionary<string, EnemyDefinition> _enemyDefinition_LUT = new();

        public EnemyDefinition GetDefinition(string id)
        {
            if(_enemyDefinition_LUT.ContainsKey(id))
            {
                return _enemyDefinition_LUT[id];
            }
            Debug.LogWarning("Failed to get enemy definition for ID " + id + " - returning null");
            return null;
        }

        public List<EnemyDefinition> GetAllDefinitionsForDifficultyPoints(int points, Dictionary<string, int> enemyCountLUT, ELevelTheme theme)
        {
            List<EnemyDefinition> returnList = new();

            foreach(EnemyDefinition definition in _enemyDefinition_LUT.Values)
            {
                if(definition.DifficultyPoints > points)
                {
                    continue;
                }

                if(definition.Theme != ELevelTheme.None)
                {
                    if (definition.Theme != theme)
                    {
                        continue;
                    }
                }

                if(enemyCountLUT.TryGetValue(definition.Id, out int count))
                {
                    if(count >= EnemyPoolingSystem.MAX_ENEMIES_PER_TYPE)
                    {
                        continue;
                    }
                }

                returnList.Add(definition);
            }

            return returnList;
        }


        #region Load

        void LoadedAsset(EnemyDefinitionSO asset)
        {

        }

        void LoadCompleted(AsyncOperationHandle<IList<EnemyDefinitionSO>> assets)
        {
            if (!assets.IsDone)
            {
                //complain here
                return;
            }

            IList<EnemyDefinitionSO> definitionSOs = assets.Result;
            if (definitionSOs == null)
            {
                //complain here
                return;
            }

            _allEnemyIds.Clear();
            _enemyDefinition_LUT.Clear();

            foreach (EnemyDefinitionSO defSO in definitionSOs)
            {
                EnemyDefinition def = defSO.Definition.CreateCopy();

                if(!def.HideInGame)
                {
                    _allEnemyIds.Add(def.Id);
                    _enemyDefinition_LUT.Add(def.Id, def);
                    Debug.Log("[LOAD] added definition with ID " + def.Id + " to LUT");

                    EnemyPoolingSystem.Instance.CreateEnemyPoolFromDef(def);
                }
            }

            FinishLoading(ELoadingState.Game);

        }

        #endregion //Load
    }
}