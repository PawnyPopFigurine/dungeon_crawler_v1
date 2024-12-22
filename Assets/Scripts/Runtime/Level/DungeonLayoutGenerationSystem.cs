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
		//TODO: theme enum
	}

	[System.Serializable]
	public class GenerationRoomData
	{
		//TODO: override theme enum
		public Guid Id = Guid.NewGuid();
		public string PrefabId;
		public Dictionary<Guid, GenerationDoorData> Door_LUT = new();
		
		public void Initialise(RoomDefinition def)
		{
			PrefabId = def.Id;
			Door_LUT.Clear();
			foreach(RoomDoor door in def.PrefabController.Doors)
			{
				GenerationDoorData doorData = new();
				doorData.SideOfRoom = door.SideOfRoom;
				Door_LUT.Add(doorData.Id, doorData);
			}
		}

	}

	[System.Serializable]
	public class GenerationDoorData
	{
		public Guid Id = Guid.NewGuid();
		public EOrthogonalDirection SideOfRoom;
		public bool IsLinked;
		public Guid LinkToDoor;
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
				roomData.Initialise(roomDef);
				layoutData.Room_LUT.Add(roomData.Id, roomData);

				/*string definitionId = RoomDefinitionLoadSystem.Instance.GetRandomRoomId(random);
				GenerationRoomData roomData = new();
				roomData.InitialiseForId(definitionId);*/

			}

			return layoutData;
		}
	}
}