using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JZK.UI
{
	public interface IUISystem
	{
		public UIController Controller { get; }

		public void SetActive(bool active);
		public void UpdateSystem();
		public void LateUpdateSystem();
	}
}