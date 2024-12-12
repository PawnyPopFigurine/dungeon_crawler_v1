using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JZK.Framework;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using JZK.Utility;
using UnityEngine.Tilemaps;

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

			_roomBoundsList = new List<BoundsInt>();
			_activeRoomsList = new List<GameObject>();
		}

		public override void StartLoading(ELoadingState state)
		{
			base.StartLoading(state);
		}

		

		public void GenerateLayout(LayoutGenerationSettings settings)
		{
			System.Random random = new(settings.Seed);
			var roomsList = ProceduralGeneration.BinarySpacePartitioning(new BoundsInt((Vector3Int)settings.StartPos, new Vector3Int(settings.DungeonWidth, settings.DungeonHeight, 0)), settings.MinRoomWidth, settings.MinRoomHeight, random);

			_roomBoundsList = roomsList;

			//SimpleRoomPrefabPlacement(random);

		}

		public void SimpleRoomPrefabPlacement(System.Random random, Tilemap tileMap)
		{
			foreach (BoundsInt roomBounds in _roomBoundsList)
			{
				if(RoomLoadSystem.Instance.GetRandomRoom(random, out Gameplay.RoomController controller))
				{
					controller.transform.parent = transform;
					Vector3Int intCentreCoord = new((int)roomBounds.center.x, (int)roomBounds.center.y, (int)roomBounds.center.z);
					Vector3 roomWorldPos = tileMap.CellToWorld(intCentreCoord);
					controller.transform.position = roomWorldPos;
					controller.gameObject.SetActive(true);
				}
				/*string randomRoomId =RoomLoadSystem.Instance.getr
				CreateRoomAtCoordinate(roomBounds.center, _roomPrefab);*/
			}
		}

		private List<BoundsInt> _roomBoundsList;
		public List<BoundsInt> RoomBoundsList => _roomBoundsList;

		private List<GameObject> _activeRoomsList;
		public List<GameObject> ActiveRoomsList => _activeRoomsList;

	}
}