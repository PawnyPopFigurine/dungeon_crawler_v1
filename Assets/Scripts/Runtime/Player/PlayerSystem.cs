using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JZK.Framework;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace JZK.Gameplay
{
	public class PlayerSystem : PersistentSystem<PlayerSystem>
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

			Load();
		}

		private PlayerController _controller;

		public delegate void PlayerEvent();
		public event PlayerEvent OnPlayerHit;


		public override void UpdateSystem()
		{
			base.UpdateSystem();

			_controller.UpdateController();

			if(!_controller.PlayerAlive)
			{
				Debug.Log("[HELLO] PLAYER DEAD");
			}
		}

		#region Load

		public void Load()
		{
			Addressables.LoadAssetAsync<GameObject>("Assets/Prefabs/Player/Player.prefab").Completed += LoadCompleted;
		}

		void LoadCompleted(AsyncOperationHandle<GameObject> op)
		{
			if (op.Result == null)
			{
				Debug.LogError(this.GetType().ToString() + "- Failed to load addressable.");
				return;
			}

			_initPerfMarker.Begin(this);
			float startTime = Time.realtimeSinceStartup;

			GameObject playerPrefab = Instantiate(op.Result);
			playerPrefab.transform.SetParent(transform);
			playerPrefab.transform.position = Vector3.zero;

			PlayerController controller = playerPrefab.GetComponent<PlayerController>();
			_controller = controller;

			_controller.Initialise();
			_controller.SetActive(false);

			FinishLoading(ELoadingState.Game);

			float endTime = Time.realtimeSinceStartup - startTime;
			_initPerfMarker.End();
			Debug.Log("INIT: " + GetType() + ".InputLoadCompleted " + endTime.ToString("F2") + " sec.)");

		}

		#endregion //Load

		public void StartForPlayerTestScene()
		{
			_controller.SetActive(true);
		}

		public Vector2 GetPlayerPos()
		{
			if (null == _controller)
			{
				return GameplayHelper.INVALID_PLAYER_POS;
			}

			return _controller.transform.position;
		}

		public void OnPlayerHitHazard(GameObject hazard)
		{
			_controller.OnPlayerHitHazard(hazard);
			OnPlayerHit?.Invoke();
		}

		public void KillPlayer()
		{
			//probs want a kill player event here
		}

		public int GetPlayerHealth()
		{
			return _controller.CurrentHealth;
		}
	}
}