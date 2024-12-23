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

        public RoomDefinition GetRandomDefinition(System.Random random, GenerationRoomData.GenerationRoomConnectionData connectionData, out bool success)
        {
            int maxAttempts = 100;

            List<RoomDefinition> allRoomDefs = new(_roomDefinition_LUT.Values);

            for (int attempt = 0; attempt < maxAttempts; ++attempt)
            {
                int index = random.Next(allRoomDefs.Count);
                RoomDefinition considerRoom = allRoomDefs[index];
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

            Debug.LogError("[GENERATION] failed to find room matching connection data - quit after " + maxAttempts.ToString() + " attempts");
            success = false;
            return null;
        }

        /*public RoomDefinition GetRandomDefinition_RequireDoors(System.Random random, EOrthogonalDirection direction, int doorCount)
        {
            int maxAttempts = 100;

            List<RoomDefinition> allRoomDefs = new(_roomDefinition_LUT.Values);

            for(int attempt = 0; attempt < maxAttempts; ++attempt)
            {
                int index = random.Next(allRoomDefs.Count);
                RoomDefinition considerRoom = allRoomDefs[index];
                RoomController controller = considerRoom.PrefabController.GetComponent<RoomController>();
                if(!controller.HasEnoughDoorsOnSide(direction, doorCount))
                {
                    continue;
                }

                return considerRoom;

            }

            Debug.LogError("[GENERATION] failed to find room with door on side " + direction.ToString() + " - quit after " + maxAttempts.ToString() + " attempts");
            return null;
        }*/

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
                    RoomController roomController = def.PrefabController.GetComponent<RoomController>();
                    roomController.InitialiseDoors();

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