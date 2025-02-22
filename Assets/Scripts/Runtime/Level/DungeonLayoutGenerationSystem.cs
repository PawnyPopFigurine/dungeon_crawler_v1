using JZK.Framework;
using JZK.Gameplay;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using Unity.VisualScripting;
using UnityEngine;

namespace JZK.Level
{
	[System.Serializable]
	public enum EDifficultyPointsMode
	{
		Fixed = 0,
		Scaling = 1,
	}

	[System.Serializable]
	public class LayoutGenerationSettings
	{
		public int Seed;
		public ELevelTheme Theme;
		public int CriticalPathRoomCount;
		[Range(0, 1)]
		public float SecondaryRoomChance;
		[Tooltip("Set to 0 or lower to remove limit")]
		public int MaxTotalRooms;
		public EDifficultyPointsMode DifficultyPointsMode;
		public int FixedEnemyPointsPerRoom;
		public int ScalingEnemyPointsStartAmount;
		public int ScalingEnemyPointsScalingAmount;
		public int ScalingEnemyPointsMaxAmount;
		public bool LastRoomNoBranches;
		//public bool BSPEnemyPlacement;
		[Range(0, 1)]
		public float ItemRoomChance;
		public int MaxItemRooms;
		public bool UseRoomDefMaxPerLevel = true;
		public bool UseEnemyDefMaxPerRoom = true;
		public bool UseEnemyDefMaxPerLevel = true;
	}

	[System.Serializable]
	public class LayoutData
	{
		public Dictionary<Guid, GenerationRoomData> Room_LUT = new();
		public Dictionary<Guid, GenerationDoorData> Door_LUT = new();
		public List<Guid> CriticalPathIds = new();
		public List<Guid> SecondaryRoomIds = new();

		public ELevelTheme Theme;

		public Dictionary<string, int> RoomCount_LUT = new();
		public Dictionary<string, int> EnemyCount_LUT = new();


		public List<Guid> GetAllRoomsOfType(ERoomType type)
		{
			List<Guid> returnList = new();

			foreach (var room in Room_LUT.Values)
			{
				if (room.RoomType != type)
				{
					continue;
				}

				returnList.Add(room.Id);
			}

			return returnList;
		}
	}

	[System.Serializable]
	public class GenerationRoomData
	{
		//TODO: override theme enum
		public Guid Id = Guid.NewGuid();
		public string PrefabId = string.Empty;
		public ERoomType RoomType;
		public List<Guid> AllDoorIds = new();
		public LayoutData ParentLayout;
		public GenerationRoomConnectionData ConnectionData;
		public int CriticalPathIndex = -1;  //how far along the crit path this room is - even if it isn't directly on it
		public bool OnCriticalPath;
		public List<EnemySpawnData> EnemySpawnData = new();
		public List<Vector3Int> UnoccupiedFloorPositions = new();	//takes placed enemies/objects into account
		public List<Vector3Int> AllFloorPositions = new();
		public List<Vector3Int> AllFloorEdgePositions = new();
		public List<List<Vector3Int>> BSPDividedFloorPositions = new();
		public List<string> SpawnItemIds = new();
		public ELevelTheme OverrideRoomTheme;
		public Guid GrammarNodeId = Guid.Empty;

		public Dictionary<string, int> EnemyCount_LUT = new();

		public class GenerationRoomConnectionData
		{
			public int RequiredUpConnections = 0;
			public int RequiredDownConnections = 0;
			public int RequiredLeftConnections = 0;
			public int RequiredRightConnections = 0;

			public void SetConnectionCount(EOrthogonalDirection direction, int count)
			{
				switch (direction)
				{
					case EOrthogonalDirection.Up:
						RequiredUpConnections = count;
						break;
					case EOrthogonalDirection.Right:
						RequiredRightConnections = count;
						break;
					case EOrthogonalDirection.Down:
						RequiredDownConnections = count;
						break;
					case EOrthogonalDirection.Left:
						RequiredLeftConnections = count;
						break;
				}
			}

			public int GetRequirementsInDirection(EOrthogonalDirection direction)
			{
				switch (direction)
				{
					case EOrthogonalDirection.Up:
						return RequiredUpConnections;
					case EOrthogonalDirection.Right:
						return RequiredRightConnections;
					case EOrthogonalDirection.Down:
						return RequiredDownConnections;
					case EOrthogonalDirection.Left:
						return RequiredLeftConnections;
					default:
						return -1;
				}
			}
		}

		public List<EOrthogonalDirection> GetAvailableSidesForConnection()
		{
			List<EOrthogonalDirection> returnList = new();

			foreach (Guid doorId in AllDoorIds)
			{
				GenerationDoorData doorData = ParentLayout.Door_LUT[doorId];
				if (doorData.IsLinked)
				{
					continue;
				}

				returnList.Add(doorData.SideOfRoom);
			}

			return returnList;
		}

		public bool CanPlaceEnemyAtPoint(EnemyDefinition def, Vector3Int startPoint)
		{
			/*if(def.PlaceAtEdge)
			{
				if(!AllFloorEdgePositions.Contains(startPoint))
				{
					return false;
				}
			}*/
			foreach(Vector3Int occupyPos in def.OccupyPoints)
			{
				Vector3Int relativeOccupyPos = startPoint + occupyPos;
				if(!UnoccupiedFloorPositions.Contains(relativeOccupyPos))
				{
					return false;
				}
			}

			foreach(Vector3Int floorPos in def.RequiredFloorPoints)
			{
				Vector3Int relativeFloorPos = startPoint + floorPos;
				if(!AllFloorPositions.Contains(relativeFloorPos))
				{
					return false;
				}
			}
			return true;
		}

		public void Initialise(LayoutData parent, int critPathIndex, bool onCritPath)
		{
			ParentLayout = parent;
			ConnectionData = new();
			CriticalPathIndex = critPathIndex;
			OnCriticalPath = onCritPath;
		}

		public void SetDefinition(RoomDefinition def)
		{
			PrefabId = def.Id;
			RoomType = def.RoomType;

			RoomController controller = def.PrefabController.GetComponent<RoomController>();

			foreach(Guid doorId in AllDoorIds)
			{
				ParentLayout.Door_LUT.Remove(doorId);
			}

			AllDoorIds.Clear();

			foreach (RoomDoor door in controller.Doors)
			{
				GenerationDoorData doorData = new();
				doorData.IndexInRoom = controller.GetIndexOfDoor(door);
				doorData.ParentRoomId = Id;
				doorData.SideOfRoom = door.SideOfRoom;
				ParentLayout.Door_LUT.Add(doorData.Id, doorData);
				AllDoorIds.Add(doorData.Id);
			}

			UnoccupiedFloorPositions = new(controller.FloorTilePositions);
			AllFloorPositions = new(controller.FloorTilePositions);
			AllFloorEdgePositions = new(controller.FloorEdgePositions);

			if (ParentLayout.RoomCount_LUT.ContainsKey(def.Id))
			{
				ParentLayout.RoomCount_LUT[def.Id] += 1;
			}
			else
			{
				ParentLayout.RoomCount_LUT.Add(def.Id, 1);
			}
		}

