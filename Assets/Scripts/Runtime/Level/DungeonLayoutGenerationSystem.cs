using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JZK.Framework;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using JZK.Utility;
using UnityEngine.Tilemaps;
using JZK.Gameplay;
using System;

namespace JZK.Level
{
	[System.Serializable]
	public class LayoutGenerationSettings
	{
		public int Seed;
		public int CriticalPathRoomCount;
	}

	[System.Serializable]
	public class LayoutData
	{
		public Dictionary<Guid, GenerationRoomData> Room_LUT = new();
		public Dictionary<Guid, GenerationDoorData> Door_LUT = new();
		public List<Guid> CriticalPathIds = new();
		//TODO: theme enum
	}

	[System.Serializable]
	public class GenerationRoomData
	{
		//TODO: override theme enum
		public Guid Id = Guid.NewGuid();
		public string PrefabId;
		//public Dictionary<Guid, GenerationDoorData> Door_LUT = new();
		public List<Guid> AllDoorIds = new();
		public LayoutData ParentLayout;
		
		public void Initialise(RoomDefinition def, LayoutData parent)
		{
			ParentLayout = parent;
			PrefabId = def.Id;
			//Door_LUT.Clear();
			AllDoorIds.Clear();

			RoomController controller = def.PrefabController.GetComponent<RoomController>();

			foreach(RoomDoor door in controller.Doors)
			{
				GenerationDoorData doorData = new();
				doorData.IndexInRoom = controller.GetIndexOfDoor(door);
				doorData.ParentRoomId = Id;
				doorData.SideOfRoom = door.SideOfRoom;
				parent.Door_LUT.Add(doorData.Id, doorData);
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
			foreach(GenerationDoorData door in ParentLayout.Door_LUT.Values)
			{
				if(door.ParentRoomId != Id)
				{
					continue;
				}

				if(door.SideOfRoom != roomSide)
				{
					continue;
				}

				if(mustBeUnlinked)
				{
					if(door.IsLinked)
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

			if(updateLinkingDoor)
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

		public LayoutData GenerateDungeonLayout(LayoutGenerationSettings settings, System.Random random)
		{
			LayoutData layoutData = new();

			for(int critPathIndex = 0; critPathIndex < settings.CriticalPathRoomCount; ++critPathIndex)
			{
				RoomDefinition roomDef = RoomDefinitionLoadSystem.Instance.GetRandomDefinition(random);
				GenerationRoomData roomData = new();
				roomData.Initialise(roomDef, layoutData);
				//foreach(GenerationDoorData doorData in roomData.doo)
				layoutData.Room_LUT.Add(roomData.Id, roomData);
				layoutData.CriticalPathIds.Add(roomData.Id);
			}

			for(int roomIndex = 0; roomIndex < layoutData.CriticalPathIds.Count; ++roomIndex)
			{
				Guid critRoomId = layoutData.CriticalPathIds[roomIndex];
				GenerationRoomData critRoom = layoutData.Room_LUT[critRoomId];
				bool isFinalRoom = roomIndex == layoutData.CriticalPathIds.Count - 1;
				if(!isFinalRoom)
				{
					Guid nextRoomId = layoutData.CriticalPathIds[roomIndex + 1];
					GenerationRoomData nextRoom = layoutData.Room_LUT[nextRoomId];
					if(!critRoom.TryLinkToRoom(nextRoom, EOrthogonalDirection.Right, out GenerationDoorData door, out GenerationDoorData nextDoor))
					{
						Debug.LogWarning("[GENERATION] failed linking from room " + critRoom.PrefabId.ToString() + " to " + nextRoom.PrefabId.ToString());
						//complain here
					}
					else
					{
						/*Debug.Log("[GENERATION] linking room " + critRoom.PrefabId.ToString() + " - " + critRoom.Id.ToString()
							+ " to room " + nextRoom.PrefabId.ToString() + " - " + nextRoom.Id.ToString());*/
                        Debug.Log("[GENERATION] linking door " + door.Id.ToString()
                            + " to door " + nextDoor.Id.ToString());
						if(critRoom == nextRoom)
						{
							Debug.Log("[GENERATION] SAME ROOM");
						}
						GenerationRoomData firstRoom = layoutData.Room_LUT[door.ParentRoomId];
						GenerationRoomData secondRoom = layoutData.Room_LUT[nextDoor.ParentRoomId];
						if(firstRoom == secondRoom)
						{
							Debug.Log("[GENERATION] SAME ROOM");
						}
                        door.Enabled = true;
						nextDoor.Enabled = true;
					}
				}
			}

			return layoutData;
		}
	}
}