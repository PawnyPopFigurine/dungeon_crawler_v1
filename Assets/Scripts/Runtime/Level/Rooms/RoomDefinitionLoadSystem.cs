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

        public string GetRandomRoomId(System.Random random)
        {
            int roomIdIndex = random.Next(0, _allDefinitionIds.Count);
            return _allDefinitionIds[roomIdIndex];
        }

        public RoomDefinition GetRandomDefinition(System.Random random)
        {
            string roomId = GetRandomRoomId(random);
            return _roomDefinition_LUT[roomId];
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

            foreach(RoomDefinitionSO defSO in definitionSOs)
            {
                RoomDefinition def = defSO.Definition.CreateCopy();
                if(!def.HideInGame)
                {
                    _allDefinitionIds.Add(def.Id);
                    _roomDefinition_LUT.Add(def.Id, def);
                    Debug.Log("[LOAD] added definition with ID " + def.Id + " to LUT");
                }
            }

            FinishLoading(ELoadingState.Game);

        }

        #endregion //Load
    }
}