using JZK.Gameplay;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

namespace JZK.Framework
{
	public class PlayerTestSceneInit : SceneInit
	{

		ISystemReference<MonoBehaviour>[] _systems = new ISystemReference<MonoBehaviour>[]
		{
			new SystemReference<Camera.CameraSystem>(),

			new SystemReference<Input.InputSystem>(),

			new SystemReference<Gameplay.GameplaySystem>(),
			new SystemReference<Gameplay.PlayerSystem>(),
			new SystemReference<Gameplay.ProjectileSystem>(),

			new SystemReference<UI.GameplayUISystem>(),
			new SystemReference<UI.PressStartUISystem>(),
			new SystemReference<UI.UIStateSystem>(),

		};

		[SerializeField] private bool _useDebugSeed = false;
		[SerializeField] private int _debugSeed;
		private int _currentSeed;
		public int CurrentSeed => _currentSeed;

		[SerializeField] List<RoomController> _testingRooms;

		[SerializeField] TileBase _theme1WallTile;
		[SerializeField] TileBase _theme1FloorTile;

		[SerializeField] Transform _playerSpawnPoint;

		public void Start()
		{
			_isTestScene = true;

			Setup(_systems);

			InitialiseSeed();

			foreach(RoomController room in _testingRooms)
			{
                room.Initialise();

                room.RepaintFloorTiles(_theme1FloorTile);
                room.RepaintWallTiles(_theme1WallTile);

				room.DisableAllDoors();
				/*room.EnableAllDoors();

                room.OpenAllDoors();*/
            }

			RoomDoor room1RightDoor = _testingRooms[0].Doors[3];
			RoomDoor room2LeftDoor = _testingRooms[1].Doors[2];

			RoomDoor room1TopDoor = _testingRooms[0].Doors[0];
			RoomDoor room3BottomDoor = _testingRooms[2].Doors[1];

			room1RightDoor.SetDoorEnabled(true);
			room2LeftDoor.SetDoorEnabled(true);

			room1TopDoor.SetDoorEnabled(true);
			room3BottomDoor.SetDoorEnabled(true);

			room1RightDoor.LinkToDoor(room2LeftDoor);
			room1TopDoor.LinkToDoor(room3BottomDoor);
        }

		private void Update()
		{
			UpdateScene();
		}

		void InitialiseSeed()
		{
			_currentSeed = _useDebugSeed ? _debugSeed : System.DateTime.Now.Millisecond;
			UnityEngine.Random.InitState(_currentSeed);
		}

		public override void LoadingStateComplete(ELoadingState state)
		{
			base.LoadingStateComplete(state);

			if(state != ELoadingState.Game)
			{
				return;
			}

			Gameplay.PlayerSystem.Instance.StartForPlayerTestScene(_playerSpawnPoint);
		}

		public void OpenAllRoomDoors()
		{
			foreach(RoomController room in _testingRooms)
			{
				room.OpenAllDoors();
			}
		}
	}
}