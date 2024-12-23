using JZK.Gameplay;
using JZK.Level;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace JZK.Framework
{
	public class LayoutGenerationTestSceneInit : SceneInit
	{
		ISystemReference<MonoBehaviour>[] _systems = new ISystemReference<MonoBehaviour>[]
		{
            new SystemReference<Camera.CameraSystem>(),

            new SystemReference<Gameplay.GameplaySystem>(),
            new SystemReference<Gameplay.PlayerSystem>(),
            new SystemReference<Gameplay.ProjectileSystem>(),

            new SystemReference<Input.InputSystem>(),

			new SystemReference<Level.DungeonLayoutGenerationSystem>(),
			new SystemReference<Level.RoomDefinitionLoadSystem>(),
			new SystemReference<Level.RoomLoadSystem>(),

            new SystemReference<UI.GameplayUISystem>(),
            new SystemReference<UI.UIStateSystem>(),

		};

		//[SerializeField] Transform _playerSpawnPoint;

		[SerializeField] private bool _useDebugSeed = false;
		[SerializeField] private int _debugSeed;

		[SerializeField] bool _printDebug;

		[SerializeField] LayoutGenerationSettings _settings;

		private int _currentDebugTileIndex;

		LayoutData _currentLayout;

		List<RoomController> _activeRoomControllers = new();

		StartPortal _currentStartPoint;



		public void Start()
		{
			Setup(_systems);

			InitialiseSeed();
		}


		private void Update()
		{
			UpdateScene();
		}

		void InitialiseSeed()
		{
			_settings.Seed = _useDebugSeed ? _debugSeed : System.DateTime.Now.Millisecond;
			UnityEngine.Random.InitState(_settings.Seed);
		}

		public void GenerateDungeon()
		{
			InitialiseSeed();

			System.Random random = new(_settings.Seed);

			LayoutData generatedLayout = DungeonLayoutGenerationSystem.Instance.GenerateDungeonLayout(_settings, random, out bool success);

			if(success)
			{
				_currentLayout = generatedLayout;
				CreateDungeonFromLayoutData();
			}
			else
			{
				Debug.LogError("[GENERATION] Layout generation failed with seed " + _settings.Seed.ToString());
			}
        }

		public void CreateDungeonFromLayoutData()
		{
			if (_currentLayout == null)
			{
				return;
			}

            float generationStartTime = Time.realtimeSinceStartup;

            Vector2 roomPrefabPos = Vector2.zero;
			int roomSpacing = 30;

			//List<RoomController> activeControllers = new();

			Dictionary<Guid, RoomController> roomController_LUT = new();

			foreach (GenerationRoomData roomData in _currentLayout.Room_LUT.Values)
			{
				if (!RoomLoadSystem.Instance.RequestRoom(roomData.PrefabId, out RoomController controller))
				{
					//complain
				}

				controller.DisableAllDoors();
				controller.gameObject.SetActive(true);
				controller.transform.position = roomPrefabPos;
				roomPrefabPos.x += roomSpacing;
				_activeRoomControllers.Add(controller);
				roomController_LUT.Add(roomData.Id, controller);

				if(RoomDefinitionLoadSystem.Instance.GetDefinition(roomData.PrefabId).RoomType == ERoomType.Start)
				{
					StartPortal portal = controller.GetComponentInChildren<StartPortal>();
					_currentStartPoint = portal;
				}
				/*if(controller.TryGetComponent(out StartPortal portal))
				{
					_currentStartPoint = portal;
				}*/
			}

			foreach (GenerationDoorData doorData in _currentLayout.Door_LUT.Values)
			{
				RoomDoor door = roomController_LUT[doorData.ParentRoomId].Doors[doorData.IndexInRoom];
				door.SetDoorEnabled(doorData.Enabled);
				Guid linkDoorId = doorData.LinkDoorId;
				if (linkDoorId != Guid.Empty)
				{
					GenerationDoorData linkDoorData = _currentLayout.Door_LUT[linkDoorId];
					RoomDoor linkDoor = roomController_LUT[linkDoorData.ParentRoomId].Doors[linkDoorData.IndexInRoom];
					door.LinkToDoor(linkDoor);
					Debug.Log("[GENERATION] RUNTIME PLACEMENT: linked door " + doorData.Id.ToString() + " to door " + linkDoorData.Id.ToString());
				}
			}

			OpenAllDoors();

            float generationEndTime = Time.realtimeSinceStartup;
            float generationTime = generationEndTime - generationStartTime;
            Debug.Log("[GENERATION] RUNTIME PLACEMENT placement took " + generationTime + " seconds to complete");
        }

		public void ClearRooms()
		{
			if (null == _currentLayout)
			{
				return;
			}

			foreach(RoomController activeRoom in _activeRoomControllers)
			{
				RoomLoadSystem.Instance.ClearRoom(activeRoom);
			}

			_activeRoomControllers.Clear();

			_currentLayout = null;
		}

		public override void LoadingStateComplete(ELoadingState state)
		{
			switch (state)
			{
				case ELoadingState.Game:
					GenerateDungeon();
					RespawnPlayer();
                    break;
			}
		}

		public void OpenAllDoors()
		{
			foreach(RoomController room in _activeRoomControllers)
			{
				room.OpenAllDoors();
			}
		}

		public void RespawnPlayer()
		{
            Gameplay.PlayerSystem.Instance.StartForPlayerTestScene(_currentStartPoint.transform);
        }
	}
}