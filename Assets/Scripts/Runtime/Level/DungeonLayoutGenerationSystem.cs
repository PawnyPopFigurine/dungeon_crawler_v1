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

		public LayoutData GenerateDungeonLayout(LayoutGenerationSettings settings, System.Random random)
		{
			LayoutData layoutData = new();


			return layoutData;
		}
	}
}