using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using JZK.Framework;

namespace JZK.UI
{
	public class PressStartUISystem : PersistentSystem<PressStartUISystem>, IUISystem
	{
		#region IUISystem

		public UIController Controller => _controller;

		public void SetActive(bool active)
		{
			if (_hasLoaded)
			{
				_controller.SetActive(active);

				_active = active;
			}
		}

		#endregion //IUISystem

		#region PersistentSystem

		public override void Initialise()
		{
			base.Initialise();
		}

		private SystemLoadData _loadData = new SystemLoadData()
		{
			LoadStates = new SystemLoadState[] { new SystemLoadState { LoadStartState = ELoadingState.FrontEnd, BlockStateUntilFinished = ELoadingState.FrontEnd } },
			UpdateAfterLoadingState = ELoadingState.None,   //Update handled manually by IUISystem
		};

		public override SystemLoadData LoadData => _loadData;

		public override void StartLoading(ELoadingState state)
		{
			LoadUIPrefab();
			base.StartLoading(state);
		}

		public override void UpdateSystem()
		{
			base.UpdateSystem();

			if (_active)
			{
				_controller.UpdateController();
			}
		}

		#endregion //PresistentSystem

		#region Load

		public void LoadUIPrefab()
		{
			Addressables.LoadAssetAsync<GameObject>("Assets/Prefabs/UI/PressStart/PressStartUI.prefab").Completed += LoadCompleted;
		}

		public void LoadCompleted(AsyncOperationHandle<GameObject> op)
		{
			if (op.Result == null)
			{
				Debug.LogError(this.GetType().ToString() + " - failed to load addressable.");
				return;
			}

			_initPerfMarker.Begin(this);
			float startTime = Time.realtimeSinceStartup;

			GameObject uiRoot = Instantiate(op.Result);
			uiRoot.transform.SetParent(transform);
			uiRoot.transform.position = Vector3.zero;
			uiRoot.transform.rotation = Quaternion.identity;
			uiRoot.transform.localScale = Vector3.one;
			_controller = uiRoot.GetComponent<PressStartUIController>();
			_controller.Initialise();
			_controller.SetActive(false);

			_hasLoaded = true;
			FinishLoading(ELoadingState.FrontEnd);

			float endTime = Time.realtimeSinceStartup - startTime;
			_initPerfMarker.End();
			Debug.Log("INIT: " + GetType() + " LoadCompleted " + endTime.ToString("F2") + " seconds.)");
		}

		#endregion //Load

		private PressStartUIController _controller;
		private bool _active = false;
		public bool Active => _active;

		private bool _hasLoaded;

		#region InputSignals

		public void Input_StartButtonPressed()
		{
			Debug.Log("[HELLO] CONGRATULATIONS!! YOU PRESSED START");
			//UIStateSystem.Instance.EnterScreen(UIStateSystem.EUIState.MainMenu);
		}

		#endregion //InputSignals
	}
}