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

        public const string DEFAULT_ENEMY_ID = "apple_Green";

        public EnemyDefinition GetDefinition(string id)
        {
            if(_enemyDefinition_LUT.ContainsKey(id))
            {
                return _enemyDefinition_LUT[id];
            }
            Debug.LogWarning("Failed to get enemy definition for ID " + id + " - returning null");
            return null;
        }

        public List<EnemyDefinition> GetAllDefsForTheme(ELevelTheme theme)
		{
            List<EnemyDefinition> returnList = new();

            foreach(EnemyDefinition def in _enemyDefinition_LUT.Values)
			{
                if(theme == ELevelTheme.None || def.Theme == ELevelTheme.None)
				{
                    returnList.Add(def);
				}
                else
				{
                    if(theme != def.Theme)
					{
                        continue;
					}

                    returnList.Add(def);
				}
			}

            return returnList;
		}

        public List<EnemyDefinition> GetAllDefinitionsClosestToDifficultyPoints(int points, Dictionary<string, int> enemyCountLUT, ELevelTheme theme)
        {
            List<EnemyDefinition> returnList = new();

            int closestFoundPointsVal = 1;

            foreach (EnemyDefinition definition in _enemyDefinition_LUT.Values)
            {
                if (definition.DifficultyPoints > points)
                {
                    continue;
                }

                if (definition.Theme != ELevelTheme.None)
                {
                    if (definition.Theme != theme)
                    {
                        continue;
                    }
                }

                if (enemyCountLUT.TryGetValue(definition.Id, out int count))
                {
                    if (count >= EnemyPoolingSystem.MAX_ENEMIES_PER_TYPE)
                    {
                        continue;
                    }
                }

                if(definition.DifficultyPoints < closestFoundPointsVal)
                {
                    continue;
                }

                if(definition.DifficultyPoints == closestFoundPointsVal)
                {
                    returnList.Add(definition);
                    continue;
                }

                if(definition.DifficultyPoints > closestFoundPointsVal)
                {
                    closestFoundPointsVal = definition.DifficultyPoints;
                    returnList.Clear();
                    returnList.Add(definition);
                }

                returnList.Add(definition);
            }

            return returnList;
        }

        public List<EnemyDefinition> TrimListForDifficultyPoints(List<EnemyDefinition> inList, int points)
		{
            List<EnemyDefinition> trimmedList = new(inList);

            foreach(EnemyDefinition def in inList)
			{
                if(def.DifficultyPoints > points)
				{
                    trimmedList.Remove(def);
				}
			}

            return trimmedList;
		}

        public List<EnemyDefinition> GetAllDefinitionsForDifficultyPointsAndTheme(int points, Dictionary<string, int> enemyCountLUT, ELevelTheme theme)
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
                def.Initialise();

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