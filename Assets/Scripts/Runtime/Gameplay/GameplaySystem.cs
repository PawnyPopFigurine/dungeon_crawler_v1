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

                /*if(SceneInit.CurrentSceneInit is PlayerTestSceneInit playerSceneInit)
				{
                    playerSceneInit.OpenAllRoomDoors();
				}*/

                /*if(SceneInit.CurrentSceneInit is LayoutGenerationTestSceneInit layoutGenerationSceneInit)
                {
                    layoutGenerationSceneInit.OpenAllDoors();
                }*/
            }
        }

        public void OpenAllRoomDoors()
        {
            foreach(var room in _allActiveRooms)
            {
                room.OpenAllDoors();
            }
        }
    }
}