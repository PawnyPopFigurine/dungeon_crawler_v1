using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
	public class PressStartUIController : UI.UIController
	{
		#region UIController

		public override void SetActive(bool active)
		{
			base.SetActive(active);
		}

		public override void UpdateController()
		{
			if(!_isActive)
			{
				return;
			}

			base.UpdateController();

			UpdateInput();
		}

		#endregion //UIController


		public void UpdateInput()
		{
			if(Input.InputSystem.Instance.UIConfirmPressed)
			{
				Input_StartButtonClicked();
			}
		}

		#region InputSignals

		public void Input_StartButtonClicked()
		{
			PressStartUISystem.Instance.Input_StartButtonPressed();
		}

		#endregion //InputSignals
	}
}