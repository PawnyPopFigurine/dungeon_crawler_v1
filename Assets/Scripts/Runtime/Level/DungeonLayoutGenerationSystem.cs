using JZK.Framework;
using JZK.Gameplay;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace JZK.Level
{
	[System.Serializable]
	public class LayoutGenerationSettings
	{
		public int Seed;
		public int CriticalPathRoomCount;
		[Range(0, 1)]
		public float SecondaryRoomChance;
		[Tooltip("Set to 0 or lower to remove limit")]
		public int MaxTotalRooms;
	}

	[System.Serializable]
	public class LayoutData
	{
		public Dictionary<Guid, GenerationRoomData> Room_LUT = new();
		public Dictionary<Guid, GenerationDoorData> Door_LUT = new();
		public List<Guid> CriticalPathIds = new();
		public List<Guid> SecondaryRoomIds = new();
		//TODO: theme enum
	}

	[System.Serializable]
	public class GenerationRoomData
	{
		//TODO: override theme enum
		public Guid Id = Guid.NewGuid();
		public string PrefabId;
		public ERoomType RoomType;
		public List<Guid> AllDoorIds = new();
		public LayoutData ParentLayout;
		public GenerationRoomConnectionData ConnectionData;
		public int CriticalPathIndex = -1;  //how far along the crit path this room is - even if it isn't directly on it
		public bool OnCriticalPath;

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
			AllDoorIds.Clear();

			RoomController controller = def.PrefabController.GetComponent<RoomController>();

			foreach (RoomDoor door in controller.Doors)
			{
				GenerationDoorData doorData = new();
				doorData.IndexInRoom = controller.GetIndexOfDoor(door);
				doorData.ParentRoomId = Id;
				doorData.SideOfRoom = door.SideOfRoom;
				ParentLayout.Door_LUT.Add(doorData.Id, doorData);
				AllDoorIds.Add(doorData.Id);
			}
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
			foreach (GenerationDoorData door in ParentLayout.Door_LUT.Values)
			{
				if (door.ParentRoomId != Id)
				{
					continue;
				}

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

		public LayoutData GenerateDungeonLayout(LayoutGenerationSettings settings, System.Random random, out bool generationSuccess)
		{
			float generationStartTime = Time.realtimeSinceStartup;
			LayoutData layoutData = new();
			generationSuccess = false;

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

				RoomDefinition roomDef = RoomDefinitionLoadSystem.Instance.GetRandomDefinition(random, critRoom.ConnectionData, out bool success, roomType);
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
			List<Guid> critRoomShuffledGuids = layoutData.CriticalPathIds.OrderBy(x => random.NextDouble()).ToList();
			foreach (Guid critRoomId in critRoomShuffledGuids)
			{
				//prevent secondary room addition if room limit is reached
				if (settings.MaxTotalRooms > 0)
				{
					int totalRoomCount = layoutData.Room_LUT.Count;
					if (totalRoomCount >= settings.MaxTotalRooms)
					{
						continue;
					}
				}

				GenerationRoomData critRoom = layoutData.Room_LUT[critRoomId];
				bool isFirstRoom = critRoom.CriticalPathIndex == 0;
				bool isLastRoom = critRoom.CriticalPathIndex == critRoomShuffledGuids.Count - 1;
				foreach (Guid doorId in critRoom.AllDoorIds)
				{
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
						ERoomType secondaryRoomType = ERoomType.StandardCombat;
						RoomDefinition secondaryRoomDef = RoomDefinitionLoadSystem.Instance.GetRandomDefinition(random, secondaryRoom.ConnectionData, out bool success, secondaryRoomType);
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
							if (!critRoom.TryLinkToRoom(secondaryRoom, doorData.SideOfRoom, out _, out GenerationDoorData secondaryRoomDoor))
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

			float generationEndTime = Time.realtimeSinceStartup;
			float generationTime = generationEndTime - generationStartTime;
			Debug.Log("[GENERATION] layout data took " + generationTime + " seconds to generate");
			generationSuccess = true;
			return layoutData;
		}
	}
}