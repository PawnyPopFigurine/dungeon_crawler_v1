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

		List<RoomDoor> _leftSideDoors = new();
		List<RoomDoor> _rightSideDoors = new();
		List<RoomDoor> _topSideDoors = new();
		List<RoomDoor> _btmSideDoors = new();

		Guid _generationId = Guid.Empty;
		public Guid GenerationId => _generationId;

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
        }

		public void Initialise()
		{
            if (_hasInitialised)
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
			foreach (Tilemap fillInWall in _fillInWalls)
			{
				fillInWall.CompressBounds();
			}

			InitialiseDoors();
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

			_currentWallTile = repaintToTile;
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

			//OpenAllDoors();
			_hasClearedRoom = true;
		}

		public void OpenAllDoors()
		{
			foreach (RoomDoor door in _doors)
			{
				if (!door.DoorEnabled)
				{
					continue;
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

			if (!_hasClearedRoom)
			{
				CloseAllDoors();
			}
		}

		public void ResetController()
		{
			//TODO: stuff here
		}

		public bool HasEnoughDoorsOnSide(EOrthogonalDirection side, int doorCount)
		{
			List<RoomDoor> doorSideList = GetDoorListForSide(side);
			return doorSideList.Count >= doorCount;
		}
	}
}