		public bool TryLinkToRoom(GenerationRoomData linkToRoom, GenerationDoorData outwardDoor, out GenerationDoorData otherRoomDoor)
		{
			EOrthogonalDirection oppositeSide = GameplayHelper.GetOppositeDirection(outwardDoor.SideOfRoom);
			if (!linkToRoom.TryGetDoorOnSide(oppositeSide, out otherRoomDoor, true))
			{
				Debug.LogWarning("[GENERATION] ROOM LINK FAIL: failed to find door on opposite side " + oppositeSide.ToString());
				return false;
			}

			outwardDoor.LinkToDoor(otherRoomDoor, true);
			return true;
		}

		public bool CanLinkToRoom(GenerationRoomData linkToRoom, EOrthogonalDirection requiredSide)
		{
			if (!TryGetDoorOnSide(requiredSide, out _, true))
			{
				return false;
			}

			EOrthogonalDirection oppositeSide = GameplayHelper.GetOppositeDirection(requiredSide);
			if (!linkToRoom.TryGetDoorOnSide(oppositeSide, out _, true))
			{
				return false;
			}

			return true;
		}

		public bool TryLinkToRoom(GenerationRoomData linkToRoom, EOrthogonalDirection requiredSide, out GenerationDoorData foundDoor, out GenerationDoorData otherRoomDoor)
		{
			foundDoor = null;
			otherRoomDoor = null;

			if (!TryGetDoorOnSide(requiredSide, out foundDoor, true))
			{
				Debug.LogWarning("[GENERATION] ROOM LINK FAIL: failed to find door on required side " + requiredSide.ToString());
				return false;
			}

			EOrthogonalDirection oppositeSide = GameplayHelper.GetOppositeDirection(requiredSide);
			if (!linkToRoom.TryGetDoorOnSide(oppositeSide, out otherRoomDoor, true))
			{
				Debug.LogWarning("[GENERATION] ROOM LINK FAIL: failed to find door on opposite side " + oppositeSide.ToString());
				return false;
			}

			foundDoor.LinkToDoor(otherRoomDoor);
			return true;
		}

