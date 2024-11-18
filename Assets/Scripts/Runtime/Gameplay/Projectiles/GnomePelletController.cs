using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JZK.Gameplay
{
    public class GnomePelletController : ProjectileController
    {
		private void OnTriggerEnter2D(Collider2D collider)
		{
			if (collider.gameObject.tag == "DestroyPlayerProjectiles")
			{
				ProjectileSystem.Instance.ClearProjectile(this);
			}
		}
		/*public void CollisionEnter(Collision collision)
		{
			if(collision.gameObject.tag == "DestroyPlayerProjectiles")
			{
				ProjectileSystem.Instance.ClearProjectile(this);
			}
		}

		public void TriggerEnter(Collider other)
		{
			if(other.gameObject.tag == "DestroyPlayerProjectiles")
			{
				ProjectileSystem.Instance.ClearProjectile(this);
			}
		}

		public void TriggerEnter2D(Collider2D collision)
		{
			if(collision.gameObject.tag == "DestroyPlayerProjectiles")
			{
				ProjectileSystem.Instance.ClearProjectile(this);
			}
		}

		public void CollisionEnter2D(Collision2D collision)
		{
			if (collision.gameObject.tag == "DestroyPlayerProjectiles")
			{
				ProjectileSystem.Instance.ClearProjectile(this);
			}
		}*/
	}
}