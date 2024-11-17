using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JZK.UI
{
	public abstract class UIController : MonoBehaviour
	{
		public delegate void ControllerEvent();

		public ControllerEvent OnEnterScreenStart;
		public ControllerEvent OnEnterScreenComplete;
		public ControllerEvent OnExitScreenStart;
		public ControllerEvent OnExitScreenComplete;

		[Header("Front End Controller")]
		[SerializeField] Canvas _rootCanvas;
		[SerializeField] CanvasGroup _rootCanvasGroup;

		protected bool _isActive;
		bool _hasCanvasGroup = false;

		#region Initialisation

		public virtual void Initialise()
		{
			_isActive = gameObject.activeSelf;
			_hasCanvasGroup = _rootCanvasGroup != null;
		}

		#endregion //Initialisation

		#region Activation

		public virtual void SetActive(bool active)
		{
			if(active == _isActive)
			{
				return;
			}

			_isActive = active;
			gameObject.SetActive(active);
		}

		public bool IsActive()
		{
			return _isActive;
		}

		#endregion //Activation

		#region Update

		public virtual void UpdateController()
		{

		}

		#endregion //Update

		public void SetInteractive(bool isInteractive)
		{
			if(_hasCanvasGroup)
			{
				_rootCanvasGroup.blocksRaycasts = isInteractive;
			}
		}



	}
}