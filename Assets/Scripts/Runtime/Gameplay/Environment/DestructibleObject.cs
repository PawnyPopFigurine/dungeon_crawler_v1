using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JZK.Gameplay
{
    public class DestructibleObject : MonoBehaviour
    {
        [SerializeField] int _maxHealth;

        int _currentHitsLeft;

		public delegate void DestructibleObjectEvent();
		public event DestructibleObjectEvent OnObjectDestroyed;

		private void Start()
		{
			Initialise();
		}

		public void Initialise()
		{
            _currentHitsLeft = _maxHealth;
		}

		public void OnHitByPlayerProjectile()
		{
			_currentHitsLeft -= 1;

			if(_currentHitsLeft <= 0)
			{
				OnHealthReachZero();
			}
		}

		void OnHealthReachZero()
		{
			gameObject.SetActive(false);
			OnObjectDestroyed?.Invoke();
		}
	}
}