using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace JZK.UI
{
	public class GameplayUIController : UI.UIController
	{
		[SerializeField] HealthMeter _healthMeter;

		#region UIController

		public override void SetActive(bool active)
		{
			base.SetActive(active);
		}

		public override void UpdateController()
		{
			if (!_isActive)
			{
				return;
			}

			base.UpdateController();

			UpdateInput();
		}

		#endregion //UIController


		public void UpdateInput()
		{


		}

		public void RefreshHealthBarForValue(int healthValue)
		{
			_healthMeter.UpdateIconsForHealthValue(healthValue);
		}

	}
}