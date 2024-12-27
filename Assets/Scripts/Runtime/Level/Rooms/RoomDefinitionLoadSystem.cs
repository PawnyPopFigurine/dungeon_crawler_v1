using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JZK.Framework;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using JZK.Gameplay;
using Unity.Collections;

namespace JZK.Level
{
    public class RoomDefinitionLoadSystem : PersistentSystem<RoomDefinitionLoadSystem>
    {
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

            Addressables.LoadAssetsAsync<RoomDefinitionSO>("room_definition", LoadedAsset).Completed += LoadCompleted;
        }

        private Dictionary<string, RoomDefinition> _roomDefinition_LUT = new();

        

        private List<string> _allDefinitionIds = new();
        private List<string> _standardCombatIds = new();
        private List<string> _nonCombatIds = new();
        private List<string> _startIds = new();
        private List<string> _endIds = new();

        public RoomDefinition GetDefinition(string id)
        {
            if (_roomDefinition_LUT.TryGetValue(id, out RoomDefinition roomDef))
            {
                return roomDef;
            }
            return null;
        }

        List<string> GetListForRoomType(ERoomType roomType)
        {
            switch(roomType)
            {
                case ERoomType.StandardCombat:
                    return _standardCombatIds;
                case ERoomType.NonCombat:
                    return _nonCombatIds;
                case ERoomType.Start:
                    return _startIds;
                case ERoomType.End:
                    return _endIds;
                default:
                    return _allDefinitionIds;
            }
        }

        public RoomDefinition GetRandomDefinition(System.Random random, GenerationRoomData.GenerationRoomConnectionData connectionData, out bool success, ERoomType requiredType = ERoomType.None)
        {
            int maxAttempts = 100;

            List<string> roomIdList = GetListForRoomType(requiredType);

            for (int attempt = 0; attempt < maxAttempts; ++attempt)
            {
                int index = random.Next(roomIdList.Count);
                string considerId = roomIdList[index];
                RoomDefinition considerRoom = _roomDefinition_LUT[considerId];
                RoomController controller = considerRoom.PrefabController.GetComponent<RoomController>();
                if (!controller.HasEnoughDoorsOnSide(EOrthogonalDirection.Up, connectionData.RequiredUpConnections))
                {
                    continue;
                }
                if (!controller.HasEnoughDoorsOnSide(EOrthogonalDirection.Down, connectionData.RequiredDownConnections))
                {
                    continue;
                }
                if (!controller.HasEnoughDoorsOnSide(EOrthogonalDirection.Left, connectionData.RequiredLeftConnections))
                {
                    continue;
                }
                if (!controller.HasEnoughDoorsOnSide(EOrthogonalDirection.Right, connectionData.RequiredRightConnections))
                {
                    continue;
                }

                success = true;
                return considerRoom;

            }

            Debug.LogError("[GENERATION] failed to find room matching connection data for type - " + requiredType.ToString() + " - quit after " + maxAttempts.ToString() + " attempts");
            success = false;
            return null;
        }

        #region Load

        void LoadedAsset(RoomDefinitionSO asset)
        {

        }

        void LoadCompleted(AsyncOperationHandle<IList<RoomDefinitionSO>> assets)
        {
            if (!assets.IsDone)
            {
                //complain here
                return;
            }

            IList<RoomDefinitionSO> definitionSOs = assets.Result;
            if (definitionSOs == null)
            {
                //complain here
                return;
            }

            _allDefinitionIds.Clear();
            _roomDefinition_LUT.Clear();

            _endIds.Clear();
            _startIds.Clear();
            _standardCombatIds.Clear();
            _nonCombatIds.Clear();

            foreach(RoomDefinitionSO defSO in definitionSOs)
            {
                RoomDefinition def = defSO.Definition.CreateCopy();
                if(!def.HideInGame)
                {
                    RoomController roomController = def.PrefabController.GetComponent<RoomController>();
                    roomController.InitialiseDoors();
                    roomController.InitialisePotentialSpawns();

                    _allDefinitionIds.Add(def.Id);
                    _roomDefinition_LUT.Add(def.Id, def);
                    switch(def.RoomType)
                    {
                        case ERoomType.StandardCombat:
                            _standardCombatIds.Add(def.Id);
                            break;
                        case ERoomType.NonCombat:
                            _nonCombatIds.Add(def.Id);
                            break;
                        case ERoomType.Start:
                            _startIds.Add(def.Id);
                            break;
                        case ERoomType.End:
                            _endIds.Add(def.Id);
                            break;
                        case ERoomType.None:
                            Debug.LogWarning("Please set type enum for room " + def.Id);
                            break;
                    }
                    Debug.Log("[LOAD] added definition with ID " + def.Id + " to LUT");
                }
            }

            FinishLoading(ELoadingState.Game);

        }

        #endregion //Load
    }
}