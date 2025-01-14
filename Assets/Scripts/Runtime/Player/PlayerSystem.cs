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
		public event PlayerEvent OnPlayerDead;

		bool _invincibleCheatActive = false;
		public bool InvincibleCheatActive => _invincibleCheatActive;

		public bool AtMaxHealth => _controller.CurrentHealth == _controller.MaxHealth;


		public override void UpdateSystem()
		{
			base.UpdateSystem();

			_controller.UpdateController();
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

		public void StartForPlayerTestScene(Transform spawnPoint)
		{
			UI.GameplayUISystem.Instance.RefreshHealthBarForCurrentHealth();
			_controller.SetActive(true);
			_controller.transform.position = spawnPoint.position;
		}

		public void SetInvincibleCheat(bool cheatActive)
		{
			_invincibleCheatActive = cheatActive;
		}

		public Vector2 GetPlayerPos()
		{
			if (null == _controller)
			{
				return GameplayHelper.INVALID_PLAYER_POS;
			}

			return _controller.transform.position;
		}

		public void SetPlayerPos(Vector2 newPos)
		{
			if (null == _controller)
			{
				return;
			}

			_controller.transform.position = newPos;
		}

		public void OnPlayerHitHazard(GameObject hazard)
		{
			if(_invincibleCheatActive)
			{
				return;
			}
			_controller.OnPlayerHitHazard(hazard);
			OnPlayerHit?.Invoke();
		}

		public void KillPlayer()
		{
			Debug.Log("[HELLO] PLAYER DEAD");

			//probs want a kill player event here
			OnPlayerDead?.Invoke();
		}

		public int GetPlayerHealth()
		{
			return _controller.CurrentHealth;
		}

		public void ResetPlayerHealth()
		{
			_controller.ResetPlayerHealth();
		}

		public void HealPlayerByAmount(int healAmount)
		{
			int potentialHealing = healAmount + _controller.CurrentHealth;
			int newHealthVal = Mathf.Clamp(potentialHealing, potentialHealing, _controller.MaxHealth);
			_controller.SetPlayerHealth(newHealthVal);
			UI.GameplayUISystem.Instance.RefreshHealthBarForCurrentHealth();
		}

		public void MaxHealPlayer()
		{
			_controller.SetPlayerHealth(_controller.MaxHealth);
			UI.GameplayUISystem.Instance.RefreshHealthBarForCurrentHealth();
		}
	}
}