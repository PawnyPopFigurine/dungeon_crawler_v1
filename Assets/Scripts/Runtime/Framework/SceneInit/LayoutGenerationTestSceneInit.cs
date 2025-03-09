using JZK.Gameplay;
using JZK.Level;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace JZK.Framework
{
	public enum EGenerationMode
	{
		None,
		FromSettings,
		FromGrammar,
	}

	public class LayoutGenerationTestSceneInit : SceneInit
	{
		ISystemReference<MonoBehaviour>[] _systems = new ISystemReference<MonoBehaviour>[]
		{
            new SystemReference<Camera.CameraSystem>(),

			new SystemReference<Gameplay.EnemyLoadSystem>(),
			new SystemReference<Gameplay.EnemyPoolingSystem>(),
            new SystemReference<Gameplay.GameplaySystem>(),
			new SystemReference<Gameplay.ItemLoadSystem>(),
			new SystemReference<Gameplay.ItemPoolingSystem>(),
			new SystemReference<Gameplay.PlayerSystem>(),
            new SystemReference<Gameplay.ProjectileSystem>(),
			new SystemReference<Gameplay.ThemeDataLoadSystem>(),

            new SystemReference<Input.InputSystem>(),

			new SystemReference<Level.DungeonLayoutGenerationSystem>(),
			new SystemReference<Level.RoomDefinitionLoadSystem>(),
			new SystemReference<Level.RoomLoadSystem>(),

            new SystemReference<UI.GameplayUISystem>(),
            new SystemReference<UI.UIStateSystem>(),

		};

		[SerializeField] private bool _useDebugSeed = false;
		[SerializeField] private int _debugSeed;

		[SerializeField] bool _printDebug;

		[SerializeField] EGenerationMode _generationMode;

		[SerializeField] LevelGrammarDefinitionSO _grammarDef;

		[SerializeField] LayoutGenerationSettings _settings;

		private int _currentDebugTileIndex;

		LayoutData _currentLayout;

		List<RoomController> _activeRoomControllers = new();

		StartPortal _currentStartPoint;

		[SerializeField] TileBase _noEnemySpawnTile;

		[SerializeField] bool _paintNoEnemySpawnTiles;

		[SerializeField] EnemySpawnData _debugSpawnData;

		[SerializeField] bool _testSpawnEnemies;

		Dictionary<Guid, RoomController> _generationData_Controller_LUT = new();

		[SerializeField] bool _paintFloorEdges;
		[SerializeField] bool _playerInvincible;

		public void Start()
		{
			Setup(_systems);

			InitialiseSeed();

			_grammarDef.Definition.Initialise();
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

			switch(_generationMode)
			{
				case EGenerationMode.FromSettings:
					{
                        System.Random random = new(_settings.Seed);

                        LayoutData generatedLayout = DungeonLayoutGenerationSystem.Instance.GenerateDungeonLayout(_settings, random, out bool success);

                        if (success)
                        {
                            _currentLayout = generatedLayout;
                            CreateDungeonFromData(_currentLayout);
                        }
                        else
                        {
                            Debug.LogError("[GENERATION] Layout generation failed with seed " + _settings.Seed.ToString());
                        }
                    }
                    
                    break;
				case EGenerationMode.FromGrammar:
					{
                        int grammarSeed = DateTime.Now.Millisecond;
                        if (_grammarDef.Definition.UseFixedSeed)
                        {
                            grammarSeed = _grammarDef.Definition.FixedSeed;
                        }

                        System.Random random = new(grammarSeed);
						Debug.Log("[GENERATION] Grammar seed " + grammarSeed);

						LayoutData generatedLayout = DungeonLayoutGenerationSystem.Instance.GenerateDungeonLayout(_grammarDef.Definition, random, out bool success);
                        if (success)
                        {
                            _currentLayout = generatedLayout;
                            CreateDungeonFromData(_currentLayout);
                        }
                        else
                        {
                            Debug.LogError("[GENERATION] Layout generation failed with seed " + _settings.Seed.ToString());
                        }
                    }
					
                    break;
				default:
					Debug.LogError("[GENERATION] no generation set up for mode " + _generationMode.ToString());
					break;
			}
        }

		void CreateDungeonFromData(LayoutData data)
		{
			CreateLayoutFromData(data);
			GameplaySystem.Instance.Debug_SetActiveRoomList(_activeRoomControllers);
			GameplaySystem.Instance.OpenAllRoomDoors();
			if (_paintNoEnemySpawnTiles)
			{
				PaintNoEnemySpawnTiles();
			}
			if (_testSpawnEnemies)
			{
				SpawnTestEnemyInEachCombatRoom();
			}
			else
			{
				SpawnEnemiesFromData(data);
			}

			if (_paintFloorEdges)
			{
				PaintFloorEdges();
			}

			SpawnItemsFromData(data);
		}

		void PaintFloorEdges()
		{
			foreach(RoomController controller in _activeRoomControllers)
			{
				foreach(Vector3Int edgePos in controller.FloorEdgePositions)
				{
					controller.FloorTilemap.SetTile(edgePos, _noEnemySpawnTile);
				}
			}
		}

		public void SpawnItemsFromData(LayoutData data)
		{
			if(data == null)
			{
				return;
			}

			foreach (Guid id in data.Room_LUT.Keys)
			{
				RoomController controller = _generationData_Controller_LUT[id];
				GenerationRoomData roomData = data.Room_LUT[id];

				foreach(ItemSpawnData itemSpawnData in roomData.ItemSpawnData)
				{
					if(!ItemPoolingSystem.Instance.RequestController(itemSpawnData.ItemId, out ItemController itemController))
					{
						//complainHere
						continue;
					}

					ItemSpawnPoint spawnPoint = controller.ItemSpawnPoint;

					itemController.SetItemIndex(itemSpawnData.ItemIndex);
					itemController.transform.parent = spawnPoint.transform;
					itemController.transform.localPosition = Vector3.zero;
					itemController.gameObject.SetActive(true);
				}
			}
		}

		public void SpawnEnemiesFromData(LayoutData data)
		{
			if(data == null)
			{
				return;
			}

			foreach(Guid id in data.Room_LUT.Keys)
			{
				RoomController controller = _generationData_Controller_LUT[id];
				GenerationRoomData roomData = data.Room_LUT[id];

                List<EnemyController> enemyList = new();

                foreach (EnemySpawnData spawnData in roomData.EnemySpawnData)
				{
					if(!EnemyPoolingSystem.Instance.RequestEnemy(spawnData.EnemyId, out EnemyController enemy))
					{
						//complain
						Debug.LogError("[GENERATION] have exceeded pool depth for enemy " + spawnData.EnemyId);
						continue;
					}

                    if (!controller.FloorTilemap.HasTile(spawnData.FloorTilePos))
                    {
						Debug.LogWarning("[GENERATION] tried placing enemy in unreachable location " + spawnData.FloorTilePos.ToString());
                        continue;
                    }

                    enemy.transform.position = controller.FloorTilemap.CellToWorld(spawnData.FloorTilePos);
					enemy.OnLevelPlacement();
                    enemy.gameObject.SetActive(true);

					enemyList.Add(enemy);
                }

                controller.SetEnemies(enemyList);
            }
        }

		public void CreateLayoutFromData(LayoutData data)
		{
			if (data == null)
			{
				Debug.LogError("[GENERATION] tried to make layout from null data");
				return;
			}

			Debug.Log("[ROOMCOUNT] layout contains " + data.Room_LUT.Count.ToString() + " total rooms - seed " + _settings.Seed);

            float generationStartTime = Time.realtimeSinceStartup;

            Vector2 roomPrefabPos = Vector2.zero;
			int roomSpacing = 50;

			_generationData_Controller_LUT.Clear();

            foreach (GenerationRoomData roomData in data.Room_LUT.Values)
			{
				if (!RoomLoadSystem.Instance.RequestRoom(roomData.PrefabId, out RoomController controller))
				{
					//complain
				}

				ELevelTheme roomTheme = roomData.OverrideRoomTheme != ELevelTheme.None ? roomData.OverrideRoomTheme : data.Theme;

				ThemeDefinition themeDef = ThemeDataLoadSystem.Instance.ThemeData_LUT[roomTheme];

				controller.DisableAllDoors();
				controller.RepaintFloorTiles(themeDef.FloorTile);
				controller.RepaintWallTiles(themeDef.WallTile);
				controller.RepaintDoors(themeDef.OpenDoor, themeDef.ShutDoor);
				controller.gameObject.SetActive(true);
				controller.transform.position = roomPrefabPos;
				roomPrefabPos.x += roomSpacing;
				_activeRoomControllers.Add(controller);
                _generationData_Controller_LUT.Add(roomData.Id, controller);

				if(RoomDefinitionLoadSystem.Instance.GetDefinition(roomData.PrefabId).RoomType == ERoomType.Start)
				{
					StartPortal portal = controller.GetComponentInChildren<StartPortal>();
					_currentStartPoint = portal;
				}
			}

			foreach (GenerationDoorData doorData in data.Door_LUT.Values)
			{
				if(!_generationData_Controller_LUT.ContainsKey(doorData.ParentRoomId))
				{
					Debug.LogError("[GENERATION] DOOR CONTAINS OUTDATED ROOM REFERENCE");
					continue;
				}
				if(doorData.IndexInRoom < 0 || doorData.IndexInRoom >= _generationData_Controller_LUT[doorData.ParentRoomId].Doors.Count)
				{
					Debug.LogError("[GENERATION] DOOR DATA HAS INVALID INDEX " + doorData.IndexInRoom + " FOR ROOM " + _generationData_Controller_LUT[doorData.ParentRoomId].Id);
					continue;
				}

				RoomDoor door = _generationData_Controller_LUT[doorData.ParentRoomId].Doors[doorData.IndexInRoom];
				door.SetDoorEnabled(doorData.Enabled);
				door.SetKeyData(doorData.LockedByKey, doorData.KeyIndex);
				Guid linkDoorId = doorData.LinkDoorId;
				if (linkDoorId != Guid.Empty)
				{
					GenerationDoorData linkDoorData = data.Door_LUT[linkDoorId];
					RoomDoor linkDoor = _generationData_Controller_LUT[linkDoorData.ParentRoomId].Doors[linkDoorData.IndexInRoom];
					door.LinkToDoor(linkDoor);
					Debug.Log("[GENERATION] RUNTIME PLACEMENT: linked door " + doorData.Id.ToString() + " to door " + linkDoorData.Id.ToString());
				}
			}

			

            float generationEndTime = Time.realtimeSinceStartup;
            float generationTime = generationEndTime - generationStartTime;
            Debug.Log("[GENERATION] RUNTIME PLACEMENT placement took " + generationTime + " seconds to complete");
        }

		void PaintNoEnemySpawnTiles()
		{
            foreach (RoomController roomController in _activeRoomControllers)
            {
                foreach (RoomDoor activeRoomDoor in roomController.Doors)
                {
                    Tilemap floorTiles = roomController.FloorTilemap;

                    if (activeRoomDoor.DoorEnabled)
                    {
                        List<Vector3Int> tilesToPaint = new();

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

                        tilesToPaint.Add(roomPosInt);

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
                                tilesToPaint.Add(doormatPos);
                            }
                        }


                        foreach (Vector3Int pos in tilesToPaint)
                        {
                            floorTiles.SetTile(pos, _noEnemySpawnTile);
                        }
                    }
                }
            }
        }

		public void ClearLayoutData()
		{
			_currentLayout = null;
		}

		public void ClearRooms()
		{
			foreach(RoomController activeRoom in _activeRoomControllers)
			{
				RoomLoadSystem.Instance.ClearRoom(activeRoom);
			}

			_activeRoomControllers.Clear();
		}

		public void ClearEnemies()
		{
			EnemyPoolingSystem.Instance.ClearAllEnemies();
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

		public void RestartLevel()
		{
			ClearEnemies();
			ClearRooms();
			CreateDungeonFromData(_currentLayout);

			RespawnPlayer();
		}

		public void RespawnPlayer()
		{
			Gameplay.PlayerSystem.Instance.SetInvincibleCheat(_playerInvincible);
			Gameplay.PlayerSystem.Instance.StartForPlayerTestScene(_currentStartPoint.transform);
        }

		public void SpawnTestEnemyInEachCombatRoom()
		{
			foreach(RoomController activeRoom in _activeRoomControllers)
			{
				RoomDefinition roomDef = RoomDefinitionLoadSystem.Instance.GetDefinition(activeRoom.Id);
				if(roomDef.RoomType != ERoomType.StandardCombat)
				{
					continue;
				}

				EnemyDefinition testEnemyDef = EnemyLoadSystem.Instance.GetDefinition(_debugSpawnData.EnemyId);

				if(!EnemyPoolingSystem.Instance.RequestEnemy(_debugSpawnData.EnemyId, out EnemyController controller))
				{
					continue;
				}

				if(!activeRoom.FloorTilemap.HasTile(_debugSpawnData.FloorTilePos))
				{
					continue;
				}

                controller.transform.position = activeRoom.FloorTilemap.CellToWorld(_debugSpawnData.FloorTilePos);
				controller.gameObject.SetActive(true);

				List<EnemyController> enemyList = new()
				{
					controller
				};

				activeRoom.SetEnemies(enemyList);
            }
		}
	}
}