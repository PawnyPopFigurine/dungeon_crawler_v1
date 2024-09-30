using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework;

namespace UI
{
	public class UIStateSystem : PersistentSystem<UIStateSystem>
	{

		#region PersistentSystem

		public SystemLoadData _loadData = new SystemLoadData()
		{
			LoadStates = SystemLoadState.NoLoadingNeeded,
			UpdateAfterLoadingState = ELoadingState.FrontEnd
		};

		public override SystemLoadData LoadData => _loadData;

		public override void Initialise()
		{
			base.Initialise();

			for(int uiIndex = 0; uiIndex < (int)EUIState.Max; uiIndex++)
			{
				EUIState state = (EUIState)uiIndex;
				IUISystem system = GetSystemForUIState(state);

				if(system == null)
				{
					continue;
				}

				system.SetActive(false);
			}


		}

		public override void UpdateSystem()
		{
			base.UpdateSystem();

			if(!HasLoadedAll())
			{
				return;
			}

			UpdateState();
		}

		public override void LateUpdateSystem()
		{
			base.LateUpdateSystem();

			LateUpdateState();
		}

		public override void SetCallbacks()
		{
			base.SetCallbacks();

			SceneInit.CurrentSceneInit.OnLoadingStateComplete -= OnLoadingStateComplete;
			SceneInit.CurrentSceneInit.OnLoadingStateComplete += OnLoadingStateComplete;
		}

		#endregion

		public enum EUIState
		{
			None,
			PressStart,
			MainMenu,

			Max
		}

		[SerializeField] EUIState _previousState = EUIState.None;
		[SerializeField] EUIState _currentState = EUIState.None;

		public EUIState PreviousState => _previousState;
		public EUIState CurrentState => _currentState;

		[SerializeField] IUISystem _activeUISystem;

		[SerializeField] bool _hasActiveUI;

		private void UpdateState()
		{
			if(_hasActiveUI)
			{
				_activeUISystem.UpdateSystem();
			}
		}

		private void LateUpdateState()
		{
			if(_hasActiveUI)
			{
				_activeUISystem.LateUpdateSystem();
			}
		}

		IUISystem GetSystemForUIState(EUIState state)
		{
			switch(state)
			{
                case EUIState.PressStart:
                    return PressStartUISystem.Instance;
                default:
					Debug.LogWarning("[UI] No UI System found for UI State " + state.ToString());
					return null;
			}
		}

		public void TransitionToState(EUIState state)
		{
			if(state == _currentState)
			{
				return;
			}

			//Must clear InputSystem here
			Input.InputSystem.Instance.Clear();

			_previousState = _currentState;
			_currentState = state;

			bool hasActiveUIBeforeTransition = _activeUISystem != null;

			if(hasActiveUIBeforeTransition)
            {
				_activeUISystem.SetActive(false);
            }

			_activeUISystem = GetSystemForUIState(state);
			_hasActiveUI = _activeUISystem != null;

			if(_hasActiveUI)
			{
				_activeUISystem.SetActive(true);
			}

		}

		public void HideAllUI()
		{
			EnterScreen(EUIState.None);
		}

		public void EnterScreen(EUIState state)
		{
			//functionality for changing audio goes here

			TransitionToState(state);
		}

		public void EnterPreviousScreen()
		{
			EnterScreen(_previousState);
		}

		public void QuitToDesktop()
		{
#if UNITY_EDITOR
			UnityEditor.EditorApplication.ExitPlaymode();
#else
			Application.Quit();
#endif

		}

		private EUIState _pauseLastState;

		public void Pause()
		{
			//TODO: reimplement pausing if needed
			/*if(_currentState == EUIState.LevelPause)
			{
				return;
			}

			_pauseLastState = CurrentState;
			EnterScreen(EUIState.LevelPause);*/
		}

		public void UnPause()
		{
			EnterScreen(_pauseLastState);
			_pauseLastState = EUIState.None;
		}

		#region Callbacks

		private void OnLoadingStateComplete(ELoadingState state)
		{
			if(state != ELoadingState.FrontEnd)
			{
				return;
			}

			if(SceneInit.IsTestScene)
			{
				return;
			}

			EnterScreen(EUIState.PressStart);
		}

		#endregion //Callbacks



	}
}