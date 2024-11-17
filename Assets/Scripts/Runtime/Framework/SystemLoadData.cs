using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JZK.Framework
{
	public class SystemLoadState
	{
		public static SystemLoadState[] NoLoadingNeeded = new SystemLoadState[0];

		public ELoadingState LoadStartState;
		public ELoadingState BlockStateUntilFinished;
		public bool LoadFinished;

		public float LoadStartTime;
		public float LoadFinishTime;

		public void StartLoading()
		{
			LoadStartTime = Time.realtimeSinceStartup;

			Debug.Log("INIT: Load State '" + LoadStartState.ToString() + "'. started loading at " + LoadStartTime.ToString("F2") + " seconds");
		}

		public void FinishLoading()
		{
			if(LoadFinished)
			{
				return;
			}

			LoadFinished = true;

			Debug.Log("INIT: Load State '" + LoadStartState.ToString() + "' took " + (LoadFinishTime - LoadStartTime).ToString("F2") + " seconds to load.");
		}
	}

	public class SystemLoadData
	{
		public SystemLoadState[] LoadStates;
		public ELoadingState UpdateAfterLoadingState;

		public bool ShouldStartLoadingState(ELoadingState state)
		{
			foreach (SystemLoadState loadState in LoadStates)
			{
				if (loadState.LoadStartState != state)
				{
					continue;
				}

				return true;
			}

			return false;
		}

		public bool IsBlockingLoadingState(ELoadingState state)
		{
			foreach(SystemLoadState loadState in LoadStates)
			{
				if(loadState.BlockStateUntilFinished != state)
				{
					continue;
				}

				if(loadState.LoadFinished)
				{
					continue;
				}

				return true;
			}

			return false;
		}

		public bool IsAllLoadingFinished()
		{
			bool incompleteLoadFound = false;
			foreach(SystemLoadState loadState in LoadStates)
			{
				if(loadState.LoadFinished)
				{
					continue;
				}

				incompleteLoadFound = true;
				break;
			}

			return !incompleteLoadFound;
		}

		public void StartLoad(ELoadingState state)
		{
			foreach(SystemLoadState loadState in LoadStates)
			{
				if(loadState.LoadStartState != state)
				{
					continue;
				}
				loadState.StartLoading();
			}
		}

		public void FinishLoad(ELoadingState state)
		{
			bool success = false;
			foreach(SystemLoadState loadState in LoadStates)
			{
				if(loadState.LoadStartState != state)
				{
					continue;
				}

				loadState.FinishLoading();
				success = true;

				if(!success)
				{
					Debug.LogError("ERROR: A system tried to finish a load that started in state " + state.ToString() + ", but it doesn't have any load data for that state. This is likely to break your game loading.");
				}
			}
		}
	}
}