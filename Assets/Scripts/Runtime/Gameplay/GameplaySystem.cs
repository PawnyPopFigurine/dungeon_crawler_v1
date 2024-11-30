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
            }
        }
    }
}