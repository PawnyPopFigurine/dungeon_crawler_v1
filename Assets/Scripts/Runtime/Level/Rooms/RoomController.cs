using JZK.Level;
using System;
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
		public Tilemap FloorTilemap => _floorTilemap;

		[SerializeField] Tilemap _wallTilemap;
		public Tilemap WallTilemap => _wallTilemap;

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

		List<RoomDoor> _leftSideDoors = new();
		List<RoomDoor> _rightSideDoors = new();
		List<RoomDoor> _topSideDoors = new();
		List<RoomDoor> _btmSideDoors = new();

		Guid _generationId = Guid.Empty;
		public Guid GenerationId => _generationId;

		[SerializeField] List<EnemyController> _enemiesInRoom = new();

		List<Vector3Int> _floorTilePositions = new();
		public List<Vector3Int> FloorTilePositions => _floorTilePositions;

		Dictionary<int, List<Vector3Int>> _doormat_LUT = new(); //referencing doors by index within room#
		public Dictionary<int, List<Vector3Int>> Doormat_LUT => _doormat_LUT;

		List<Vector3Int> _floorEdgePositions = new();
		public List<Vector3Int> FloorEdgePositions => _floorEdgePositions;

		[SerializeField] List<Tilemap> _corridorFloors;
		[SerializeField] List<Tilemap> _corridorWalls;

		[SerializeField] ItemSpawnPoint _itemSpawnPoint;
		public ItemSpawnPoint ItemSpawnPoint => _itemSpawnPoint;
		
		void InitialiseDoormats()
		{
			_doormat_LUT.Clear();

			for(int doorIndex = 0; doorIndex < _doors.Count; ++doorIndex)
			{
				RoomDoor activeRoomDoor = _doors[doorIndex];

                Tilemap floorTiles = _floorTilemap;

                List<Vector3Int> doormatTiles = new();

                Vector3Int facingOffset = Vector3Int.zero;

                if (activeRoomDoor.SideOfRoom == EOrthogonalDirection.Right)
                {
                    facingOffset.x = -1;
                }

                if (activeRoomDoor.SideOfRoom == EOrthogonalDirection.Up)
                {
                    facingOffset.y = -1;
                }

                Vector3Int roomPosInt = new(
                    (int)activeRoomDoor.transform.localPosition.x,
                    (int)activeRoomDoor.transform.localPosition.y);

                roomPosInt += facingOffset;

                doormatTiles.Add(roomPosInt);

                BoundsInt doormatBounds = new();

                doormatBounds.x = roomPosInt.x;
                doormatBounds.y = roomPosInt.y;

                switch (activeRoomDoor.SideOfRoom)
                {
                    case EOrthogonalDirection.Down:
                        doormatBounds.min = new(roomPosInt.x - 2, roomPosInt.y);
                        doormatBounds.max = new(roomPosInt.x + 1, roomPosInt.y + 2);
                        break;
                    case EOrthogonalDirection.Up:
                        doormatBounds.min = new(roomPosInt.x - 2, roomPosInt.y - 2);
                        doormatBounds.max = new(roomPosInt.x + 1, roomPosInt.y);
                        break;
                    case EOrthogonalDirection.Right:
                        doormatBounds.min = new(roomPosInt.x - 2, roomPosInt.y - 2);
                        doormatBounds.max = new(roomPosInt.x, roomPosInt.y + 1);
                        break;
                    case EOrthogonalDirection.Left:
                        doormatBounds.min = new(roomPosInt.x, roomPosInt.y - 2);
                        doormatBounds.max = new(roomPosInt.x + 2, roomPosInt.y + 1);
                        break;
                }



                for (int doormatX = doormatBounds.min.x; doormatX <= doormatBounds.max.x; ++doormatX)
                {
                    for (int doormatY = doormatBounds.min.y; doormatY <= doormatBounds.max.y; ++doormatY)
                    {
                        Vector3Int doormatPos = new(doormatX, doormatY);
                        doormatTiles.Add(doormatPos);
                    }
                }

				_doormat_LUT.Add(doorIndex, doormatTiles);
            }
        }

		public int GetIndexOfDoor(RoomDoor roomDoor)
		{
			if(!_doors.Contains(roomDoor))
			{
				return -1;
			}

			return _doors.IndexOf(roomDoor);
		}

		public void InitialiseOnLoad()
		{
			Initialise();
		}

		public void InitialisePotentialSpawns()
		{
			_floorTilePositions.Clear();
			_floorEdgePositions.Clear();

            for (int floorTileX = _floorTilemap.cellBounds.xMin; floorTileX < _floorTilemap.cellBounds.xMax; ++floorTileX)
            {
                for (int floorTileY = _floorTilemap.cellBounds.yMin; floorTileY < _floorTilemap.cellBounds.yMax; ++floorTileY)
                {
                    Vector3Int floorTilePos = new(floorTileX, floorTileY);

                    if (_floorTilemap.HasTile(floorTilePos))
                    {
                        _floorTilePositions.Add(floorTilePos);

                        if (!_floorTilemap.HasTile(new(floorTileX - 1, floorTileY)))
                        {
                            _floorEdgePositions.Add(floorTilePos);
                        }

                        else if (!_floorTilemap.HasTile(new(floorTileX + 1, floorTileY)))
                        {
                            _floorEdgePositions.Add(floorTilePos);
                        }

                        else if (!_floorTilemap.HasTile(new(floorTileX, floorTileY - 1)))
                        {
                            _floorEdgePositions.Add(floorTilePos);
                        }

                        else if (!_floorTilemap.HasTile(new(floorTileX, floorTileY + 1)))
                        {
                            _floorEdgePositions.Add(floorTilePos);
                        }
                    }
                }
            }
        }

		public void InitialiseDoors()
		{
			_topSideDoors.Clear();
			_rightSideDoors.Clear();
			_btmSideDoors.Clear();
			_leftSideDoors.Clear();

            foreach (RoomDoor door in _doors)
            {
                door.Initialise();

                switch (door.SideOfRoom)
                {
                    case EOrthogonalDirection.Up:
                        _topSideDoors.Add(door);
                        break;
                    case EOrthogonalDirection.Right:
                        _rightSideDoors.Add(door);
                        break;
                    case EOrthogonalDirection.Down:
                        _btmSideDoors.Add(door);
                        break;
                    case EOrthogonalDirection.Left:
                        _leftSideDoors.Add(door);
                        break;
                }
            }

			InitialiseDoormats();
        }

		public void Initialise()
		{
            if (_hasInitialised)
            {
                return;
            }

            _hasInitialised = true;

			/*_initialFloorTile = _floorTilemap.GetTile(Vector3Int.zero);
			_initialWallTile = _floorTilemap.GetTile(Vector3Int.zero);*/

			_currentFloorTile = _initialFloorTile;
			_currentWallTile = _initialWallTile;

			_hasClearedRoom = false;

            _debugRoomCentreSprite.enabled = false;

            _floorTilemap.CompressBounds();
			_wallTilemap.CompressBounds();
			foreach (Tilemap fillInWall in _fillInWalls)
			{
				fillInWall.CompressBounds();
			}

			InitialiseDoors();
			InitialisePotentialSpawns();
		}

		public List<RoomDoor> GetDoorListForSide(EOrthogonalDirection roomSide)
		{
			switch (roomSide)
			{
				case EOrthogonalDirection.Up:
					return _topSideDoors;
				case EOrthogonalDirection.Right:
					return _rightSideDoors;
				case EOrthogonalDirection.Down:
					return _btmSideDoors;
				case EOrthogonalDirection.Left:
					return _leftSideDoors;
				default:
					return null;
			}
		}

		//public List<RoomDoor> GetAllDoorsOnSide()

		public bool TryGetDoorOnSide(EOrthogonalDirection roomSide, out RoomDoor foundDoor, bool mustBeUnlinked = false)
		{
			List<RoomDoor> doorsOnSide = GetDoorListForSide(roomSide);

			foundDoor = null;

			if (null == doorsOnSide)
			{
				return false;
			}

			if (doorsOnSide.Count == 0)
			{
				return false;
			}

			foreach (RoomDoor door in doorsOnSide)
			{
				if (mustBeUnlinked)
				{
					if (door.IsLinked)
					{
						continue;
					}
				}

				foundDoor = door;
				return true;
			}

			foundDoor = null;
			return false;
		}

		public void TrySetDoorEnabledOnSide(EOrthogonalDirection requiredSide, bool enabled, out bool success)
		{
			List<RoomDoor> doorsOnSide = GetDoorListForSide(requiredSide);

			success = false;

			if (null == doorsOnSide)
			{
				return;
			}

			if (doorsOnSide.Count == 0)
			{
				return;
			}

			foreach (RoomDoor door in doorsOnSide)
			{
				if (door.DoorEnabled == enabled)
				{
					continue;
				}

				door.SetDoorEnabled(enabled);
				success = true;
				return;
			}
		}

		public bool TryLinkToRoom(RoomController linkToRoom, EOrthogonalDirection requiredSide)
		{
			if (!TryGetDoorOnSide(requiredSide, out RoomDoor foundDoor, true))
			{
				return false;
			}

			EOrthogonalDirection oppositeSide = GameplayHelper.GetOppositeDirection(requiredSide);
			if (!linkToRoom.TryGetDoorOnSide(oppositeSide, out RoomDoor otherRoomDoor, true))
			{
				return false;
			}

			foundDoor.LinkToDoor(otherRoomDoor);
			return true;
		}

		public void RepaintFloorTiles(TileBase repaintToTile)
		{
			if (repaintToTile == _currentFloorTile)
			{
				Debug.LogWarning("[ROOM] tried to repaint floor tile to current tile " + _currentFloorTile.name + " - aborting action");
				return;
			}

			_floorTilemap.SwapTile(_currentFloorTile, repaintToTile);

			foreach(Tilemap corrdiorFloor in _corridorFloors)
			{
				corrdiorFloor.SwapTile(_currentFloorTile, repaintToTile);
			}

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

			foreach (Tilemap fillInWall in _fillInWalls)
			{
				fillInWall.SwapTile(_currentWallTile, repaintToTile);
			}

			foreach(Tilemap corridorWall in _corridorWalls)
			{
				corridorWall.SwapTile(_currentWallTile, repaintToTile);
			}

			_currentWallTile = repaintToTile;
		}

		public void RepaintDoors(Sprite openSprite, Sprite shutSprite)
		{
			foreach(RoomDoor door in _doors)
			{
				door.SetDoorSprites(openSprite, shutSprite);
			}
		}

		public bool ShouldDoorsCloseOnFirstEntry()
		{
			if(RoomDefinitionLoadSystem.Instance.GetDefinition(_id).RoomType == ERoomType.StandardCombat)
			{
				if(_enemiesInRoom.Count > 0)
				{
					return true;
				}
			}
			return false;
		}

		public void CloseAllDoors()
		{
			foreach (RoomDoor door in _doors)
			{
				if (!door.DoorEnabled)
				{
					continue;
				}

				door.SetIsOpen(false);
			}
		}

		public void ClearRoom()
		{
			if (_hasClearedRoom)
			{
				Debug.LogWarning("[ROOM] tried to clear already claered room " + this.name + " - aborting action");
				return;
			}

			_hasClearedRoom = true;
		}

		public void OnKeyCollected(int keyIndex)
		{
			foreach(RoomDoor door in _doors)
			{
				if(!door.DoorEnabled)
				{
					continue;
				}

				door.OnKeyCollected(keyIndex);
			}
		}

		public void OpenAllDoors(bool includeLocked = false)
		{
			foreach (RoomDoor door in _doors)
			{
				if (!door.DoorEnabled)
				{
					continue;
				}

				if(door.LockedByKey && !includeLocked)
				{
					if(!GameplaySystem.Instance.HasKey(door.KeyIndex))
					{
                        continue;
                    }
				}

				door.SetIsOpen(true);
			}
		}

		public void EnableAllDoors()
		{
			foreach (RoomDoor door in _doors)
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

			ERoomType roomType = RoomDefinitionLoadSystem.Instance.GetDefinition(_id).RoomType;
			switch(roomType)
			{
				case ERoomType.StandardCombat:
					if (!_hasClearedRoom)
					{
						if (ShouldDoorsCloseOnFirstEntry())
						{
							CloseAllDoors();

							foreach (EnemyController enemy in _enemiesInRoom)
							{
								enemy.OnRoomEntered();
							}
						}
					}
					break;
				case ERoomType.StandardItem:
					if(!_hasClearedRoom)
					{
						_itemSpawnPoint.TrySpawnItem();
					}
					ClearRoom();
					break;
			}
		}

		public void ResetController()
		{
			foreach(EnemyController enemy in _enemiesInRoom)
			{
				enemy.OnEnemyKilled -= OnRoomEnemyKilled;
			}

			_enemiesInRoom.Clear();
			_hasClearedRoom = false;

			if(null != _itemSpawnPoint)
			{
				_itemSpawnPoint.ResetSpawn();
			}
			//TODO: stuff here
		}

		public bool HasEnoughDoorsOnSide(EOrthogonalDirection side, int doorCount)
		{
			List<RoomDoor> doorSideList = GetDoorListForSide(side);
			return doorSideList.Count >= doorCount;
		}

		public bool HasExactDoorsOnSide(EOrthogonalDirection side, int doorCount)
		{
			List<RoomDoor> doorSideList = GetDoorListForSide(side);
			return doorSideList.Count == doorCount;
		}

		public void SetEnemyCallbacks()
		{
			foreach(EnemyController enemy in _enemiesInRoom)
			{
				enemy.OnEnemyKilled -= OnRoomEnemyKilled;
				enemy.OnEnemyKilled += OnRoomEnemyKilled;
			}
		}

		public void SetEnemies(List<EnemyController> enemies)
		{
			_enemiesInRoom.Clear();
			_enemiesInRoom = new(enemies);
			SetEnemyCallbacks();
		}

		public void OnRoomEnemyKilled()
		{
			bool allEnemiesKilled = true;

			foreach(EnemyController enemy in _enemiesInRoom)
			{
				if(enemy.IsAlive)
				{
					allEnemiesKilled = false;
					break;
				}
			}

			if(allEnemiesKilled)
			{
				ClearRoom();
				GameplaySystem.Instance.OpenAllRoomDoors();
			}
		}
	}
}