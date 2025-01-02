using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace JZK.UI
{
	public class GameplayUIController : UI.UIController
	{
		[SerializeField] HealthMeter _healthMeter;
		[SerializeField] GameObject _deathOverlayRootGO;

		[SerializeField] Button _restartButton;

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

		public override void Initialise()
		{
			base.Initialise();

			_deathOverlayRootGO.SetActive(false);
		}

		#endregion //UIController


		public void UpdateInput()
		{


		}

		public void OnPlayerDead()
		{
			_deathOverlayRootGO.SetActive(true);
		}

		public void RefreshHealthBarForValue(int healthValue)
		{
			_healthMeter.UpdateIconsForHealthValue(healthValue);
		}

		public void RestartButtonPressed()
		{
			Debug.Log("RESTART");
			_deathOverlayRootGO.SetActive(false);
			Gameplay.GameplaySystem.Instance.Restart();
		}

	}
}