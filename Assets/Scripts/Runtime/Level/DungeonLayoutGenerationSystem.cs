using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JZK.Framework;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using JZK.Utility;
using UnityEngine.Tilemaps;
using JZK.Gameplay;

namespace JZK.Level
{
	[System.Serializable]
	public class LayoutGenerationSettings
	{
		public int Seed;
		public Vector2Int StartPos;
		public int MinRoomWidth;
		public int MinRoomHeight;
		public int DungeonWidth;
		public int DungeonHeight;
	}

	[System.Serializable]
	public class LayoutData
	{
		[System.Serializable]
		public class LayoutTileData
		{
			
		}

		private List<BoundsInt> _roomBoundsList = new();
		public List<BoundsInt> RoomBoundsList => _roomBoundsList;
		public void SetRoomBounds(List<BoundsInt> roomBounds)
		{
			_roomBoundsList = roomBounds;
		}

		private List<RoomController> _activeRoomsList = new();
		public List<RoomController> ActiveRoomsList => _activeRoomsList;
		public void SetActiveRooms(List<RoomController> activeRooms)
		{
			_activeRoomsList = activeRooms;
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

		public LayoutData GenerateDungeonLayout(LayoutGenerationSettings settings, System.Random random, Tilemap tileMap)
		{
			LayoutData layoutData = new();
			CreateRoomBounds(settings, layoutData);
			return layoutData;
		}
		

		public void CreateRoomBounds(LayoutGenerationSettings settings, LayoutData layoutData)
		{
			System.Random random = new(settings.Seed);
			var roomsList = ProceduralGeneration.BinarySpacePartitioning(new BoundsInt((Vector3Int)settings.StartPos, new Vector3Int(settings.DungeonWidth, settings.DungeonHeight, 0)), settings.MinRoomWidth, settings.MinRoomHeight, random);

			layoutData.SetRoomBounds(roomsList);
		}
	}
}