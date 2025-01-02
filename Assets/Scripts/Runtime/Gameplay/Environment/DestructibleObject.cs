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

		[SerializeField] EnemyController _controller;

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
			if(null != _controller)
			{
				if(!_controller.PlayerInRoom)
				{
					return;
				}
			}
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

        public void ResetObject()
        {
			_currentHitsLeft = _maxHealth;
			gameObject.SetActive(true);
        }
    }
}