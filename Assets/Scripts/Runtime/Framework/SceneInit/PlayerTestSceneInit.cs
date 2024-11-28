using JZK.Gameplay;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace JZK.Framework
{
	public class PlayerTestSceneInit : SceneInit
	{

		ISystemReference<MonoBehaviour>[] _systems = new ISystemReference<MonoBehaviour>[]
		{
			new SystemReference<Camera.CameraSystem>(),

			new SystemReference<Input.InputSystem>(),

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

		[SerializeField] RoomController _testingRoom;

		[SerializeField] TileBase _theme1WallTile;
		[SerializeField] TileBase _theme1FloorTile;

		[SerializeField] Transform _playerSpawnPoint;

		public void Start()
		{
			_isTestScene = true;

			Setup(_systems);

			InitialiseSeed();

			_testingRoom.RepaintFloorTiles(_theme1FloorTile);
			_testingRoom.RepaintWallTiles(_theme1WallTile);

			_testingRoom.Doors[0].SetDoorEnabled(true);
			_testingRoom.Doors[1].SetDoorEnabled(false);
            _testingRoom.Doors[2].SetDoorEnabled(false);
            _testingRoom.Doors[3].SetDoorEnabled(true);

			_testingRoom.Doors[0].SetIsOpen(true);
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
	}
}