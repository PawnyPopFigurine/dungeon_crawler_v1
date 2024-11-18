using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JZK.Gameplay
{
    public class DeathCollision : MonoBehaviour
    {
		public void OnTriggerEnter2D(Collider2D collision)
		{
			if(collision.gameObject.tag == "PlayerTrigger")
			{
				PlayerSystem.Instance.OnPlayerHitHazard(this.gameObject);
			}
		}

		public void OnTriggerStay2D(Collider2D collision)
		{
			if (collision.gameObject.tag == "PlayerTrigger")
			{
				PlayerSystem.Instance.OnPlayerHitHazard(this.gameObject);
			}
		}
	}
}