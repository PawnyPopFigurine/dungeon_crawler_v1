using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Framework
{

	public interface ISystemReference<out T> where T : MonoBehaviour
	{
		public void InstantiateSystem();
		public void InitialiseSystem();
		public void SetCallbacks();

		public SystemLoadData LoadData
		{
			get;
		}

		public void StartLoading(ELoadingState state);

		public void UpdateSystem(bool profiling = false);
		public void LateUpdateSystem();
		public void FixedUpdateSystem();
	}

	[System.Serializable]
	public class SystemReference<T> : ISystemReference<T> where T : MonoBehaviour
	{
		Type _systemType;
		public Type SystemType => _systemType;

		T _system;
		public T System
		{
			get { return _system; }
			set { _system = value; }
		}

		SystemLoadData _loadData;

		public SystemLoadData LoadData
		{
			get { return _loadData; }
			set { _loadData = value; }
		}

		PersistentSystem<T> _persistentSystem;

		public SystemReference()
		{
			_systemType = typeof(T);
		}

		public void InstantiateSystem()
		{
			GameObject gameObject = new GameObject();
			Type type = SystemType;
			Component systemComponent = gameObject.AddComponent(_systemType);

			if(systemComponent is not T systemObject)
			{
				Debug.LogError("SystemSetup created a component of a type different to the one it was told to! This should not happen! Type: " + SystemType.ToString() + ")");
				return;
			}

			if(systemObject is PersistentSystem<T> ps)
			{
				_persistentSystem = ps;
			}

			else
			{
				Debug.LogError("SystemSetup created a component that is not a persistent system! Type: " + SystemType.ToString() + "}");
				return;
			}

			gameObject.name = type.ToString();

			System = systemObject;
			LoadData = _persistentSystem.LoadData;
		}

		public void InitialiseSystem()
		{
			_persistentSystem.Initialise();
		}

		public void SetCallbacks()
		{
			_persistentSystem.SetCallbacks();
		}

		public void StartLoading(ELoadingState state)
		{
			_persistentSystem.StartLoading(state);
		}

		public void UpdateSystem(bool profiling = false)
		{
			if(profiling)
			{
				_persistentSystem.UpdateSystemWithProfiling();
			}
			else
			{
				_persistentSystem.UpdateSystem();
			}
		}

		public void LateUpdateSystem()
		{
			_persistentSystem.LateUpdateSystem();
		}

		public void FixedUpdateSystem()
		{
			_persistentSystem.FixedUpdateSystem();
		}
	}

	public enum ELoadingState
	{
		/// <summary>
		/// Used for debugging systems such as TerminalSystem
		/// </summary>
		SplashData = 0,
		Splash,

		/// <summary>
		/// Non-blocking early stuff
		/// </summary>
		AsyncData,
		Async,

		/// <summary>
		/// Blocking key framework stuff
		/// </summary>
		BootSequenceData,
		BootSequence,

		/// <summary>
		/// Blocking required by front end
		/// </summary>
		FrontEndData,
		FrontEnd,

		/// <summary>
		/// Empty marker state
		/// </summary>
		InputReady,

		/// <summary>
		/// Empty marker state
		/// </summary>
		ReadyForMenus,

		/// <summary>
		/// Blocking required by game - see also async which may need to have completed by now
		/// </summary>
		GameData,
		Game,

		/// <summary>
		/// Empty marker state
		/// </summary>
		GameReady,

		/// <summary>
		/// On demand game stuff that can start loading from this point onwards
		/// </summary>
		LazyGameData,
		LazyGame,

		/// <summary>
		/// All loading complete
		/// </summary>
		Finished,
		None,
	}

	[Serializable]
	public class SceneInit : MonoBehaviour
	{
		public delegate void LoadingStateEvent(ELoadingState state);
		public LoadingStateEvent OnLoadingStateComplete;

		public static SceneInit CurrentSceneInit;

		public static ELoadingState CurrentLoadingState;

		[Header("Scene Init")]
		[SerializeField]
		ELoadingState _currentLoadingState;

		public static float TotalLoadTime { get; private set; } = -1;

		protected bool _isTestScene;
		public static bool IsTestScene => CurrentSceneInit._isTestScene;

		ISystemReference<MonoBehaviour>[] _systems;

#if UNITY_EDITOR || DEBUG
		[SerializeField]
		bool _printFirstUnloadedSystem;

		public void PrintFirstUnloadedSystem()
		{
			_printFirstUnloadedSystem = true;
		}

#endif //UNITY_EDITOR || DEBUG

		protected void Setup(ISystemReference<MonoBehaviour>[] systems)
		{
			CurrentSceneInit = this;

			_systems = systems;

			InstantiateSystems();
			InitialiseSystems();
			SetSystemCallbacks();

			CurrentLoadingState = ELoadingState.None;
			ELoadingState startingLoadingState = 0;
			StartLoadingState(startingLoadingState);
		}

		void InstantiateSystems()
		{
			for(int systemIndex = 0; systemIndex < _systems.Length; systemIndex++)
			{
				_systems[systemIndex].InstantiateSystem();
			}
		}

		void InitialiseSystems()
		{
			for(int systemIndex = 0; systemIndex < _systems.Length; systemIndex++)
			{
				_systems[systemIndex].InitialiseSystem();
			}
		}

		void SetSystemCallbacks()
		{
			for (int systemIndex = 0; systemIndex < _systems.Length; systemIndex++)
			{
				_systems[systemIndex].SetCallbacks();
			}
		}

		private float _startLoadTime = 0f;
		private float _lastLoadTime = 0f;

		public void StartLoadingState(ELoadingState loadingState)
		{
			if(loadingState != CurrentLoadingState && CurrentLoadingState != ELoadingState.None)
			{
				_lastLoadTime = Time.realtimeSinceStartup;
				Debug.Log("\n" + _lastLoadTime + " : Loading State Complete [" + CurrentLoadingState + "]");
			}

			Debug.Log("\n" + Time.realtimeSinceStartup + " : Starting Loading State [" + loadingState.ToString() + "]");

			if(loadingState != ELoadingState.Finished)
			{
				for (int systemIndex = 0; systemIndex < _systems.Length; ++systemIndex)
				{
					if(!_systems[systemIndex].LoadData.ShouldStartLoadingState(loadingState))
					{
						continue;
					}

					_systems[systemIndex].StartLoading(loadingState);
				}
			}

			else
			{
				QualitySettings.asyncUploadTimeSlice = 4;
				QualitySettings.asyncUploadBufferSize = 16;
				TotalLoadTime = Time.realtimeSinceStartup - _startLoadTime;
				Debug.Log("Total load time was: " + TotalLoadTime.ToString("F2") + " secs.");
			}

			CurrentLoadingState = loadingState;
			_currentLoadingState = loadingState;
		}

		public void UpdateScene()
		{
			if (CurrentLoadingState != ELoadingState.Finished)
			{
				UpdateLoadingState();
			}

			//Using the UpdateAfterLoadState to order the updates - room for more nuanced control here
			for (int loadingStateIndex = 0; loadingStateIndex < (int)CurrentLoadingState; loadingStateIndex++)
			{
				ELoadingState state = (ELoadingState)loadingStateIndex;

				for (int systemIndex = 0; systemIndex < _systems.Length; systemIndex++)
				{
					if (_systems[systemIndex].LoadData.UpdateAfterLoadingState != state)
					{
						continue;
					}

					_systems[systemIndex].UpdateSystem(true);
				}
			}
		}

		public void LateUpdate()
		{
			//Using the UpdateAfterLoadState to order the updates - room for more nuanced control here
			for(int loadingStateIndex = 0; loadingStateIndex < (int)CurrentLoadingState; loadingStateIndex++)
			{
				ELoadingState state = (ELoadingState)loadingStateIndex;

				for(int systemIndex = 0; systemIndex < _systems.Length; systemIndex++)
				{
					if(_systems[systemIndex].LoadData == null)
					{
						Debug.Log("null load data");
					}

					if(_systems[systemIndex].LoadData.UpdateAfterLoadingState != state)
					{
						continue;
					}

					_systems[systemIndex].LateUpdateSystem();
				}
			}
		}

		public void UpdateLoadingState()
		{
			bool nextLoadStateBlocked = false;
			for (int systemIndex = 0; systemIndex < _systems.Length; systemIndex++)
			{
				if(_systems[systemIndex].LoadData == null)
				{
					Debug.Log("null load data");
				}
				if(!_systems[systemIndex].LoadData.IsBlockingLoadingState(CurrentLoadingState))
				{
					continue;
				}

#if UNITY_EDITOR || DEBUG
				if(_printFirstUnloadedSystem)
				{
					Debug.Log(this.name + " - Next loading state is blocked by " + _systems[systemIndex].ToString());
					_printFirstUnloadedSystem = false;
				}
#endif //UNITY_EDITOR || DEBUG

				nextLoadStateBlocked = true;
				break;
			}

			if(nextLoadStateBlocked)
			{
				return;
			}

			OnLoadingStateComplete?.Invoke(_currentLoadingState);
			StartLoadingState((ELoadingState)((int)CurrentLoadingState + 1));
		}

		public static bool HasCompletedLoadingState(ELoadingState state)
		{
			return CurrentLoadingState > state;
		}
	}
}