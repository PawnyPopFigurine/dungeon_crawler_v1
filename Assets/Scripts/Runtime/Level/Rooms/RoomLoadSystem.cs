using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JZK.Framework;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using JZK.Gameplay;

namespace JZK.Level
{
	public class RoomLoadSystem : PersistentSystem<RoomLoadSystem>
	{
		private SystemLoadData _loadData = new SystemLoadData()
		{
			LoadStates = new SystemLoadState[] { new SystemLoadState { LoadStartState = ELoadingState.Game, BlockStateUntilFinished = ELoadingState.Game } },
			UpdateAfterLoadingState = ELoadingState.Game,
		};

		public override SystemLoadData LoadData
		{
			get { return _loadData; }
		}

		public override void StartLoading(ELoadingState state)
		{
			base.StartLoading(state);

			Addressables.LoadAssetsAsync<GameObject>("game_room_prefab", LoadedAsset).Completed += LoadCompleted;
		}

		public static int MAX_ROOMS_PER_TYPE = 10;
		public Vector2 ROOM_RESET_POS => transform.position;

		private Dictionary<string, List<RoomController>> _roomsPool = new();
		private List<RoomController> _activeRooms = new();

		private List<string> _allRoomIds = new();

		public bool RequestRoom(string roomId, out RoomController controller)
		{
			controller = null;
			if(!_roomsPool.TryGetValue(roomId, out List<RoomController> roomPool))
			{
				Debug.LogWarning("Tried to access room pool of invalid ID " + roomId);
				return false;
			}
			
			if(roomPool.Count == 0)
			{
				Debug.LogWarning("Room pool for type " + roomId + " is empty!");
				return false;
			}

			controller = roomPool[0];
			roomPool.Remove(controller);
			_activeRooms.Add(controller);

			return true;
		}

		public bool GetRandomRoom(System.Random random, out RoomController controller)
		{
			string roomId = GetRandomRoomId(random);
			return RequestRoom(roomId, out controller);
		}

		public string GetRandomRoomId(System.Random random)
		{
			int roomIdIndex = random.Next(0, _allRoomIds.Count);
			return _allRoomIds[roomIdIndex];
		}

		#region Load

		void LoadedAsset(GameObject asset)
		{

		}

		void LoadCompleted(AsyncOperationHandle<IList<GameObject>> assets)
		{
			if (!assets.IsDone)
			{
				//complain here
				return;
			}

			IList<GameObject> prefabs = assets.Result;
			if (prefabs == null)
			{
				//complain here
				return;
			}

			foreach (GameObject prefab in prefabs)
			{
				RoomController roomComponent = prefab.GetComponent<RoomController>();

				if (roomComponent == null)
				{
					Debug.LogError(this.name + " - tried to load room " + prefab.name + " with no RoomController component!!");
					continue;
				}

				string roomId = roomComponent.Id;

				List<RoomController> roomsOfId = new(MAX_ROOMS_PER_TYPE);
				for(int roomIndex = 0; roomIndex < MAX_ROOMS_PER_TYPE; ++roomIndex)
				{
					GameObject prefabGO = Instantiate(prefab);
					prefabGO.name = roomId + "_" + roomIndex.ToString();
					RoomController controller = prefabGO.GetComponent<RoomController>();
					controller.InitialiseOnLoad();
					roomsOfId.Add(controller);
					prefabGO.SetActive(false);
					prefabGO.transform.SetParent(transform);
				}

				_roomsPool.Add(roomId, roomsOfId);
				_allRoomIds.Add(roomId);
			}

			FinishLoading(ELoadingState.Game);

		}

		private void ReturnRoomToPool(RoomController controller)
		{
			if (!_activeRooms.Contains(controller))
			{
				return;
			}

			_roomsPool[controller.Id].Add(controller);
			_activeRooms.Remove(controller);
		}

		public void ClearRoom(RoomController controller)
		{
			if (!_activeRooms.Contains(controller))
			{
				Debug.LogWarning(this.name + " - tried to clear projectile controller " + controller.Id.ToString() + " but it is not active - aborting action");
				return;
			}

			controller.transform.position = ROOM_RESET_POS;
			controller.gameObject.SetActive(false);
			controller.ResetController();
			controller.transform.parent = transform;

			ReturnRoomToPool(controller);
		}

		public void ClearAllActiveRooms()
		{
			foreach (RoomController projectile in _activeRooms)
			{
				projectile.transform.position = ROOM_RESET_POS;
				projectile.gameObject.SetActive(false);
				projectile.ResetController();
			}

			ReturnAllActiveRoomsToPool();
		}

		public void ReturnAllActiveRoomsToPool()
		{
			List<RoomController> activeCache = new(_activeRooms);

			foreach (RoomController controller in activeCache)
			{
				ReturnRoomToPool(controller);
			}
		}

		#endregion //Load
	}
}