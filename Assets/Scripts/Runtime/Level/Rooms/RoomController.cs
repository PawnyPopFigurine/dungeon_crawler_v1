using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace JZK.Gameplay
{
    public class RoomController : MonoBehaviour
    {
        [SerializeField] Grid _grid;

        [SerializeField] Tilemap _floorTilemap;
        [SerializeField] Tilemap _wallTilemap;

        [SerializeField] TileBase _initialFloorTile;
        [SerializeField] TileBase _initialWallTile;

        private TileBase _currentFloorTile;
        private TileBase _currentWallTile;

        [SerializeField] List<RoomDoor> _doors;
        public List<RoomDoor> Doors => _doors;

        [SerializeField] List<Tilemap> _fillInWalls = new();

        bool _hasClearedRoom;
        public bool HasClearedRoom => _hasClearedRoom;

        [SerializeField] SpriteRenderer _debugRoomCentreSprite;

        public void Initialise()
        {
            _currentFloorTile = _initialFloorTile;
            _currentWallTile = _initialWallTile;

            _hasClearedRoom = false;

            _debugRoomCentreSprite.enabled = false;
        }

        public void RepaintFloorTiles(TileBase repaintToTile)
        {
            if (repaintToTile == _currentFloorTile)
            {
                Debug.LogWarning("[ROOM] tried to repaint floor tile to current tile " + _currentFloorTile.name + " - aborting action");
                return;
            }

            _floorTilemap.SwapTile(_currentFloorTile, repaintToTile);
            _currentFloorTile = repaintToTile;
        }

        public void RepaintWallTiles(TileBase repaintToTile)
        {
            if (repaintToTile == _currentWallTile)
            {
                Debug.LogWarning("[ROOM] tried to repaint wall tile to current tile " + _currentWallTile.name + " - aborting action");
                return;
            }

            _wallTilemap.SwapTile(_currentWallTile, repaintToTile);

            foreach(Tilemap fillInWall in _fillInWalls)
            {
                fillInWall.SwapTile(_currentWallTile, repaintToTile);
            }

            _currentWallTile = repaintToTile;
        }

        public void CloseAllDoors()
        {
            foreach(RoomDoor door in _doors)
            {
                if(!door.DoorEnabled)
                {
                    continue;
                }

                door.SetIsOpen(false);
            }
        }

        public void ClearRoom()
        {
            if(_hasClearedRoom)
            {
                Debug.LogWarning("[ROOM] tried to clear already claered room " + this.name + " - aborting action");
                return;
            }

            OpenAllDoors();
            _hasClearedRoom = true;
        }

        public void OpenAllDoors()
        {
            foreach(RoomDoor door in _doors)
            {
                if(!door.DoorEnabled)
                {
                    continue;
                }

                door.SetIsOpen(true);
            }
        }

        public void OnPlayerEnterRoom()
        {
            Debug.Log("[HELLO] player has entered room " + this.gameObject.name);

            GameplaySystem.Instance.OnPlayerEnterRoom(this);

            if(!_hasClearedRoom)
            {
                CloseAllDoors();
            }
        }
    }
}