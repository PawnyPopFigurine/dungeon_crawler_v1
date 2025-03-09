using JZK.Framework;
using JZK.Input;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JZK.Gameplay
{
    public class GameplaySystem : PersistentSystem<GameplaySystem>
    {
        private SystemLoadData _loadData = new SystemLoadData()
        {
            LoadStates = SystemLoadState.NoLoadingNeeded,
            UpdateAfterLoadingState = ELoadingState.Game
        };

        public override SystemLoadData LoadData => _loadData;

        RoomController _currentPlayerOccupiedRoom;
        public RoomController CurrentPlayerOccupiedRoom => _currentPlayerOccupiedRoom;

        private List<RoomController> _allActiveRooms = new List<RoomController>();

        private List<int> _foundKeyIndexes = new();

        public bool HasKey(int keyIndex)
        {
            return _foundKeyIndexes.Contains(keyIndex);
        }

        public void OnKeyCollected(int keyIndex)
        {
            foreach (RoomController room in _allActiveRooms)
            {
                room.OnKeyCollected(keyIndex);
            }

            _foundKeyIndexes.Add(keyIndex);
        }

    

        public void Debug_SetActiveRoomList(List<RoomController> roomList)
        {
            _allActiveRooms.Clear();
            foreach (var room in roomList)
            {
                _allActiveRooms.Add(room);
            }
        }

        public void OnPlayerEnterRoom(RoomController roomController)
        {
            if(roomController == _currentPlayerOccupiedRoom)
            {
                return;
            }

            _currentPlayerOccupiedRoom = roomController;
        }

        public override void UpdateSystem()
        {
            base.UpdateSystem();

            if(InputSystem.Instance.Debug_ClearCurrentRoomPressed)
            {
                if(_currentPlayerOccupiedRoom == null)
                {
                    return;
                }

                _currentPlayerOccupiedRoom.ClearRoom();

                OpenAllRoomDoors();
            }
        }


        public void OpenAllRoomDoors()
        {
            foreach(var room in _allActiveRooms)
            {
                room.OpenAllDoors();
            }
        }

        public void Restart()
		{
            PlayerSystem.Instance.ResetPlayerHealth();

            if (SceneInit.CurrentSceneInit is LayoutGenerationTestSceneInit layoutInit)
			{
                layoutInit.RestartLevel();
			}

            _foundKeyIndexes.Clear();
		}
    }
}