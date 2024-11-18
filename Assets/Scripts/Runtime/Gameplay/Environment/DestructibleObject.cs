using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JZK.Gameplay
{
    public class DestructibleObject : MonoBehaviour
    {
        [SerializeField] int _maxHealth;

        int _currentHitsLeft;

		private void Start()
		{
			Initialise();
		}

		public void Initialise()
		{
            _currentHitsLeft = _maxHealth;
		}

		/*public void OnCollisionEnter2D(Collision2D collision)
		{
			if(collision.gameObject.tag == "PlayerProjectile")
			{
				OnHitByPlayerProjectile();
			}
		}*/

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
		}
	}
}