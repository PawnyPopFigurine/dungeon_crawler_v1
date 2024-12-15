using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace JZK.Gameplay
{
    public class RoomController : MonoBehaviour
    {
        [SerializeField] string _id;
        public string Id => _id;
        public int HashedId => Animator.StringToHash(_id);

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

        bool _hasInitialised;

        public void InitialiseOnLoad()
		{
            Initialise();
		}

        public void Initialise()
        {
            if(_hasInitialised)
			{
                return;
			}

            _hasInitialised = true;

            _currentFloorTile = _initialFloorTile;
            _currentWallTile = _initialWallTile;

            _hasClearedRoom = false;

            _debugRoomCentreSprite.enabled = false;

            _floorTilemap.CompressBounds();
            _wallTilemap.CompressBounds();
            foreach(Tilemap fillInWall in _fillInWalls)
			{
                fillInWall.CompressBounds();
			}
        }

        public List<(int, int)> GetFloorNodePositions(Vector3Int roomCentre)
		{
            List<(int, int)> floorNodes = new();
            BoundsInt floorBounds = _floorTilemap.cellBounds;
            TileBase[] floorTiles = _floorTilemap.GetTilesBlock(floorBounds);
            for (int x = 0; x < floorBounds.size.x; ++x)
            {
                for (int y = 0; y < floorBounds.size.y; ++y)
                {
                    TileBase tile = floorTiles[x + y * floorBounds.size.x];
                    if (null != tile)
                    {
                        Debug.Log("Has floor tile at LOCAL position X " + x.ToString() + ", Y " + y.ToString() + " - GRID position X " + (x + floorBounds.x + roomCentre.x).ToString() + ", Y " + (y + floorBounds.y + roomCentre.y).ToString());
                        floorNodes.Add((x + floorBounds.x + roomCentre.x, y + floorBounds.y + roomCentre.y));
                    }
                }
            }

            return floorNodes;
        }

        public List<(int, int)> GetWallNodePositions(Vector3Int roomCentre)
		{
            List<(int, int)> wallNodes = new();
            BoundsInt wallBounds = _wallTilemap.cellBounds;
            TileBase[] wallTiles = _wallTilemap.GetTilesBlock(wallBounds);
            for (int x = 0; x < wallBounds.size.x; ++x)
            {
                for (int y = 0; y < wallBounds.size.y; ++y)
                {
                    TileBase tile = wallTiles[x + y * wallBounds.size.x];
                    if (null != tile)
                    {
                        Debug.Log("Has wall tile at LOCAL position X " + x.ToString() + ", Y " + y.ToString() + " - GRID position X " + (x + wallBounds.x + roomCentre.x).ToString() + ", Y " + (y + wallBounds.y + roomCentre.y).ToString());
                        wallNodes.Add((x + wallBounds.x + roomCentre.x, y + wallBounds.y + roomCentre.y));
                    }
                }
            }

            return wallNodes;
        }

        public void PrintFloorNodePositions(Vector3Int roomCentre)
		{
            BoundsInt floorBounds = _floorTilemap.cellBounds;
            TileBase[] floorTiles = _floorTilemap.GetTilesBlock(floorBounds);
            for(int x = 0; x < floorBounds.size.x; ++x)
			{
                for(int y = 0; y < floorBounds.size.y; ++y)
				{
                    TileBase tile = floorTiles[x + y * floorBounds.size.x];
                    if(null != tile)
					{
                        Debug.Log("Has floor tile at LOCAL position X " + x.ToString() + ", Y " + y.ToString() + " - GRID position X " + (x + floorBounds.x + roomCentre.x).ToString() + ", Y " + (y + floorBounds.y + roomCentre.y).ToString());
					}
				}
			}
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

        public void EnableAllDoors()
        {
            foreach(RoomDoor door in _doors)
            {
                door.SetDoorEnabled(true);
            }
        }

        public void DisableAllDoors()
        {
            foreach (RoomDoor door in _doors)
            {
                door.SetDoorEnabled(false);
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

        public void ResetController()
		{
            //TODO: stuff here
		}
    }
}