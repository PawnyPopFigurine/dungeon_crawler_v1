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
			new SystemReference<Gameplay.ItemLoadSystem>(),
			new SystemReference<Gameplay.ItemPoolingSystem>(),
			new SystemReference<Gameplay.PlayerSystem>(),
			new SystemReference<Gameplay.ProjectileSystem>(),

			new SystemReference<Level.RoomDefinitionLoadSystem>(),

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

			_testingRooms[0].EnableAllDoors();
			_testingRooms[1].TrySetDoorEnabledOnSide(EOrthogonalDirection.Left, true, out _);
			_testingRooms[2].TrySetDoorEnabledOnSide(EOrthogonalDirection.Down, true, out _);
			_testingRooms[3].TrySetDoorEnabledOnSide(EOrthogonalDirection.Right, true, out _);
			_testingRooms[4].TrySetDoorEnabledOnSide(EOrthogonalDirection.Up, true, out _);
			_testingRooms[4].TrySetDoorEnabledOnSide(EOrthogonalDirection.Up, true, out _);
			_testingRooms[5].TrySetDoorEnabledOnSide(EOrthogonalDirection.Down, true, out _);

			if (!_testingRooms[0].TryLinkToRoom(_testingRooms[1], EOrthogonalDirection.Right))
			{
				//complain here
			}

			if(!_testingRooms[0].TryLinkToRoom(_testingRooms[2], EOrthogonalDirection.Up))
			{
				//complain here
			}

			if(!_testingRooms[0].TryLinkToRoom(_testingRooms[3], EOrthogonalDirection.Left))
			{
				//complain
			}

			if(!_testingRooms[0].TryLinkToRoom(_testingRooms[4], EOrthogonalDirection.Down))
			{
				//complain
			}

			if(!_testingRooms[4].TryLinkToRoom(_testingRooms[5], EOrthogonalDirection.Up))
			{
				//complain
			}

			foreach(var room in _testingRooms)
			{
				room.SetEnemyCallbacks();
			}

			GameplaySystem.Instance.Debug_SetActiveRoomList(_testingRooms);
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

		/*public void OpenAllRoomDoors()
		{
			foreach(RoomController room in _testingRooms)
			{
				room.OpenAllDoors();
			}
		}*/
	}
}