		public bool TryGetDoorOnSide(EOrthogonalDirection roomSide, out GenerationDoorData foundDoor, bool mustBeUnlinked = false)
		{
			foreach (Guid doorId in AllDoorIds)
			{
				GenerationDoorData door = ParentLayout.Door_LUT[doorId];

				if (door.SideOfRoom != roomSide)
				{
					continue;
				}

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

	}

	[System.Serializable]
	public class GenerationDoorData
	{
		public Guid Id = Guid.NewGuid();
		public EOrthogonalDirection SideOfRoom;
		public bool IsLinked;
		public Guid LinkDoorId;
		public bool Enabled;
		public Guid ParentRoomId;
		public int IndexInRoom;


		public void LinkToDoor(GenerationDoorData newDestination, bool updateLinkingDoor = true)
		{
			if (null == newDestination)
			{
				LinkDoorId = Guid.Empty;
				IsLinked = false;
				return;
			}

			LinkDoorId = newDestination.Id;
			IsLinked = true;

			if (updateLinkingDoor)
			{
				newDestination.LinkToDoor(this, false);
			}
		}

	}

	[System.Serializable]
	public class EnemySpawnData
	{
		public string EnemyId;
		public Vector3Int FloorTilePos;
	}

	[System.Serializable]
	public class GenerationNodeLinkData
	{
		/*public Guid OutwardNodeId;
		public Guid InwardNodeId;*/
		public Guid NodeID;

		public Dictionary<Guid, OuterLink> OuterLink_LUT = new();

		public class OuterLink
		{
			public Guid NodeID;
			public Guid FromNodeID; //should be ID of owning GenerationNodeLinkData
			public EOrthogonalDirection FixedDirection = EOrthogonalDirection.Invalid;
			public List<EOrthogonalDirection> PossibleDirections = new();

			public void FixLinkDirection(EOrthogonalDirection direction, Dictionary<Guid, GenerationNodeLinkData> nodeLinks_LUT)
			{
				FixedDirection = direction;

				GenerationNodeLinkData outerNodeLinkData = nodeLinks_LUT[NodeID];

				foreach (Guid innerNodeId in outerNodeLinkData.OuterLink_LUT.Keys)
				{
					if (innerNodeId != FromNodeID)
					{
						continue;
					}

					EOrthogonalDirection innerSetDirection = GameplayHelper.GetOppositeDirection(direction);
					OuterLink innerLink = outerNodeLinkData.OuterLink_LUT[innerNodeId];
					innerLink.FixedDirection = innerSetDirection;
				}
			}
		}

		public int MaxUpLinks;
		public int MaxDownLinks;
		public int MaxLeftLinks;
		public int MaxRightLinks;

		//passed through to connection data
		public int MinUpLinks;
		public int MinDownLinks;
		public int MinLeftLinks;
		public int MinRightLinks;
	}

	public class DungeonLayoutGenerationSystem : PersistentSystem<DungeonLayoutGenerationSystem>
	{
		private SystemLoadData _loadData = new SystemLoadData()
		{
			LoadStates = SystemLoadState.NoLoadingNeeded,
			UpdateAfterLoadingState = ELoadingState.Game
		};

		public override SystemLoadData LoadData => _loadData;


		public override void Initialise()
		{
			base.Initialise();
		}

		public override void StartLoading(ELoadingState state)
		{
			base.StartLoading(state);
		}

		const int SET_ENEMY_ATTEMPTS = 200;
		const int SET_ENEMY_POS_ATTEMPTS = 500;

		public LayoutData GenerateDungeonLayout(LevelGrammarDefinition grammar, System.Random random, out bool generationSuccess)
		{
            float generationStartTime = Time.realtimeSinceStartup;

			generationSuccess = false;

			Dictionary<Guid, Guid> nodeIdToGenDataId_LUT = new();
			Dictionary<Guid, Guid> genDataIdToNodeId_LUT = new();

			List<(Guid, Guid)> successfulLinks = new();

            LayoutData layoutData = new();

			layoutData.Theme = grammar.BaseLevelTheme;

			Dictionary<Guid, GenerationNodeLinkData> nodeLinks_LUT = new();

			//initial room data creation
			foreach (LevelGrammarNodeDefinition roomNode in grammar.Nodes)
			{
				GenerationRoomData roomData = new GenerationRoomData();
				roomData.Initialise(layoutData, -1, false);
				roomData.GrammarNodeId = roomNode.NodeGuid;

				if (roomNode.OverrideTheme != ELevelTheme.None && roomNode.UseOverrideTheme)
				{
					roomData.OverrideRoomTheme = roomNode.OverrideTheme;
				}

				if(roomNode.UseFixedId)
				{
					RoomDefinition roomDef = RoomDefinitionLoadSystem.Instance.GetDefinition(roomNode.FixedId);
					roomData.SetDefinition(roomDef);
				}

				roomData.ConnectionData = new();

				layoutData.Room_LUT.TryAdd(roomData.Id, roomData);

				nodeIdToGenDataId_LUT.TryAdd(roomNode.NodeGuid, roomData.Id);
				genDataIdToNodeId_LUT.TryAdd(roomData.Id, roomNode.NodeGuid);
			}

			foreach (LevelGrammarNodeDefinition roomNode in grammar.Nodes)
			{
				GenerationNodeLinkData linkData = new();
				linkData.NodeID = roomNode.NodeGuid;

				foreach(RoomLinkData roomLinkDef in roomNode.RoomLinkData)
				{
					Guid linkToNode = roomLinkDef.LinkToNode.Id;
					GenerationNodeLinkData.OuterLink outerLinkData = new();
					outerLinkData.NodeID = linkToNode;
					outerLinkData.FromNodeID = linkData.NodeID;
					if(roomLinkDef.UseFixedSide)
					{
						outerLinkData.FixedDirection = roomLinkDef.FixedSide;
					}

					linkData.OuterLink_LUT.Add(linkToNode, outerLinkData);
				}

				nodeLinks_LUT.Add(roomNode.NodeGuid, linkData);
			}

			//Fix linking directions of all rooms linking into one with a preset definition, so we know random prefabs we find will definitely
			//	A) be able to link and
			//	B) want to link the right way
			foreach (Guid nodeId in nodeLinks_LUT.Keys)
			{
				Guid roomId = nodeIdToGenDataId_LUT[nodeId];
				GenerationNodeLinkData linkData = nodeLinks_LUT[nodeId];
				GenerationRoomData roomData = layoutData.Room_LUT[roomId];

				if(roomData.PrefabId != string.Empty)
				{
					RoomDefinition roomDef = RoomDefinitionLoadSystem.Instance.GetDefinition(roomData.PrefabId);
					RoomController linkToPrefabController = roomDef.PrefabController.GetComponent<RoomController>();

					List<Guid> shuffledLinkNodeIds = linkData.OuterLink_LUT.Keys.OrderBy(x => random.NextDouble()).ToList();

					List<RoomDoor> availableDoors = new(linkToPrefabController.Doors);

					foreach(Guid linkToNodeId in shuffledLinkNodeIds)
					{
						GenerationNodeLinkData.OuterLink outerLink = linkData.OuterLink_LUT[linkToNodeId];

						bool fixedLinkSuccess = false;

						//if outer link has fixed direction, still need to find available door to assign here
						if (outerLink.FixedDirection != EOrthogonalDirection.Invalid)
						{
							List<RoomDoor> availableDoorsOnFixedSide = new();

							List<RoomDoor> doorListOnFixedSide = linkToPrefabController.GetDoorListForSide(outerLink.FixedDirection);
							if(doorListOnFixedSide.Count == 0)
							{
								//complain here
								Debug.Log("[GRAMMARGEN] node " + grammar.Nodes_LUT[nodeId].Id + " fixed room " + linkToPrefabController + " has no doors AT ALL on link side " + outerLink.FixedDirection.ToString() +
									" linking to node " + grammar.Nodes_LUT[linkToNodeId].Id + " - will randomise direction");
							}
							else
							{
								foreach (RoomDoor door in doorListOnFixedSide)
								{
									if (availableDoors.Contains(door))
									{
										availableDoorsOnFixedSide.Add(door);
									}
								}

								if (availableDoorsOnFixedSide.Count == 0)
								{
									//complain here
									Debug.Log("[GRAMMARGEN] node " + grammar.Nodes_LUT[nodeId].Id + " fixed room " + linkToPrefabController + " has no doors REMAINING on link side " + outerLink.FixedDirection.ToString() +
									" linking to node " + grammar.Nodes_LUT[linkToNodeId].Id + " - will randomise direction");
								}

								else
								{
									int doorIndex = random.Next(availableDoorsOnFixedSide.Count);
									RoomDoor roomDoor = availableDoorsOnFixedSide[doorIndex];
									EOrthogonalDirection outerSetDirection = roomDoor.SideOfRoom;
									outerLink.FixLinkDirection(outerSetDirection, nodeLinks_LUT);
									availableDoors.Remove(roomDoor);

									fixedLinkSuccess = true;
								}
							}
						}

						if(!fixedLinkSuccess)
						{
							//randomly choose available door to assign
							int doorIndex = random.Next(availableDoors.Count);
							RoomDoor roomDoor = availableDoors[doorIndex];
							EOrthogonalDirection outerSetDirection = roomDoor.SideOfRoom;
							outerLink.FixLinkDirection(outerSetDirection, nodeLinks_LUT);
							availableDoors.Remove(roomDoor);
						}
					}
				}
			}

			//fix link directions randomly
			foreach (Guid nodeId in nodeLinks_LUT.Keys)
			{
				Guid roomId = nodeIdToGenDataId_LUT[nodeId];
				GenerationNodeLinkData linkData = nodeLinks_LUT[nodeId];
				GenerationRoomData roomData = layoutData.Room_LUT[roomId];

				foreach(Guid linkToNode in linkData.OuterLink_LUT.Keys)
				{
					GenerationNodeLinkData.OuterLink outerLink = linkData.OuterLink_LUT[linkToNode];
					if(outerLink.FixedDirection != EOrthogonalDirection.Invalid)
					{
						continue;
					}

					EOrthogonalDirection setDirection = GameplayHelper.GetRandomDirection(random);
					outerLink.FixLinkDirection(setDirection, nodeLinks_LUT);
				}
			}

			//set connection data
			foreach (Guid nodeId in nodeLinks_LUT.Keys)
			{
				Guid roomId = nodeIdToGenDataId_LUT[nodeId];
				GenerationNodeLinkData linkData = nodeLinks_LUT[nodeId];
				GenerationRoomData roomData = layoutData.Room_LUT[roomId];

				foreach (Guid linkToNode in linkData.OuterLink_LUT.Keys)
				{
					GenerationNodeLinkData.OuterLink outerLink = linkData.OuterLink_LUT[linkToNode];
					if(outerLink.FixedDirection == EOrthogonalDirection.Invalid)
					{
						Debug.LogError("[GRAMMARGEN] [ERROR] fixed direction should never be Invalid!");
					}
					else
					{
						int prevConnectionCount = roomData.ConnectionData.GetRequirementsInDirection(outerLink.FixedDirection);
						roomData.ConnectionData.SetConnectionCount(outerLink.FixedDirection, prevConnectionCount + 1);
					}
				}
			}

			//set room defs
			foreach(LevelGrammarNodeDefinition roomNode in grammar.Nodes)
			{
				if(roomNode.UseFixedId)
				{
					continue;
				}

				Guid roomId = nodeIdToGenDataId_LUT[roomNode.NodeGuid];
				GenerationRoomData roomData = layoutData.Room_LUT[roomId];
				ERoomType roomType = ERoomType.None;
				if (roomNode.UseFixedRoomType)
				{
					roomType = roomNode.FixedRoomType;
				}
				else
				{
					roomType = GameplayHelper.GetRandomRoomTypeForGrammarNode(random);
				}

				RoomDefinition roomDef = GetRandomRoomForConnectionDataAndType(roomData.ConnectionData,
					roomType,
					layoutData,
					random,
					true,
					out bool success);
				if (!success)
				{
					//complain here
				}
				else
				{
					roomData.SetDefinition(roomDef);
				}
			}

			//link room data
			foreach (Guid linkDataId in nodeLinks_LUT.Keys)
			{
				GenerationNodeLinkData linkData = nodeLinks_LUT[linkDataId];

				LevelGrammarNodeDefinition roomNode = grammar.Nodes_LUT[linkData.NodeID];
				Guid roomDataId = nodeIdToGenDataId_LUT[linkData.NodeID];


				GenerationRoomData roomData = layoutData.Room_LUT[roomDataId];

				foreach (Guid linkToNodeId in linkData.OuterLink_LUT.Keys)
				{
					GenerationNodeLinkData.OuterLink outerLink = linkData.OuterLink_LUT[linkToNodeId];

					Guid linkToRoomGuid = nodeIdToGenDataId_LUT[linkToNodeId];
					GenerationRoomData linkToRoomData = layoutData.Room_LUT[linkToRoomGuid];

					LevelGrammarNodeDefinition linkToNode = grammar.Nodes_LUT[linkToNodeId];


					bool identicalLinkFound = false;

					foreach ((Guid, Guid) successLink in successfulLinks)
					{
						if (successLink.Item1 == roomNode.NodeGuid && successLink.Item2 == linkToNodeId ||
							successLink.Item2 == roomNode.NodeGuid && successLink.Item1 == linkToNodeId)
						{
							identicalLinkFound = true;
						}
					}

					if (identicalLinkFound)
					{
						continue;
					}


					EOrthogonalDirection outDirection = outerLink.FixedDirection;

					if (!roomData.TryLinkToRoom(linkToRoomData, outDirection, out GenerationDoorData thisRoomDoor, out GenerationDoorData otherRoomDoor))
					{
						Debug.LogWarning("[GENERATION] failed linking from room " + roomData.PrefabId.ToString() +
							" node ID " + roomNode.Id + 
							" - outward direction " + outDirection.ToString() +
							" to room " + linkToRoomData.PrefabId.ToString() +
							" node ID " + linkToNode.Id
							);
					}

					else
					{
						thisRoomDoor.Enabled = true;
						otherRoomDoor.Enabled = true;

						(Guid, Guid) successLink = new(linkToNodeId, roomNode.NodeGuid);
						successfulLinks.Add(successLink);

						Debug.Log("[GENERATION] linked node " + roomNode.Id + " to node " + linkToNode.Id);
					}
				}
			}

            //find active doors in combat rooms and remove potential enemy spawns using doormats
            List<Guid> combatRooms = layoutData.GetAllRoomsOfType(ERoomType.StandardCombat);
            foreach (Guid combatRoom in combatRooms)
            {
                GenerationRoomData roomData = layoutData.Room_LUT[combatRoom];

                foreach (Guid door in roomData.AllDoorIds)
                {
                    GenerationDoorData doorData = layoutData.Door_LUT[door];
                    if (!doorData.Enabled)
                    {
                        continue;
                    }

                    RoomController controller = RoomDefinitionLoadSystem.Instance.GetDefinition(roomData.PrefabId).PrefabController.GetComponent<RoomController>();
                    List<Vector3Int> activeDoormat = controller.Doormat_LUT[doorData.IndexInRoom];

                    foreach (Vector3Int doormatPos in activeDoormat)
                    {
                        roomData.UnoccupiedFloorPositions.Remove(doormatPos);
                    }
                }
            }

            //populate combat rooms with enemies
            foreach (Guid combatRoom in combatRooms)
            {
                GenerationRoomData roomData = layoutData.Room_LUT[combatRoom];
				Guid nodeGuid = genDataIdToNodeId_LUT[combatRoom];
				LevelGrammarNodeDefinition node = grammar.Nodes_LUT[nodeGuid];

				//Fixed enemy spawns
				foreach(FixedEnemySpawnDataItem fixedSpawn in node.FixedEnemySpawns)
				{
                    EnemyDefinition fixedEnemyDef = EnemyLoadSystem.Instance.GetDefinition(fixedSpawn.EnemyId);

                    EnemySpawnData spawnData = new();
					spawnData.EnemyId = fixedSpawn.EnemyId;

					bool spawnPosFail = false;

					if(fixedSpawn.UseFixedCoords)
					{
						spawnData.FloorTilePos = new(
							fixedSpawn.FixedCoords.x,
							fixedSpawn.FixedCoords.y);
					}
					else
					{
						if(!TryGetSpawnPosForEnemy(fixedEnemyDef, roomData, random, out Vector3Int spawnPos))
						{
							//complain here
							Debug.LogWarning("[GENERATION] [GRAMMARGEN] [ENEMYGEN] Failed to place fixed enemy " + fixedEnemyDef.Id + " in node " + node.Id + " - it will not appear!");
							spawnPosFail = true;
						}
						else
						{
							spawnData.FloorTilePos = spawnPos;
						}
					}

					if(!spawnPosFail)
					{
						foreach (Vector3Int occupyPos in fixedEnemyDef.OccupyPoints)
						{
							Vector3Int relativeOccupyPos = occupyPos + spawnData.FloorTilePos;
							roomData.UnoccupiedFloorPositions.Remove(relativeOccupyPos);
						}

						roomData.EnemySpawnData.Add(spawnData);
					}
				}

				//Random enemy spawns
				if(node.SpawnRandomEnemies)
				{
					if(node.RandomEnemySpawnData.FixEnemyCount)
					{
                        //divide the total points by the enemy count, and pick a random enemy of those with the highest possible points value
                        int roomPointsToSpend = node.RandomEnemySpawnData.DifficultyPoints;
						int pointsGoalPerEnemy = (int)roomPointsToSpend / node.RandomEnemySpawnData.EnemyCount;

						for(int enemyCount = 0; enemyCount < node.RandomEnemySpawnData.EnemyCount; ++enemyCount)
						{
							bool setEnemySuccess = false;

                            for (int setEnemyAttempt = 0; setEnemyAttempt < SET_ENEMY_ATTEMPTS; ++setEnemyAttempt)
                            {
                                if (roomPointsToSpend <= 0)
                                {
                                    Debug.Log("[ENEMYGEN] spent all enemy points for room " + roomData.Id.ToString() + " after " + setEnemyAttempt.ToString() + " attempts");
                                    break;
                                }

                                if (pointsGoalPerEnemy <= 0)
                                {
                                    break;
                                }

                                EnemyDefinition def = GetRandomEnemyDefForDifficultyPoints(pointsGoalPerEnemy, layoutData, grammar.BaseLevelTheme, true, true, roomData.Id, random, true);

                                if (!TryGetSpawnPosForEnemy(def, roomData, random, out Vector3Int validPos))
                                {
                                    continue;
                                }

                                EnemySpawnData spawnData = new();
                                spawnData.EnemyId = def.Id;

                                spawnData.FloorTilePos = validPos;

                                foreach (Vector3Int occupyPos in def.OccupyPoints)
                                {
                                    Vector3Int relativeOccupyPos = occupyPos + spawnData.FloorTilePos;
                                    roomData.UnoccupiedFloorPositions.Remove(relativeOccupyPos);
                                }

                                roomData.EnemySpawnData.Add(spawnData);

                                if (roomData.EnemyCount_LUT.ContainsKey(def.Id))
                                {
                                    roomData.EnemyCount_LUT[def.Id]++;
                                }
                                else
                                {
                                    roomData.EnemyCount_LUT.Add(def.Id, 1);
                                }

                                Debug.Log("[ENEMYGEN] placing enemy of type " + def.Id +
                                " in room " + roomData.CriticalPathIndex.ToString() +
                                " - spending " + def.DifficultyPoints.ToString() +
                                " of remaining " + roomPointsToSpend.ToString() + " points, leaving " +
                                (roomPointsToSpend - def.DifficultyPoints).ToString() + " remaining points");

                                roomPointsToSpend -= def.DifficultyPoints;

                                if (layoutData.EnemyCount_LUT.ContainsKey(def.Id))
                                {
                                    layoutData.EnemyCount_LUT[def.Id]++;
                                }
                                else
                                {
                                    layoutData.EnemyCount_LUT.Add(spawnData.EnemyId, 1);
                                }

								setEnemySuccess = true;
                                break;
                            }

							if(!setEnemySuccess)
							{
								EnemyDefinition def = EnemyLoadSystem.Instance.GetDefinition("apple_Green");

                                if (!TryGetSpawnPosForEnemy(def, roomData, random, out Vector3Int validPos))
                                {
                                    continue;
                                }

                                EnemySpawnData spawnData = new();
                                spawnData.EnemyId = def.Id;

                                spawnData.FloorTilePos = validPos;

                                foreach (Vector3Int occupyPos in def.OccupyPoints)
                                {
                                    Vector3Int relativeOccupyPos = occupyPos + spawnData.FloorTilePos;
                                    roomData.UnoccupiedFloorPositions.Remove(relativeOccupyPos);
                                }

                                roomData.EnemySpawnData.Add(spawnData);

                                if (roomData.EnemyCount_LUT.ContainsKey(def.Id))
                                {
                                    roomData.EnemyCount_LUT[def.Id]++;
                                }
                                else
                                {
                                    roomData.EnemyCount_LUT.Add(def.Id, 1);
                                }

                                Debug.Log("[ENEMYGEN] placing enemy of type " + def.Id +
                                " in room " + roomData.CriticalPathIndex.ToString() +
                                " - spending " + def.DifficultyPoints.ToString() +
                                " of remaining " + roomPointsToSpend.ToString() + " points, leaving " +
                                (roomPointsToSpend - def.DifficultyPoints).ToString() + " remaining points");

                                roomPointsToSpend -= def.DifficultyPoints;

                                if (layoutData.EnemyCount_LUT.ContainsKey(def.Id))
                                {
                                    layoutData.EnemyCount_LUT[def.Id]++;
                                }
                                else
                                {
                                    layoutData.EnemyCount_LUT.Add(spawnData.EnemyId, 1);
                                }
                            }
                        }
                    }
					else
					{
                        int roomPointsToSpend = node.RandomEnemySpawnData.DifficultyPoints;

                        Debug.Log("[ENEMYGEN] have " +
                        roomPointsToSpend.ToString() + " enemy points for room with index " + roomData.CriticalPathIndex);

                        for (int setEnemyAttempt = 0; setEnemyAttempt < SET_ENEMY_ATTEMPTS; ++setEnemyAttempt)
                        {
                            if (roomPointsToSpend <= 0)
                            {
                                Debug.Log("[ENEMYGEN] spent all enemy points for room " + roomData.Id.ToString() + " after " + setEnemyAttempt.ToString() + " attempts");
                                break;
                            }

                            EnemyDefinition def = GetRandomEnemyDefForDifficultyPoints(roomPointsToSpend, layoutData, grammar.BaseLevelTheme, true, true, roomData.Id, random);

                            if (!TryGetSpawnPosForEnemy(def, roomData, random, out Vector3Int validPos))
                            {
                                continue;
                            }

                            EnemySpawnData spawnData = new();
                            spawnData.EnemyId = def.Id;

                            spawnData.FloorTilePos = validPos;

                            foreach (Vector3Int occupyPos in def.OccupyPoints)
                            {
                                Vector3Int relativeOccupyPos = occupyPos + spawnData.FloorTilePos;
                                roomData.UnoccupiedFloorPositions.Remove(relativeOccupyPos);
                            }

                            roomData.EnemySpawnData.Add(spawnData);

                            if (roomData.EnemyCount_LUT.ContainsKey(def.Id))
                            {
                                roomData.EnemyCount_LUT[def.Id]++;
                            }
                            else
                            {
                                roomData.EnemyCount_LUT.Add(def.Id, 1);
                            }

                            Debug.Log("[ENEMYGEN] placing enemy of type " + def.Id +
                            " in room " + roomData.CriticalPathIndex.ToString() +
                            " - spending " + def.DifficultyPoints.ToString() +
                            " of remaining " + roomPointsToSpend.ToString() + " points, leaving " +
                            (roomPointsToSpend - def.DifficultyPoints).ToString() + " remaining points");

                            roomPointsToSpend -= def.DifficultyPoints;

                            if (layoutData.EnemyCount_LUT.ContainsKey(def.Id))
                            {
                                layoutData.EnemyCount_LUT[def.Id]++;
                            }
                            else
                            {
                                layoutData.EnemyCount_LUT.Add(spawnData.EnemyId, 1);
                            }
                        }
                    }
                    
                }	
            }

            //assign item rooms with item IDs
            List<Guid> itemRoomIds = layoutData.GetAllRoomsOfType(ERoomType.StandardItem);
            foreach (Guid itemId in itemRoomIds)
            {
                GenerationRoomData itemRoom = layoutData.Room_LUT[itemId];
                itemRoom.SpawnItemIds.Clear();
                ItemDefinition itemDef = ItemLoadSystem.Instance.GetRandomDefinition(random);
                itemRoom.SpawnItemIds.Add(itemDef.Id);
            }

            float generationEndTime = Time.realtimeSinceStartup;
            float generationTime = generationEndTime - generationStartTime;
            Debug.Log("[GENERATION] layout data took " + generationTime + " seconds to generate");
            int itemRoomCount = layoutData.GetAllRoomsOfType(ERoomType.StandardItem).Count;
            Debug.Log("[ITEMROOMGEN] layout data has " + itemRoomCount + " total item rooms");
            generationSuccess = true;
			return layoutData;
		}

		public GenerationNodeLinkData.OuterLink FindEquivalentLink(Dictionary<Guid, GenerationNodeLinkData> allLinks, GenerationNodeLinkData.OuterLink refOutLink, GenerationNodeLinkData refLinkData)
		{
			foreach(Guid linkDataId in allLinks.Keys)
			{
				GenerationNodeLinkData linkData = allLinks[linkDataId];
				if(linkData.NodeID != refOutLink.NodeID)
				{
					continue;
				}

				foreach(Guid linkToNodeId in linkData.OuterLink_LUT.Keys)
				{
					if(linkToNodeId != refLinkData.NodeID)
					{
						continue;
					}

					return linkData.OuterLink_LUT[linkToNodeId];
				}
			}

			return null;
		}


		public LayoutData GenerateDungeonLayout(LayoutGenerationSettings settings, System.Random random, out bool generationSuccess)
		{
			float generationStartTime = Time.realtimeSinceStartup;
			LayoutData layoutData = new();
			generationSuccess = false;

			layoutData.Theme = settings.Theme;

			//create number of room datas needed for critical path
			for (int critPathIndex = 0; critPathIndex < settings.CriticalPathRoomCount; ++critPathIndex)
			{
				GenerationRoomData roomData = new();
				roomData.Initialise(layoutData, critPathIndex, true);
				layoutData.Room_LUT.Add(roomData.Id, roomData);
				layoutData.CriticalPathIds.Add(roomData.Id);
			}

			//set connection requirements for each room in crit path
			EOrthogonalDirection lastOutwardConnection = EOrthogonalDirection.Invalid;
			List<EOrthogonalDirection> critPathOutwardConnections = new();

			for (int roomIndex = 0; roomIndex < layoutData.CriticalPathIds.Count; ++roomIndex)
			{
				Guid critRoomId = layoutData.CriticalPathIds[roomIndex];
				GenerationRoomData critRoom = layoutData.Room_LUT[critRoomId];
				bool isFirstRoom = roomIndex == 0;
				bool isFinalRoom = roomIndex == layoutData.CriticalPathIds.Count - 1;


				if (isFirstRoom)
				{
					EOrthogonalDirection direction = GameplayHelper.GetRandomDirection(random);
					int requirementsInDirection = critRoom.ConnectionData.GetRequirementsInDirection(direction);
					critRoom.ConnectionData.SetConnectionCount(direction, requirementsInDirection + 1);
					lastOutwardConnection = direction;
					critPathOutwardConnections.Add(lastOutwardConnection);
				}
				else
				{
					EOrthogonalDirection inwardConnection = GameplayHelper.GetOppositeDirection(lastOutwardConnection);
					int requirementsInDirection = critRoom.ConnectionData.GetRequirementsInDirection(inwardConnection);
					critRoom.ConnectionData.SetConnectionCount(inwardConnection, requirementsInDirection + 1);

					if (!isFinalRoom)
					{
						EOrthogonalDirection direction = GameplayHelper.GetRandomDirection(random);
						int requirementsInDirection1 = critRoom.ConnectionData.GetRequirementsInDirection(direction);
						critRoom.ConnectionData.SetConnectionCount(direction, requirementsInDirection1 + 1);
						lastOutwardConnection = direction;
						critPathOutwardConnections.Add(lastOutwardConnection);
					}
				}
			}

			//assign room definitions to each crit room using connection data
			for (int roomIndex = 0; roomIndex < layoutData.CriticalPathIds.Count; ++roomIndex)
			{
				Guid critRoomId = layoutData.CriticalPathIds[roomIndex];
				GenerationRoomData critRoom = layoutData.Room_LUT[critRoomId];
				bool isFirstRoom = roomIndex == 0;
				bool isFinalRoom = roomIndex == layoutData.CriticalPathIds.Count - 1;

				ERoomType roomType = ERoomType.StandardCombat;
				if (isFirstRoom)
				{
					roomType = ERoomType.Start;
				}
				if (isFinalRoom)
				{
					roomType = ERoomType.End;
				}
				//RoomDefinition roomDef = RoomDefinitionLoadSystem.Instance.GetRandomDefinition(random, critRoom.ConnectionData, out bool success, roomType);
				RoomDefinition roomDef = GetRandomRoomForConnectionDataAndType(critRoom.ConnectionData, roomType, layoutData, random, settings.UseRoomDefMaxPerLevel, out bool success);
				if (success)
				{
					critRoom.SetDefinition(roomDef);
				}
				else
				{
					Debug.LogError("[GENERATION] failed finding prefab for critical path room " + roomIndex.ToString() + " - "
						+ " CONNETION DATA: Up - " + critRoom.ConnectionData.RequiredUpConnections + " - "
						+ " Down - " + critRoom.ConnectionData.RequiredDownConnections + " - "
						+ " Left - " + critRoom.ConnectionData.RequiredLeftConnections + " - "
						+ " Right - " + critRoom.ConnectionData.RequiredRightConnections);
					return null;
					//complain
				}
			}

			//link doors
			for (int roomIndex = 0; roomIndex < layoutData.CriticalPathIds.Count; ++roomIndex)
			{
				Guid critRoomId = layoutData.CriticalPathIds[roomIndex];
				GenerationRoomData critRoom = layoutData.Room_LUT[critRoomId];
				bool isFinalRoom = roomIndex == layoutData.CriticalPathIds.Count - 1;
				if (!isFinalRoom)
				{
					Guid nextRoomId = layoutData.CriticalPathIds[roomIndex + 1];
					GenerationRoomData nextRoom = layoutData.Room_LUT[nextRoomId];

					if (!critRoom.TryLinkToRoom(nextRoom, critPathOutwardConnections[roomIndex], out GenerationDoorData door, out GenerationDoorData nextDoor))
					{
						Debug.LogWarning("[GENERATION] failed linking from room " + critRoom.PrefabId.ToString() + " - " + critPathOutwardConnections[roomIndex].ToString() + " to " + nextRoom.PrefabId.ToString());
						return null;
					}
					else
					{
						door.Enabled = true;
						nextDoor.Enabled = true;
					}
				}
			}

			//using door data from definitions, go through all unused doors and decide whether to add a secondary room
			bool maxRoomCountReached = false;
			List<Guid> critRoomShuffledGuids = layoutData.CriticalPathIds.OrderBy(x => random.NextDouble()).ToList();
			foreach (Guid critRoomId in critRoomShuffledGuids)
			{
				GenerationRoomData critRoom = layoutData.Room_LUT[critRoomId];
				bool isFirstRoom = critRoom.CriticalPathIndex == 0;
				bool isLastRoom = critRoom.CriticalPathIndex == critRoomShuffledGuids.Count - 1;

				if(isLastRoom && settings.LastRoomNoBranches)
				{
					continue;
				}

				foreach (Guid doorId in critRoom.AllDoorIds)
				{
					//prevent secondary room addition if room limit is reached
					if (maxRoomCountReached)
					{
						continue;
					}

					//work out if max rooms reached
					if (settings.MaxTotalRooms > 0)
					{
						int totalRoomCount = layoutData.Room_LUT.Count;
						if (totalRoomCount >= settings.MaxTotalRooms)
						{
							maxRoomCountReached = true;
							continue;
						}
					}

					GenerationDoorData doorData = layoutData.Door_LUT[doorId];
					if (doorData.IsLinked)
					{
						continue;
					}

					if (random.NextDouble() < settings.SecondaryRoomChance)
					{
						Debug.Log("[GENERATION] create a secondary room from crit room " + critRoom.CriticalPathIndex);
						GenerationRoomData secondaryRoom = new();
						secondaryRoom.Initialise(layoutData, critRoom.CriticalPathIndex, false);
						layoutData.Room_LUT.Add(secondaryRoom.Id, secondaryRoom);
						layoutData.SecondaryRoomIds.Add(secondaryRoom.Id);
						EOrthogonalDirection secondaryRoomInDirection = GameplayHelper.GetOppositeDirection(doorData.SideOfRoom);
						int currentConnectionCount = secondaryRoom.ConnectionData.GetRequirementsInDirection(secondaryRoomInDirection);
						secondaryRoom.ConnectionData.SetConnectionCount(secondaryRoomInDirection, currentConnectionCount + 1);
						int critCurrentConnectionCount = critRoom.ConnectionData.GetRequirementsInDirection(doorData.SideOfRoom);
						critRoom.ConnectionData.SetConnectionCount(doorData.SideOfRoom, critCurrentConnectionCount + 1);
						ERoomType secondaryRoomType = ERoomType.StandardCombat;
						//RoomDefinition secondaryRoomDef = RoomDefinitionLoadSystem.Instance.GetRandomDefinition(random, secondaryRoom.ConnectionData, out bool success, secondaryRoomType);
						RoomDefinition secondaryRoomDef = GetRandomRoomForConnectionDataAndType(secondaryRoom.ConnectionData, secondaryRoomType, layoutData, random, settings.UseRoomDefMaxPerLevel, out bool success);
						if (!success)
						{
							Debug.LogError("[GENERATION] failed finding prefab for secondary path room " + secondaryRoom.CriticalPathIndex.ToString() + " - "
							+ " CONNETION DATA: Up - " + critRoom.ConnectionData.RequiredUpConnections + " - "
							+ " Down - " + critRoom.ConnectionData.RequiredDownConnections + " - "
							+ " Left - " + critRoom.ConnectionData.RequiredLeftConnections + " - "
							+ " Right - " + critRoom.ConnectionData.RequiredRightConnections);
							return null;
						}
						else
						{
							secondaryRoom.SetDefinition(secondaryRoomDef);
							if (!critRoom.TryLinkToRoom(secondaryRoom, doorData, out GenerationDoorData secondaryRoomDoor))
							{
								//complain here
								return null;
							}
							else
							{
								doorData.Enabled = true;
								secondaryRoomDoor.Enabled = true;
							}
						}
					}
				}
			}


			//replace existing combat rooms with item room definitions as required
			List<Guid> combatRooms_PreItems = layoutData.GetAllRoomsOfType(ERoomType.StandardCombat);
			List<Guid> combatRooms_PreItems_ShuffledGuids = combatRooms_PreItems.OrderBy(x => random.NextDouble()).ToList();
			int numItemRooms = 0;
			foreach(Guid combatRoom in combatRooms_PreItems_ShuffledGuids)
			{
				GenerationRoomData combatRoomData = layoutData.Room_LUT[combatRoom];

				if(numItemRooms >= settings.MaxItemRooms)
				{
					continue;
				}

				if (random.NextDouble() < settings.ItemRoomChance)
				{
					//RoomDefinition itemRoomDef = RoomDefinitionLoadSystem.Instance.GetRandomDefinition(random, combatRoomData.ConnectionData, out bool success, ERoomType.StandardItem);
					RoomDefinition itemRoomDef = GetRandomRoomForConnectionDataAndType(combatRoomData.ConnectionData, ERoomType.StandardItem, layoutData, random, settings.UseRoomDefMaxPerLevel, out bool success);
					if (!success)
					{
						Debug.LogError("[ITEMROOMGEN] failed finding prefab for item room "
							+ " CONNETION DATA: Up - " + combatRoomData.ConnectionData.RequiredUpConnections + " - "
							+ " Down - " + combatRoomData.ConnectionData.RequiredDownConnections + " - "
							+ " Left - " + combatRoomData.ConnectionData.RequiredLeftConnections + " - "
							+ " Right - " + combatRoomData.ConnectionData.RequiredRightConnections);
						continue;
					}
					//find every door that was linking to prev definition
					List<Guid> linkingIdsCache = new(combatRoomData.AllDoorIds);
					List<GenerationDoorData> reattachDoors = new();
					foreach(Guid linkingId in linkingIdsCache)
					{
						Guid linkToDoorId = layoutData.Door_LUT[linkingId].LinkDoorId;
						if(linkToDoorId != Guid.Empty)
						{
							GenerationDoorData reattachDoor = layoutData.Door_LUT[linkToDoorId];
							reattachDoors.Add(reattachDoor);
						}
					}


					combatRoomData.SetDefinition(itemRoomDef);
					foreach(GenerationDoorData door in reattachDoors)
					{
						door.LinkToDoor(null);
						GenerationRoomData roomData = layoutData.Room_LUT[door.ParentRoomId];
						if(!roomData.TryLinkToRoom(combatRoomData, door, out GenerationDoorData combatRoomDoor))
						{
							//complain here;
						}
						else
						{
							combatRoomDoor.Enabled = true;
						}
					}
					
					numItemRooms++;
				}
			}


			//find active doors in combat rooms and remove potential enemy spawns using doormats
			List<Guid> combatRooms = layoutData.GetAllRoomsOfType(ERoomType.StandardCombat);
			foreach (Guid combatRoom in combatRooms)
			{
				GenerationRoomData roomData = layoutData.Room_LUT[combatRoom];

				foreach (Guid door in roomData.AllDoorIds)
				{
					GenerationDoorData doorData = layoutData.Door_LUT[door];
					if (!doorData.Enabled)
					{
						continue;
					}

					RoomController controller = RoomDefinitionLoadSystem.Instance.GetDefinition(roomData.PrefabId).PrefabController.GetComponent<RoomController>();
					List<Vector3Int> activeDoormat = controller.Doormat_LUT[doorData.IndexInRoom];

					foreach (Vector3Int doormatPos in activeDoormat)
					{
						roomData.UnoccupiedFloorPositions.Remove(doormatPos);
					}
				}
			}


			//populate combat rooms with enemies until enemy points value has been reached
			foreach (Guid combatRoom in combatRooms)
			{
				GenerationRoomData roomData = layoutData.Room_LUT[combatRoom];
				int roomPointsToSpend = GetDifficultyPointsForSettings(settings, roomData);

				Debug.Log("[ENEMYGEN] have " +
				roomPointsToSpend.ToString() + " enemy points for room with index " + roomData.CriticalPathIndex);

				for (int setEnemyAttempt = 0; setEnemyAttempt < SET_ENEMY_ATTEMPTS; ++setEnemyAttempt)
				{
					if (roomPointsToSpend <= 0)
					{
						Debug.Log("[ENEMYGEN] spent all enemy points for room " + roomData.Id.ToString() + " after " + setEnemyAttempt.ToString() + " attempts");
						break;
					}

					EnemyDefinition def = GetRandomEnemyDefForDifficultyPoints(roomPointsToSpend, layoutData, settings.Theme, settings.UseEnemyDefMaxPerLevel, settings.UseEnemyDefMaxPerRoom, roomData.Id, random);

					if(!TryGetSpawnPosForEnemy(def, roomData, random, out Vector3Int validPos))
					{
						continue;
					}

					EnemySpawnData spawnData = new();
					spawnData.EnemyId = def.Id;

					spawnData.FloorTilePos = validPos;

					foreach(Vector3Int occupyPos in def.OccupyPoints)
					{
						Vector3Int relativeOccupyPos = occupyPos + spawnData.FloorTilePos;
						roomData.UnoccupiedFloorPositions.Remove(relativeOccupyPos);
					}

					roomData.EnemySpawnData.Add(spawnData);

					if(roomData.EnemyCount_LUT.ContainsKey(def.Id))
					{
						roomData.EnemyCount_LUT[def.Id]++;
					}
					else
					{
						roomData.EnemyCount_LUT.Add(def.Id, 1);
					}

					Debug.Log("[ENEMYGEN] placing enemy of type " + def.Id +
					" in room " + roomData.CriticalPathIndex.ToString() +
					" - spending " + def.DifficultyPoints.ToString() +
					" of remaining " + roomPointsToSpend.ToString() + " points, leaving " +
					(roomPointsToSpend - def.DifficultyPoints).ToString() + " remaining points");

					roomPointsToSpend -= def.DifficultyPoints;

					if (layoutData.EnemyCount_LUT.ContainsKey(def.Id))
					{
						layoutData.EnemyCount_LUT[def.Id]++;
					}
					else
					{
						layoutData.EnemyCount_LUT.Add(spawnData.EnemyId, 1);
					}
				}
			}

			//assign item rooms with item IDs
			List<Guid> itemRoomIds = layoutData.GetAllRoomsOfType(ERoomType.StandardItem);
			foreach(Guid itemId in itemRoomIds)
			{
				GenerationRoomData itemRoom = layoutData.Room_LUT[itemId];
				itemRoom.SpawnItemIds.Clear();
				ItemDefinition itemDef = ItemLoadSystem.Instance.GetRandomDefinition(random);
				itemRoom.SpawnItemIds.Add(itemDef.Id);
			}

			float generationEndTime = Time.realtimeSinceStartup;
			float generationTime = generationEndTime - generationStartTime;
			Debug.Log("[GENERATION] layout data took " + generationTime + " seconds to generate");
			int itemRoomCount = layoutData.GetAllRoomsOfType(ERoomType.StandardItem).Count;
			Debug.Log("[ITEMROOMGEN] layout data has " + itemRoomCount + " total item rooms");
			generationSuccess = true;
			return layoutData;
		}

		public int GetDifficultyPointsForSettings(LayoutGenerationSettings settings, GenerationRoomData roomData)
		{
			switch(settings.DifficultyPointsMode)
			{
				case EDifficultyPointsMode.Fixed:
				default:
					return settings.FixedEnemyPointsPerRoom;
				case EDifficultyPointsMode.Scaling:
					int rawVal = settings.ScalingEnemyPointsStartAmount + (settings.ScalingEnemyPointsScalingAmount * (roomData.CriticalPathIndex - 1));
					int maxVal = settings.ScalingEnemyPointsMaxAmount;
					if(maxVal <= 0)
					{
						return rawVal;
					}
					return rawVal >= maxVal ? maxVal : rawVal;
					//return settings.ScalingEnemyPointsStartAmount + (settings.ScalingEnemyPointsScalingAmount * (roomData.CriticalPathIndex - 1));

			}
		}

		EnemyDefinition GetRandomEnemyDefForDifficultyPoints(int difficultyPoints, LayoutData layoutData, ELevelTheme theme, bool useMaxPerLevel, bool useMaxPerRoom, Guid roomId, System.Random random, bool onlyClosestToDiffPoints = false)
		{
			List<EnemyDefinition> allPossibleDefs;
			if(onlyClosestToDiffPoints)
			{
				allPossibleDefs = EnemyLoadSystem.Instance.GetAllDefinitionsClosestToDifficultyPoints(difficultyPoints, layoutData.EnemyCount_LUT, theme);
			}
			else
			{
				allPossibleDefs = EnemyLoadSystem.Instance.GetAllDefinitionsForDifficultyPointsAndTheme(difficultyPoints, layoutData.EnemyCount_LUT, theme);
            }
            if (allPossibleDefs.Count == 0)
			{
				Debug.LogWarning("[ENEMYGEN] - no possible enemy definitions remaining for room " + roomId.ToString() + " - it will appear with no enemies!");
				return null;
			}

			List<WeightedListItem> weightedItems = new();
			foreach (EnemyDefinition possibleDef in allPossibleDefs)
			{
				weightedItems.Add(possibleDef);
			}

			List<EnemyDefinition> defsExceedingMaxLimit = new();

			GenerationRoomData roomData = layoutData.Room_LUT[roomId];
			foreach(EnemyDefinition enemy in allPossibleDefs)
			{
				if (defsExceedingMaxLimit.Contains(enemy))
				{
					continue;
				}

				if (enemy.MaxPerLevel > 0 && useMaxPerLevel)
				{
					if(layoutData.EnemyCount_LUT.TryGetValue(enemy.Id, out int countForLevel))
					{
						if(countForLevel >= enemy.MaxPerLevel)
						{
							defsExceedingMaxLimit.Add(enemy);
						}
					}
				}

				if(enemy.MaxPerRoom > 0 && useMaxPerRoom)
				{
					if(roomData.EnemyCount_LUT.TryGetValue(enemy.Id, out int countForRoom))
					{
						if(countForRoom >= enemy.MaxPerRoom)
						{
							defsExceedingMaxLimit.Add(enemy);
						}
					}
				}
			}

			if(defsExceedingMaxLimit.Count != allPossibleDefs.Count)
			{
				foreach(EnemyDefinition removeDef in defsExceedingMaxLimit)
				{
					allPossibleDefs.Remove(removeDef);
				}
			}

			List<WeightedListItem> listItems = new();
			foreach(EnemyDefinition def in allPossibleDefs)
			{
				listItems.Add(def);
			}

			EnemyDefinition enemyDef = (EnemyDefinition)GameplayHelper.GetWeightedListItem(listItems, random);
			return enemyDef;
		}

		RoomDefinition GetRandomRoomForConnectionDataAndType(GenerationRoomData.GenerationRoomConnectionData connectionData, ERoomType type, LayoutData layoutData, System.Random random, bool useRoomsMaxPerLevel, out bool success)
		{
			if(!useRoomsMaxPerLevel)
			{
				return RoomDefinitionLoadSystem.Instance.GetRandomDefinition(random, connectionData, out success, type);
			}
			List<RoomDefinition> possibleRooms = RoomDefinitionLoadSystem.Instance.GetAllDefinitionsForTypeAndConnections(connectionData, out success, type);
			List<RoomDefinition> roomsExceedingMaxLimit = new();
			foreach (RoomDefinition room in possibleRooms)
			{
				int limit = room.MaxPerLevel;
				if(room.MaxPerLevel > 0)
				{
					if (layoutData.RoomCount_LUT.TryGetValue(room.Id, out int currentAmount))
					{
						if (currentAmount >= limit)
						{
							roomsExceedingMaxLimit.Add(room);
						}
					}
				}
			}

			if (roomsExceedingMaxLimit.Count != possibleRooms.Count)
			{
				foreach (RoomDefinition overLimitRoom in roomsExceedingMaxLimit)
				{
					possibleRooms.Remove(overLimitRoom);
				}
			}

			List<WeightedListItem> listItems = new();
			foreach (RoomDefinition room in possibleRooms)
			{
				listItems.Add(room);
			}
			RoomDefinition roomDef = (RoomDefinition)GameplayHelper.GetWeightedListItem(listItems, random);
			return roomDef;
		}

		bool TryGetSpawnPosForEnemy(EnemyDefinition enemyDef, GenerationRoomData roomData, System.Random random, out Vector3Int validPos)
		{
			for(int setSpawnAttempt = 0; setSpawnAttempt < SET_ENEMY_POS_ATTEMPTS; ++setSpawnAttempt)
			{				
				int floorPosIndex = random.Next(roomData.UnoccupiedFloorPositions.Count);
				Vector3Int floorPos = roomData.UnoccupiedFloorPositions[floorPosIndex];

				if (!roomData.CanPlaceEnemyAtPoint(enemyDef, floorPos))
				{
					continue;
				}

				Debug.Log("[ENEMYGEN] found spawn for enemy " + enemyDef + " after " + setSpawnAttempt.ToString() + " attempts");
				validPos = floorPos;
				return true;
			}

			validPos = new(-1, -1);
			return false;
		}
	}
}