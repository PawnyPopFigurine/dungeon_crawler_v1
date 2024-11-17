using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Profiling;

namespace JZK.Framework
{

	public abstract class PersistentSystem<T> : MonoBehaviour where T : MonoBehaviour
	{

		protected ProfilerMarker _loadPerfMarker;
		protected ProfilerMarker _initPerfMarker;

		protected static double NANO_TO_SECONDS_FACTOR = 1d / 1000000000d;
		protected static T _instance;

		private bool _updateProfilingEnabled = true;

		private string _profilerString;

		private bool _isInitialised = false;
		public bool IsInitialised => _isInitialised;

		public virtual void Awake()
		{
			_loadPerfMarker = new ProfilerMarker(this + ".Load");
			_initPerfMarker = new ProfilerMarker(this + ".Init");

			applicationInQuitting = false;

			if(_instance != null && _instance != this)
			{
				GameObject.Destroy(this.gameObject);
				return;
			}

			DontDestroyOnLoad(this);

			_instance = this as T;
			_profilerString = string.Format("{0} - UpdateSystem()", this.GetType());

		}

		public abstract SystemLoadData LoadData
		{
			get;
		}

		public virtual void Initialise()
		{
			if(_isInitialised)
			{
				return;
			}

			_isInitialised = true;
		}

		public virtual void SetCallbacks()
		{

		}

		public virtual void StartLoading(ELoadingState state)
		{
			Debug.Log(this.GetType().ToString() + "StartLoading(" + state.ToString() + ")...");

			LoadData.StartLoad(state);
		}

		public void FinishLoading(ELoadingState state)
		{
			Debug.Log(this.GetType().ToString() + "FinishLoading(" + state.ToString() + ")...");

			LoadData.FinishLoad(state);
		}

		public bool HasLoadedAll()
		{
			return LoadData.IsAllLoadingFinished();
		}

		public void UpdateSystemWithProfiling()
		{
			if (_updateProfilingEnabled) UnityEngine.Profiling.Profiler.BeginSample(_profilerString);
			UpdateSystem();
			if (_updateProfilingEnabled) UnityEngine.Profiling.Profiler.EndSample();
		}

		public virtual void UpdateSystem()
		{

		}

		public virtual void LateUpdateSystem()
		{

		}

		public virtual void FixedUpdateSystem()
		{

		}

		public virtual void ResetSystem()
		{

		}

		public void Update()
		{
			
		}

		public static T Instance
		{
			get
			{
				if(applicationInQuitting)
				{
					return null;
				}

				return _instance;
			}
		}

		private static bool applicationInQuitting = false;

		/// <summary>
		/// When Unity quits, it destroys objects in a random order.
		/// In prinviple, a Singleton is only destroyed when the application quits.
		/// If any script calls Instance after it's been destroyed,
		///		it creates a buggy ghost object that will stay on the Editor scene
		///		even after stopping playing the Application. Really bad!
		///	So, this was made to be sure we're not creating that buggy ghost object.
		/// </summary>

		public virtual void OnDestroy()
		{
			if(_instance == this)
			{
				return;
			}

			applicationInQuitting = true;
		}

		public void OnEnable()
		{
			applicationInQuitting = false;
		}
	}
}